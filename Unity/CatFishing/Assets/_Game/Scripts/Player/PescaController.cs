using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Gestiona las mecánicas principales del minijuego de pesca y la interacción con la caña.
/// [Relaciones: PlayerMovement, CamaraMovement, InventorySystem, UIManager, FishManager, SoundManager, GameManager, ItemData, GatoNPC]
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

    /// <summary>
    /// Asigna las referencias de componentes y desactiva la interfaz de pesca visual.
    /// </summary>
    void Start()
    {
        movimientoJugador = GetComponentInParent<PlayerMovement>();
        controlCamara = FindFirstObjectByType<CamaraMovement>();

        if (panelMinijuego != null)
        {
            panelMinijuego.SetActive(false);
        }

        if (barraProgreso != null)
        {
            barraProgreso.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Escucha la entrada del ratón y determina el estado de las mecánicas de pesca.
    /// </summary>
    void Update()
    {
        if (Mouse.current != null)
        {
            if (InventorySystem.Instance != null && !InventorySystem.Instance.TieneCanaEnMano())
            {
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.OcultarInteraccion();
                }
            }
            else
            {
                if (enMinijuego)
                {
                    ControlarMinijuego();
                }
                else if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    if (puedeLanzar && enZonaDePesca)
                    {
                        EmpezarLanzamiento();
                    }
                    else if (esperandoPez)
                    {
                        CancelarPesca();
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("No se detecta el ratón conectado.");
        }
    }

    /// <summary>
    /// Permite o bloquea el inicio de la acción de pescar según la ubicación espacial.
    /// </summary>
    public void EstablecerZonaDePesca(bool estado)
    {
        enZonaDePesca = estado;
        ActualizarTextosUI();
    }

    /// <summary>
    /// Cambia dinámicamente los avisos de la interfaz en base a la fase de la pesca en la que está el jugador.
    /// </summary>
    private void ActualizarTextosUI()
    {
        if (UIManager.Instance != null)
        {
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
    }

    /// <summary>
    /// Lanza el anzuelo y prepara el sistema para aguardar una captura.
    /// </summary>
    private void EmpezarLanzamiento()
    {
        if (FishManager.Instance != null && FishManager.Instance.pecesCargados)
        {
            puedeLanzar = false;

            if (movimientoJugador != null)
            {
                movimientoJugador.movimientoBloqueado = true;
            }

            if (controlCamara != null)
            {
                controlCamara.rotacionBloqueada = true;
            }

            if (animadorCana != null)
            {
                animadorCana.SetTrigger("Lanzar");
            }

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.SFX_Lanzar();
            }

            esperandoPez = true;
            ActualizarTextosUI();
            cronometroEspera = StartCoroutine(RutinaEsperarPez());
        }
        else
        {
            Debug.LogWarning("No se puede pescar porque los peces aún no se han descargado.");
        }
    }

    /// <summary>
    /// Introduce un tiempo de espera aleatorio antes de que un pez pique el anzuelo.
    /// </summary>
    private IEnumerator RutinaEsperarPez()
    {
        yield return new WaitForSeconds(Random.Range(2f, 5f));
        EmpezarMinijuego();
    }

    /// <summary>
    /// Inicializa las variables lógicas y de interfaz correspondientes a la fase interactiva de forcejeo.
    /// </summary>
    private void EmpezarMinijuego()
    {
        esperandoPez = false;
        enMinijuego = true;
        ActualizarTextosUI();

        if (animadorCana != null)
        {
            animadorCana.SetTrigger("Picar");
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SFX_EmpezarForcejeo();
        }

        if (panelMinijuego != null)
        {
            panelMinijuego.SetActive(true);
        }

        if (barraProgreso != null)
        {
            barraProgreso.gameObject.SetActive(true);
        }

        pezActualEnJuego = ElegirPezDefinitivo();
        ConfigurarDificultadDinamica(pezActualEnJuego);

        if (iconoPez != null && iconoPez.GetComponent<Image>() != null && iconoIncognita != null)
        {
            iconoPez.GetComponent<Image>().sprite = iconoIncognita;
        }

        posBarra = 0.1f;
        posPez = 0.5f;
        progreso = 0.15f;
    }

    /// <summary>
    /// Decide de manera estocástica el pez que morderá el anzuelo combinando requerimientos de misiones e influencia de equipamiento.
    /// </summary>
    private ItemData ElegirPezDefinitivo()
    {
        ItemData resultado;
        ItemData pezMision;

        if (Random.Range(0f, 100f) <= probabilidadPezMision)
        {
            pezMision = BuscarPezRequeridoPorGatos();
            if (pezMision != null)
            {
                resultado = pezMision;
            }
            else
            {
                resultado = ElegirPezPorCaña();
            }
        }
        else
        {
            resultado = ElegirPezPorCaña();
        }

        return resultado;
    }

    /// <summary>
    /// Consulta los datos de cada NPC en la isla buscando sus peces deseados en caso de necesidad.
    /// </summary>
    private ItemData BuscarPezRequeridoPorGatos()
    {
        GatoNPC[] gatosEnIsla;
        List<string> idsBuscados;
        string idElegido;
        ItemData resultado = null;

        gatosEnIsla = FindObjectsByType<GatoNPC>(FindObjectsSortMode.None);
        idsBuscados = new List<string>();

        foreach (GatoNPC gato in gatosEnIsla)
        {
            if (!string.IsNullOrEmpty(gato.idPezDeseado))
            {
                idsBuscados.Add(gato.idPezDeseado);
            }
        }

        if (idsBuscados.Count > 0)
        {
            idElegido = idsBuscados[Random.Range(0, idsBuscados.Count)];

            foreach (ItemData pez in FishManager.Instance.todosLosPeces)
            {
                if (pez.ID == idElegido)
                {
                    resultado = pez;
                }
            }
        }

        return resultado;
    }

    /// <summary>
    /// Calcula una probabilidad de rareza base teniendo en cuenta la herramienta equipada por el jugador.
    /// </summary>
    private ItemData ElegirPezPorCaña()
    {
        ItemData canaActual;
        Rareza rarezaCana;
        float dado;
        Rareza rarezaBuscada;
        List<ItemData> candidatos;
        ItemData resultado;

        canaActual = InventorySystem.Instance.ObtenerItemEnMano();
        rarezaCana = canaActual != null ? canaActual.rareza : Rareza.Comun;
        dado = Random.Range(0f, 100f);
        rarezaBuscada = Rareza.Comun;

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

        candidatos = new List<ItemData>();

        foreach (ItemData pez in FishManager.Instance.todosLosPeces)
        {
            if (pez.rareza == rarezaBuscada)
            {
                candidatos.Add(pez);
            }
        }

        if (candidatos.Count > 0)
        {
            resultado = candidatos[Random.Range(0, candidatos.Count)];
        }
        else
        {
            resultado = FishManager.Instance.todosLosPeces[
                Random.Range(0, FishManager.Instance.todosLosPeces.Count)
            ];
        }

        return resultado;
    }

    /// <summary>
    /// Modifica la velocidad del pez y tamaño de zona segura basado en la brecha de nivel y bufos en memoria.
    /// </summary>
    private void ConfigurarDificultadDinamica(ItemData pez)
    {
        ItemData canaActual;
        int nivelCana;
        int nivelPez;
        int diferencia;

        if (pez != null)
        {
            canaActual = InventorySystem.Instance.ObtenerItemEnMano();
            nivelCana = canaActual != null ? (int)canaActual.rareza : 0;
            nivelPez = (int)pez.rareza;
            diferencia = nivelPez - nivelCana;

            velocidadPezActual = 0.5f + (diferencia * 0.3f);
            tamanoBarraActual = 0.3f - (diferencia * 0.05f);

            if (GameManager.Instance != null)
            {
                velocidadPezActual += GameManager.Instance.bufoReduccionDificultad;
            }

            velocidadPezActual = Mathf.Clamp(velocidadPezActual, 0.2f, 2.5f);
            tamanoBarraActual = Mathf.Clamp(tamanoBarraActual, 0.08f, 0.5f);

            if (barraVerde != null)
            {
                barraVerde.sizeDelta = new Vector2(
                    barraVerde.sizeDelta.x,
                    tamanoBarraActual * alturaContenedor
                );
            }
        }
        else
        {
            Debug.LogWarning("No se puede configurar la dificultad sin saber el pez.");
        }
    }

    /// <summary>
    /// Maneja el ciclo en tiempo real que mueve el pez con ruido Perlin y la barra en respuesta al clic del jugador.
    /// </summary>
    private void ControlarMinijuego()
    {
        float diferencia;
        bool dentroDeZona;

        tiempoPez += Time.deltaTime * velocidadPezActual;
        posPez = Mathf.PerlinNoise(tiempoPez, 0f);

        if (Mouse.current.leftButton.isPressed)
        {
            posBarra += velocidadSubidaBarra * Time.deltaTime;
        }
        else
        {
            posBarra -= gravedadBarra * Time.deltaTime;
        }

        posBarra = Mathf.Clamp(posBarra, 0f, 1f);

        if (barraVerde != null)
            barraVerde.anchoredPosition = new Vector2(0, posBarra * alturaContenedor);
        if (iconoPez != null)
            iconoPez.anchoredPosition = new Vector2(0, posPez * alturaContenedor);

        diferencia = Mathf.Abs(posBarra - posPez);
        dentroDeZona = diferencia < (tamanoBarraActual / 2f);

        if (iconoPez != null && iconoPez.GetComponent<Image>() != null)
        {
            iconoPez.GetComponent<Image>().color = dentroDeZona ? Color.green : Color.white;
        }

        if (dentroDeZona)
        {
            progreso += velocidadProgreso * Time.deltaTime;
        }
        else
        {
            progreso -= (velocidadProgreso * 0.2f) * Time.deltaTime;
        }

        progreso = Mathf.Clamp(progreso, 0f, 1f);

        if (barraProgreso != null)
            barraProgreso.value = progreso;

        ActualizarColorBarra();

        if (progreso >= 1f)
        {
            GanarPesca();
        }
        else if (progreso <= 0f)
        {
            PerderPesca();
        }
    }

    /// <summary>
    /// Ajusta visualmente el tono rojo o verde de la barra lateral según la proximidad a victoria o derrota.
    /// </summary>
    private void ActualizarColorBarra()
    {
        if (imagenRelleno != null)
        {
            if (progreso < 0.5f)
            {
                imagenRelleno.color = Color.Lerp(Color.red, Color.yellow, progreso * 2f);
            }
            else
            {
                imagenRelleno.color = Color.Lerp(Color.yellow, Color.green, (progreso - 0.5f) * 2f);
            }
        }
    }

    /// <summary>
    /// Procesos al completar el minijuego, como repartir recompensas y guardar progreso general.
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

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegistrarPezCapturado(pezActualEnJuego.ID);

                if (Random.Range(0f, 1f) <= GameManager.Instance.bufoProbabilidadDoble)
                {
                    InventorySystem.Instance.AnadirObjeto(pezActualEnJuego);
                    GameManager.Instance.RegistrarPezCapturado(pezActualEnJuego.ID);

                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.MostrarTooltipTemporal("¡MILAGRO! ¡Captura Doble!", 3f);
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning(
                "No se entregó el premio porque el inventario no funciona o no había pez."
            );
        }

        Invoke("ResetearSistema", 2f);
    }

    /// <summary>
    /// Procesa las consecuencias de bajar a 0% el progreso del forcejeo perdiendo la presa.
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
    /// Recoge la caña forzosamente anulando la espera del pez mediante una interrupción manual.
    /// </summary>
    private void CancelarPesca()
    {
        if (cronometroEspera != null)
        {
            StopCoroutine(cronometroEspera);
        }

        esperandoPez = false;

        if (animadorCana != null)
            animadorCana.SetTrigger("Recoger");
        if (SoundManager.Instance != null)
            SoundManager.Instance.SFX_PararForcejeo();

        Invoke("ResetearSistema", 1f);
    }

    /// <summary>
    /// Desactiva todos los gráficos de interfaz relacionados con la captura.
    /// </summary>
    private void TerminarMinijuego()
    {
        enMinijuego = false;

        if (panelMinijuego != null)
            panelMinijuego.SetActive(false);
        if (barraProgreso != null)
            barraProgreso.gameObject.SetActive(false);
    }

    /// <summary>
    /// Devuelve el control total al jugador permitiendo nuevos lanzamientos.
    /// </summary>
    private void ResetearSistema()
    {
        puedeLanzar = true;
        esperandoPez = false;
        enMinijuego = false;

        if (movimientoJugador != null)
        {
            movimientoJugador.movimientoBloqueado = false;
        }

        if (controlCamara != null)
        {
            controlCamara.rotacionBloqueada = false;
        }

        ActualizarTextosUI();
    }
}
