using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Gestor global de la interfaz de usuario.
/// Controla el menú de pausa, los diálogos, barras de progresión y evita la superposición de interfaces.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Sistema de Diálogos")]
    public GameObject panelDialogo;
    public TextMeshProUGUI textoDialogo;

    [Header("Progresión Afinidad (En Diálogo)")]
    public GameObject panelBarraAfinidad;
    public Slider barraAfinidad;
    public TextMeshProUGUI textoNivelAfinidad;

    [Header("Textos Directos")]
    public TextMeshProUGUI textoInteraccion;
    public TextMeshProUGUI textoTooltip;

    [Header("Menú de Pausa")]
    public GameObject panelPausa;
    public Button botonMenuPrincipal;
    public Button botonSalir;
    public string nombreEscenaMenu = "MainMenu";

    [HideInInspector]
    public bool pausaAbierta = false;

    private bool interaccionConsumida = false;

    void Awake()
    {
        Instance = this;

        if (panelDialogo)
            panelDialogo.SetActive(false);
        if (panelBarraAfinidad)
            panelBarraAfinidad.SetActive(false);
        if (textoInteraccion)
            textoInteraccion.gameObject.SetActive(false);
        if (textoTooltip)
            textoTooltip.gameObject.SetActive(false);
        if (panelPausa)
            panelPausa.SetActive(false);
    }

    void Start()
    {
        if (botonMenuPrincipal)
            botonMenuPrincipal.onClick.AddListener(IrMenuPrincipal);
        if (botonSalir)
            botonSalir.onClick.AddListener(TogglePausa);
    }

    void Update()
    {
        interaccionConsumida = false;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (!HayAlgunaInterfazSecundariaAbierta())
            {
                TogglePausa();
            }
        }
    }

    /// <summary>
    /// Actúa como semáforo. Si una interacción ocurre, bloquea al resto de elementos en el mismo frame.
    /// </summary>
    public bool ConsumirInteraccion()
    {
        if (interaccionConsumida || HayAlgunaInterfazSecundariaAbierta() || pausaAbierta)
            return false;

        interaccionConsumida = true;
        return true;
    }

    /// <summary>
    /// Rastrea todos los mánagers para saber si hay algún panel ocupando la pantalla.
    /// </summary>
    public bool HayAlgunaInterfazSecundariaAbierta()
    {
        bool mercado = MarketManager.Instance != null && MarketManager.Instance.mercadoAbierto;
        bool taberna = TavernManager.Instance != null && TavernManager.Instance.tabernaAbierta;
        bool iglesia = ChurchManager.Instance != null && ChurchManager.Instance.iglesiaAbierta;
        bool coleccion =
            CollectionManager.Instance != null && CollectionManager.Instance.coleccionAbierta;
        bool mochila = InventorySystem.Instance != null && InventorySystem.Instance.mochilaAbierta;
        bool dialogo = panelDialogo != null && panelDialogo.activeSelf;

        SettingsManager sm = FindFirstObjectByType<SettingsManager>();
        bool settings = sm != null && sm.settingsPanel != null && sm.settingsPanel.activeSelf;

        return mercado || taberna || iglesia || coleccion || mochila || dialogo || settings;
    }

    /// <summary>
    /// Abre o cierra el menú de pausa congelando el tiempo y los controles del jugador.
    /// </summary>
    private void TogglePausa()
    {
        pausaAbierta = !pausaAbierta;
        if (panelPausa != null)
            panelPausa.SetActive(pausaAbierta);

        CamaraMovement cam = FindFirstObjectByType<CamaraMovement>();
        PlayerMovement mov = FindFirstObjectByType<PlayerMovement>();

        if (pausaAbierta)
        {
            Time.timeScale = 0f;
            if (cam != null)
            {
                cam.rotacionBloqueada = true;
                cam.DesbloquearCursor();
            }
            if (mov != null)
                mov.movimientoBloqueado = true;
        }
        else
        {
            Time.timeScale = 1f;
            if (cam != null)
            {
                cam.rotacionBloqueada = false;
                cam.BloquearCursor();
            }
            if (mov != null)
                mov.movimientoBloqueado = false;
        }
    }

    /// <summary>
    /// Restaura el tiempo, cambia la música y carga la escena inicial.
    /// </summary>
    private void IrMenuPrincipal()
    {
        Time.timeScale = 1f;
        if (SoundManager.Instance != null)
            SoundManager.Instance.ReproducirMusicaMenu();
        SceneManager.LoadScene(nombreEscenaMenu);
    }

    public void MostrarBocadillo(string frase)
    {
        if (panelDialogo)
            panelDialogo.SetActive(true);
        if (textoDialogo)
            textoDialogo.text = frase;
    }

    public void OcultarBocadillo()
    {
        if (panelDialogo)
            panelDialogo.SetActive(false);
        if (panelBarraAfinidad)
            panelBarraAfinidad.SetActive(false);
    }

    public void MostrarAfinidadGato(int nivel, int xpActual, int xpNecesaria)
    {
        if (panelBarraAfinidad)
            panelBarraAfinidad.SetActive(true);
        if (textoNivelAfinidad)
            textoNivelAfinidad.text = $"Afinidad {nivel} | XP {xpActual}/{xpNecesaria}";
        if (barraAfinidad)
        {
            barraAfinidad.maxValue = xpNecesaria;
            barraAfinidad.value = xpActual;
        }
    }

    public void MostrarInteraccion(string mensaje)
    {
        if (textoInteraccion)
        {
            textoInteraccion.gameObject.SetActive(true);
            textoInteraccion.text = mensaje;
        }
    }

    public void OcultarInteraccion()
    {
        if (textoInteraccion)
            textoInteraccion.gameObject.SetActive(false);
    }

    public void MostrarTooltip(string mensaje)
    {
        if (textoTooltip)
        {
            CancelInvoke("OcultarTooltip");
            textoTooltip.gameObject.SetActive(true);
            textoTooltip.text = mensaje;
        }
    }

    public void MostrarTooltipTemporal(string mensaje, float tiempo)
    {
        MostrarTooltip(mensaje);
        Invoke("OcultarTooltip", tiempo);
    }

    public void OcultarTooltip()
    {
        if (textoTooltip)
            textoTooltip.gameObject.SetActive(false);
    }

    public void MostrarSubidaNivelGlobal(int nivel)
    {
        MostrarTooltipTemporal($"¡NIVEL AUMENTADO A {nivel}!", 4f);
    }
}
