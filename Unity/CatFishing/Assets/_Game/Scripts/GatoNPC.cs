using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Gestiona la relación entre el jugador y un gato, su impaciencia y la conversación inicial.
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

    private enum EstadoInteraccion
    {
        Inactivo,
        LeyendoWelcome,
        LeyendoRespuesta,
    }

    private EstadoInteraccion estadoActual = EstadoInteraccion.Inactivo;

    /// <summary>
    /// Conecta con el jugador e inicia la descarga de frases propias del gato.
    /// </summary>
    void Start()
    {
        controlCamara = FindFirstObjectByType<CamaraMovement>();
        controlMovimiento = FindFirstObjectByType<PlayerMovement>();
        CargarDatosDesdeFirebase();
    }

    /// <summary>
    /// Localiza al gato en internet y trae sus frases y progreso de amistad antiguo.
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
    /// Comprueba de forma continua la paciencia del gato y si el jugador pulsa la barra.
    /// </summary>
    void Update()
    {
        bool puedeInteractuar;

        if (interaccionBloqueada)
        {
            return;
        }

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

    /// <summary>
    /// Pasa de fase dentro de la charla, deteniéndose a comprobar si se da comida al final.
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
    /// Lee qué objeto tiene el jugador equipado e inspecciona si es el pez correcto.
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
    /// Suma la amistad, otorga dinero al jugador y expulsa al gato de la escena en positivo.
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
    /// Establece qué nivel tiene el gato matemáticamente leyendo su total de experiencia.
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
    /// Resta experiencia al gato si se acaba su temporizador y lo marcha del nivel.
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
    /// Lanza la queja del gato cuando se le da un objeto incorrecto que no quiere comer.
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
    /// Lanza la frase desilusionada cuando el jugador interactúa con la mano vacía.
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
    /// Extrae un texto interno basándose en su clave de búsqueda.
    /// </summary>
    private string ObtenerFrase(string clave)
    {
        if (todosLosDialogos.ContainsKey(clave))
        {
            return todosLosDialogos[clave];
        }
        else
        {
            Debug.LogWarning("No se encontró la línea del gato con la clave solicitada.");
            return "...";
        }
    }

    /// <summary>
    /// Emite la frase hacia la burbuja de la interfaz central de usuario.
    /// </summary>
    private void MostrarDialogo(string frase)
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.MostrarBocadillo(frase);
        }
    }

    /// <summary>
    /// Reproduce el maullido, bloquea controles y fija la posición de ambos.
    /// </summary>
    private void ComenzarInteraccion()
    {
        CatBehavior behavior;

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

        behavior = GetComponent<CatBehavior>();
        if (behavior != null && controlMovimiento != null)
        {
            behavior.IniciarInteraccion(controlMovimiento.transform);
        }
    }

    /// <summary>
    /// Libera las restricciones al jugador y retira la burbuja de conversación de la pantalla.
    /// </summary>
    private void TerminarInteraccionCompleta()
    {
        CatBehavior behavior;

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

        behavior = GetComponent<CatBehavior>();
        if (behavior != null)
        {
            behavior.FinalizarInteraccion();
        }
    }

    /// <summary>
    /// Limpia al personaje finalizado de la escena.
    /// </summary>
    private void DesaparecerDeLaIsla()
    {
        TerminarInteraccionCompleta();
        Destroy(gameObject);
    }

    /// <summary>
    /// Avisa al jugador si se aproxima mientras la interaccion se puede realizar.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !interaccionBloqueada)
        {
            jugadorCerca = true;
        }
    }

    /// <summary>
    /// Avisa de que el jugador se va, limpiando el texto flotante si no se le está hablando.
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
