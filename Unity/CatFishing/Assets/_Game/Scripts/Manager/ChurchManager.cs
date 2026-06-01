using System.Collections.Generic;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Gestiona la lógica de la iglesia, controlando las rachas de rezos y las recompensas diarias.
/// [Interacción BD]
/// [Relaciones: GameManager, SoundManager, ServiceData, ChurchSlot, CamaraMovement, PlayerMovement]
/// </summary>
public class ChurchManager : MonoBehaviour
{
    public static ChurchManager Instance;

    [Header("Interfaz Visual")]
    public GameObject panelIglesia;
    public TextMeshProUGUI textoRacha;
    public Transform contenedorBendiciones;
    public GameObject prefabChurchSlot;
    public Button botonCerrar;
    public Button botonRezar;

    public bool iglesiaAbierta { get; private set; }
    private bool bufosCargados = false;
    private CamaraMovement cam;
    private PlayerMovement mov;

    /// <summary>
    /// Inicializa la instancia del Singleton para el gestor de la iglesia.
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
    /// Asigna referencias a componentes y configura los eventos de los botones.
    /// </summary>
    void Start()
    {
        cam = FindFirstObjectByType<CamaraMovement>();
        mov = FindFirstObjectByType<PlayerMovement>();

        if (panelIglesia != null)
        {
            panelIglesia.SetActive(false);
        }
        if (botonCerrar != null)
        {
            botonCerrar.onClick.AddListener(CerrarIglesia);
        }
        if (botonRezar != null)
        {
            botonRezar.onClick.AddListener(Rezar);
        }
    }

    /// <summary>
    /// Comprueba si se presiona la tecla de escape para cerrar el panel.
    /// </summary>
    void Update()
    {
        if (iglesiaAbierta && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CerrarIglesia();
        }
    }

    /// <summary>
    /// Muestra el panel de la iglesia y bloquea los controles del jugador.
    /// </summary>
    public void AbrirIglesia()
    {
        iglesiaAbierta = true;
        panelIglesia.SetActive(true);
        BloquearControles(true);

        ActualizarUIRacha();
    }

    /// <summary>
    /// Oculta el panel de la iglesia y desbloquea los controles del jugador.
    /// </summary>
    public void CerrarIglesia()
    {
        iglesiaAbierta = false;
        panelIglesia.SetActive(false);
        BloquearControles(false);
    }

    /// <summary>
    /// Realiza el rezo diario y recarga los beneficios asociados a la racha actual.
    /// </summary>
    private void Rezar()
    {
        if (GameManager.Instance != null && !GameManager.Instance.haRezadoHoy)
        {
            GameManager.Instance.RealizarRezoDiario();
            ActualizarUIRacha();

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.ReproducirRezarIglesia();
            }

            bufosCargados = false;
            CargarBendicionesFirebase(GameManager.Instance.rachaIglesia);
        }
        else
        {
            Debug.LogWarning("Intento de rezo duplicado o fallido.");
        }
    }

    /// <summary>
    /// Actualiza los textos de la interfaz con los días de racha actuales.
    /// </summary>
    private void ActualizarUIRacha()
    {
        int rachaActual;
        rachaActual = GameManager.Instance != null ? GameManager.Instance.rachaIglesia : 0;

        if (textoRacha != null)
        {
            textoRacha.text = $"Días: {rachaActual}";
        }

        if (GameManager.Instance != null && botonRezar != null)
        {
            botonRezar.interactable = !GameManager.Instance.haRezadoHoy;
        }

        CargarBendicionesFirebase(rachaActual);
    }

    /// <summary>
    /// [BD] Descarga los servicios tipo racha desde Firestore e instancia sus representaciones visuales.
    /// </summary>
    private async void CargarBendicionesFirebase(int racha)
    {
        FirebaseFirestore db;
        QuerySnapshot snapshot;
        List<DocumentSnapshot> documentos;
        DocumentSnapshot doc;
        Dictionary<string, object> dict;
        ServiceData serv;
        GameObject slotGO;
        ChurchSlot slotScript;

        if (!bufosCargados)
        {
            db = FirebaseFirestore.DefaultInstance;

            try
            {
                snapshot = await db.Collection("services")
                    .WhereEqualTo("type", "streak")
                    .GetSnapshotAsync();

                foreach (Transform child in contenedorBendiciones)
                {
                    Destroy(child.gameObject);
                }

                documentos = new List<DocumentSnapshot>(snapshot.Documents);

                for (int i = 0; i < documentos.Count && i < racha; i++)
                {
                    doc = documentos[i];
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

                    serv.icono = Resources.Load<Sprite>("ServiceIcons/" + serv.ID);

                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.ActivarServicio(serv, false);
                    }

                    slotGO = Instantiate(prefabChurchSlot, contenedorBendiciones);
                    slotGO.transform.localScale = Vector3.one;

                    slotScript = slotGO.GetComponent<ChurchSlot>();
                    if (slotScript != null)
                    {
                        slotScript.ConfigurarSlot(serv);
                    }
                }

                bufosCargados = true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Error al consultar la Iglesia: " + e.Message);
            }
        }
    }

    /// <summary>
    /// Habilita o deshabilita los sistemas de movimiento y cámara del jugador.
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
