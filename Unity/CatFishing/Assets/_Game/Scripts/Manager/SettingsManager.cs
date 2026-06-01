using System;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestiona la configuración gráfica, sonora y de controles del jugador, persistiendo los datos.
/// [Interacción BD]
/// [Relaciones: CamaraMovement, SoundManager]
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

    private FirebaseFirestore db;
    private FirebaseAuth auth;
    private CamaraMovement cam;

    /// <summary>
    /// Configura las referencias y carga la configuración si el usuario está autenticado.
    /// </summary>
    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
        cam = FindFirstObjectByType<CamaraMovement>();

        ConfigurarEventosEnTiempoReal();

        if (auth.CurrentUser != null)
        {
            LoadSettingsFromDatabase();
        }
        else
        {
            Debug.LogWarning("No se encontraron ajustes porque no hay usuario registrado.");
        }
    }

    /// <summary>
    /// Cierra el panel de configuración si se presiona la tecla Escape.
    /// </summary>
    void Update()
    {
        if (
            settingsPanel != null
            && settingsPanel.activeSelf
            && UnityEngine.InputSystem.Keyboard.current != null
            && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame
        )
        {
            CloseSettingsPanel();
        }
    }

    /// <summary>
    /// Asigna métodos a los eventos de cambio de valor en la interfaz gráfica.
    /// </summary>
    private void ConfigurarEventosEnTiempoReal()
    {
        if (masterSlider != null)
            masterSlider.onValueChanged.AddListener(
                delegate
                {
                    ApplySettingsToGame();
                }
            );
        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(
                delegate
                {
                    ApplySettingsToGame();
                }
            );
        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(
                delegate
                {
                    ApplySettingsToGame();
                }
            );
        if (mouseSensSlider != null)
            mouseSensSlider.onValueChanged.AddListener(
                delegate
                {
                    ApplySettingsToGame();
                }
            );
        if (invertYToggle != null)
            invertYToggle.onValueChanged.AddListener(
                delegate
                {
                    ApplySettingsToGame();
                }
            );
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(
                delegate
                {
                    ApplySettingsToGame();
                }
            );
        if (graphicsDropdown != null)
            graphicsDropdown.onValueChanged.AddListener(
                delegate
                {
                    ApplySettingsToGame();
                }
            );
    }

    /// <summary>
    /// Abre el menú de opciones visualmente y recarga los valores desde la base de datos.
    /// </summary>
    public void OpenSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
        LoadSettingsFromDatabase();
    }

    /// <summary>
    /// Guarda los ajustes modificados en la nube y oculta el menú.
    /// </summary>
    public void CloseSettingsPanel()
    {
        SaveSettingsToDatabase();

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    /// <summary>
    /// [BD] Descarga los parámetros de configuración de la cuenta del usuario.
    /// </summary>
    public void LoadSettingsFromDatabase()
    {
        string uid;

        if (auth.CurrentUser != null)
        {
            uid = auth.CurrentUser.UserId;
            db.Collection("users")
                .Document(uid)
                .GetSnapshotAsync()
                .ContinueWithOnMainThread(task =>
                {
                    Dictionary<string, object> settings;

                    if (task.IsCompleted && task.Result.Exists)
                    {
                        settings = task.Result.GetValue<Dictionary<string, object>>("settings");
                        if (settings != null)
                        {
                            UpdateUIValues(settings);
                            ApplySettingsToGame();
                        }
                    }
                });
        }
        else
        {
            Debug.LogWarning("No se puede consultar la nube si no hay usuario.");
        }
    }

    /// <summary>
    /// Ajusta los componentes visuales de la interfaz con los datos recuperados.
    /// </summary>
    private void UpdateUIValues(Dictionary<string, object> settings)
    {
        if (settings.ContainsKey("master_volume") && masterSlider != null)
            masterSlider.value = Convert.ToSingle(settings["master_volume"]);
        if (settings.ContainsKey("music_volume") && musicSlider != null)
            musicSlider.value = Convert.ToSingle(settings["music_volume"]);
        if (settings.ContainsKey("sfx_volume") && sfxSlider != null)
            sfxSlider.value = Convert.ToSingle(settings["sfx_volume"]);
        if (settings.ContainsKey("mouse_sens") && mouseSensSlider != null)
            mouseSensSlider.value = Convert.ToSingle(settings["mouse_sens"]);
        if (settings.ContainsKey("invert_y") && invertYToggle != null)
            invertYToggle.isOn = Convert.ToBoolean(settings["invert_y"]);
        if (settings.ContainsKey("fullscreen") && fullscreenToggle != null)
            fullscreenToggle.isOn = Convert.ToBoolean(settings["fullscreen"]);
        if (settings.ContainsKey("graphics_quality") && graphicsDropdown != null)
            graphicsDropdown.value = Convert.ToInt32(settings["graphics_quality"]);
    }

    /// <summary>
    /// Aplica los valores actuales de la interfaz a los sistemas de juego en ejecución.
    /// </summary>
    public void ApplySettingsToGame()
    {
        if (masterSlider != null)
        {
            AudioListener.volume = masterSlider.value;
        }

        if (SoundManager.Instance != null)
        {
            if (musicSlider != null)
                SoundManager.Instance.volumenMusica = musicSlider.value;
            if (sfxSlider != null)
                SoundManager.Instance.volumenEfectos = sfxSlider.value;
        }

        if (cam != null)
        {
            if (mouseSensSlider != null)
                cam.velocidad = mouseSensSlider.value;
            if (invertYToggle != null)
                cam.invertirY = invertYToggle.isOn;
        }

        if (graphicsDropdown != null)
        {
            QualitySettings.SetQualityLevel(graphicsDropdown.value);
        }

        if (fullscreenToggle != null)
        {
            Screen.fullScreen = fullscreenToggle.isOn;
        }
    }

    /// <summary>
    /// [BD] Sobrescribe el documento del jugador en Firestore con sus preferencias actuales.
    /// </summary>
    private void SaveSettingsToDatabase()
    {
        Dictionary<string, object> updatedSettings;

        if (auth.CurrentUser != null)
        {
            updatedSettings = new Dictionary<string, object>
            {
                { "master_volume", masterSlider != null ? masterSlider.value : 1f },
                { "music_volume", musicSlider != null ? musicSlider.value : 1f },
                { "sfx_volume", sfxSlider != null ? sfxSlider.value : 1f },
                { "mouse_sens", mouseSensSlider != null ? mouseSensSlider.value : 1f },
                { "invert_y", invertYToggle != null ? invertYToggle.isOn : false },
                { "fullscreen", fullscreenToggle != null ? fullscreenToggle.isOn : true },
                { "graphics_quality", graphicsDropdown != null ? graphicsDropdown.value : 2 },
            };

            db.Collection("users")
                .Document(auth.CurrentUser.UserId)
                .UpdateAsync("settings", updatedSettings);
        }
        else
        {
            Debug.LogWarning("Se intentó guardar pero el jugador se desconectó a medias.");
        }
    }
}
