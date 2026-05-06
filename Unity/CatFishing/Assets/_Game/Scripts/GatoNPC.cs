using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Gestiona la interacción del NPC Gato, aplicando pérdida de afinidad si se agota la paciencia
/// y registrando el encuentro y XP en la Colección del jugador.
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

    void Start()
    {
        controlCamara = FindFirstObjectByType<CamaraMovement>();
        controlMovimiento = FindFirstObjectByType<PlayerMovement>();
        CargarDatosDesdeFirebase();
    }

    private async void CargarDatosDesdeFirebase()
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        DocumentReference docRef = db.Collection("cats").Document(idGatoDB);

        try
        {
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                if (snapshot.ContainsField("name"))
                    nombreGato = snapshot.GetValue<string>("name");

                Dictionary<string, object> dialoguesMap = snapshot.GetValue<
                    Dictionary<string, object>
                >("dialogues");
                foreach (KeyValuePair<string, object> entry in dialoguesMap)
                    todosLosDialogos[entry.Key] = entry.Value.ToString();

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
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error Firebase Gato: " + e.Message);
        }
    }

    /// <summary>
    /// Revisa la paciencia y permite interactuar siempre que otra interfaz no reclame prioridad.
    /// </summary>
    void Update()
    {
        if (interaccionBloqueada)
            return;

        tiempoPaciencia -= Time.deltaTime;
        if (tiempoPaciencia <= 0 && estadoActual == EstadoInteraccion.Inactivo)
        {
            interaccionBloqueada = true;
            BajarAfinidadPorImpaciencia();
            return;
        }

        bool puedeInteractuar =
            (jugadorCerca || estadoActual != EstadoInteraccion.Inactivo) && datosCargados;

        if (puedeInteractuar)
        {
            if (estadoActual == EstadoInteraccion.Inactivo && UIManager.Instance != null)
                UIManager.Instance.MostrarInteraccion("Pulsa [ESPACIO] para hablar");

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                if (estadoActual == EstadoInteraccion.Inactivo && UIManager.Instance != null)
                {
                    if (!UIManager.Instance.ConsumirInteraccion())
                        return;
                }
                AvanzarInteraccion();
            }
        }
    }

    private void AvanzarInteraccion()
    {
        switch (estadoActual)
        {
            case EstadoInteraccion.Inactivo:
                ComenzarInteraccion();
                estadoActual = EstadoInteraccion.LeyendoWelcome;
                string claveWelcome = "welcome" + Mathf.Clamp(nivelAfinidad, 1, 10).ToString("D2");
                MostrarDialogo(ObtenerFrase(claveWelcome));
                break;
            case EstadoInteraccion.LeyendoWelcome:
                estadoActual = EstadoInteraccion.LeyendoRespuesta;
                IntentarDarComida();
                break;
            case EstadoInteraccion.LeyendoRespuesta:
                TerminarInteraccionCompleta();
                break;
        }
    }

    private void IntentarDarComida()
    {
        if (InventorySystem.Instance == null)
        {
            TerminarInteraccionCompleta();
            return;
        }

        ItemData itemEnMano = InventorySystem.Instance.ObtenerItemEnMano();

        if (itemEnMano != null && itemEnMano.ID == idPezDeseado)
        {
            InventorySystem.Instance.ConsumirItemEnMano();
            interaccionBloqueada = true;
            ReaccionFeliz();
        }
        else if (itemEnMano != null)
            ReaccionEnfadado();
        else
            ReaccionTriste();
    }

    /// <summary>
    /// Suma XP de afinidad, emite sonido feliz, recalcula nivel y actualiza UI.
    /// </summary>
    private void ReaccionFeliz()
    {
        string claveHappy = "happy" + Mathf.Clamp(nivelAfinidad, 1, 10).ToString("D2");
        MostrarDialogo(ObtenerFrase(claveHappy));

        if (SoundManager.Instance != null)
            SoundManager.Instance.ReproducirGatoFeliz();

        if (nivelAfinidad < 10 && GameManager.Instance != null)
        {
            int xpGanada = Mathf.RoundToInt(50 * GameManager.Instance.bufoAfinidad);
            xpAfinidadActual += xpGanada;

            GameManager.Instance.ActualizarAfinidadGato(idGatoDB, xpAfinidadActual);
            CalcularNivelAfinidad();

            if (UIManager.Instance != null)
                UIManager.Instance.MostrarAfinidadGato(
                    nivelAfinidad,
                    xpAfinidadActual,
                    xpNecesariaAfinidad
                );
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AnadirExperiencia(recompensaExperiencia);
            GameManager.Instance.AnadirDinero(recompensaMonedas);
        }

        Invoke("DesaparecerDeLaIsla", 4f);
    }

    /// <summary>
    /// Calcula el nivel actual del gato basado en la XP total acumulada.
    /// </summary>
    private void CalcularNivelAfinidad()
    {
        nivelAfinidad = 1;
        int xpRestante = xpAfinidadActual;
        xpNecesariaAfinidad = 50;

        while (xpRestante >= xpNecesariaAfinidad && nivelAfinidad < 10)
        {
            xpRestante -= xpNecesariaAfinidad;
            nivelAfinidad++;
            xpNecesariaAfinidad = Mathf.RoundToInt(xpNecesariaAfinidad * 1.5f);
        }

        if (nivelAfinidad >= 10)
            xpAfinidadActual = xpNecesariaAfinidad;
    }

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
    /// Reacción de enfado con su respectivo sonido.
    /// </summary>
    private void ReaccionEnfadado()
    {
        MostrarDialogo(ObtenerFrase("angry"));
        if (SoundManager.Instance != null)
            SoundManager.Instance.ReproducirGatoEnfadado();
    }

    /// <summary>
    /// Reacción de tristeza con su respectivo sonido.
    /// </summary>
    private void ReaccionTriste()
    {
        MostrarDialogo(ObtenerFrase("sad"));
        if (SoundManager.Instance != null)
            SoundManager.Instance.ReproducirGatoEnfadado();
    }

    private string ObtenerFrase(string clave)
    {
        return todosLosDialogos.ContainsKey(clave) ? todosLosDialogos[clave] : "...";
    }

    private void MostrarDialogo(string frase)
    {
        if (UIManager.Instance != null)
            UIManager.Instance.MostrarBocadillo(frase);
    }

    /// <summary>
    /// Bloquea los controles, reproduce el maullido propio del gato y muestra la UI.
    /// </summary>
    private void ComenzarInteraccion()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.ReproducirSonidoPersonalizado(maullidoPersonalizado);

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
            controlCamara.rotacionBloqueada = true;
        if (controlMovimiento != null)
            controlMovimiento.movimientoBloqueado = true;

        CatBehavior behavior = GetComponent<CatBehavior>();
        if (behavior != null && controlMovimiento != null)
            behavior.IniciarInteraccion(controlMovimiento.transform);
    }

    private void TerminarInteraccionCompleta()
    {
        estadoActual = EstadoInteraccion.Inactivo;
        if (controlCamara != null)
            controlCamara.rotacionBloqueada = false;
        if (controlMovimiento != null)
            controlMovimiento.movimientoBloqueado = false;
        if (UIManager.Instance != null)
            UIManager.Instance.OcultarBocadillo();

        CatBehavior behavior = GetComponent<CatBehavior>();
        if (behavior != null)
            behavior.FinalizarInteraccion();
    }

    private void DesaparecerDeLaIsla()
    {
        TerminarInteraccionCompleta();
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !interaccionBloqueada)
            jugadorCerca = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = false;
            if (estadoActual == EstadoInteraccion.Inactivo && UIManager.Instance != null)
                UIManager.Instance.OcultarInteraccion();
        }
    }
}
