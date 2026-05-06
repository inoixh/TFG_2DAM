using System;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        if (auth.CurrentUser != null)
        {
            LoadSettingsFromDatabase();
        }
    }

    /// <summary>
    /// Abre el panel de opciones ocultando el menú principal o de pausa si existen.
    /// </summary>
    public void OpenSettingsPanel()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
        if (UIManager.Instance != null && UIManager.Instance.panelPausa != null)
            if (settingsPanel != null)
                settingsPanel.SetActive(true);

        LoadSettingsFromDatabase();
    }

    /// <summary>
    /// Guarda los ajustes, cierra el panel y vuelve al menú donde te encontrabas.
    /// </summary>
    public void CloseSettingsPanel()
    {
        SaveSettingsToDatabase();

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
        else if (UIManager.Instance != null && UIManager.Instance.panelPausa != null)
            UIManager.Instance.panelPausa.SetActive(true);
    }

    /// <summary>
    /// Descarga la configuración de Firestore y la aplica a los sistemas del juego.
    /// </summary>
    public void LoadSettingsFromDatabase()
    {
        if (auth.CurrentUser == null)
            return;

        string uid = auth.CurrentUser.UserId;
        db.Collection("users")
            .Document(uid)
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && task.Result.Exists)
                {
                    Dictionary<string, object> settings = task.Result.GetValue<
                        Dictionary<string, object>
                    >("settings");

                    if (settings != null)
                    {
                        UpdateUIValues(settings);
                        ApplySettingsToGame();
                    }
                }
            });
    }

    /// <summary>
    /// Actualiza los elementos visuales con los datos descargados.
    /// </summary>
    private void UpdateUIValues(Dictionary<string, object> settings)
    {
        if (settings.ContainsKey("master_volume"))
            masterSlider.value = Convert.ToSingle(settings["master_volume"]);
        if (settings.ContainsKey("music_volume"))
            musicSlider.value = Convert.ToSingle(settings["music_volume"]);
        if (settings.ContainsKey("sfx_volume"))
            sfxSlider.value = Convert.ToSingle(settings["sfx_volume"]);
        if (settings.ContainsKey("mouse_sens"))
            mouseSensSlider.value = Convert.ToSingle(settings["mouse_sens"]);
        if (settings.ContainsKey("invert_y"))
            invertYToggle.isOn = Convert.ToBoolean(settings["invert_y"]);
        if (settings.ContainsKey("fullscreen"))
            fullscreenToggle.isOn = Convert.ToBoolean(settings["fullscreen"]);
        if (settings.ContainsKey("graphics_quality"))
            graphicsDropdown.value = Convert.ToInt32(settings["graphics_quality"]);
    }

    /// <summary>
    /// Conecta los valores de la configuración con los motores de Unity y otros Managers.
    /// </summary>
    public void ApplySettingsToGame()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.volumenMusica = musicSlider.value * 0.01f;
            SoundManager.Instance.volumenEfectos = sfxSlider.value * 0.01f;
        }

        CamaraMovement cam =
            Camera.main != null ? Camera.main.GetComponent<CamaraMovement>() : null;
        if (cam != null)
        {
            cam.velocidad = mouseSensSlider.value * 10f;
        }

        QualitySettings.SetQualityLevel(graphicsDropdown.value);
        Screen.fullScreen = fullscreenToggle.isOn;
    }

    /// <summary>
    /// Sincroniza los valores actuales de la UI con la base de datos en la nube.
    /// </summary>
    private void SaveSettingsToDatabase()
    {
        if (auth.CurrentUser == null)
            return;

        Dictionary<string, object> updatedSettings = new Dictionary<string, object>
        {
            { "master_volume", masterSlider.value },
            { "music_volume", musicSlider.value },
            { "sfx_volume", sfxSlider.value },
            { "mouse_sens", mouseSensSlider.value },
            { "invert_y", invertYToggle.isOn },
            { "fullscreen", fullscreenToggle.isOn },
            { "graphics_quality", graphicsDropdown.value },
        };

        db.Collection("users")
            .Document(auth.CurrentUser.UserId)
            .UpdateAsync("settings", updatedSettings);
        ApplySettingsToGame();
    }
}
