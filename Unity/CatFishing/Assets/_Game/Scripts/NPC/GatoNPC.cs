using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controla la lógica de diálogos, misiones y afinidad individual de un gato específico.
/// [Interacción BD]
/// [Relaciones: CamaraMovement, PlayerMovement, CatBehavior, GameManager, UIManager, InventorySystem, SoundManager, ItemData]
/// </summary>
public class GatoNPC : MonoBehaviour
{
    [Header("Configuración Base de Datos")]
    public string idGatoDB;
    public string idPezDeseado;
    public string nombreGato = "Gato";

    [Header("Progreso y Recompensas")]
    public int recompensaExperiencia = 25;
    public int recompensaMonedas = 10;

    [Header("Comportamiento")]
    public float tiempoPaciencia = 300f;

    [Header("Audio")]
    public AudioClip maullidoPersonalizado;

    private int xpAfinidadActual = 0;
    private int nivelAfinidad = 1;
    private int xpNecesariaAfinidad = 50;

    private Dictionary<string, string> todosLosDialogos = new Dictionary<string, string>();
    private bool datosCargados = false;
    private bool jugadorCerca = false;
    private bool interaccionBloqueada = false;

    private CamaraMovement controlCamara;
    private PlayerMovement controlMovimiento;
    private CatBehavior behavior;

    private enum EstadoInteraccion
    {
        Inactivo,
        LeyendoWelcome,
        LeyendoRespuesta,
    }

    private EstadoInteraccion estadoActual = EstadoInteraccion.Inactivo;

    /// <summary>
    /// Guarda referencias y ejecuta la descarga de datos primarios del NPC.
    /// </summary>
    void Start()
    {
        controlCamara = FindFirstObjectByType<CamaraMovement>();
        controlMovimiento = FindFirstObjectByType<PlayerMovement>();
        behavior = GetComponent<CatBehavior>();
        CargarDatosDesdeFirebase();
    }

    /// <summary>
    /// [BD] Recopila las frases de diálogo y sincroniza la afinidad acumulada del servidor.
    /// </summary>
    private async void CargarDatosDesdeFirebase()
    {
        FirebaseFirestore db;
        DocumentReference docRef;
        DocumentSnapshot snapshot;
        Dictionary<string, object> dialoguesMap;

        db = FirebaseFirestore.DefaultInstance;
        docRef = db.Collection("cats").Document(idGatoDB);

        try
        {
            snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                if (snapshot.ContainsField("name"))
                {
                    nombreGato = snapshot.GetValue<string>("name");
                }

                dialoguesMap = snapshot.GetValue<Dictionary<string, object>>("dialogues");

                foreach (KeyValuePair<string, object> entry in dialoguesMap)
                {
                    todosLosDialogos[entry.Key] = entry.Value.ToString();
                }

                datosCargados = true;

                if (
                    GameManager.Instance != null
                    && GameManager.Instance.afinidadGatos.ContainsKey(idGatoDB)
                )
                {
                    xpAfinidadActual = GameManager.Instance.afinidadGatos[idGatoDB];
                }
                else if (GameManager.Instance != null)
                {
                    GameManager.Instance.ActualizarAfinidadGato(idGatoDB, 0);
                }

                CalcularNivelAfinidad();
            }
            else
            {
                Debug.LogWarning("El perfil de gato no existe en la base de datos.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Fallo al descargar la información del gato: " + e.Message);
        }
    }

