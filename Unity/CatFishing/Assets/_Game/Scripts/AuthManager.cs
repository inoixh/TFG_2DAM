using System.Collections.Generic;
using System.Text.RegularExpressions;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestiona el registro, el inicio de sesión y la conexión del usuario con la base de datos.
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

    /// <summary>
    /// Prepara las herramientas de base de datos y comprueba si hay una sesión guardada al abrir el juego.
    /// </summary>
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
    /// Activa la pantalla para iniciar sesión y oculta las demás.
    /// </summary>
    public void ShowLoginPanel()
    {
        loginPanel.SetActive(true);
        registerPanel.SetActive(false);
        loginFeedbackText.text = "";
    }

    /// <summary>
    /// Activa la pantalla para crear una cuenta nueva y oculta las demás.
    /// </summary>
    public void ShowRegisterPanel()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
        regFeedbackText.text = "";
    }

    /// <summary>
    /// Activa el menú principal del juego tras confirmar los datos del usuario.
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
        else
        {
            Debug.LogWarning("No se puede cargar el perfil porque no hay usuario activo.");
        }
    }

    /// <summary>
    /// Descarga el nombre del usuario desde la base de datos usando su identificador.
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
                    Debug.LogWarning(
                        "Error al conectar con la base de datos para obtener el perfil."
                    );
                }
                else
                {
                    DocumentSnapshot snap = task.Result;
                    string username;

                    if (snap.Exists && snap.TryGetValue("username", out username))
                    {
                        loggedInUsernameText.text = "@" + username;
                    }
                    else
                    {
                        loggedInUsernameText.text = "@Usuario";
                        Debug.LogWarning(
                            "El documento del usuario no existe o no tiene nombre registrado."
                        );
                    }
                }
            });
    }

    /// <summary>
    /// Comprueba que los campos tengan texto e intenta iniciar sesión en la cuenta.
    /// </summary>
    public void OnLoginButtonClicked()
    {
        string email = loginEmailInput.text;
        string password = loginPasswordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            loginFeedbackText.text = "<color=red>Por favor, rellena todos los campos.</color>";
        }
        else
        {
            loginFeedbackText.text = "<color=green>Iniciando sesión...</color>";

            auth.SignInWithEmailAndPasswordAsync(email, password)
                .ContinueWithOnMainThread(task =>
                {
                    if (task.IsCanceled || task.IsFaulted)
                    {
                        loginFeedbackText.text =
                            "<color=red>Error: Email o contraseña incorrectos.</color>";
                        Debug.LogWarning("Fallo en la autenticación del login.");
                    }
                    else
                    {
                        PlayerPrefs.SetInt("RememberMe", rememberMeToggle.isOn ? 1 : 0);
                        PlayerPrefs.Save();
                        ShowMainMenu();
                    }
                });
        }
    }

    /// <summary>
    /// Revisa que los datos cumplan las normas e intenta crear un usuario nuevo.
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
        }
        else if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            regFeedbackText.text = "<color=red>Dirección email no válida.</color>";
        }
        else if (phone.Length != 9)
        {
            regFeedbackText.text =
                "<color=red>El número telefónico debe tener al menos 9 caracteres.</color>";
        }
        else if (
            !Regex.IsMatch(user, @"^[a-zA-Z0-9]+$") || !Regex.IsMatch(password, @"^[a-zA-Z0-9]+$")
        )
        {
            regFeedbackText.text =
                "<color=red>El nombre de usuario y contraseña solo pueden contener letras y números.</color>";
        }
        else if (password.Length < 6)
        {
            regFeedbackText.text =
                "<color=red>La contraseña debe ser de al menos 6 caracteres.</color>";
        }
        else
        {
            regFeedbackText.text = "<color=green>Creando cuenta...</color>";

            auth.CreateUserWithEmailAndPasswordAsync(email, password)
                .ContinueWithOnMainThread(task =>
                {
                    if (task.IsCanceled || task.IsFaulted)
                    {
                        regFeedbackText.text = "<color=red>Error al crear la cuenta.</color>";
                        Debug.LogWarning("Fallo al registrar al nuevo usuario.");
                    }
                    else
                    {
                        FirebaseUser newUser = task.Result.User;
                        SaveUserDataToFirestore(newUser.UserId, user, email, phone);
                    }
                });
        }
    }

    /// <summary>
    /// Crea un documento en la base de datos con la información personal del nuevo usuario.
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
                    Debug.LogWarning("No se pudo escribir el documento en Firestore.");
                }
            });
    }

    /// <summary>
    /// Borra los datos locales y finaliza la sesión activa del jugador.
    /// </summary>
    public void OnLogoutButtonClicked()
    {
        auth.SignOut();
        PlayerPrefs.SetInt("RememberMe", 0);
        PlayerPrefs.Save();
        ShowLoginPanel();
    }
}
