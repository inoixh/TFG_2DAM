using System.Collections.Generic;
using System.Text.RegularExpressions;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gestiona la autenticación de usuarios, registro y conexión inicial a la base de datos.
/// [Interacción BD]
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
    /// Inicializa los servicios de autenticación y comprueba si existe una sesión activa.
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
    /// Muestra el panel de inicio de sesión y oculta los demás.
    /// </summary>
    public void ShowLoginPanel()
    {
        loginPanel.SetActive(true);
        registerPanel.SetActive(false);
        loginFeedbackText.text = "";
    }

    /// <summary>
    /// Muestra el panel de registro y oculta los demás.
    /// </summary>
    public void ShowRegisterPanel()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
        regFeedbackText.text = "";
    }

    /// <summary>
    /// Muestra el menú principal y carga el perfil del usuario activo.
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
    /// [BD] Obtiene el nombre de usuario desde Firestore y actualiza la interfaz.
    /// </summary>
    private void FetchUserProfile(string uid)
    {
        loggedInUsernameText.text = "Cargando...";
        db.Collection("users")
            .Document(uid)
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                DocumentSnapshot snap;
                string username;

                if (task.IsFaulted || task.IsCanceled)
                {
                    loggedInUsernameText.text = "@Usuario";
                    Debug.LogWarning(
                        "Error al conectar con la base de datos para obtener el perfil."
                    );
                }
                else
                {
                    snap = task.Result;

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
    /// [BD] Inicia sesión validando el correo electrónico y contraseña.
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
    /// [BD] Crea un nuevo usuario verificando el formato de los datos introducidos.
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
                        FirebaseUser newUser;

                        newUser = task.Result.User;
                        SaveUserDataToFirestore(newUser.UserId, user, email, phone);
                    }
                });
        }
    }

    /// <summary>
    /// [BD] Almacena los datos iniciales y configuraciones del nuevo usuario en Firestore.
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
    /// [BD] Cierra la sesión activa y limpia las credenciales guardadas.
    /// </summary>
    public void OnLogoutButtonClicked()
    {
        auth.SignOut();
        PlayerPrefs.SetInt("RememberMe", 0);
        PlayerPrefs.Save();
        ShowLoginPanel();
    }
}
