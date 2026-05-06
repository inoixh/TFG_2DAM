using System.Collections.Generic;
using Firebase.Extensions;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Gestiona la lógica comercial, la generación dinámica del catálogo desde Firestore
/// y la actualización visual de las transacciones.
/// </summary>
public class MarketManager : MonoBehaviour
{
    public static MarketManager Instance;

    [Header("Paneles Principales")]
    public GameObject panelMercado;

    [Header("Panel de Previsualización")]
    public Image imagenPreview;
    public Sprite iconoGenerico;
    public TextMeshProUGUI textoNombre;
    public TextMeshProUGUI textoRareza;
    public TextMeshProUGUI textoDescripcion;
    public TextMeshProUGUI textoPrecio;

    [Header("Botones de Acción")]
    public Button botonComprar;
    public Button botonVender;
    public Button botonCerrar;

    [Header("Opciones de Tienda")]
    public Transform contenedorProductos;
    public GameObject prefabMarketSlot;

    public bool mercadoAbierto { get; private set; }

    private ItemData itemSeleccionado;
    private int indiceInventarioSeleccionado = -1;
    private bool seleccionDesdeTienda = false;
    private bool catalogoCargado = false;

    [HideInInspector]
    public List<ItemData> todosLosItems = new List<ItemData>();

