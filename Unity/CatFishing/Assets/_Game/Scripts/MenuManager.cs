using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Gestiona la navegación del menú principal, el audio de la interfaz y la lógica
/// de carga y creación de partidas guardadas (Save Slots) conectadas a Firestore.
/// </summary>
public class MenuManager : MonoBehaviour
{
    [Header("Paneles de Navegación")]
    public GameObject mainMenuPanel;
    public GameObject saveSlotsPanel;

    [Header("UI de Partidas Guardadas")]
    public TextMeshProUGUI[] textosSlots;
    public Button[] botonesSlots;

    private FirebaseFirestore db;
    private FirebaseAuth auth;

    private void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
    }

    /// <summary>
    /// Oculta el menú principal, muestra el panel de selección de partidas
    /// y solicita la descarga de datos a la base de datos.
    /// </summary>
    public void OpenSaveSlotsPanel()
    {
        saveSlotsPanel.SetActive(true);
        CargarDatosSlots();
    }

    /// <summary>
    /// Oculta el panel de partidas y vuelve al menú principal.
    /// </summary>
    public void CloseSaveSlotsPanel()
    {
        saveSlotsPanel.SetActive(false);
    }

    /// <summary>
    /// Consulta Firestore para comprobar el estado de los 3 slots de guardado.
    /// Actualiza la UI mostrando los datos de la partida o habilitando la creación de una nueva.
    /// </summary>
    private void CargarDatosSlots()
    {
        if (auth.CurrentUser == null)
            return;
        string uid = auth.CurrentUser.UserId;

        for (int i = 0; i < 3; i++)
        {
            int index = i;
            textosSlots[index].text = "Cargando...";
            botonesSlots[index].interactable = false;

            DocumentReference docRef = db.Collection("users")
                .Document(uid)
                .Collection("save_slots")
                .Document("slot_" + index);

            docRef
                .GetSnapshotAsync()
                .ContinueWithOnMainThread(task =>
                {
                    botonesSlots[index].interactable = true;

                    if (task.IsCompleted && !task.IsFaulted && task.Result.Exists)
                    {
                        DocumentSnapshot snap = task.Result;
                        int nivel = snap.GetValue<int>("lvl");
                        int dinero = snap.GetValue<int>("money");

                        textosSlots[index].text =
                            $"Partida {index + 1}\n<size=80%>Nivel {nivel} - Monedas: {dinero}</size>";

                        botonesSlots[index].onClick.RemoveAllListeners();
                        botonesSlots[index].onClick.AddListener(() => IniciarPartida(index));
                    }
                    else
                    {
                        textosSlots[index].text =
                            $"Partida {index + 1}\n<color=#A8E6CF><size=80%>+ Nueva Partida</size></color>";

                        botonesSlots[index].onClick.RemoveAllListeners();
                        botonesSlots[index].onClick.AddListener(() => CrearNuevaPartida(index));
                    }
                });
        }
    }

    /// <summary>
    /// Crea un nuevo documento en la subcolección save_slots con los valores iniciales
    /// por defecto y automáticamente inicia el juego.
    /// </summary>
    private void CrearNuevaPartida(int slotIndex)
    {
        if (auth.CurrentUser == null)
            return;
        string uid = auth.CurrentUser.UserId;

        botonesSlots[slotIndex].interactable = false;
        textosSlots[slotIndex].text = "Creando mundo...";

        Dictionary<string, object> nuevaPartida = new Dictionary<string, object>
        {
            { "lvl", 1 },
            { "xp", 0 },
            { "money", 0 },
            { "church_streak", 0 },
            { "tavern_lvl", 1 },
            { "last_login", FieldValue.ServerTimestamp },
        };

        db.Collection("users")
            .Document(uid)
            .Collection("save_slots")
            .Document("slot_" + slotIndex)
            .SetAsync(nuevaPartida)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    IniciarPartida(slotIndex);
                }
            });
    }

    /// <summary>
    /// Guarda en las preferencias locales el slot seleccionado, invoca la transición musical y carga la escena del juego.
    /// </summary>
    private void IniciarPartida(int slotIndex)
    {
        PlayerPrefs.SetInt("CurrentSaveSlot", slotIndex);
        PlayerPrefs.Save();

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.ReproducirMusicaJuego();
        }

        changeScene("Game");
    }

    /// <summary>
    /// Carga una nueva escena por su nombre en Unity.
    /// </summary>
    public void changeScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }
}
