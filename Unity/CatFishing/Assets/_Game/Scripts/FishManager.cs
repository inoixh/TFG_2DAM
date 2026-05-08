using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;

/// <summary>
/// Prepara la base de datos de los peces disponibles descargando su información desde la nube.
/// </summary>
public class FishManager : MonoBehaviour
{
    public static FishManager Instance;

    [Header("Modelos 3D")]
    public GameObject prefabBolsaComun;
    public GameObject prefabBolsaRara;
    public GameObject prefabBolsaEspecial;
    public GameObject prefabBolsaEpica;
    public GameObject prefabBolsaLegendaria;

    [Header("Icono por Defecto")]
    public Sprite iconoGenerico;

    [HideInInspector]
    public List<ItemData> todosLosPeces = new List<ItemData>();

    [HideInInspector]
    public bool pecesCargados = false;

    /// <summary>
    /// Asegura que solo exista un gestor de peces en toda la partida.
    /// </summary>
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Pide la descarga de datos al encender el juego.
    /// </summary>
    void Start()
    {
        CargarPecesDesdeFirebase();
    }

    /// <summary>
    /// Obtiene cada pez guardado en internet y genera su molde para el inventario.
    /// </summary>
    private async void CargarPecesDesdeFirebase()
    {
        FirebaseFirestore db;
        CollectionReference pecesRef;
        QuerySnapshot snapshot;
        Dictionary<string, object> pezData;
        ItemData nuevoPez;
        string rarezaStr;
        Sprite spriteUnico;

        db = FirebaseFirestore.DefaultInstance;
        pecesRef = db.Collection("fish");

        try
        {
            snapshot = await pecesRef.GetSnapshotAsync();

            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                pezData = doc.ToDictionary();
                nuevoPez = ScriptableObject.CreateInstance<ItemData>();
                nuevoPez.ID = doc.Id;

                if (pezData.ContainsKey("name"))
                {
                    nuevoPez.nombreDisplay = pezData["name"].ToString();
                }

                if (pezData.ContainsKey("description"))
                {
                    nuevoPez.descripcion = pezData["description"].ToString();
                }

                if (pezData.ContainsKey("price"))
                {
                    nuevoPez.precioVenta = System.Convert.ToInt32(pezData["price"]);
                }

                if (pezData.ContainsKey("rarity"))
                {
                    rarezaStr = pezData["rarity"].ToString().ToLower();
                }
                else
                {
                    rarezaStr = "común";
                }

                AsignarRarezaYModelo(nuevoPez, rarezaStr);

                spriteUnico = Resources.Load<Sprite>("FishIcons/" + nuevoPez.ID);

                if (spriteUnico != null)
                {
                    nuevoPez.icono = spriteUnico;
                }
                else
                {
                    nuevoPez.icono = iconoGenerico;
                }

                todosLosPeces.Add(nuevoPez);
            }

            pecesCargados = true;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(
                "Error al intentar obtener los peces desde la base de datos: " + e.Message
            );
        }
    }

    /// <summary>
    /// Ajusta la bolsa visual y la dificultad del pez según el valor de texto leído.
    /// </summary>
    private void AsignarRarezaYModelo(ItemData pez, string rarezaStr)
    {
        switch (rarezaStr)
        {
            case "común":
            case "comun":
                pez.rareza = Rareza.Comun;
                pez.modelo3D = prefabBolsaComun;
                pez.dificultadMovimiento = 0.5f;
                break;
            case "raro":
                pez.rareza = Rareza.Raro;
                pez.modelo3D = prefabBolsaRara;
                pez.dificultadMovimiento = 1f;
                break;
            case "especial":
                pez.rareza = Rareza.Especial;
                pez.modelo3D = prefabBolsaEspecial;
                pez.dificultadMovimiento = 1.5f;
                break;
            case "épico":
            case "epico":
                pez.rareza = Rareza.Epico;
                pez.modelo3D = prefabBolsaEpica;
                pez.dificultadMovimiento = 2f;
                break;
            case "legendario":
                pez.rareza = Rareza.Legendario;
                pez.modelo3D = prefabBolsaLegendaria;
                pez.dificultadMovimiento = 2.5f;
                break;
            default:
                pez.rareza = Rareza.Comun;
                pez.modelo3D = prefabBolsaComun;
                pez.dificultadMovimiento = 0.5f;
                Debug.LogWarning("El pez tiene una rareza no reconocida, se asignó como Común.");
                break;
        }
    }
}