    [HideInInspector]
    public bool itemsCargados = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        mercadoAbierto = false;
    }

    private void Start()
    {
        if (panelMercado != null)
            panelMercado.SetActive(false);

        botonComprar.onClick.AddListener(ComprarItem);
        botonVender.onClick.AddListener(VenderItem);
        botonCerrar.onClick.AddListener(CerrarMercado);

        CargarCatalogoDesdeFirebase();
    }

    private void Update()
    {
        if (mercadoAbierto && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CerrarMercado();
        }
    }

    /// <summary>
    /// Se conecta a Firestore, construye los ItemData en memoria, carga sus recursos (2D y 3D)
    /// y rellena la tienda filtrando la basura.
    /// </summary>
    private async void CargarCatalogoDesdeFirebase()
    {
        if (catalogoCargado)
            return;

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        CollectionReference itemsRef = db.Collection("items");

        try
        {
            QuerySnapshot snapshot = await itemsRef.GetSnapshotAsync();

            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                Dictionary<string, object> itemDict = doc.ToDictionary();

                ItemData nuevoItem = ScriptableObject.CreateInstance<ItemData>();
                nuevoItem.ID = doc.Id;

                string tipoItem = "";
                Sprite spriteUnico;
                GameObject modeloUnico;

                if (itemDict.ContainsKey("name"))
                    nuevoItem.nombreDisplay = itemDict["name"].ToString();
                if (itemDict.ContainsKey("description"))
                    nuevoItem.descripcion = itemDict["description"].ToString();
                if (itemDict.ContainsKey("price"))
                    nuevoItem.precioVenta = System.Convert.ToInt32(itemDict["price"]);

                if (itemDict.ContainsKey("type"))
                {
                    tipoItem = itemDict["type"].ToString().ToLower();
                    if (tipoItem == "rod")
                        nuevoItem.esCanaDePescar = true;
                }

                // Extracción dinámica de la rareza desde la BD
                string rarezaStr = itemDict.ContainsKey("rarity")
                    ? itemDict["rarity"].ToString().ToLower()
                    : "común";
                nuevoItem.rareza = DeterminarRareza(rarezaStr);

                spriteUnico = Resources.Load<Sprite>("ItemIcons/" + nuevoItem.ID);
                nuevoItem.icono = spriteUnico != null ? spriteUnico : iconoGenerico;

                modeloUnico = Resources.Load<GameObject>("ItemModels/" + nuevoItem.ID);
                nuevoItem.modelo3D = modeloUnico;

                todosLosItems.Add(nuevoItem);

                if (tipoItem != "trash")
                {
                    GameObject slotGO = Instantiate(prefabMarketSlot, contenedorProductos);
                    slotGO.transform.localScale = Vector3.one;

                    MarketSlot slotScript = slotGO.GetComponent<MarketSlot>();
                    if (slotScript != null)
                    {
                        slotScript.ConfigurarSlot(nuevoItem);
                    }
                }
            }

            catalogoCargado = true;
            itemsCargados = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error cargando base de datos del mercado: " + e.Message);
        }
    }

    /// <summary>
    /// Traduce el texto de la BD al enumerador correspondiente de rareza.
    /// </summary>
    private Rareza DeterminarRareza(string rarezaStr)
    {
        switch (rarezaStr)
        {
            case "común":
            case "comun":
                return Rareza.Comun;
            case "raro":
                return Rareza.Raro;
            case "especial":
                return Rareza.Especial;
            case "épico":
            case "epico":
                return Rareza.Epico;
            case "legendario":
                return Rareza.Legendario;
            default:
                return Rareza.Comun;
        }
    }

    /// <summary>
    /// Bloquea los controles del jugador, muestra la interfaz del mercado y activa la mochila.
    /// </summary>
    public void AbrirMercado()
    {
        mercadoAbierto = true;
        panelMercado.SetActive(true);

        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.panelMochila.SetActive(true);
        }

        CamaraMovement camara = FindFirstObjectByType<CamaraMovement>();
        if (camara != null)
        {
            camara.rotacionBloqueada = true;
            camara.DesbloquearCursor();
        }

        PlayerMovement movimiento = FindFirstObjectByType<PlayerMovement>();
        if (movimiento != null)
        {
            movimiento.movimientoBloqueado = true;
        }

        LimpiarPreview();
    }

    /// <summary>
    /// Oculta la interfaz del mercado, la mochila y devuelve el control de movimiento y cámara al jugador.
    /// </summary>
    public void CerrarMercado()
    {
        mercadoAbierto = false;
        panelMercado.SetActive(false);

        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.panelMochila.SetActive(false);
        }

        CamaraMovement camara = FindFirstObjectByType<CamaraMovement>();
        if (camara != null)
        {
            camara.rotacionBloqueada = false;
            camara.BloquearCursor();
        }

        PlayerMovement movimiento = FindFirstObjectByType<PlayerMovement>();
        if (movimiento != null)
        {
            movimiento.movimientoBloqueado = false;
        }
    }

    /// <summary>
    /// Muestra los datos de un objeto procedente del catálogo de la tienda y ajusta los botones.
    /// </summary>
    public void SeleccionarItemTienda(ItemData item)
    {
        itemSeleccionado = item;
        seleccionDesdeTienda = true;
        indiceInventarioSeleccionado = -1;

        ActualizarPreview(item);
        ConfigurarBotones(true, false);
    }

    /// <summary>
    /// Muestra los datos de un objeto procedente de la mochila del jugador y ajusta los botones.
    /// </summary>
    public void SeleccionarItemInventario(ItemData item, int indiceSlot)
    {
        if (item == null)
            return;

        itemSeleccionado = item;
        seleccionDesdeTienda = false;
        indiceInventarioSeleccionado = indiceSlot;

        ActualizarPreview(item);
        ConfigurarBotones(false, true);
    }

    /// <summary>
    /// Rellena el panel de vista previa con la información gráfica y de texto del objeto.
    /// </summary>
    private void ActualizarPreview(ItemData item)
    {
        imagenPreview.sprite = item.icono;
        imagenPreview.color = Color.white;
        textoNombre.text = item.nombreDisplay;
        textoRareza.text = item.rareza.NombreFormateado();
        textoDescripcion.text = item.descripcion;
        textoPrecio.text = item.precioVenta.ToString();
    }

    /// <summary>
    /// Restablece el panel de previsualización visualmente cuando no hay nada seleccionado.
    /// </summary>
    private void LimpiarPreview()
    {
        itemSeleccionado = null;
        imagenPreview.sprite = iconoGenerico;
        imagenPreview.color = new Color(1, 1, 1, 0);
        textoNombre.text = "Selecciona un objeto";
        textoRareza.text = "";
        textoDescripcion.text = "";
        textoPrecio.text = "";

        ConfigurarBotones(false, false);
    }

    /// <summary>
    /// Interacciona con el componente Button para activar o desactivar su funcionalidad.
    /// </summary>
    private void ConfigurarBotones(bool puedeComprar, bool puedeVender)
    {
        botonComprar.interactable = puedeComprar;
        botonVender.interactable = puedeVender;
    }

    /// <summary>
    /// Ejecuta la compra, descuenta dinero, añade a mochila y reproduce sonido.
    /// </summary>
    private void ComprarItem()
    {
        if (itemSeleccionado == null || !seleccionDesdeTienda)
            return;

        int dineroActual = GameManager.Instance.ObtenerDinero();

        if (dineroActual >= itemSeleccionado.precioVenta)
        {
            bool anadido = InventorySystem.Instance.AnadirObjeto(itemSeleccionado);
            if (anadido)
            {
                GameManager.Instance.AnadirDinero(-itemSeleccionado.precioVenta);
                if (SoundManager.Instance != null)
                    SoundManager.Instance.ReproducirComprar();
            }
        }
    }

    /// <summary>
    /// Ejecuta la venta, retira de la mochila, suma dinero y reproduce sonido.
    /// </summary>
    private void VenderItem()
    {
        if (itemSeleccionado == null || seleccionDesdeTienda || indiceInventarioSeleccionado == -1)
            return;

        int gananciaFinal = Mathf.RoundToInt(
            itemSeleccionado.precioVenta * GameManager.Instance.bufoPrecioVenta
        );
        GameManager.Instance.AnadirDinero(gananciaFinal);
        InventorySystem.Instance.LimpiarCasilla(indiceInventarioSeleccionado);

        if (SoundManager.Instance != null)
            SoundManager.Instance.ReproducirVender();
        LimpiarPreview();
    }
}
