using System.Collections.Generic;
using Firebase.Extensions;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Muestra el álbum del jugador con los gatos y peces descubiertos.
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

    /// <summary>
    /// Establece este script como el gestor principal y destruye copias repetidas.
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
    /// Prepara los botones y pide a la base de datos la información de los gatos.
    /// </summary>
    void Start()
    {
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
    /// Escucha el teclado para abrir o cerrar el álbum pulsando teclas específicas.
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
    /// Descarga la lista completa de gatos desde la nube y los guarda en la memoria.
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
    /// Crea las casillas visuales ordenadas por rareza según los datos cargados.
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
    /// Muestra el panel del álbum y bloquea los movimientos del jugador.
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
    /// Oculta el panel del álbum y devuelve el control al jugador.
    /// </summary>
    public void CerrarColeccion()
    {
        coleccionAbierta = false;
        panelColeccion.SetActive(false);
        BloquearControles(false);
    }

    /// <summary>
    /// Activa la sección visual de los peces y apaga la de los gatos.
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
    /// Activa la sección visual de los gatos y apaga la de los peces.
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
    /// Rellena el marco de detalles lateral con la información del pez pulsado.
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
    /// Rellena el marco de detalles lateral con la información del gato pulsado.
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
    /// Mueve la barra visual calculando el nivel en base a la experiencia conseguida.
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
    /// Vacía la información mostrada sobre el pez seleccionado.
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
    /// Vacía la información mostrada sobre el gato seleccionado.
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
    /// Encuentra los datos de un pez comprobando su texto identificador.
    /// </summary>
    private ItemData BuscarPezGlobal(string id)
    {
        foreach (ItemData pez in FishManager.Instance.todosLosPeces)
        {
            if (pez.ID == id)
            {
                return pez;
            }
        }
        return null;
    }

    /// <summary>
    /// Convierte la palabra leída de la base de datos a una categoría válida.
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
    /// Informa a los controles de movimiento y cámara para detener al jugador.
    /// </summary>
    private void BloquearControles(bool estado)
    {
        CamaraMovement cam;
        PlayerMovement mov;

        cam = FindFirstObjectByType<CamaraMovement>();
        mov = FindFirstObjectByType<PlayerMovement>();

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
