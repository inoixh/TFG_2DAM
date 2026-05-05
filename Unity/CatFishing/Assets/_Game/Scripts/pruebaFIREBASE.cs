using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;

/// <summary>
/// Script temporal para verificar que las dependencias de Google y Firestore están operativas.
/// </summary>
public class FirebaseManager : MonoBehaviour
{
    private FirebaseFirestore db;

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result == DependencyStatus.Available)
            {
                Debug.Log("<color=green>¡Firebase conectado correctamente!</color>");
                db = FirebaseFirestore.DefaultInstance;
            }
            else
            {
                Debug.LogError("No se pudo resolver las dependencias de Firebase: " + task.Result);
            }
        });
    }
}