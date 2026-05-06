using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Gestiona la mecánica de pesca, la dificultad dinámica, las animaciones y la comunicación
/// en tiempo real con la interfaz de usuario para indicar las acciones disponibles.
/// </summary>
public class PescaController : MonoBehaviour
{
    [Header("Referencias")]
    public Animator animadorCana;

    [HideInInspector]
    public PlayerMovement movimientoJugador;

    [Header("Interfaz Minijuego")]
    public GameObject panelMinijuego;
    public RectTransform barraVerde;
    public RectTransform iconoPez;
    public Sprite iconoIncognita;
    public Slider barraProgreso;
    public Image imagenRelleno;
    public float alturaContenedor = 300f;

    [Header("Configuración Minijuego")]
    public float velocidadSubidaBarra = 2f;
    public float gravedadBarra = 1.5f;
    public float velocidadProgreso = 0.3f;

    [Range(0f, 100f)]
    public float probabilidadPezMision = 35f;

    [HideInInspector]
    public bool enZonaDePesca = false;

    private bool puedeLanzar = true;
    private bool esperandoPez = false;
    private bool enMinijuego = false;
    private float posBarra = 0.5f;
    private float posPez = 0.5f;
    private float progreso = 0f;
    private float tiempoPez = 0f;

    private float velocidadPezActual = 0.5f;
    private float tamanoBarraActual = 0.3f;

    private ItemData pezActualEnJuego;
    private Coroutine cronometroEspera;
    private CamaraMovement controlCamara;

    void Start()
    {
        movimientoJugador = GetComponentInParent<PlayerMovement>();
        controlCamara = FindFirstObjectByType<CamaraMovement>();

        if (panelMinijuego != null)
            panelMinijuego.SetActive(false);
        if (barraProgreso != null)
            barraProgreso.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Mouse.current == null)
            return;

        if (InventorySystem.Instance != null && !InventorySystem.Instance.TieneCanaEnMano())
        {
            if (UIManager.Instance != null)
                UIManager.Instance.OcultarInteraccion();
            return;
        }

