using UnityEngine;
using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Descarga la base de datos de peces desde Firebase y construye los objetos ItemData dinámicamente en memoria.
/// Asigna los modelos 3D (bolsas de colores) por rareza y busca los iconos 2D automáticamente.
/// </summary>
public class FishManager : MonoBehaviour
{
    public static FishManager Instance;

    [Header("Modelos 3D (Bolsas por Rareza)")]
    public GameObject prefabBolsaComun;
    public GameObject prefabBolsaRara;
    public GameObject prefabBolsaEspecial;
    public GameObject prefabBolsaEpica;
    public GameObject prefabBolsaLegendaria;

    [Header("Icono por Defecto")]
    [Tooltip("La foto que se usará si el pez aún no tiene su propia imagen generada por IA.")]
    public Sprite iconoGenerico;

    [HideInInspector]
    public List<ItemData> todosLosPeces = new List<ItemData>();
    
    [HideInInspector]
    public bool pecesCargados = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        CargarPecesDesdeFirebase();
    }

    /// <summary>
    /// Se conecta a la colección "fish" de Firestore y crea los ItemData en tiempo de ejecución.
    /// </summary>
    private async void CargarPecesDesdeFirebase()
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        CollectionReference pecesRef = db.Collection("fish"); 

        try
        {
            QuerySnapshot snapshot = await pecesRef.GetSnapshotAsync();
            
            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                Dictionary<string, object> pezData = doc.ToDictionary();

                ItemData nuevoPez = ScriptableObject.CreateInstance<ItemData>();
                nuevoPez.ID = doc.Id;
                
                if (pezData.ContainsKey("name")) nuevoPez.nombreDisplay = pezData["name"].ToString();
                if (pezData.ContainsKey("description")) nuevoPez.descripcion = pezData["description"].ToString();
                if (pezData.ContainsKey("price")) nuevoPez.precioVenta = System.Convert.ToInt32(pezData["price"]);

                string rarezaStr = pezData.ContainsKey("rarity") ? pezData["rarity"].ToString().ToLower() : "común";
                AsignarRarezaYModelo(nuevoPez, rarezaStr);

                Sprite spriteUnico = Resources.Load<Sprite>("FishIcons/" + nuevoPez.ID);
                nuevoPez.icono = spriteUnico != null ? spriteUnico : iconoGenerico;

                todosLosPeces.Add(nuevoPez);
            }
            
            pecesCargados = true;
            Debug.Log($"[Firebase] {todosLosPeces.Count} peces cargados y construidos en memoria.");
        }
        catch (System.Exception e) 
        { 
            Debug.LogError("Error cargando base de datos de peces: " + e.Message); 
        }
    }

    /// <summary>
    /// Traduce el string de la base de datos a un Enum y asigna la dificultad y el Prefab 3D de la bolsa.
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
                break;
        }
    }
}