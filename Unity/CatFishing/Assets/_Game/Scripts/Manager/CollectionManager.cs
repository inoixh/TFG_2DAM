using System.Collections.Generic;
using Firebase.Extensions;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Controla el catálogo de recolección de gatos conocidos y peces pescados por el jugador.
/// [Interacción BD]
/// [Relaciones: FishManager, GameManager, CatData, ItemData, CollectionSlot, CamaraMovement, PlayerMovement]
/// </summary>
public class CollectionManager : MonoBehaviour
{
    public static CollectionManager Instance;

    [Header("Paneles Principales")]
    public GameObject panelColeccion;
    public Button botonCerrar;
    public GameObject prefabCollectionSlot;
    public Sprite iconoBloqueadoGato;
    public Sprite iconoBloqueadoPez;

    [Header("Gestión de Visibilidad")]
    public GameObject scrollViewPeces;
    public GameObject scrollViewGatos;

    [Header("Gestión de Instanciación")]
    public Transform contenidoPeces;
    public Transform contenidoGatos;

    [Header("Vista Previa Peces")]
    public GameObject panelPreviewPeces;
    public Image previewPezImagen;
    public TextMeshProUGUI previewPezNombre;
    public TextMeshProUGUI previewPezRareza;
    public TextMeshProUGUI previewPezDesc;
    public TextMeshProUGUI previewPezEstadistica;

    [Header("Vista Previa Gatos")]
    public GameObject panelPreviewGatos;
    public Image previewGatoImagen;
    public TextMeshProUGUI previewGatoNombre;
    public TextMeshProUGUI previewGatoRareza;
    public TextMeshProUGUI previewGatoDesc;
    public TextMeshProUGUI previewGatoPezFav;
    public Slider barraAfinidadGato;
    public TextMeshProUGUI textoAfinidadGato;

    public bool coleccionAbierta { get; private set; }

    private bool catalogoGatosCargado = false;
    private bool uiGenerada = false;
    private List<CatData> todosLosGatos = new List<CatData>();
    private CamaraMovement cam;
    private PlayerMovement mov;

    /// <summary>
    /// Configura la instancia Singleton del gestor de colección.
    /// </summary>
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Inicializa la interfaz gráfica e inicia la carga de datos del catálogo.
    /// </summary>
    void Start()
    {
        cam = FindFirstObjectByType<CamaraMovement>();
        mov = FindFirstObjectByType<PlayerMovement>();

        if (panelColeccion != null)
        {
            panelColeccion.SetActive(false);
        }

        if (botonCerrar != null)
        {
            botonCerrar.onClick.AddListener(CerrarColeccion);
        }

        CargarCatalogoGatos();
    }

