using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;

/// <summary>
/// Mantiene la lista global de peces del juego descargada desde la base de datos.
/// [Interacción BD]
/// [Relaciones: ItemData]
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

    private FirebaseFirestore db;

    /// <summary>
    /// Inicializa el Singleton del gestor de peces.
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
    /// Inicia el proceso de carga de datos desde Firestore.
    /// </summary>
    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        CargarPecesDesdeFirebase();
    }

    /// <summary>
    /// [BD] Obtiene la información de cada pez y genera su objeto de datos.
    /// </summary>
    private async void CargarPecesDesdeFirebase()
    {
        CollectionReference pecesRef;
        QuerySnapshot snapshot;
        Dictionary<string, object> pezData;
        ItemData nuevoPez;
        string rarezaStr;
        Sprite spriteUnico;

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
    /// Configura los atributos físicos y de dificultad del pez según su rareza.
    /// </summary>
    private void AsignarRarezaYModelo(ItemData pez, string rarezaStr)
    {
        switch (rarezaStr)
        {
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
