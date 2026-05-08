using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controla todos los textos, menús y avisos que aparecen en la pantalla del jugador.
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

    /// <summary>
    /// Recaba las responsabilidades estructurales asimilando ser la cabeza global para todos los diálogos visuales emergentes.
    /// </summary>
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

    /// <summary>
    /// Registra en su propia base a los emisores para poder interactuar activamente con su pulso.
    /// </summary>
    void Start()
    {
        if (botonMenuPrincipal)
            botonMenuPrincipal.onClick.AddListener(IrMenuPrincipal);
        if (botonSalir)
            botonSalir.onClick.AddListener(TogglePausa);
    }

    /// <summary>
    /// Permanece alerta garantizando un control puro que vigila cruces entre peticiones del teclado.
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
    /// Filtra sin contemplaciones las pulsaciones sobrepuestas cediendo terreno al primero que pase el corte limpio.
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
    /// Analiza como perro guardián todos los cuadros importantes buscando obstrucciones en el perímetro ocular.
    /// </summary>
    public bool HayAlgunaInterfazSecundariaAbierta()
    {
        bool mercado;
        bool taberna;
        bool iglesia;
        bool coleccion;
        bool mochila;
        bool dialogo;
        SettingsManager sm;
        bool settings;

        mercado = MarketManager.Instance != null && MarketManager.Instance.mercadoAbierto;
        taberna = TavernManager.Instance != null && TavernManager.Instance.tabernaAbierta;
        iglesia = ChurchManager.Instance != null && ChurchManager.Instance.iglesiaAbierta;
        coleccion =
            CollectionManager.Instance != null && CollectionManager.Instance.coleccionAbierta;
        mochila = InventorySystem.Instance != null && InventorySystem.Instance.mochilaAbierta;
        dialogo = panelDialogo != null && panelDialogo.activeSelf;

        sm = FindFirstObjectByType<SettingsManager>();
        settings = sm != null && sm.settingsPanel != null && sm.settingsPanel.activeSelf;

        return mercado || taberna || iglesia || coleccion || mochila || dialogo || settings;
    }

    /// <summary>
    /// Altera el ritmo de los procesos físicos inmovilizando los músculos vitales a costa del panel principal.
    /// </summary>
    private void TogglePausa()
    {
        CamaraMovement cam;
        PlayerMovement mov;

        pausaAbierta = !pausaAbierta;
        if (panelPausa != null)
            panelPausa.SetActive(pausaAbierta);

        cam = FindFirstObjectByType<CamaraMovement>();
        mov = FindFirstObjectByType<PlayerMovement>();

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
    /// Fuerza su transbordo reavivando el tono melancólico para ir directos al salón original.
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
    /// Descubre el globo comunicativo para plasmar la expresión conversacional.
    /// </summary>
    public void MostrarBocadillo(string frase)
    {
        if (panelDialogo != null)
            panelDialogo.SetActive(true);
        if (textoDialogo != null)
            textoDialogo.text = frase;
    }

    /// <summary>
    /// Corta de raíz la muestra escénica despidiéndose de todo vestigio comunicador.
    /// </summary>
    public void OcultarBocadillo()
    {
        if (panelDialogo != null)
            panelDialogo.SetActive(false);
        if (panelBarraAfinidad != null)
            panelBarraAfinidad.SetActive(false);
    }

    /// <summary>
    /// Ajusta las divisiones medidoras y alumbra con fidelidad los dígitos expuestos.
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
    /// Proyecta una petición imperativa frontalmente y sin adornos en base al estímulo provocado.
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
    /// Extirpa la necesidad perentoria de ver la orden descolocada en lo visible.
    /// </summary>
    public void OcultarInteraccion()
    {
        if (textoInteraccion != null)
        {
            textoInteraccion.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Manifiesta en una burbuja de aviso una clarificación auxiliar indispensable que acompaña la señal visual.
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
    /// Apalanca lo mismo que su homólogo primigenio añadiendo una condicional destructiva de tiempo para no pervivir.
    /// </summary>
    public void MostrarTooltipTemporal(string mensaje, float tiempo)
    {
        MostrarTooltip(mensaje);
        Invoke("OcultarTooltip", tiempo);
    }

    /// <summary>
    /// Arranca de sus cimientos lo que fuera proyectado a duras penas en su propio cuadro vital.
    /// </summary>
    public void OcultarTooltip()
    {
        if (textoTooltip != null)
        {
            textoTooltip.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Redirige al invocador para dar notoriedad y peso social a un aumento de la pericia experimentada.
    /// </summary>
    public void MostrarSubidaNivelGlobal(int nivel)
    {
        MostrarTooltipTemporal($"¡NIVEL AUMENTADO A {nivel}!", 4f);
    }
}