    /// <summary>
    /// Escucha las entradas del teclado para abrir o cerrar la colección.
    /// </summary>
    void Update()
    {
        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            if (coleccionAbierta)
            {
                CerrarColeccion();
            }
            else
            {
                AbrirColeccion();
            }
        }
        else if (coleccionAbierta && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CerrarColeccion();
        }
    }

    /// <summary>
    /// [BD] Descarga la información de todos los gatos disponibles desde la base de datos.
    /// </summary>
    private async void CargarCatalogoGatos()
    {
        FirebaseFirestore db;
        QuerySnapshot snapshot;
        Dictionary<string, object> dict;
        CatData gato;
        string rarezaStr;

        if (!catalogoGatosCargado)
        {
            db = FirebaseFirestore.DefaultInstance;

            try
            {
                snapshot = await db.Collection("cats").GetSnapshotAsync();

                foreach (DocumentSnapshot doc in snapshot.Documents)
                {
                    dict = doc.ToDictionary();
                    gato = ScriptableObject.CreateInstance<CatData>();
                    gato.ID = doc.Id;

                    if (dict.ContainsKey("name"))
                    {
                        gato.nombreDisplay = dict["name"].ToString();
                    }
                    if (dict.ContainsKey("description"))
                    {
                        gato.descripcion = dict["description"].ToString();
                    }
                    if (dict.ContainsKey("fav_fish_id"))
                    {
                        gato.pezFavoritoID = dict["fav_fish_id"].ToString();
                    }

                    if (dict.ContainsKey("rarity"))
                    {
                        rarezaStr = dict["rarity"].ToString().ToLower();
                    }
                    else
                    {
                        rarezaStr = "común";
                    }

                    gato.rareza = DeterminarRareza(rarezaStr);
                    gato.icono = Resources.Load<Sprite>("CatIcons/" + gato.ID);
                    todosLosGatos.Add(gato);
                }

                catalogoGatosCargado = true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Error al cargar los gatos de la colección: " + e.Message);
            }
        }
    }

    /// <summary>
    /// Construye dinámicamente las listas visuales de peces y gatos desbloqueados.
    /// </summary>
    private void GenerarUI()
    {
        List<ItemData> pecesOrdenados;
        bool desbloqueadoPez;
        GameObject slotGOPez;
        bool desbloqueadoGato;
        GameObject slotGOGato;

        if (
            !uiGenerada
            && catalogoGatosCargado
            && FishManager.Instance != null
            && FishManager.Instance.pecesCargados
        )
        {
            pecesOrdenados = new List<ItemData>(FishManager.Instance.todosLosPeces);
            pecesOrdenados.Sort((p1, p2) => p1.rareza.CompareTo(p2.rareza));

            foreach (ItemData pez in pecesOrdenados)
            {
                desbloqueadoPez = GameManager.Instance.pecesCapturados.ContainsKey(pez.ID);
                slotGOPez = Instantiate(prefabCollectionSlot, contenidoPeces);
                slotGOPez.transform.localScale = Vector3.one;
                slotGOPez
                    .GetComponent<CollectionSlot>()
                    .ConfigurarPez(pez, desbloqueadoPez, iconoBloqueadoPez);
            }

            todosLosGatos.Sort((g1, g2) => g1.rareza.CompareTo(g2.rareza));

            foreach (CatData gato in todosLosGatos)
            {
                desbloqueadoGato = GameManager.Instance.afinidadGatos.ContainsKey(gato.ID);
                slotGOGato = Instantiate(prefabCollectionSlot, contenidoGatos);
                slotGOGato.transform.localScale = Vector3.one;
                slotGOGato
                    .GetComponent<CollectionSlot>()
                    .ConfigurarGato(gato, desbloqueadoGato, iconoBloqueadoGato);
            }

            uiGenerada = true;
        }
        else
        {
            Debug.LogWarning("No se puede generar la interfaz porque faltan datos por cargar.");
        }
    }

    /// <summary>
    /// Despliega el menú de la colección y detiene las acciones del jugador.
    /// </summary>
    public void AbrirColeccion()
    {
        coleccionAbierta = true;
        panelColeccion.SetActive(true);
        BloquearControles(true);

        if (!uiGenerada)
        {
            GenerarUI();
        }

        MostrarPestañaPeces();
    }

    /// <summary>
    /// Oculta el menú de colección y restaura las capacidades de movimiento.
    /// </summary>
    public void CerrarColeccion()
    {
        coleccionAbierta = false;
        panelColeccion.SetActive(false);
        BloquearControles(false);
    }

    /// <summary>
    /// Activa la vista del catálogo de peces y desactiva la de gatos.
    /// </summary>
    public void MostrarPestañaPeces()
    {
        if (scrollViewGatos)
            scrollViewGatos.SetActive(false);
        if (panelPreviewGatos)
            panelPreviewGatos.SetActive(false);
        if (scrollViewPeces)
            scrollViewPeces.SetActive(true);
        if (panelPreviewPeces)
            panelPreviewPeces.SetActive(true);
        LimpiarPreviewPeces();
    }

    /// <summary>
    /// Activa la vista del catálogo de gatos y desactiva la de peces.
    /// </summary>
    public void MostrarPestañaGatos()
    {
        if (scrollViewPeces)
            scrollViewPeces.SetActive(false);
        if (panelPreviewPeces)
            panelPreviewPeces.SetActive(false);
        if (scrollViewGatos)
            scrollViewGatos.SetActive(true);
        if (panelPreviewGatos)
            panelPreviewGatos.SetActive(true);
        LimpiarPreviewGatos();
    }

    /// <summary>
    /// Muestra los detalles de un pez específico en el panel de vista previa.
    /// </summary>
    public void SeleccionarPez(ItemData pez)
    {
        previewPezImagen.sprite = pez.icono;
        previewPezNombre.text = pez.nombreDisplay;
        previewPezRareza.text = pez.rareza.NombreFormateado();
        previewPezDesc.text = pez.descripcion;
        previewPezEstadistica.text =
            "Veces pescado: " + GameManager.Instance.pecesCapturados[pez.ID];
    }

    /// <summary>
    /// Muestra los detalles y nivel de afinidad de un gato en la vista previa.
    /// </summary>
    public void SeleccionarGato(CatData gato)
    {
        ItemData pezFav;
        int xpTotal;

        previewGatoImagen.sprite = gato.icono;
        previewGatoNombre.text = gato.nombreDisplay;
        previewGatoRareza.text = gato.rareza.NombreFormateado();
        previewGatoDesc.text = gato.descripcion;

        pezFav = BuscarPezGlobal(gato.pezFavoritoID);

        if (pezFav != null)
        {
            previewGatoPezFav.text = pezFav.nombreDisplay;
        }
        else
        {
            previewGatoPezFav.text = "Desconocido";
        }

        xpTotal = GameManager.Instance.afinidadGatos[gato.ID];
        ActualizarBarraAfinidadVisual(xpTotal);
    }

    /// <summary>
    /// Calcula el nivel virtual y el progreso de experiencia del gato para la interfaz.
    /// </summary>
    private void ActualizarBarraAfinidadVisual(int xpTotal)
    {
        int nivelVirtual;
        int xpRestante;
        int xpRequerida;

        nivelVirtual = 1;
        xpRestante = xpTotal;
        xpRequerida = 50;

        while (xpRestante >= xpRequerida && nivelVirtual < 10)
        {
            xpRestante -= xpRequerida;
            nivelVirtual++;
            xpRequerida = Mathf.RoundToInt(xpRequerida * 1.5f);
        }

        if (nivelVirtual >= 10)
        {
            xpRestante = xpRequerida;
        }

        if (textoAfinidadGato)
        {
            textoAfinidadGato.text = $"Lvl {nivelVirtual} | XP {xpRestante}/{xpRequerida}";
        }

        if (barraAfinidadGato)
        {
            barraAfinidadGato.maxValue = xpRequerida;
            barraAfinidadGato.value = xpRestante;
        }
    }

    /// <summary>
    /// Restablece el panel de vista previa de peces a su estado predeterminado.
    /// </summary>
    private void LimpiarPreviewPeces()
    {
        if (previewPezImagen)
            previewPezImagen.sprite = iconoBloqueadoPez;
        if (previewPezNombre)
            previewPezNombre.text = "Selecciona un pez";
        if (previewPezRareza)
            previewPezRareza.text = "";
        if (previewPezDesc)
            previewPezDesc.text = "";
        if (previewPezEstadistica)
            previewPezEstadistica.text = "";
    }

    /// <summary>
    /// Restablece el panel de vista previa de gatos a su estado inicial.
    /// </summary>
    private void LimpiarPreviewGatos()
    {
        if (previewGatoImagen)
            previewGatoImagen.sprite = iconoBloqueadoGato;
        if (previewGatoNombre)
            previewGatoNombre.text = "Selecciona un gato";
        if (previewGatoRareza)
            previewGatoRareza.text = "";
        if (previewGatoDesc)
            previewGatoDesc.text = "";
        if (previewGatoPezFav)
            previewGatoPezFav.text = "";
        if (textoAfinidadGato)
            textoAfinidadGato.text = "Lvl ? | XP 0/0";
        if (barraAfinidadGato)
            barraAfinidadGato.value = 0;
    }

    /// <summary>
    /// Recupera los datos de un pez mediante su identificador.
    /// </summary>
    private ItemData BuscarPezGlobal(string id)
    {
        ItemData pezEncontrado = null;

        foreach (ItemData pez in FishManager.Instance.todosLosPeces)
        {
            if (pezEncontrado == null && pez.ID == id)
            {
                pezEncontrado = pez;
            }
        }
        
        return pezEncontrado;
    }

    /// <summary>
    /// Convierte una cadena de texto en el enumerador de rareza correspondiente.
    /// </summary>
    private Rareza DeterminarRareza(string rarezaStr)
    {
        Rareza rarezaFinal = Rareza.Comun;

        switch (rarezaStr)
        {
            case "común":
            case "comun":
                rarezaFinal = Rareza.Comun;
                break;
            case "raro":
                rarezaFinal = Rareza.Raro;
                break;
            case "especial":
                rarezaFinal = Rareza.Especial;
                break;
            case "épico":
            case "epico":
                rarezaFinal = Rareza.Epico;
                break;
            case "legendario":
                rarezaFinal = Rareza.Legendario;
                break;
            default:
                rarezaFinal = Rareza.Comun;
                break;
        }

        return rarezaFinal;
    }

    /// <summary>
    /// Suspende o reanuda la interacción del jugador con el entorno.
    /// </summary>
    private void BloquearControles(bool estado)
    {
        if (cam != null)
        {
            cam.rotacionBloqueada = estado;
            if (estado)
            {
                cam.DesbloquearCursor();
            }
            else
            {
                cam.BloquearCursor();
            }
        }

        if (mov != null)
        {
            mov.movimientoBloqueado = estado;
        }
    }
}
