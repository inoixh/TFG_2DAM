using System.Collections.Generic;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Administra la compra de bufos y servicios en la taberna.
/// [Interacción BD]
/// [Relaciones: ServiceData, ServiceSlot, GameManager, SoundManager, UIManager, CamaraMovement, PlayerMovement]
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

    private CamaraMovement cam;
    private PlayerMovement mov;

    /// <summary>
    /// Establece la referencia Singleton global.
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
    /// Asigna las referencias externas e inicia la carga asíncrona de datos desde la nube.
    /// </summary>
    void Start()
    {
        cam = FindFirstObjectByType<CamaraMovement>();
        mov = FindFirstObjectByType<PlayerMovement>();

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
    /// Supervisa las entradas de teclado para cerrar el menú activo.
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
    /// [BD] Descarga los servicios tipo bufo desde Firestore y rellena la lista de ofertas.
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
                Debug.LogWarning("Error al generar los servicios: " + e.Message);
            }
        }
    }

    /// <summary>
    /// Muestra la interfaz gráfica de la tienda y restringe las acciones del jugador.
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
    /// Oculta el panel principal y devuelve el control completo al jugador.
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
    /// Carga y visualiza los datos de un bufo específico en la sección de detalles.
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
    /// Restablece el panel de información a su estado por defecto.
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
    /// Verifica los fondos del jugador y procesa la compra del servicio seleccionado.
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
                Debug.LogWarning("No tienes suficiente dinero.");
            }
        }
        else
        {
            Debug.LogWarning("No existe el servicio.");
        }
    }

    /// <summary>
    /// Cambia el estado interactivo del movimiento y la vista del jugador.
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
