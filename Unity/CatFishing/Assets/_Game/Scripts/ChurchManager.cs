using System.Collections.Generic;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Gestiona la interfaz de la Iglesia, permite el rezo manual diario
/// y aplica bendiciones pasivas (bufos) basadas en la constancia del jugador.
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

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (panelIglesia != null)
            panelIglesia.SetActive(false);
        if (botonCerrar != null)
            botonCerrar.onClick.AddListener(CerrarIglesia);
        if (botonRezar != null)
            botonRezar.onClick.AddListener(Rezar);
    }

    void Update()
    {
        if (iglesiaAbierta && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CerrarIglesia();
        }
    }

    public void AbrirIglesia()
    {
        iglesiaAbierta = true;
        panelIglesia.SetActive(true);
        BloquearControles(true);

        ActualizarUIRacha();
    }

    public void CerrarIglesia()
    {
        iglesiaAbierta = false;
        panelIglesia.SetActive(false);
        BloquearControles(false);
    }

    /// <summary>
    /// Invoca el método de rezo del GameManager si el jugador no lo ha hecho hoy, actualizando UI y Bufos.
    /// </summary>
    private void Rezar()
    {
        if (GameManager.Instance != null && !GameManager.Instance.haRezadoHoy)
        {
            GameManager.Instance.RealizarRezoDiario();
            ActualizarUIRacha();

            bufosCargados = false;
            CargarBendicionesFirebase(GameManager.Instance.rachaIglesia);
        }
    }

    /// <summary>
    /// Sincroniza visualmente los textos y la interactividad del botón de rezo.
    /// </summary>
    private void ActualizarUIRacha()
    {
        int rachaActual = GameManager.Instance != null ? GameManager.Instance.rachaIglesia : 0;
        if (textoRacha != null)
            textoRacha.text = $"Racha de fe: {rachaActual} días";

        if (GameManager.Instance != null && botonRezar != null)
        {
            botonRezar.interactable = !GameManager.Instance.haRezadoHoy;
        }

        CargarBendicionesFirebase(rachaActual);
    }

    /// <summary>
    /// Consulta Firestore buscando servicios de tipo 'streak' y activa tantos como días de racha tenga el jugador.
    /// </summary>
    private async void CargarBendicionesFirebase(int racha)
    {
        if (bufosCargados)
            return;
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        try
        {
            QuerySnapshot snapshot = await db.Collection("services")
                .WhereEqualTo("type", "streak")
                .GetSnapshotAsync();

            foreach (Transform child in contenedorBendiciones)
                Destroy(child.gameObject);

            int bufosActivados = 0;

            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                if (bufosActivados >= racha)
                    break;

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

                serv.icono = Resources.Load<Sprite>("ServiceIcons/" + serv.ID);

                if (GameManager.Instance != null)
                    GameManager.Instance.ActivarServicio(serv);

                GameObject slotGO = Instantiate(prefabChurchSlot, contenedorBendiciones);
                slotGO.transform.localScale = Vector3.one;

                ChurchSlot slotScript = slotGO.GetComponent<ChurchSlot>();
                if (slotScript != null)
                {
                    slotScript.ConfigurarSlot(serv);
                }

                bufosActivados++;
            }
            bufosCargados = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error Iglesia: " + e.Message);
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
