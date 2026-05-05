using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;
using System;

/// <summary>
/// Gestiona la configuración del usuario, su persistencia en Firestore y la aplicación 
/// de los ajustes a los sistemas de audio, control y gráficos del juego.
/// </summary>
public class SettingsManager : MonoBehaviour
{
    [Header("Audio UI")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Controles UI")]
    public Slider mouseSensSlider;
    public Toggle invertYToggle;

    [Header("Gráficos UI")]
    public Toggle fullscreenToggle;
    public TMP_Dropdown graphicsDropdown;

    [Header("Paneles")]
    public GameObject settingsPanel;
    public GameObject mainMenuPanel;

    private FirebaseFirestore db;
    private FirebaseAuth auth;

    private void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;

        // Intentamos cargar los ajustes al iniciar si ya hay un usuario logeado
        if (auth.CurrentUser != null)
        {
            LoadSettingsFromDatabase();
        }
    }

    /// <summary>
    /// Abre el panel de opciones y sincroniza la UI con los datos más recientes de la nube.
    /// </summary>
    public void OpenSettingsPanel()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
        LoadSettingsFromDatabase();
    }

    /// <summary>
    /// Guarda los ajustes, cierra el panel y vuelve al menú principal.
    /// </summary>
    public void CloseSettingsPanel()
    {
        SaveSettingsToDatabase();
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    /// <summary>
    /// Descarga la configuración de Firestore y la aplica a los sistemas del juego.
    /// </summary>
    public void LoadSettingsFromDatabase()
    {
        if (auth.CurrentUser == null) return;

        string uid = auth.CurrentUser.UserId;
        db.Collection("users").Document(uid).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                Dictionary<string, object> settings = task.Result.GetValue<Dictionary<string, object>>("settings");

                if (settings != null)
                {
                    // 1. Actualizar valores en la UI
                    UpdateUIValues(settings);

                    // 2. Aplicar los cambios al juego (Audio, Gráficos, Sensibilidad)
                    ApplySettingsToGame();
                }
            }
        });
    }

    /// <summary>
    /// Actualiza los elementos visuales (sliders, toggles) con los datos descargados.
    /// </summary>
    private void UpdateUIValues(Dictionary<string, object> settings)
    {
        if (settings.ContainsKey("master_volume")) masterSlider.value = Convert.ToSingle(settings["master_volume"]);
        if (settings.ContainsKey("music_volume")) musicSlider.value = Convert.ToSingle(settings["music_volume"]);
        if (settings.ContainsKey("sfx_volume")) sfxSlider.value = Convert.ToSingle(settings["sfx_volume"]);
        if (settings.ContainsKey("mouse_sens")) mouseSensSlider.value = Convert.ToSingle(settings["mouse_sens"]);
        if (settings.ContainsKey("invert_y")) invertYToggle.isOn = Convert.ToBoolean(settings["invert_y"]);
        if (settings.ContainsKey("fullscreen")) fullscreenToggle.isOn = Convert.ToBoolean(settings["fullscreen"]);
        if (settings.ContainsKey("graphics_quality")) graphicsDropdown.value = Convert.ToInt32(settings["graphics_quality"]);
    }

    /// <summary>
    /// Conecta los valores de la configuración con los motores de Unity y otros Managers.
    /// </summary>
    public void ApplySettingsToGame()
    {
        // Aplicar Audio al SoundManager (multiplicamos por 0.01 porque tus sliders van de 0 a 100)
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.volumenMusica = musicSlider.value * 0.01f;
            SoundManager.Instance.volumenEfectos = sfxSlider.value * 0.01f;
            // Aquí podrías añadir un volumen general si tu SoundManager lo soporta
        }

        // Aplicar Sensibilidad (buscamos la cámara en la escena)
        CamaraMovement cam = Camera.main.GetComponent<CamaraMovement>();
        if (cam != null)
        {
            cam.velocidad = mouseSensSlider.value * 10f; // Ajusta el multiplicador según tu gusto
        }

        // Aplicar Gráficos nativos de Unity
        QualitySettings.SetQualityLevel(graphicsDropdown.value);
        Screen.fullScreen = fullscreenToggle.isOn;
    }

    /// <summary>
    /// Sincroniza los valores actuales de la UI con la base de datos en la nube.
    /// </summary>
    private void SaveSettingsToDatabase()
    {
        if (auth.CurrentUser == null) return;

        Dictionary<string, object> updatedSettings = new Dictionary<string, object>
        {
            { "master_volume", masterSlider.value },
            { "music_volume", musicSlider.value },
            { "sfx_volume", sfxSlider.value },
            { "mouse_sens", mouseSensSlider.value },
            { "invert_y", invertYToggle.isOn },
            { "fullscreen", fullscreenToggle.isOn },
            { "graphics_quality", graphicsDropdown.value }
        };

        db.Collection("users").Document(auth.CurrentUser.UserId).UpdateAsync("settings", updatedSettings);
        
        // Aplicamos al momento para que el cambio sea instantáneo al cerrar
        ApplySettingsToGame();
    }
}