using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla la información principal del jugador, los modificadores actuales y guarda el progreso en la nube.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Interfaz de Usuario")]
    public TextMeshProUGUI textoNivel;
    public TextMeshProUGUI textoDinero;
    public Slider barraExperiencia;

    [Header("Configuración de Progresión")]
    public int baseXP = 100;
    public float multiplicadorXP = 1.5f;

    [Header("Modificadores Activos")]
    public float bufoGananciaXP = 1f;
    public float bufoAfinidad = 1f;
    public float bufoRarezaPesca = 1f;
    public float bufoPrecioVenta = 1f;
    public float bufoReduccionDificultad = 0f;
    public float bufoProbabilidadDoble = 0f;

    [Header("Datos de Colección y Racha")]
    public int rachaIglesia = 0;
    public bool haRezadoHoy = false;
    public Dictionary<string, int> pecesCapturados = new Dictionary<string, int>();
    public Dictionary<string, int> afinidadGatos = new Dictionary<string, int>();

    private string idUsuario;
    private string idSaveSlot;
    private int nivelActual = 1;
    private int experienciaActual = 0;
    private int dineroActual = 0;
    private int experienciaNecesaria = 100;

    /// <summary>
    /// Vincula la ruta de guardado en base al slot escogido en el menú principal.
    /// </summary>
    void Awake()
    {
        Instance = this;
        ConfigurarRutasFirebase();
    }

    /// <summary>
    /// Arranca la descarga del perfil del jugador.
    /// </summary>
    void Start()
    {
        CargarDatosJugador();
    }

    /// <summary>
    /// Lee qué sesión y qué jugador están activos para saber dónde escribir los datos.
    /// </summary>
    private void ConfigurarRutasFirebase()
    {
        int slotIndex;

        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            idUsuario = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        }
        else
        {
            Debug.LogWarning("No se detecta sesión de usuario activa para configurar las rutas.");
        }

        slotIndex = PlayerPrefs.GetInt("CurrentSaveSlot", 0);
        idSaveSlot = "slot_" + slotIndex;
    }

    /// <summary>
    /// Obtiene las estadísticas guardadas y actualiza la pantalla del nivel y monedas.
    /// </summary>
    private void CargarDatosJugador()
    {
        FirebaseFirestore db;
        DocumentReference docRef;

        if (string.IsNullOrEmpty(idUsuario))
        {
            Debug.LogWarning("No se puede cargar el jugador sin un identificador válido.");
        }
        else
        {
            db = FirebaseFirestore.DefaultInstance;
            docRef = db.Collection("users")
                .Document(idUsuario)
                .Collection("save_slots")
                .Document(idSaveSlot);

            docRef
                .GetSnapshotAsync()
                .ContinueWithOnMainThread(task =>
                {
                    if (task.IsCompleted && task.Result.Exists)
                    {
                        DocumentSnapshot snap = task.Result;
                        Dictionary<string, object> pecesDB;
                        Dictionary<string, object> gatosDB;

                        if (snap.ContainsField("lvl"))
                            nivelActual = snap.GetValue<int>("lvl");
                        if (snap.ContainsField("xp"))
                            experienciaActual = snap.GetValue<int>("xp");
                        if (snap.ContainsField("money"))
                            dineroActual = snap.GetValue<int>("money");
                        if (snap.ContainsField("church_streak"))
                            rachaIglesia = snap.GetValue<int>("church_streak");

                        if (snap.ContainsField("achieved_fish"))
                        {
                            pecesDB = snap.GetValue<Dictionary<string, object>>("achieved_fish");
                            foreach (KeyValuePair<string, object> pez in pecesDB)
                            {
                                pecesCapturados[pez.Key] = Convert.ToInt32(pez.Value);
                            }
                        }

                        if (snap.ContainsField("met_cats"))
                        {
                            gatosDB = snap.GetValue<Dictionary<string, object>>("met_cats");
                            foreach (KeyValuePair<string, object> gato in gatosDB)
                            {
                                afinidadGatos[gato.Key] = Convert.ToInt32(gato.Value);
                            }
                        }

                        ValidarRachaDiaria(snap);

                        PlayerPrefs.SetInt("Lvl", nivelActual);
                        PlayerPrefs.SetInt("CurrentLevel", nivelActual);
                        PlayerPrefs.Save();

                        CalcularExperienciaNecesaria();
                        ActualizarInterfaz();
                    }
                    else
                    {
                        Debug.LogWarning("No se encontraron datos en la nube para este jugador.");
                    }
                });
        }
    }

    /// <summary>
    /// Analiza el reloj global para saber si han pasado 24 horas y si se mantiene la bonificación de la iglesia.
    /// </summary>
    private void ValidarRachaDiaria(DocumentSnapshot snap)
    {
        Timestamp ultimaConexion;
        DateTime fechaUltima;
        DateTime fechaHoy;
        double diasDiferencia;

        if (snap.ContainsField("church_last_pray"))
        {
            ultimaConexion = snap.GetValue<Timestamp>("church_last_pray");
            fechaUltima = ultimaConexion.ToDateTime().Date;
            fechaHoy = DateTime.UtcNow.Date;

            diasDiferencia = (fechaHoy - fechaUltima).TotalDays;

            if (diasDiferencia == 0)
            {
                haRezadoHoy = true;
            }
            else if (diasDiferencia == 1)
            {
                haRezadoHoy = false;
            }
            else
            {
                haRezadoHoy = false;
                rachaIglesia = 0;
            }
        }
        else
        {
            haRezadoHoy = false;
        }

        GuardarDatosJugador();
    }

    /// <summary>
    /// Suma un día al contador de días seguidos jugando y registra la hora en la nube.
    /// </summary>
    public void RealizarRezoDiario()
    {
        FirebaseFirestore db;
        DocumentReference docRef;

        if (haRezadoHoy || string.IsNullOrEmpty(idUsuario))
        {
            Debug.LogWarning("No es posible rezar otra vez hoy o no hay usuario conectado.");
        }
        else
        {
            haRezadoHoy = true;
            rachaIglesia++;

            db = FirebaseFirestore.DefaultInstance;
            docRef = db.Collection("users")
                .Document(idUsuario)
                .Collection("save_slots")
                .Document(idSaveSlot);
            docRef.UpdateAsync("church_last_pray", FieldValue.ServerTimestamp);

            GuardarDatosJugador();

            if (UIManager.Instance != null)
            {
                UIManager.Instance.MostrarTooltipTemporal(
                    $"¡Fe renovada! Racha actual: {rachaIglesia} días",
                    4f
                );
            }
        }
    }

    /// <summary>
    /// Anota un pez nuevo en la colección para saber la cantidad obtenida total.
    /// </summary>
    public void RegistrarPezCapturado(string pezID)
    {
        if (pecesCapturados.ContainsKey(pezID))
        {
            pecesCapturados[pezID]++;
        }
        else
        {
            pecesCapturados[pezID] = 1;
        }
        GuardarDatosJugador();
    }

    /// <summary>
    /// Fija la nueva cantidad de puntos de amistad conseguida con un gato concreto.
    /// </summary>
    public void ActualizarAfinidadGato(string gatoID, int nuevaAfinidad)
    {
        afinidadGatos[gatoID] = nuevaAfinidad;
        GuardarDatosJugador();
    }

    /// <summary>
    /// Modifica el bolsillo del jugador y avisa a los textos visuales de la pantalla.
    /// </summary>
    public void AnadirDinero(int cantidad)
    {
        dineroActual += cantidad;
        ActualizarInterfaz();
        GuardarDatosJugador();
    }

    /// <summary>
    /// Calcula si el nivel debe subir basándose en los puntos recibidos.
    /// </summary>
    public void AnadirExperiencia(int cantidad)
    {
        experienciaActual += Mathf.RoundToInt(cantidad * bufoGananciaXP);

        while (experienciaActual >= experienciaNecesaria)
        {
            experienciaActual -= experienciaNecesaria;
            nivelActual++;
            CalcularExperienciaNecesaria();

            PlayerPrefs.SetInt("CurrentLevel", nivelActual);
            PlayerPrefs.Save();

            if (UIManager.Instance != null)
            {
                UIManager.Instance.MostrarSubidaNivelGlobal(nivelActual);
            }
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.ReproducirSubirNivel();
            }
        }

        ActualizarInterfaz();
        GuardarDatosJugador();
    }

    /// <summary>
    /// Comprueba de qué tipo es el beneficio y modifica temporal o permanentemente el multiplicador interno.
    /// </summary>
    public void ActivarServicio(ServiceData servicio, bool esTemporal = true)
    {
        float duracionSegundos;

        duracionSegundos = 180f;

        switch (servicio.objetivoEfecto)
        {
            case "rare_spawn_multiplier":
                bufoRarezaPesca = servicio.valorEfecto;
                if (esTemporal)
                    StartCoroutine(
                        TemporizadorBufo(
                            "rare_spawn_multiplier",
                            1f,
                            duracionSegundos,
                            servicio.nombreDisplay
                        )
                    );
                break;
            case "xp_gain_multiplier":
                bufoGananciaXP = servicio.valorEfecto;
                if (esTemporal)
                    StartCoroutine(
                        TemporizadorBufo(
                            "xp_gain_multiplier",
                            1f,
                            duracionSegundos,
                            servicio.nombreDisplay
                        )
                    );
                break;
            case "affinity_gain_multiplier":
                bufoAfinidad = servicio.valorEfecto;
                if (esTemporal)
                    StartCoroutine(
                        TemporizadorBufo(
                            "affinity_gain_multiplier",
                            1f,
                            duracionSegundos,
                            servicio.nombreDisplay
                        )
                    );
                break;
            case "sell_price_multiplier":
                bufoPrecioVenta = servicio.valorEfecto;
                if (esTemporal)
                    StartCoroutine(
                        TemporizadorBufo(
                            "sell_price_multiplier",
                            1f,
                            duracionSegundos,
                            servicio.nombreDisplay
                        )
                    );
                break;
            case "minigame_difficulty":
                bufoReduccionDificultad = servicio.valorEfecto;
                if (esTemporal)
                    StartCoroutine(
                        TemporizadorBufo(
                            "minigame_difficulty",
                            0f,
                            duracionSegundos,
                            servicio.nombreDisplay
                        )
                    );
                break;
            case "double_catch_chance":
                bufoProbabilidadDoble = servicio.valorEfecto;
                if (esTemporal)
                    StartCoroutine(
                        TemporizadorBufo(
                            "double_catch_chance",
                            0f,
                            duracionSegundos,
                            servicio.nombreDisplay
                        )
                    );
                break;
            default:
                Debug.LogWarning("El servicio recibido no tiene un objetivo válido configurado.");
                break;
        }
    }

    /// <summary>
    /// Cuenta el tiempo en segundo plano y anula el efecto de la taberna al terminar.
    /// </summary>
    private IEnumerator TemporizadorBufo(
        string tipoBufo,
        float valorBase,
        float tiempo,
        string nombreServicio
    )
    {
        yield return new WaitForSeconds(tiempo);

        switch (tipoBufo)
        {
            case "rare_spawn_multiplier":
                bufoRarezaPesca = valorBase;
                break;
            case "xp_gain_multiplier":
                bufoGananciaXP = valorBase;
                break;
            case "affinity_gain_multiplier":
                bufoAfinidad = valorBase;
                break;
            case "sell_price_multiplier":
                bufoPrecioVenta = valorBase;
                break;
            case "minigame_difficulty":
                bufoReduccionDificultad = valorBase;
                break;
            case "double_catch_chance":
                bufoProbabilidadDoble = valorBase;
                break;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.MostrarTooltipTemporal(
                $"El efecto temporal de {nombreServicio} se ha agotado.",
                4f
            );
        }
    }

    /// <summary>
    /// Define cuánto cuesta pasar al siguiente número basándose en la multiplicación básica.
    /// </summary>
    private void CalcularExperienciaNecesaria()
    {
        experienciaNecesaria = Mathf.RoundToInt(
            baseXP * Mathf.Pow(multiplicadorXP, nivelActual - 1)
        );
    }

    /// <summary>
    /// Inserta los números almacenados en las zonas de texto superior de la pantalla.
    /// </summary>
    private void ActualizarInterfaz()
    {
        if (textoNivel != null)
        {
            textoNivel.text =
                "Lvl " + nivelActual.ToString() + " | XP " + experienciaActual.ToString();
        }

        if (textoDinero != null)
        {
            textoDinero.text = dineroActual.ToString();
        }

        if (barraExperiencia != null)
        {
            barraExperiencia.maxValue = experienciaNecesaria;
            barraExperiencia.value = experienciaActual;
        }
    }

    /// <summary>
    /// Devuelve las monedas actuales para que las tiendas sepan si te pueden cobrar.
    /// </summary>
    public int ObtenerDinero()
    {
        return dineroActual;
    }

    /// <summary>
    /// Compacta todo el progreso global de la partida y lo inyecta en Firestore.
    /// </summary>
    public void GuardarDatosJugador()
    {
        FirebaseFirestore db;
        DocumentReference docRef;
        Dictionary<string, object> datos;

        if (string.IsNullOrEmpty(idUsuario))
        {
            Debug.LogWarning("No se puede guardar porque no hay un jugador asignado.");
        }
        else
        {
            db = FirebaseFirestore.DefaultInstance;
            docRef = db.Collection("users")
                .Document(idUsuario)
                .Collection("save_slots")
                .Document(idSaveSlot);

            datos = new Dictionary<string, object>
            {
                { "lvl", nivelActual },
                { "xp", experienciaActual },
                { "money", dineroActual },
                { "church_streak", rachaIglesia },
                { "achieved_fish", pecesCapturados },
                { "met_cats", afinidadGatos },
            };

            docRef.SetAsync(datos, SetOptions.MergeAll);
        }
    }
}