    /// <summary>
    /// Comprueba la reducción de paciencia y procesa el evento del diálogo si el jugador actúa.
    /// </summary>
    void Update()
    {
        bool puedeInteractuar;

        if (!interaccionBloqueada)
        {
            tiempoPaciencia -= Time.deltaTime;

            if (tiempoPaciencia <= 0 && estadoActual == EstadoInteraccion.Inactivo)
            {
                interaccionBloqueada = true;
                BajarAfinidadPorImpaciencia();
            }
            else
            {
                puedeInteractuar =
                    (jugadorCerca || estadoActual != EstadoInteraccion.Inactivo) && datosCargados;

                if (puedeInteractuar)
                {
                    if (estadoActual == EstadoInteraccion.Inactivo && UIManager.Instance != null)
                    {
                        UIManager.Instance.MostrarInteraccion("Pulsa [ESPACIO] para hablar");
                    }

                    if (Keyboard.current.spaceKey.wasPressedThisFrame)
                    {
                        if (estadoActual == EstadoInteraccion.Inactivo && UIManager.Instance != null)
                        {
                            if (UIManager.Instance.ConsumirInteraccion())
                            {
                                AvanzarInteraccion();
                            }
                            else
                            {
                                Debug.LogWarning("Otro panel está abierto, el gato no responde.");
                            }
                        }
                        else
                        {
                            AvanzarInteraccion();
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Alterna el estado conversacional dependiendo de la fase de la charla en la que esté la interacción.
    /// </summary>
    private void AvanzarInteraccion()
    {
        string claveWelcome;

        switch (estadoActual)
        {
            case EstadoInteraccion.Inactivo:
                ComenzarInteraccion();
                estadoActual = EstadoInteraccion.LeyendoWelcome;
                claveWelcome = "welcome" + Mathf.Clamp(nivelAfinidad, 1, 10).ToString("D2");
                MostrarDialogo(ObtenerFrase(claveWelcome));
                break;

            case EstadoInteraccion.LeyendoWelcome:
                estadoActual = EstadoInteraccion.LeyendoRespuesta;
                IntentarDarComida();
                break;

            case EstadoInteraccion.LeyendoRespuesta:
                TerminarInteraccionCompleta();
                break;

            default:
                Debug.LogWarning("El gato entró en un paso del diálogo desconocido.");
                break;
        }
    }

    /// <summary>
    /// Extrae el objeto portado y responde afirmativa o negativamente si coincide con la demanda.
    /// </summary>
    private void IntentarDarComida()
    {
        ItemData itemEnMano;

        if (InventorySystem.Instance == null)
        {
            TerminarInteraccionCompleta();
            Debug.LogWarning("No se encontró el inventario para leer la comida.");
        }
        else
        {
            itemEnMano = InventorySystem.Instance.ObtenerItemEnMano();

            if (itemEnMano != null && itemEnMano.ID == idPezDeseado)
            {
                InventorySystem.Instance.ConsumirItemEnMano();
                interaccionBloqueada = true;
                ReaccionFeliz();
            }
            else if (itemEnMano != null)
            {
                ReaccionEnfadado();
            }
            else
            {
                ReaccionTriste();
            }
        }
    }

    /// <summary>
    /// Suma la experiencia, da las recompensas al jugador y activa los sonidos victoriosos.
    /// </summary>
    private void ReaccionFeliz()
    {
        string claveHappy;
        int xpGanada;

        claveHappy = "happy" + Mathf.Clamp(nivelAfinidad, 1, 10).ToString("D2");
        MostrarDialogo(ObtenerFrase(claveHappy));

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.ReproducirGatoFeliz();
        }

        if (nivelAfinidad < 10 && GameManager.Instance != null)
        {
            xpGanada = Mathf.RoundToInt(50 * GameManager.Instance.bufoAfinidad);
            xpAfinidadActual += xpGanada;

            GameManager.Instance.ActualizarAfinidadGato(idGatoDB, xpAfinidadActual);
            CalcularNivelAfinidad();

            if (UIManager.Instance != null)
            {
                UIManager.Instance.MostrarAfinidadGato(
                    nivelAfinidad,
                    xpAfinidadActual,
                    xpNecesariaAfinidad
                );
            }
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AnadirExperiencia(recompensaExperiencia);
            GameManager.Instance.AnadirDinero(recompensaMonedas);
        }

        Invoke("DesaparecerDeLaIsla", 4f);
    }

    /// <summary>
    /// Interpola la experiencia total para descifrar de qué nivel exacto debe gozar la amistad.
    /// </summary>
    private void CalcularNivelAfinidad()
    {
        int xpRestante;

        nivelAfinidad = 1;
        xpRestante = xpAfinidadActual;
        xpNecesariaAfinidad = 50;

        while (xpRestante >= xpNecesariaAfinidad && nivelAfinidad < 10)
        {
            xpRestante -= xpNecesariaAfinidad;
            nivelAfinidad++;
            xpNecesariaAfinidad = Mathf.RoundToInt(xpNecesariaAfinidad * 1.5f);
        }

        if (nivelAfinidad >= 10)
        {
            xpAfinidadActual = xpNecesariaAfinidad;
        }
    }

    /// <summary>
    /// Reduce permanentemente el afecto de este personaje y fuerza su marcha.
    /// </summary>
    private void BajarAfinidadPorImpaciencia()
    {
        if (xpAfinidadActual > 0 && GameManager.Instance != null)
        {
            xpAfinidadActual = Mathf.Max(0, xpAfinidadActual - 15);
            GameManager.Instance.ActualizarAfinidadGato(idGatoDB, xpAfinidadActual);
        }
        MostrarDialogo("¡Llevo esperando horas! Me marcho a otra parte...");
        Invoke("DesaparecerDeLaIsla", 3f);
    }

    /// <summary>
    /// Procesa el disgusto del gato al ofrecerle algo equivocado.
    /// </summary>
    private void ReaccionEnfadado()
    {
        MostrarDialogo(ObtenerFrase("angry"));
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.ReproducirGatoEnfadado();
        }
    }

    /// <summary>
    /// Muestra un texto decepcionado por haber acudido al gato con las manos vacías.
    /// </summary>
    private void ReaccionTriste()
    {
        MostrarDialogo(ObtenerFrase("sad"));
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.ReproducirGatoEnfadado();
        }
    }

    /// <summary>
    /// Traduce la clave de texto a su oración almacenada en la descarga local.
    /// </summary>
    private string ObtenerFrase(string clave)
    {
        string frase;

        if (todosLosDialogos.ContainsKey(clave))
        {
            frase = todosLosDialogos[clave];
        }
        else
        {
            Debug.LogWarning("No se encontró la línea del gato con la clave solicitada.");
            frase = "...";
        }

        return frase;
    }

    /// <summary>
    /// Imprime el texto de respuesta del NPC apoyándose del UIManager de la partida.
    /// </summary>
    private void MostrarDialogo(string frase)
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.MostrarBocadillo(frase);
        }
    }

    /// <summary>
    /// Detiene la acción global del juego preparándose formalmente para dialogar.
    /// </summary>
    private void ComenzarInteraccion()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.ReproducirSonidoPersonalizado(maullidoPersonalizado);
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.OcultarInteraccion();
            UIManager.Instance.MostrarAfinidadGato(
                nivelAfinidad,
                xpAfinidadActual,
                xpNecesariaAfinidad
            );
        }

