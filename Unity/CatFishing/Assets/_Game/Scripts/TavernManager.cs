using System.Collections.Generic;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Controla la tienda de bebidas y aplica las mejoras temporales al jugador.
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

    /// <summary>
    /// Evita que haya duplicados del controlador maestro en la ciudad.
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
    /// Engancha físicamente el código con la gráfica de los pulsadores para reaccionar a clics.
    /// </summary>
    void Start()
    {
        if (panelTaberna != null)
        {
            panelTaberna.SetActive(false);
        }

        if (botonComprar != null)
        {
            botonComprar.onClick.AddListener(ComprarServicio);
        }
        if (botonCerrar != null)
        {
            botonCerrar.onClick.AddListener(CerrarTaberna);
        }

        CargarServicios();
    }

    /// <summary>
    /// Escanea ininterrumpidamente el teclado forzando las salidas directas con el botón Escape.
    /// </summary>
    void Update()
    {
        if (
            tabernaAbierta
            && Keyboard.current != null
            && Keyboard.current.escapeKey.wasPressedThisFrame
        )
        {
            CerrarTaberna();
        }
    }

    /// <summary>
    /// Conecta con Firestore descartando todos los registros que no estén calificados para la zona tabernera.
    /// </summary>
    private async void CargarServicios()
    {
        FirebaseFirestore db;
        QuerySnapshot snapshot;
        Dictionary<string, object> dict;
        ServiceData serv;
        GameObject slotGO;
        ServiceSlot slotScript;

        if (!catalogoCargado)
        {
            db = FirebaseFirestore.DefaultInstance;

            try
            {
                snapshot = await db.Collection("services")
                    .WhereEqualTo("type", "buff")
                    .GetSnapshotAsync();

                foreach (DocumentSnapshot doc in snapshot.Documents)
                {
                    dict = doc.ToDictionary();
                    serv = ScriptableObject.CreateInstance<ServiceData>();
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

                    slotGO = Instantiate(prefabServicioSlot, contenedorServicios);
                    slotGO.transform.localScale = Vector3.one;

                    slotScript = slotGO.GetComponent<ServiceSlot>();
                    if (slotScript != null)
                    {
                        slotScript.ConfigurarSlot(serv);
                    }
                }
                catalogoCargado = true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Error al generar las cajas de bebidas taberneras: " + e.Message);
            }
        }
    }

    /// <summary>
    /// Rompe la inmovilidad de la tienda dejándose presenciar con interrupción temporal.
    /// </summary>
    public void AbrirTaberna()
    {
        tabernaAbierta = true;
        if (panelTaberna != null)
        {
            panelTaberna.SetActive(true);
        }
        BloquearControles(true);
        LimpiarPreview();
    }

    /// <summary>
    /// Recupera los privilegios del avatar guardando la lista en reposo.
    /// </summary>
    public void CerrarTaberna()
    {
        tabernaAbierta = false;
        if (panelTaberna != null)
        {
            panelTaberna.SetActive(false);
        }
        BloquearControles(false);
    }

    /// <summary>
    /// Presenta una ficha técnica para que el jugador sepa de qué trata el preparado.
    /// </summary>
    public void SeleccionarServicio(ServiceData servicio)
    {
        servicioSeleccionado = servicio;

        if (imagenPreview != null && servicio != null)
        {
            imagenPreview.sprite = servicio.icono;
            imagenPreview.color = Color.white;
        }

        if (textoNombre != null)
            textoNombre.text = servicio.nombreDisplay;
        if (textoDescripcion != null)
            textoDescripcion.text = servicio.descripcion;
        if (textoPrecio != null)
            textoPrecio.text = servicio.precioBase.ToString();

        if (botonComprar != null)
        {
            botonComprar.interactable = true;
        }
    }

    /// <summary>
    /// Restablece sin piedad el borrador explicativo vaciándolo a cero para la próxima apertura.
    /// </summary>
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
        {
            botonComprar.interactable = false;
        }
    }

    /// <summary>
    /// Pide al GameManager que audite los recursos y, si cuadra, entrega la modificación exigida.
    /// </summary>
    private void ComprarServicio()
    {
        if (servicioSeleccionado != null)
        {
            if (GameManager.Instance.ObtenerDinero() >= servicioSeleccionado.precioBase)
            {
                GameManager.Instance.AnadirDinero(-servicioSeleccionado.precioBase);
                GameManager.Instance.ActivarServicio(servicioSeleccionado);

                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.ReproducirConsumirTaberna();
                }

                if (UIManager.Instance != null)
                {
                    UIManager.Instance.MostrarTooltipTemporal(
                        $"¡{servicioSeleccionado.nombreDisplay} activado!",
                        3f
                    );
                }
            }
            else
            {
                Debug.LogWarning("Carente de recursos económicos para formalizar su servicio.");
            }
        }
        else
        {
            Debug.LogWarning("Has pedido encargar pero no hay rastro del documento.");
        }
    }

    /// <summary>
    /// Mueve el centro de poder aislando la acción ratonil del jugador central.
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
