using System.Collections.Generic;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Gestiona la compra de consumibles en la taberna y la aplicación de sus bufos al GameManager.
/// </summary>
public class TavernManager : MonoBehaviour
{
    public static TavernManager Instance;

    [Header("Paneles Principales")]
    public GameObject panelTaberna;
    public Transform contenedorServicios;
    public GameObject prefabServicioSlot;

    [Header("Panel de Previsualización")]
    public Image imagenPreview;
    public TextMeshProUGUI textoNombre;
    public TextMeshProUGUI textoDescripcion;
    public TextMeshProUGUI textoPrecio;

    [Header("Botones de Acción")]
    public Button botonComprar;
    public Button botonCerrar;

    public bool tabernaAbierta { get; private set; }
    private ServiceData servicioSeleccionado;
    private bool catalogoCargado = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (panelTaberna != null)
            panelTaberna.SetActive(false);
        if (botonComprar != null)
            botonComprar.onClick.AddListener(ComprarServicio);
        if (botonCerrar != null)
            botonCerrar.onClick.AddListener(CerrarTaberna);
        CargarServicios();
    }

    void Update()
    {
        if (tabernaAbierta && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CerrarTaberna();
        }
    }

    /// <summary>
    /// Se conecta a Firestore, filtra los servicios de tipo 'buff' y los instancia en la UI.
    /// </summary>
    private async void CargarServicios()
    {
        if (catalogoCargado)
            return;
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        try
        {
            QuerySnapshot snapshot = await db.Collection("services")
                .WhereEqualTo("type", "buff")
                .GetSnapshotAsync();

            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                Dictionary<string, object> dict = doc.ToDictionary();
                ServiceData serv = ScriptableObject.CreateInstance<ServiceData>();

                serv.ID = doc.Id;
                if (dict.ContainsKey("name"))
                    serv.nombreDisplay = dict["name"].ToString();
                if (dict.ContainsKey("description"))
                    serv.descripcion = dict["description"].ToString();
                if (dict.ContainsKey("effect_target"))
                    serv.objetivoEfecto = dict["effect_target"].ToString();
                if (dict.ContainsKey("effect_value"))
                    serv.valorEfecto = System.Convert.ToSingle(dict["effect_value"]);
                if (dict.ContainsKey("price"))
                    serv.precioBase = System.Convert.ToInt32(dict["price"]);

                serv.icono = Resources.Load<Sprite>("ServiceIcons/" + serv.ID);

                GameObject slotGO = Instantiate(prefabServicioSlot, contenedorServicios);
                slotGO.transform.localScale = Vector3.one;

                ServiceSlot slotScript = slotGO.GetComponent<ServiceSlot>();
                if (slotScript != null)
                {
                    slotScript.ConfigurarSlot(serv);
                }
            }
            catalogoCargado = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error Taberna: " + e.Message);
        }
    }

    public void AbrirTaberna()
    {
        tabernaAbierta = true;
        panelTaberna.SetActive(true);
        BloquearControles(true);
        LimpiarPreview();
    }

    public void CerrarTaberna()
    {
        tabernaAbierta = false;
        panelTaberna.SetActive(false);
        BloquearControles(false);
    }

    public void SeleccionarServicio(ServiceData servicio)
    {
        servicioSeleccionado = servicio;
        imagenPreview.sprite = servicio.icono;
        imagenPreview.color = Color.white;
        textoNombre.text = servicio.nombreDisplay;
        textoDescripcion.text = servicio.descripcion;
        textoPrecio.text = servicio.precioBase.ToString();
        if (botonComprar != null)
            botonComprar.interactable = true;
    }

    private void LimpiarPreview()
    {
        servicioSeleccionado = null;
        if (imagenPreview != null)
        {
            imagenPreview.sprite = null;
            imagenPreview.color = new Color(1, 1, 1, 0);
        }
        if (textoNombre != null)
            textoNombre.text = "Selecciona un consumible";
        if (textoDescripcion != null)
            textoDescripcion.text = "";
        if (textoPrecio != null)
            textoPrecio.text = "";
        if (botonComprar != null)
            botonComprar.interactable = false;
    }

    private void ComprarServicio()
    {
        if (
            servicioSeleccionado != null
            && GameManager.Instance.ObtenerDinero() >= servicioSeleccionado.precioBase
        )
        {
            GameManager.Instance.AnadirDinero(-servicioSeleccionado.precioBase);
            GameManager.Instance.ActivarServicio(servicioSeleccionado);

            if (UIManager.Instance != null)
                UIManager.Instance.MostrarTooltipTemporal(
                    $"¡{servicioSeleccionado.nombreDisplay} activado!",
                    3f
                );
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
