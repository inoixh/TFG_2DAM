using System.Collections.Generic;
using Firebase.Extensions;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Muestra los objetos que vende el mercado, calcula precios y permite comprar y vender artículos.
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

    /// <summary>
    /// Nombra a este script como el mercado principal y apaga su pantalla al inicio.
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        mercadoAbierto = false;
    }

    /// <summary>
    /// Prepara los botones y pide descargar los productos de internet.
    /// </summary>
    private void Start()
    {
        if (panelMercado != null)
        {
            panelMercado.SetActive(false);
        }

        if (botonComprar != null)
            botonComprar.onClick.AddListener(ComprarItem);
        if (botonVender != null)
            botonVender.onClick.AddListener(VenderItem);
        if (botonCerrar != null)
            botonCerrar.onClick.AddListener(CerrarMercado);

        CargarCatalogoDesdeFirebase();
    }

    /// <summary>
    /// Permite cerrar la tienda usando la tecla Escape en lugar de hacer clic en la equis.
    /// </summary>
    private void Update()
    {
        if (mercadoAbierto && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CerrarMercado();
        }
    }

    /// <summary>
    /// Se conecta a la base de datos de los artículos, crea sus modelos en el juego y los pone en los escaparates visuales.
    /// </summary>
    private async void CargarCatalogoDesdeFirebase()
    {
        FirebaseFirestore db;
        CollectionReference itemsRef;
        QuerySnapshot snapshot;
        Dictionary<string, object> itemDict;
        ItemData nuevoItem;
        string tipoItem;
        Sprite spriteUnico;
        GameObject modeloUnico;
        string rarezaStr;
        GameObject slotGO;
        MarketSlot slotScript;

        if (!catalogoCargado)
        {
            db = FirebaseFirestore.DefaultInstance;
            itemsRef = db.Collection("items");

            try
            {
                snapshot = await itemsRef.GetSnapshotAsync();

                foreach (DocumentSnapshot doc in snapshot.Documents)
                {
                    itemDict = doc.ToDictionary();
                    nuevoItem = ScriptableObject.CreateInstance<ItemData>();
                    nuevoItem.ID = doc.Id;
                    tipoItem = "";

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
                        {
                            nuevoItem.esCanaDePescar = true;
                        }
                    }

                    if (itemDict.ContainsKey("rarity"))
                    {
                        rarezaStr = itemDict["rarity"].ToString().ToLower();
                    }
                    else
                    {
                        rarezaStr = "común";
                    }

                    nuevoItem.rareza = DeterminarRareza(rarezaStr);

                    spriteUnico = Resources.Load<Sprite>("ItemIcons/" + nuevoItem.ID);
                    if (spriteUnico != null)
                    {
                        nuevoItem.icono = spriteUnico;
                    }
                    else
                    {
                        nuevoItem.icono = iconoGenerico;
                    }

                    modeloUnico = Resources.Load<GameObject>("ItemModels/" + nuevoItem.ID);
                    nuevoItem.modelo3D = modeloUnico;

                    todosLosItems.Add(nuevoItem);

                    if (tipoItem != "trash")
                    {
                        slotGO = Instantiate(prefabMarketSlot, contenedorProductos);
                        slotGO.transform.localScale = Vector3.one;

                        slotScript = slotGO.GetComponent<MarketSlot>();
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
                Debug.LogWarning(
                    "Error al intentar obtener los objetos de la tienda: " + e.Message
                );
            }
        }
    }

    /// <summary>
    /// Traduce la categoría descargada a un valor entendible por el juego.
    /// </summary>
    private Rareza DeterminarRareza(string rarezaStr)
    {
        Rareza resultado;

        switch (rarezaStr)
        {
            case "común":
            case "comun":
                resultado = Rareza.Comun;
                break;
            case "raro":
                resultado = Rareza.Raro;
                break;
            case "especial":
                resultado = Rareza.Especial;
                break;
            case "épico":
            case "epico":
                resultado = Rareza.Epico;
                break;
            case "legendario":
                resultado = Rareza.Legendario;
                break;
            default:
                resultado = Rareza.Comun;
                break;
        }

        return resultado;
    }

    /// <summary>
    /// Muestra la pantalla del mercado, abre la mochila del jugador y congela la cámara.
    /// </summary>
    public void AbrirMercado()
    {
        CamaraMovement camara;
        PlayerMovement movimiento;

        mercadoAbierto = true;

        if (panelMercado != null)
        {
            panelMercado.SetActive(true);
        }

        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.panelMochila.SetActive(true);
        }

        camara = FindFirstObjectByType<CamaraMovement>();
        if (camara != null)
        {
            camara.rotacionBloqueada = true;
            camara.DesbloquearCursor();
        }

        movimiento = FindFirstObjectByType<PlayerMovement>();
        if (movimiento != null)
        {
            movimiento.movimientoBloqueado = true;
        }

        LimpiarPreview();
    }

    /// <summary>
    /// Oculta la pantalla del mercado, cierra la mochila y devuelve el movimiento normal al jugador.
    /// </summary>
    public void CerrarMercado()
    {
        CamaraMovement camara;
        PlayerMovement movimiento;

        mercadoAbierto = false;

        if (panelMercado != null)
        {
            panelMercado.SetActive(false);
        }

        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.panelMochila.SetActive(false);
        }

        camara = FindFirstObjectByType<CamaraMovement>();
        if (camara != null)
        {
            camara.rotacionBloqueada = false;
            camara.BloquearCursor();
        }

        movimiento = FindFirstObjectByType<PlayerMovement>();
        if (movimiento != null)
        {
            movimiento.movimientoBloqueado = false;
        }
    }

    /// <summary>
    /// Marca un objeto que vende el mercado para poder ver sus datos y habilita el botón de comprar.
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
    /// Marca un objeto propio de la mochila para ver sus datos y habilita el botón de vender.
    /// </summary>
    public void SeleccionarItemInventario(ItemData item, int indiceSlot)
    {
        if (item != null)
        {
            itemSeleccionado = item;
            seleccionDesdeTienda = false;
            indiceInventarioSeleccionado = indiceSlot;

            ActualizarPreview(item);
            ConfigurarBotones(false, true);
        }
        else
        {
            Debug.LogWarning("Se intentó seleccionar un objeto vacío de la mochila.");
        }
    }

    /// <summary>
    /// Escribe los detalles del objeto seleccionado en el cuadro lateral de información.
    /// </summary>
    private void ActualizarPreview(ItemData item)
    {
        if (imagenPreview != null)
        {
            imagenPreview.sprite = item.icono;
            imagenPreview.color = Color.white;
        }

        if (textoNombre != null)
            textoNombre.text = item.nombreDisplay;
        if (textoRareza != null)
            textoRareza.text = item.rareza.NombreFormateado();
        if (textoDescripcion != null)
            textoDescripcion.text = item.descripcion;
        if (textoPrecio != null)
            textoPrecio.text = item.precioVenta.ToString();
    }

    /// <summary>
    /// Borra los detalles del cuadro lateral para dejarlo como nuevo.
    /// </summary>
    private void LimpiarPreview()
    {
        itemSeleccionado = null;

        if (imagenPreview != null)
        {
            imagenPreview.sprite = iconoGenerico;
            imagenPreview.color = new Color(1, 1, 1, 0);
        }

        if (textoNombre != null)
            textoNombre.text = "Selecciona un objeto";
        if (textoRareza != null)
            textoRareza.text = "";
        if (textoDescripcion != null)
            textoDescripcion.text = "";
        if (textoPrecio != null)
            textoPrecio.text = "";

        ConfigurarBotones(false, false);
    }

    /// <summary>
    /// Enciende o apaga los botones de comprar y vender según se le pida.
    /// </summary>
    private void ConfigurarBotones(bool puedeComprar, bool puedeVender)
    {
        if (botonComprar != null)
            botonComprar.interactable = puedeComprar;
        if (botonVender != null)
            botonVender.interactable = puedeVender;
    }

    /// <summary>
    /// Comprueba si el jugador tiene oro, se lo cobra, le guarda el objeto y emite el sonido comercial.
    /// </summary>
    private void ComprarItem()
    {
        int dineroActual;
        bool anadido;

        if (itemSeleccionado != null && seleccionDesdeTienda)
        {
            dineroActual = GameManager.Instance.ObtenerDinero();

            if (dineroActual >= itemSeleccionado.precioVenta)
            {
                anadido = InventorySystem.Instance.AnadirObjeto(itemSeleccionado);
                if (anadido)
                {
                    GameManager.Instance.AnadirDinero(-itemSeleccionado.precioVenta);
                    if (SoundManager.Instance != null)
                    {
                        SoundManager.Instance.ReproducirComprar();
                    }
                }
            }
            else
            {
                Debug.LogWarning(
                    "El jugador intentó comprar un objeto sin tener dinero suficiente."
                );
            }
        }
        else
        {
            Debug.LogWarning(
                "Se pulsó comprar pero no hay objeto seleccionado válido de la tienda."
            );
        }
    }

    /// <summary>
    /// Aumenta el dinero del jugador y elimina el objeto de la mochila emitiendo el sonido de venta.
    /// </summary>
    private void VenderItem()
    {
        int gananciaFinal;

        if (itemSeleccionado != null && !seleccionDesdeTienda && indiceInventarioSeleccionado != -1)
        {
            gananciaFinal = Mathf.RoundToInt(
                itemSeleccionado.precioVenta * GameManager.Instance.bufoPrecioVenta
            );
            GameManager.Instance.AnadirDinero(gananciaFinal);
            InventorySystem.Instance.LimpiarCasilla(indiceInventarioSeleccionado);

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.ReproducirVender();
            }

            LimpiarPreview();
        }
        else
        {
            Debug.LogWarning(
                "No se puede vender un objeto que no está bien seleccionado desde la mochila."
            );
        }
    }
}
