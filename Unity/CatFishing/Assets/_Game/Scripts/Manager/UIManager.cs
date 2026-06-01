using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controla de forma centralizada la interfaz en pantalla, incluyendo diálogos, tooltips y el menú de pausa.
/// [Relaciones: CamaraMovement, PlayerMovement, SettingsManager, MarketManager, TavernManager, ChurchManager, CollectionManager, InventorySystem, SoundManager]
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

    private CamaraMovement cam;
    private PlayerMovement mov;
    private SettingsManager sm;

    /// <summary>
    /// Crea la referencia global de la UI y oculta todos los paneles inicialmente.
    /// </summary>
    void Awake()
    {
        Instance = this;

        cam = FindFirstObjectByType<CamaraMovement>();
        mov = FindFirstObjectByType<PlayerMovement>();
        sm = FindFirstObjectByType<SettingsManager>();

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

    /// <summary>
    /// Asigna funcionalidades a los botones de navegación principales.
    /// </summary>
    void Start()
    {
        if (botonMenuPrincipal)
            botonMenuPrincipal.onClick.AddListener(IrMenuPrincipal);
        if (botonSalir)
            botonSalir.onClick.AddListener(TogglePausa);
    }

    /// <summary>
    /// Reinicia los consumibles de interacción y vigila los comandos de pausa.
    /// </summary>
    void Update()
    {
        interaccionConsumida = false;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (!HayAlgunaInterfazSecundariaAbierta())
            {
                TogglePausa();
            }
        }
    }

    /// <summary>
    /// Verifica y marca si el jugador puede interactuar con un objeto en el frame actual.
    /// </summary>
    public bool ConsumirInteraccion()
    {
        bool resultado;

        if (!interaccionConsumida && !HayAlgunaInterfazSecundariaAbierta() && !pausaAbierta)
        {
            interaccionConsumida = true;
            resultado = true;
        }
        else
        {
            resultado = false;
        }

        return resultado;
    }

    /// <summary>
    /// Comprueba si cualquier menú auxiliar del juego está superpuesto bloqueando la acción.
    /// </summary>
    public bool HayAlgunaInterfazSecundariaAbierta()
    {
        bool mercado;
        bool taberna;
        bool iglesia;
        bool coleccion;
        bool mochila;
        bool dialogo;
        bool settings;

        mercado = MarketManager.Instance != null && MarketManager.Instance.mercadoAbierto;
        taberna = TavernManager.Instance != null && TavernManager.Instance.tabernaAbierta;
        iglesia = ChurchManager.Instance != null && ChurchManager.Instance.iglesiaAbierta;
        coleccion =
            CollectionManager.Instance != null && CollectionManager.Instance.coleccionAbierta;
        mochila = InventorySystem.Instance != null && InventorySystem.Instance.mochilaAbierta;
        dialogo = panelDialogo != null && panelDialogo.activeSelf;
        settings = sm != null && sm.settingsPanel != null && sm.settingsPanel.activeSelf;

        return mercado || taberna || iglesia || coleccion || mochila || dialogo || settings;
    }

    /// <summary>
    /// Alterna la visibilidad del menú de opciones y pausa el transcurso del tiempo del motor.
    /// </summary>
    private void TogglePausa()
    {
        pausaAbierta = !pausaAbierta;
        if (panelPausa != null)
            panelPausa.SetActive(pausaAbierta);

        if (pausaAbierta)
        {
            Time.timeScale = 0f;
            if (cam != null)
            {
                cam.rotacionBloqueada = true;
                cam.DesbloquearCursor();
            }
            if (mov != null)
            {
                mov.movimientoBloqueado = true;
            }
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
            {
                mov.movimientoBloqueado = false;
            }
        }
    }

    /// <summary>
    /// Reanuda el motor y devuelve al jugador a la pantalla de inicio.
    /// </summary>
    private void IrMenuPrincipal()
    {
        Time.timeScale = 1f;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.ReproducirMusicaMenu();
        }

        SceneManager.LoadScene(nombreEscenaMenu);
    }

    /// <summary>
    /// Muestra un panel de texto conversacional en pantalla.
    /// </summary>
    public void MostrarBocadillo(string frase)
    {
        if (panelDialogo != null)
            panelDialogo.SetActive(true);
        if (textoDialogo != null)
            textoDialogo.text = frase;
    }

    /// <summary>
    /// Oculta el panel conversacional y cualquier métrica social asociada.
    /// </summary>
    public void OcultarBocadillo()
    {
        if (panelDialogo != null)
            panelDialogo.SetActive(false);
        if (panelBarraAfinidad != null)
            panelBarraAfinidad.SetActive(false);
    }

    /// <summary>
    /// Rellena y muestra un medidor de amistad durante la charla con un NPC animal.
    /// </summary>
    public void MostrarAfinidadGato(int nivel, int xpActual, int xpNecesaria)
    {
        if (panelBarraAfinidad != null)
            panelBarraAfinidad.SetActive(true);
        if (textoNivelAfinidad != null)
            textoNivelAfinidad.text = $"Afinidad Lvl {nivel} | XP {xpActual}/{xpNecesaria}";
        if (barraAfinidad != null)
        {
            barraAfinidad.maxValue = xpNecesaria;
            barraAfinidad.value = xpActual;
        }
    }

    /// <summary>
    /// Muestra una alerta visual en el centro de la pantalla avisando sobre una acción disponible.
    /// </summary>
    public void MostrarInteraccion(string mensaje)
    {
        if (textoInteraccion != null)
        {
            textoInteraccion.gameObject.SetActive(true);
            textoInteraccion.text = mensaje;
        }
    }

    /// <summary>
    /// Elimina el aviso visual central de acción rápida.
    /// </summary>
    public void OcultarInteraccion()
    {
        if (textoInteraccion != null)
        {
            textoInteraccion.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Habilita una ventana emergente informativa genérica con el mensaje provisto.
    /// </summary>
    public void MostrarTooltip(string mensaje)
    {
        if (textoTooltip != null)
        {
            CancelInvoke("OcultarTooltip");
            textoTooltip.gameObject.SetActive(true);
            textoTooltip.text = mensaje;
        }
    }

    /// <summary>
    /// Muestra una ventana emergente que se desvanece automáticamente tras ciertos segundos.
    /// </summary>
    public void MostrarTooltipTemporal(string mensaje, float tiempo)
    {
        MostrarTooltip(mensaje);
        Invoke("OcultarTooltip", tiempo);
    }

    /// <summary>
    /// Cierra y desactiva forzosamente el recuadro informativo superior.
    /// </summary>
    public void OcultarTooltip()
    {
        if (textoTooltip != null)
        {
            textoTooltip.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Activa un tooltip especial resaltando una progresión estructural general.
    /// </summary>
    public void MostrarSubidaNivelGlobal(int nivel)
    {
        MostrarTooltipTemporal($"¡NIVEL AUMENTADO A {nivel}!", 4f);
    }
}