        if (controlCamara != null)
        {
            controlCamara.rotacionBloqueada = true;
        }

        if (controlMovimiento != null)
        {
            controlMovimiento.movimientoBloqueado = true;
        }

        if (behavior != null && controlMovimiento != null)
        {
            behavior.IniciarInteraccion(controlMovimiento.transform);
        }
    }

    /// <summary>
    /// Cierra y restablece todos los valores a los tiempos que precedían a la entrevista.
    /// </summary>
    private void TerminarInteraccionCompleta()
    {
        estadoActual = EstadoInteraccion.Inactivo;

        if (controlCamara != null)
        {
            controlCamara.rotacionBloqueada = false;
        }

        if (controlMovimiento != null)
        {
            controlMovimiento.movimientoBloqueado = false;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.OcultarBocadillo();
        }

        if (behavior != null)
        {
            behavior.FinalizarInteraccion();
        }
    }

    /// <summary>
    /// Ejecuta el fin de vida del objeto y de cualquier diálogo residual.
    /// </summary>
    private void DesaparecerDeLaIsla()
    {
        TerminarInteraccionCompleta();
        Destroy(gameObject);
    }

    /// <summary>
    /// Habilita los mensajes de acción cuando un jugador cruza la zona conversacional.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !interaccionBloqueada)
        {
            jugadorCerca = true;
        }
    }

    /// <summary>
    /// Deshabilita cualquier notificación visual cuando el jugador recula.
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = false;
            if (estadoActual == EstadoInteraccion.Inactivo && UIManager.Instance != null)
            {
                UIManager.Instance.OcultarInteraccion();
            }
        }
    }
}
