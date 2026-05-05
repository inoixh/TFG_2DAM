using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Gestiona la progresión global, economía, rachas diarias, bufos activos temporales y la colección.
/// Sincroniza automáticamente los datos persistentes con Firestore.
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

    [Header("Modificadores Activos (Bufos)")]
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

    void Awake()
    {
        Instance = this;
        ConfigurarRutasFirebase();
    }

    void Start()
    {
        CargarDatosJugador();
    }

    private void ConfigurarRutasFirebase()
    {
        if (FirebaseAuth.DefaultInstance.CurrentUser != null) idUsuario = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        int slotIndex = PlayerPrefs.GetInt("CurrentSaveSlot", 0);
        idSaveSlot = "slot_" + slotIndex;
    }

    private void CargarDatosJugador()
    {
        if (string.IsNullOrEmpty(idUsuario)) return;

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        DocumentReference docRef = db.Collection("users").Document(idUsuario).Collection("save_slots").Document(idSaveSlot);

        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                DocumentSnapshot snap = task.Result;
                
                if (snap.ContainsField("lvl")) nivelActual = snap.GetValue<int>("lvl");
                if (snap.ContainsField("xp")) experienciaActual = snap.GetValue<int>("xp");
                if (snap.ContainsField("money")) dineroActual = snap.GetValue<int>("money");
                if (snap.ContainsField("church_streak")) rachaIglesia = snap.GetValue<int>("church_streak");

                if (snap.ContainsField("achieved_fish"))
                {
                    Dictionary<string, object> pecesDB = snap.GetValue<Dictionary<string, object>>("achieved_fish");
                    foreach (var pez in pecesDB) pecesCapturados[pez.Key] = Convert.ToInt32(pez.Value);
                }

                if (snap.ContainsField("met_cats"))
                {
                    Dictionary<string, object> gatosDB = snap.GetValue<Dictionary<string, object>>("met_cats");
                    foreach (var gato in gatosDB) afinidadGatos[gato.Key] = Convert.ToInt32(gato.Value);
                }

                ValidarRachaDiaria(snap);

                PlayerPrefs.SetInt("Lvl", nivelActual);
                PlayerPrefs.SetInt("CurrentLevel", nivelActual);
                PlayerPrefs.Save();

                CalcularExperienciaNecesaria();
                ActualizarInterfaz();
            }
        });
    }

    private void ValidarRachaDiaria(DocumentSnapshot snap)
    {
        if (snap.ContainsField("church_last_pray"))
        {
            Timestamp ultimaConexion = snap.GetValue<Timestamp>("church_last_pray");
            DateTime fechaUltima = ultimaConexion.ToDateTime().Date;
            DateTime fechaHoy = DateTime.UtcNow.Date;

            double diasDiferencia = (fechaHoy - fechaUltima).TotalDays;

            if (diasDiferencia == 0) haRezadoHoy = true;
            else if (diasDiferencia == 1) haRezadoHoy = false;
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

    public void RealizarRezoDiario()
    {
        if (haRezadoHoy || string.IsNullOrEmpty(idUsuario)) return;

        haRezadoHoy = true;
        rachaIglesia++;

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        DocumentReference docRef = db.Collection("users").Document(idUsuario).Collection("save_slots").Document(idSaveSlot);
        docRef.UpdateAsync("church_last_pray", FieldValue.ServerTimestamp);

        GuardarDatosJugador();

        if (UIManager.Instance != null) UIManager.Instance.MostrarTooltipTemporal($"¡Fe renovada! Racha actual: {rachaIglesia} días", 4f);
    }

    public void RegistrarPezCapturado(string pezID)
    {
        if (pecesCapturados.ContainsKey(pezID)) pecesCapturados[pezID]++;
        else pecesCapturados[pezID] = 1;
        GuardarDatosJugador();
    }

    public void ActualizarAfinidadGato(string gatoID, int nuevaAfinidad)
    {
        afinidadGatos[gatoID] = nuevaAfinidad;
        GuardarDatosJugador();
    }

    public void AnadirDinero(int cantidad)
    {
        dineroActual += cantidad;
        ActualizarInterfaz();
        GuardarDatosJugador();
    }

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
            
            if (UIManager.Instance != null) UIManager.Instance.MostrarSubidaNivelGlobal(nivelActual);
        }

        ActualizarInterfaz();
        GuardarDatosJugador();
    }

    // =========================================================================
    // SISTEMA DE BUFOS TEMPORALES
    // =========================================================================

    /// <summary>
    /// Aplica el modificador e inicia un temporizador para revertirlo.
    /// </summary>
    public void ActivarServicio(ServiceData servicio)
    {
        float duracionSegundos = 180f; // Todos los consumibles duran 3 minutos (180 seg)

        switch (servicio.objetivoEfecto)
        {
            case "rare_spawn_multiplier": 
                bufoRarezaPesca = servicio.valorEfecto; 
                StartCoroutine(TemporizadorBufo("rare_spawn_multiplier", 1f, duracionSegundos, servicio.nombreDisplay));
                break;
            case "xp_gain_multiplier": 
                bufoGananciaXP = servicio.valorEfecto; 
                StartCoroutine(TemporizadorBufo("xp_gain_multiplier", 1f, duracionSegundos, servicio.nombreDisplay));
                break;
            case "affinity_gain_multiplier": 
                bufoAfinidad = servicio.valorEfecto; 
                StartCoroutine(TemporizadorBufo("affinity_gain_multiplier", 1f, duracionSegundos, servicio.nombreDisplay));
                break;
            case "sell_price_multiplier": 
                bufoPrecioVenta = servicio.valorEfecto; 
                StartCoroutine(TemporizadorBufo("sell_price_multiplier", 1f, duracionSegundos, servicio.nombreDisplay));
                break;
            case "minigame_difficulty": 
                bufoReduccionDificultad = servicio.valorEfecto; 
                StartCoroutine(TemporizadorBufo("minigame_difficulty", 0f, duracionSegundos, servicio.nombreDisplay));
                break;
            case "double_catch_chance": 
                bufoProbabilidadDoble = servicio.valorEfecto; 
                StartCoroutine(TemporizadorBufo("double_catch_chance", 0f, duracionSegundos, servicio.nombreDisplay));
                break;
        }
    }

    /// <summary>
    /// Espera el tiempo establecido y devuelve la estadística a su valor base.
    /// </summary>
    private IEnumerator TemporizadorBufo(string tipoBufo, float valorBase, float tiempo, string nombreServicio)
    {
        yield return new WaitForSeconds(tiempo);

        switch (tipoBufo)
        {
            case "rare_spawn_multiplier": bufoRarezaPesca = valorBase; break;
            case "xp_gain_multiplier": bufoGananciaXP = valorBase; break;
            case "affinity_gain_multiplier": bufoAfinidad = valorBase; break;
            case "sell_price_multiplier": bufoPrecioVenta = valorBase; break;
            case "minigame_difficulty": bufoReduccionDificultad = valorBase; break;
            case "double_catch_chance": bufoProbabilidadDoble = valorBase; break;
        }

        if (UIManager.Instance != null) 
        {
            UIManager.Instance.MostrarTooltipTemporal($"El efecto de {nombreServicio} se ha agotado.", 4f);
        }
    }

    private void CalcularExperienciaNecesaria() { experienciaNecesaria = Mathf.RoundToInt(baseXP * Mathf.Pow(multiplicadorXP, nivelActual - 1)); }

    private void ActualizarInterfaz()
    {
        if (textoNivel != null) textoNivel.text = "Lvl " + nivelActual.ToString() + " | XP " + experienciaActual.ToString();
        if (textoDinero != null) textoDinero.text = dineroActual.ToString();
        if (barraExperiencia != null) { barraExperiencia.maxValue = experienciaNecesaria; barraExperiencia.value = experienciaActual; }
    }

    public int ObtenerDinero() { return dineroActual; }

    public void GuardarDatosJugador()
    {
        if (string.IsNullOrEmpty(idUsuario)) return;

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        DocumentReference docRef = db.Collection("users").Document(idUsuario).Collection("save_slots").Document(idSaveSlot);

        Dictionary<string, object> datos = new Dictionary<string, object>
        {
            { "lvl", nivelActual },
            { "xp", experienciaActual },
            { "money", dineroActual },
            { "church_streak", rachaIglesia },
            { "achieved_fish", pecesCapturados },
            { "met_cats", afinidadGatos }
        };

        docRef.SetAsync(datos, SetOptions.MergeAll);
    }
}