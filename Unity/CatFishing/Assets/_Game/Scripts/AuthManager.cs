using System.Collections.Generic;
using System.Text.RegularExpressions;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestiona la autenticación de usuarios, registro, inicio de sesión y conexión con Firestore.
/// </summary>
public class AuthManager : MonoBehaviour
{
    [Header("Main Menu")]
    public TextMeshProUGUI loggedInUsernameText;

    [Header("Panels")]
    public GameObject loginPanel;
    public GameObject registerPanel;
    public GameObject mainMenuPanel;

    [Header("Login")]
    public TMP_InputField loginEmailInput;
    public TMP_InputField loginPasswordInput;
    public Toggle rememberMeToggle;
    public TextMeshProUGUI loginFeedbackText;

    [Header("Register")]
    public TMP_InputField regUsernameInput;
    public TMP_InputField regEmailInput;
    public TMP_InputField regNumberInput;
    public TMP_InputField regPasswordInput;
    public TextMeshProUGUI regFeedbackText;

    private FirebaseAuth auth;
    private FirebaseFirestore db;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;

        if (auth.CurrentUser != null && PlayerPrefs.GetInt("RememberMe", 0) == 1)
        {
            ShowMainMenu();
        }
        else
        {
            auth.SignOut();
            ShowLoginPanel();
        }
    }

    /// <summary>
    /// Muestra el panel de inicio de sesión y oculta el resto.
    /// </summary>
    public void ShowLoginPanel()
    {
        loginPanel.SetActive(true);
        registerPanel.SetActive(false);
        loginFeedbackText.text = "";
    }

    /// <summary>
    /// Muestra el panel de registro de nueva cuenta y oculta el resto.
    /// </summary>
    public void ShowRegisterPanel()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
        regFeedbackText.text = "";
    }

    /// <summary>
    /// Muestra el menú principal del juego tras una autenticación exitosa.
    /// </summary>
    public void ShowMainMenu()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(false);
        mainMenuPanel.SetActive(true);

        if (auth.CurrentUser != null)
        {
            FetchUserProfile(auth.CurrentUser.UserId);
        }
    }

    /// <summary>
    /// Descarga y muestra el nombre de usuario desde Firestore usando su UID.
    /// Incluye comprobación segura para evitar quedarse congelado en 'Loading...'.
    /// </summary>
    private void FetchUserProfile(string uid)
    {
        loggedInUsernameText.text = "Cargando...";

        db.Collection("users")
            .Document(uid)
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    loggedInUsernameText.text = "@Usuario";
                    return;
                }

                DocumentSnapshot snap = task.Result;
                if (snap.Exists && snap.TryGetValue("username", out string username))
                {
                    loggedInUsernameText.text = "@" + username;
                }
                else
                {
                    loggedInUsernameText.text = "@Usuario";
                }
            });
    }

    /// <summary>
    /// Valida los campos e intenta iniciar sesión con Firebase Auth.
    /// </summary>
    public void OnLoginButtonClicked()
    {
        string email = loginEmailInput.text;
        string password = loginPasswordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            loginFeedbackText.text = "<color=red>Por favor, rellena todos los campos.</color>";
            return;
        }

        loginFeedbackText.text = "<color=green>Iniciando sesión...</color>";

        auth.SignInWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    loginFeedbackText.text =
                        "<color=red>Error: Email o contraseña incorrectos.</color>";
                    return;
                }

                PlayerPrefs.SetInt("RememberMe", rememberMeToggle.isOn ? 1 : 0);
                PlayerPrefs.Save();
                ShowMainMenu();
            });
    }

    /// <summary>
    /// Valida el formato de los datos introducidos y registra un nuevo usuario en Firebase.
    /// </summary>
    public void OnRegisterButtonClicked()
    {
        string user = regUsernameInput.text;
        string email = regEmailInput.text;
        string phone = regNumberInput.text;
        string password = regPasswordInput.text;

        if (
            string.IsNullOrEmpty(user)
            || string.IsNullOrEmpty(email)
            || string.IsNullOrEmpty(phone)
            || string.IsNullOrEmpty(password)
        )
        {
            regFeedbackText.text = "<color=red>Por favor, rellena todos los campos.</color>";
            return;
        }

        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            regFeedbackText.text = "<color=red>Dirección email no válida.</color>";
            return;
        }

        if (phone.Length != 9)
        {
            regFeedbackText.text =
                "<color=red>El número telefónico debe tener al menos 9 caracteres.</color>";
            return;
        }

        if (!Regex.IsMatch(user, @"^[a-zA-Z0-9]+$") || !Regex.IsMatch(password, @"^[a-zA-Z0-9]+$"))
        {
            regFeedbackText.text =
                "<color=red>El nombre de usuario y contraseña solo pueden contener letras y números.</color>";
            return;
        }

        if (password.Length < 6)
        {
            regFeedbackText.text =
                "<color=red>La contraseña debe ser de al menos 6 caracteres.</color>";
            return;
        }

        regFeedbackText.text = "<color=green>Creando cuenta...</color>";

        auth.CreateUserWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    regFeedbackText.text = "<color=red>Error al crear la cuenta.</color>";
                    return;
                }

                FirebaseUser newUser = task.Result.User;
                SaveUserDataToFirestore(newUser.UserId, user, email, phone);
            });
    }

    /// <summary>
    /// Guarda los datos adicionales del usuario recién registrado en Firestore.
    /// </summary>
    private void SaveUserDataToFirestore(string uid, string username, string email, string phone)
    {
        Dictionary<string, object> defaultSettings = new Dictionary<string, object>
        {
            { "master_volume", 1.0f },
            { "music_volume", 1.0f },
            { "sfx_volume", 1.0f },
        };

        Dictionary<string, object> userData = new Dictionary<string, object>
        {
            { "uid", uid },
            { "username", username },
            { "email", email },
            { "number", phone },
            { "signup_date", FieldValue.ServerTimestamp },
            { "settings", defaultSettings },
        };

        db.Collection("users")
            .Document(uid)
            .SetAsync(userData)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    PlayerPrefs.SetInt("RememberMe", 1);
                    PlayerPrefs.Save();
                    ShowMainMenu();
                }
                else
                {
                    regFeedbackText.text = "<color=red>Error al guardar los datos.</color>";
                }
            });
    }

    /// <summary>
    /// Cierra la sesión activa en Firebase y redirige al panel de login.
    /// </summary>
    public void OnLogoutButtonClicked()
    {
        auth.SignOut();
        PlayerPrefs.SetInt("RememberMe", 0);
        PlayerPrefs.Save();
        ShowLoginPanel();
    }
}
