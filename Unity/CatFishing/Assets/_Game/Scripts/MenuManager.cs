using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Gestiona las pantallas de inicio y permite al jugador elegir o crear su partida.
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

    /// <summary>
    /// Prepara las herramientas de la base de datos al encender el menú.
    /// </summary>
    private void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
    }

    /// <summary>
    /// Muestra la pantalla para elegir partida y pide los datos a internet.
    /// </summary>
    public void OpenSaveSlotsPanel()
    {
        if (saveSlotsPanel != null)
        {
            saveSlotsPanel.SetActive(true);
            CargarDatosSlots();
        }
        else
        {
            Debug.LogWarning("No se asignó el panel de partidas en el Inspector.");
        }
    }

    /// <summary>
    /// Oculta la pantalla de selección de partida para volver al menú principal.
    /// </summary>
    public void CloseSaveSlotsPanel()
    {
        if (saveSlotsPanel != null)
        {
            saveSlotsPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("No se asignó el panel de partidas en el Inspector.");
        }
    }

    /// <summary>
    /// Descarga el progreso de los tres huecos de guardado y actualiza los botones visuales.
    /// </summary>
    private void CargarDatosSlots()
    {
        string uid;
        DocumentReference docRef;

        if (auth.CurrentUser != null)
        {
            uid = auth.CurrentUser.UserId;

            for (int i = 0; i < 3; i++)
            {
                int index = i;

                textosSlots[index].text = "Cargando...";
                botonesSlots[index].interactable = false;

                docRef = db.Collection("users")
                    .Document(uid)
                    .Collection("save_slots")
                    .Document("slot_" + index);

                docRef
                    .GetSnapshotAsync()
                    .ContinueWithOnMainThread(task =>
                    {
                        DocumentSnapshot snap;
                        int nivel;
                        int dinero;

                        botonesSlots[index].interactable = true;

                        if (task.IsCompleted && !task.IsFaulted && task.Result.Exists)
                        {
                            snap = task.Result;
                            nivel = snap.GetValue<int>("lvl");
                            dinero = snap.GetValue<int>("money");

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
        else
        {
            Debug.LogWarning("No hay un usuario activo para cargar las partidas.");
        }
    }

    /// <summary>
    /// Genera los datos base de un mundo nuevo en internet y entra al juego automáticamente.
    /// </summary>
    private void CrearNuevaPartida(int slotIndex)
    {
        string uid;
        Dictionary<string, object> nuevaPartida;

        if (auth.CurrentUser != null)
        {
            uid = auth.CurrentUser.UserId;

            botonesSlots[slotIndex].interactable = false;
            textosSlots[slotIndex].text = "Creando mundo...";

            nuevaPartida = new Dictionary<string, object>
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
                    else
                    {
                        Debug.LogWarning("Error al crear la nueva partida en la base de datos.");
                        botonesSlots[slotIndex].interactable = true;
                    }
                });
        }
        else
        {
            Debug.LogWarning("No hay un usuario activo para crear una partida.");
        }
    }

    /// <summary>
    /// Guarda el número de partida en la memoria local y arranca la escena principal.
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
    /// Transiciona el juego a la pantalla que se indique por texto.
    /// </summary>
    public void changeScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }
}