        if (enMinijuego)
        {
            ControlarMinijuego();
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (puedeLanzar && enZonaDePesca)
                EmpezarLanzamiento();
            else if (esperandoPez)
                CancelarPesca();
        }
    }

    /// <summary>
    /// Recibe la señal física del agua y actualiza el estado visual del jugador.
    /// </summary>
    public void EstablecerZonaDePesca(bool estado)
    {
        enZonaDePesca = estado;
        ActualizarTextosUI();
    }

    /// <summary>
    /// Evalúa el estado actual de la caña para mostrar u ocultar los avisos de interacción.
    /// </summary>
    private void ActualizarTextosUI()
    {
        if (UIManager.Instance == null)
            return;

        if (enMinijuego)
        {
            UIManager.Instance.OcultarInteraccion();
        }
        else if (esperandoPez)
        {
            UIManager.Instance.MostrarInteraccion("Pulsa [CLICK IZQ] para dejar de pescar");
        }
        else if (puedeLanzar && enZonaDePesca)
        {
            UIManager.Instance.MostrarInteraccion("Pulsa [CLICK IZQ] para pescar");
        }
        else
        {
            UIManager.Instance.OcultarInteraccion();
        }
    }

    /// <summary>
    /// Prepara el lanzamiento de la caña, bloquea controles y reproduce sonido.
    /// </summary>
    private void EmpezarLanzamiento()
    {
        if (FishManager.Instance == null || !FishManager.Instance.pecesCargados)
            return;

        puedeLanzar = false;
        if (movimientoJugador != null)
            movimientoJugador.movimientoBloqueado = true;
        if (controlCamara != null)
            controlCamara.rotacionBloqueada = true;

        if (animadorCana != null)
            animadorCana.SetTrigger("Lanzar");
        if (SoundManager.Instance != null)
            SoundManager.Instance.SFX_Lanzar();

        esperandoPez = true;
        ActualizarTextosUI();

        cronometroEspera = StartCoroutine(RutinaEsperarPez());
    }

    /// <summary>
    /// Gestiona el tiempo aleatorio que tarda un pez en picar el anzuelo.
    /// </summary>
    private IEnumerator RutinaEsperarPez()
    {
        yield return new WaitForSeconds(Random.Range(2f, 5f));
        EmpezarMinijuego();
    }

    /// <summary>
    /// Activa el panel de minijuego, selecciona el pez e inicia el sonido del carrete.
    /// </summary>
    private void EmpezarMinijuego()
    {
        esperandoPez = false;
        enMinijuego = true;
        ActualizarTextosUI();

        if (animadorCana != null)
            animadorCana.SetTrigger("Picar");
        if (SoundManager.Instance != null)
            SoundManager.Instance.SFX_EmpezarForcejeo();

        panelMinijuego.SetActive(true);
        barraProgreso.gameObject.SetActive(true);

        pezActualEnJuego = ElegirPezDefinitivo();
        ConfigurarDificultadDinamica(pezActualEnJuego);

        if (iconoPez.GetComponent<Image>() != null && iconoIncognita != null)
        {
            iconoPez.GetComponent<Image>().sprite = iconoIncognita;
        }

        posBarra = 0.1f;
        posPez = 0.5f;
        progreso = 0.15f;
    }

    /// <summary>
    /// Determina el pez a pescar, dando prioridad a las misiones activas de los NPCs.
    /// </summary>
    private ItemData ElegirPezDefinitivo()
    {
        if (Random.Range(0f, 100f) <= probabilidadPezMision)
        {
            ItemData pezMision = BuscarPezRequeridoPorGatos();
            if (pezMision != null)
                return pezMision;
        }
        return ElegirPezPorCaña();
    }

    /// <summary>
    /// Escanea la isla en busca de gatos y extrae aleatoriamente el ID de un pez deseado.
    /// </summary>
    private ItemData BuscarPezRequeridoPorGatos()
    {
        GatoNPC[] gatosEnIsla = FindObjectsByType<GatoNPC>(FindObjectsSortMode.None);
        List<string> idsBuscados = new List<string>();

        foreach (GatoNPC gato in gatosEnIsla)
        {
            if (!string.IsNullOrEmpty(gato.idPezDeseado))
                idsBuscados.Add(gato.idPezDeseado);
        }

        if (idsBuscados.Count == 0)
            return null;

        string idElegido = idsBuscados[Random.Range(0, idsBuscados.Count)];
        foreach (ItemData pez in FishManager.Instance.todosLosPeces)
        {
            if (pez.ID == idElegido)
                return pez;
        }
        return null;
    }

    /// <summary>
    /// Ajusta las probabilidades de aparición de peces basándose en la rareza de la caña equipada.
    /// </summary>
    private ItemData ElegirPezPorCaña()
    {
        ItemData canaActual = InventorySystem.Instance.ObtenerItemEnMano();
        Rareza rarezaCana = canaActual != null ? canaActual.rareza : Rareza.Comun;

        float dado = Random.Range(0f, 100f);
        Rareza rarezaBuscada = Rareza.Comun;

        switch (rarezaCana)
        {
            case Rareza.Comun:
                if (dado > 99)
                    rarezaBuscada = Rareza.Legendario;
                else if (dado > 95)
                    rarezaBuscada = Rareza.Epico;
                else if (dado > 85)
                    rarezaBuscada = Rareza.Especial;
                else if (dado > 60)
                    rarezaBuscada = Rareza.Raro;
                break;
            case Rareza.Raro:
                if (dado > 96)
                    rarezaBuscada = Rareza.Legendario;
                else if (dado > 85)
                    rarezaBuscada = Rareza.Epico;
                else if (dado > 65)
                    rarezaBuscada = Rareza.Especial;
                else if (dado > 40)
                    rarezaBuscada = Rareza.Raro;
                break;
            case Rareza.Epico:
                if (dado > 90)
                    rarezaBuscada = Rareza.Legendario;
                else if (dado > 70)
                    rarezaBuscada = Rareza.Epico;
                else if (dado > 45)
                    rarezaBuscada = Rareza.Especial;
                else if (dado > 20)
                    rarezaBuscada = Rareza.Raro;
                break;
            case Rareza.Legendario:
                if (dado > 75)
                    rarezaBuscada = Rareza.Legendario;
                else if (dado > 50)
                    rarezaBuscada = Rareza.Epico;
                else if (dado > 25)
                    rarezaBuscada = Rareza.Especial;
                else if (dado > 10)
                    rarezaBuscada = Rareza.Raro;
                break;
        }

        List<ItemData> candidatos = new List<ItemData>();
        foreach (var pez in FishManager.Instance.todosLosPeces)
        {
            if (pez.rareza == rarezaBuscada)
                candidatos.Add(pez);
        }

        if (candidatos.Count > 0)
            return candidatos[Random.Range(0, candidatos.Count)];
        return FishManager.Instance.todosLosPeces[
            Random.Range(0, FishManager.Instance.todosLosPeces.Count)
        ];
    }

    /// <summary>
    /// Calcula la dificultad evaluando la diferencia de nivel y aplicando el bufo de la Iglesia.
    /// </summary>
    private void ConfigurarDificultadDinamica(ItemData pez)
    {
        if (pez == null)
            return;

        ItemData canaActual = InventorySystem.Instance.ObtenerItemEnMano();
        int nivelCana = canaActual != null ? (int)canaActual.rareza : 0;
        int nivelPez = (int)pez.rareza;

        int diferencia = nivelPez - nivelCana;

        velocidadPezActual = 0.5f + (diferencia * 0.3f);
        tamanoBarraActual = 0.3f - (diferencia * 0.05f);

        if (GameManager.Instance != null)
            velocidadPezActual += GameManager.Instance.bufoReduccionDificultad;

        velocidadPezActual = Mathf.Clamp(velocidadPezActual, 0.2f, 2.5f);
        tamanoBarraActual = Mathf.Clamp(tamanoBarraActual, 0.08f, 0.5f);

        barraVerde.sizeDelta = new Vector2(
            barraVerde.sizeDelta.x,
            tamanoBarraActual * alturaContenedor
        );
    }

    /// <summary>
    /// Gestiona la lógica de movimiento de las barras y la progresión de victoria.
    /// </summary>
    private void ControlarMinijuego()
    {
        tiempoPez += Time.deltaTime * velocidadPezActual;
        posPez = Mathf.PerlinNoise(tiempoPez, 0f);

        if (Mouse.current.leftButton.isPressed)
            posBarra += velocidadSubidaBarra * Time.deltaTime;
        else
            posBarra -= gravedadBarra * Time.deltaTime;

        posBarra = Mathf.Clamp(posBarra, 0f, 1f);
        barraVerde.anchoredPosition = new Vector2(0, posBarra * alturaContenedor);
        iconoPez.anchoredPosition = new Vector2(0, posPez * alturaContenedor);

        float diferencia = Mathf.Abs(posBarra - posPez);
        bool dentroDeZona = diferencia < (tamanoBarraActual / 2f);
        iconoPez.GetComponent<Image>().color = dentroDeZona ? Color.green : Color.white;

        if (dentroDeZona)
            progreso += velocidadProgreso * Time.deltaTime;
        else
            progreso -= (velocidadProgreso * 0.2f) * Time.deltaTime;

        progreso = Mathf.Clamp(progreso, 0f, 1f);
        barraProgreso.value = progreso;

        ActualizarColorBarra();

        if (progreso >= 1f)
            GanarPesca();
        else if (progreso <= 0f)
            PerderPesca();
    }

    /// <summary>
    /// Interpola colores para reflejar visualmente el progreso de la pesca.
    /// </summary>
    private void ActualizarColorBarra()
    {
        if (imagenRelleno == null)
            return;
        if (progreso < 0.5f)
            imagenRelleno.color = Color.Lerp(Color.red, Color.yellow, progreso * 2f);
        else
            imagenRelleno.color = Color.Lerp(Color.yellow, Color.green, (progreso - 0.5f) * 2f);
    }

    /// <summary>
    /// Finaliza el minijuego con éxito, registra en la colección y reproduce sonido de ganar.
    /// </summary>
    private void GanarPesca()
    {
        TerminarMinijuego();
        if (animadorCana != null)
            animadorCana.SetTrigger("Recoger");
        if (SoundManager.Instance != null)
            SoundManager.Instance.SFX_Ganar();

        if (InventorySystem.Instance != null && pezActualEnJuego != null)
        {
            InventorySystem.Instance.AnadirObjeto(pezActualEnJuego);
            GameManager.Instance.RegistrarPezCapturado(pezActualEnJuego.ID);

            if (
                GameManager.Instance != null
                && Random.Range(0f, 1f) <= GameManager.Instance.bufoProbabilidadDoble
            )
            {
                InventorySystem.Instance.AnadirObjeto(pezActualEnJuego);
                GameManager.Instance.RegistrarPezCapturado(pezActualEnJuego.ID);
                if (UIManager.Instance != null)
                    UIManager.Instance.MostrarTooltipTemporal("¡MILAGRO! ¡Captura Doble!", 3f);
            }
        }
        Invoke("ResetearSistema", 2f);
    }

    /// <summary>
    /// Finaliza el minijuego con fallo y reproduce sonido de perder.
    /// </summary>
    private void PerderPesca()
    {
        TerminarMinijuego();
        if (animadorCana != null)
            animadorCana.SetTrigger("Recoger");
        if (SoundManager.Instance != null)
            SoundManager.Instance.SFX_Perder();
        Invoke("ResetearSistema", 1f);
    }

    /// <summary>
    /// Cancela abruptamente la pesca, parando los sonidos en curso.
    /// </summary>
    private void CancelarPesca()
    {
        if (cronometroEspera != null)
            StopCoroutine(cronometroEspera);
        esperandoPez = false;

        if (animadorCana != null)
            animadorCana.SetTrigger("Recoger");
        if (SoundManager.Instance != null)
            SoundManager.Instance.SFX_PararForcejeo();
        Invoke("ResetearSistema", 1f);
    }

    /// <summary>
    /// Oculta la interfaz gráfica del minijuego.
    /// </summary>
    private void TerminarMinijuego()
    {
        enMinijuego = false;
        panelMinijuego.SetActive(false);
        barraProgreso.gameObject.SetActive(false);
    }

    /// <summary>
    /// Libera los controles del jugador y restaura los avisos de la UI según la posición.
    /// </summary>
    private void ResetearSistema()
    {
        puedeLanzar = true;
        esperandoPez = false;
        enMinijuego = false;

        if (movimientoJugador != null)
            movimientoJugador.movimientoBloqueado = false;
        if (controlCamara != null)
            controlCamara.rotacionBloqueada = false;

        ActualizarTextosUI();
    }
}
