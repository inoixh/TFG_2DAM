using System.Collections.Generic;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Gestiona la iglesia y asigna mejoras estables que duran todo el día.
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

    /// <summary>
    /// Se nombra a sí mismo mánager global y revisa que solo haya uno.
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
    /// Conecta las funciones a los botones visuales.
    /// </summary>
    void Start()
    {
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
    /// Revisa si el jugador pulsa la tecla para marcharse del edificio.
    /// </summary>
    void Update()
    {
        if (iglesiaAbierta && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CerrarIglesia();
        }
    }

    /// <summary>
    /// Activa la pantalla religiosa y frena al jugador.
    /// </summary>
    public void AbrirIglesia()
    {
        iglesiaAbierta = true;
        panelIglesia.SetActive(true);
        BloquearControles(true);

        ActualizarUIRacha();
    }

    /// <summary>
    /// Oculta el edificio y permite caminar al jugador otra vez.
    /// </summary>
    public void CerrarIglesia()
    {
        iglesiaAbierta = false;
        panelIglesia.SetActive(false);
        BloquearControles(false);
    }

    /// <summary>
    /// Manda la señal de rezar y hace sonar la bendición si no lo habías hecho antes.
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
    /// Sincroniza la cantidad de días rezados que pone en pantalla.
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
    /// Busca mejoras en Firebase según tu nivel de devoción y las activa sin reloj interno.
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

        if (bufosCargados)
        {
            return;
        }

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

    /// <summary>
    /// Controla si el jugador puede usar su cámara y moverse.
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
