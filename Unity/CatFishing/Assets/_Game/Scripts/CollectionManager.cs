using System.Collections.Generic;
using Firebase.Extensions;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Gestiona la visualización del álbum del jugador, separando entidades (Gatos y Peces).
/// Descarga el catálogo completo de gatos, lo ordena por rareza y lo cruza con el progreso guardado.
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

    [Header("Gestión de Visibilidad (Scroll Views completos)")]
    public GameObject scrollViewPeces;
    public GameObject scrollViewGatos;

    [Header("Gestión de Instanciación (Objetos Content)")]
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

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (panelColeccion != null)
            panelColeccion.SetActive(false);
        if (botonCerrar != null)
            botonCerrar.onClick.AddListener(CerrarColeccion);

        CargarCatalogoGatos();
    }

    void Update()
    {
        if (Keyboard.current.cKey.wasPressedThisFrame)
        {
            if (coleccionAbierta)
                CerrarColeccion();
            else
                AbrirColeccion();
        }
        else if (coleccionAbierta && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CerrarColeccion();
        }
    }

    /// <summary>
    /// Descarga la base de datos de gatos desde Firestore para poder construir el álbum completo.
    /// </summary>
    private async void CargarCatalogoGatos()
    {
        if (catalogoGatosCargado)
            return;

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        try
        {
            QuerySnapshot snapshot = await db.Collection("cats").GetSnapshotAsync();
            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                Dictionary<string, object> dict = doc.ToDictionary();
                CatData gato = ScriptableObject.CreateInstance<CatData>();

                gato.ID = doc.Id;
                if (dict.ContainsKey("name"))
                    gato.nombreDisplay = dict["name"].ToString();
                if (dict.ContainsKey("description"))
                    gato.descripcion = dict["description"].ToString();
                if (dict.ContainsKey("fav_fish_id"))
                    gato.pezFavoritoID = dict["fav_fish_id"].ToString();

                string rarezaStr = dict.ContainsKey("rarity")
                    ? dict["rarity"].ToString().ToLower()
                    : "común";
                gato.rareza = DeterminarRareza(rarezaStr);

                gato.icono = Resources.Load<Sprite>("CatIcons/" + gato.ID);
                todosLosGatos.Add(gato);
            }
            catalogoGatosCargado = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error Colección Gatos: " + e.Message);
        }
    }

    /// <summary>
    /// Instancia las casillas gráficas de peces y gatos basándose en los datos descargados
    /// y en el progreso guardado del jugador, ordenándolos previamente por rareza.
    /// </summary>
    private void GenerarUI()
    {
        if (
            uiGenerada
            || !catalogoGatosCargado
            || FishManager.Instance == null
            || !FishManager.Instance.pecesCargados
        )
            return;

        List<ItemData> pecesOrdenados = new List<ItemData>(FishManager.Instance.todosLosPeces);
        pecesOrdenados.Sort((p1, p2) => p1.rareza.CompareTo(p2.rareza));

        foreach (ItemData pez in pecesOrdenados)
        {
            bool desbloqueado = GameManager.Instance.pecesCapturados.ContainsKey(pez.ID);
            GameObject slotGO = Instantiate(prefabCollectionSlot, contenidoPeces);
            slotGO.transform.localScale = Vector3.one;
            slotGO
                .GetComponent<CollectionSlot>()
                .ConfigurarPez(pez, desbloqueado, iconoBloqueadoPez);
        }

        todosLosGatos.Sort((g1, g2) => g1.rareza.CompareTo(g2.rareza));

        foreach (CatData gato in todosLosGatos)
        {
            bool desbloqueado = GameManager.Instance.afinidadGatos.ContainsKey(gato.ID);
            GameObject slotGO = Instantiate(prefabCollectionSlot, contenidoGatos);
            slotGO.transform.localScale = Vector3.one;
            slotGO
                .GetComponent<CollectionSlot>()
                .ConfigurarGato(gato, desbloqueado, iconoBloqueadoGato);
        }

        uiGenerada = true;
    }

    public void AbrirColeccion()
    {
        coleccionAbierta = true;
        panelColeccion.SetActive(true);
        BloquearControles(true);

        if (!uiGenerada)
            GenerarUI();
        MostrarPestañaPeces();
    }

    public void CerrarColeccion()
    {
        coleccionAbierta = false;
        panelColeccion.SetActive(false);
        BloquearControles(false);
    }

    /// <summary>
    /// Cambia la visualización a la categoría de Peces, forzando la ocultación de la sección contraria.
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
    /// Cambia la visualización a la categoría de Gatos, forzando la ocultación de la sección contraria.
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
    /// Rellena el panel lateral con los datos extraídos de un pez desbloqueado.
    /// </summary>
    public void SeleccionarPez(ItemData pez)
    {
        previewPezImagen.sprite = pez.icono;
        previewPezNombre.text = pez.nombreDisplay;
        previewPezRareza.text = pez.rareza.NombreFormateado(); // <- Uso del nuevo método
        previewPezDesc.text = pez.descripcion;
        previewPezEstadistica.text =
            "Veces pescado: " + GameManager.Instance.pecesCapturados[pez.ID];
    }

    /// <summary>
    /// Rellena el panel lateral con los datos extraídos de un gato desbloqueado, incluyendo cálculo de afinidad.
    /// </summary>
    public void SeleccionarGato(CatData gato)
    {
        previewGatoImagen.sprite = gato.icono;
        previewGatoNombre.text = gato.nombreDisplay;
        previewGatoRareza.text = gato.rareza.NombreFormateado(); // <- Uso del nuevo método
        previewGatoDesc.text = gato.descripcion;

        ItemData pezFav = BuscarPezGlobal(gato.pezFavoritoID);
        previewGatoPezFav.text = (pezFav != null ? pezFav.nombreDisplay : "Desconocido");

        int xpTotal = GameManager.Instance.afinidadGatos[gato.ID];
        ActualizarBarraAfinidadVisual(xpTotal);
    }

    /// <summary>
    /// Reutiliza el algoritmo de cálculo de niveles para representar gráficamente el progreso de afinidad.
    /// </summary>
    private void ActualizarBarraAfinidadVisual(int xpTotal)
    {
        int nivelVirtual = 1;
        int xpRestante = xpTotal;
        int xpRequerida = 50;

        while (xpRestante >= xpRequerida && nivelVirtual < 10)
        {
            xpRestante -= xpRequerida;
            nivelVirtual++;
            xpRequerida = Mathf.RoundToInt(xpRequerida * 1.5f);
        }

        if (nivelVirtual >= 10)
            xpRestante = xpRequerida;

        if (textoAfinidadGato)
            textoAfinidadGato.text = $"Lvl {nivelVirtual} | XP {xpRestante}/{xpRequerida}";
        if (barraAfinidadGato)
        {
            barraAfinidadGato.maxValue = xpRequerida;
            barraAfinidadGato.value = xpRestante;
        }
    }

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

    private ItemData BuscarPezGlobal(string id)
    {
        foreach (ItemData pez in FishManager.Instance.todosLosPeces)
            if (pez.ID == id)
                return pez;
        return null;
    }

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

    private void BloquearControles(bool estado)
    {
        CamaraMovement cam = FindFirstObjectByType<CamaraMovement>();
        PlayerMovement mov = FindFirstObjectByType<PlayerMovement>();
        if (cam != null)
        {
            cam.rotacionBloqueada = estado;
            if (estado)
                cam.DesbloquearCursor();
            else
                cam.BloquearCursor();
        }
        if (mov != null)
            mov.movimientoBloqueado = estado;
    }
}
