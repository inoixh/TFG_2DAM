using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Gestiona la mochila, la barra de acceso rápido y la lógica de equipamiento de objetos.
/// [Interacción BD]
/// [Relaciones: ItemData, InventorySlot, CamaraMovement, PlayerMovement, PescaController, FishManager, MarketManager, UIManager]
/// </summary>
public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance;

    [Header("Referencias de Escena")]
    public Transform manoDelJugador;
    public GameObject panelMochila;

    [Header("Datos de la Caña Inicial")]
    public ItemData datosCana;

    [Header("Interfaz de Usuario")]
    public InventorySlot[] slotsHotbar;
    public InventorySlot[] slotsMochila;

    [HideInInspector]
    public bool mochilaAbierta = false;

    private string idUsuario;
    private string idSaveSlot;
    private ItemData[] inventario = new ItemData[15];
    private int slotSeleccionado = 0;
    private GameObject objetoActualEnMano;
    private int slotOrigen = -1;

    private CamaraMovement camara;
    private PlayerMovement movimiento;
    private PescaController pesca;

    /// <summary>
    /// Inicializa el Singleton y asigna los identificadores del usuario activo.
    /// </summary>
    void Awake()
    {
        int slotIndex;
        Instance = this;

        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            idUsuario = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        }
        else
        {
            Debug.LogWarning("Error de seguridad: Ningún usuario logueado en el juego.");
        }

        slotIndex = PlayerPrefs.GetInt("CurrentSaveSlot", 0);
        idSaveSlot = "slot_" + slotIndex;
    }

    /// <summary>
    /// Configura las referencias necesarias y espera la carga de datos externos antes de continuar.
    /// </summary>
    IEnumerator Start()
    {
        camara = FindFirstObjectByType<CamaraMovement>();
        movimiento = FindFirstObjectByType<PlayerMovement>();
        pesca = FindFirstObjectByType<PescaController>();

        if (panelMochila != null)
        {
            panelMochila.SetActive(false);
        }
        else
        {
            Debug.LogWarning("No se asignó el panel de la mochila en el inspector.");
        }

        ConfigurarIndicesSlots();

        yield return new WaitUntil(() =>
            FishManager.Instance != null && FishManager.Instance.pecesCargados
        );
        yield return new WaitUntil(() =>
            MarketManager.Instance != null && MarketManager.Instance.itemsCargados
        );

        CargarInventarioDesdeNube();
    }

    /// <summary>
    /// Escucha los comandos del teclado para manejar el equipamiento de objetos y abrir la mochila.
    /// </summary>
    void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            EquiparSlot(0);
        }
        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            EquiparSlot(1);
        }
        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            EquiparSlot(2);
        }
        if (Keyboard.current.digit4Key.wasPressedThisFrame)
        {
            EquiparSlot(3);
        }
        if (Keyboard.current.digit5Key.wasPressedThisFrame)
        {
            EquiparSlot(4);
        }

        if (
            Keyboard.current.iKey.wasPressedThisFrame || Keyboard.current.tabKey.wasPressedThisFrame
        )
        {
            ToggleMochila();
        }
    }

    /// <summary>
    /// Asigna los índices correspondientes a cada casilla de la interfaz.
    /// </summary>
    private void ConfigurarIndicesSlots()
    {
        int idx;
        for (int i = 0; i < slotsHotbar.Length; i++)
        {
            if (slotsHotbar[i] != null)
            {
                slotsHotbar[i].ConfigurarSlot(i);
            }
        }

        for (int i = 0; i < slotsMochila.Length; i++)
        {
            if (slotsMochila[i] != null)
            {
                idx = i + 5;
                slotsMochila[i].ConfigurarSlot(idx);
            }
        }
    }

    /// <summary>
    /// Gestiona el intercambio de objetos entre casillas o su envío al mercado.
    /// </summary>
    public void ClickEnSlot(int indiceClicado)
    {
        ItemData itemClicado;
        ItemData temp;

        itemClicado = inventario[indiceClicado];

        if (MarketManager.Instance != null && MarketManager.Instance.mercadoAbierto)
        {
            MarketManager.Instance.SeleccionarItemInventario(itemClicado, indiceClicado);
        }
        else
        {
            if (slotOrigen == -1)
            {
                if (inventario[indiceClicado] != null)
                {
                    slotOrigen = indiceClicado;
                }
                else
                {
                    Debug.LogWarning("Se clicó una casilla vacía como origen.");
                }
            }
            else
            {
                temp = inventario[indiceClicado];
                inventario[indiceClicado] = inventario[slotOrigen];
                inventario[slotOrigen] = temp;

                slotOrigen = -1;
                ActualizarTodaLaUI();
                EquiparSlot(slotSeleccionado);
                GuardarInventarioEnNube();
            }
        }
    }

    /// <summary>
    /// Alterna la visibilidad del panel de la mochila y bloquea los controles del jugador.
    /// </summary>
    private void ToggleMochila()
    {
        mochilaAbierta = !mochilaAbierta;

        if (panelMochila != null)
        {
            panelMochila.SetActive(mochilaAbierta);
        }

        Cursor.visible = mochilaAbierta;
        Cursor.lockState = mochilaAbierta ? CursorLockMode.None : CursorLockMode.Locked;

        if (camara != null)
        {
            camara.rotacionBloqueada = mochilaAbierta;
        }

        if (movimiento != null)
        {
            movimiento.movimientoBloqueado = mochilaAbierta;
        }

        if (!mochilaAbierta)
        {
            slotOrigen = -1;
        }
    }

    /// <summary>
    /// Asigna el objeto seleccionado a la mano del jugador y actualiza las animaciones.
    /// </summary>
    public void EquiparSlot(int indice)
    {
        ItemData itemActual;
        Vector3 escalaDeseada;
        Vector3 escalaDelHueso;
        Animator animadorPropio;

        if (indice < 5)
        {
            slotSeleccionado = indice;
            for (int i = 0; i < slotsHotbar.Length; i++)
            {
                if (slotsHotbar[i] != null)
                {
                    slotsHotbar[i].Seleccionar(i == slotSeleccionado);
                }
            }

            itemActual = inventario[slotSeleccionado];

            if (objetoActualEnMano != null)
            {
                Destroy(objetoActualEnMano);
            }

            if (itemActual != null && itemActual.modelo3D != null)
            {
                objetoActualEnMano = Instantiate(itemActual.modelo3D, manoDelJugador);
                escalaDeseada = itemActual.modelo3D.transform.localScale;
                escalaDelHueso = manoDelJugador.lossyScale;

                objetoActualEnMano.transform.localScale = new Vector3(
                    escalaDeseada.x / escalaDelHueso.x,
                    escalaDeseada.y / escalaDelHueso.y,
                    escalaDeseada.z / escalaDelHueso.z
                );

                objetoActualEnMano.transform.localRotation = itemActual
                    .modelo3D
                    .transform
                    .localRotation;

                if (itemActual.esCanaDePescar)
                {
                    objetoActualEnMano.transform.localPosition = itemActual
                        .modelo3D
                        .transform
                        .localPosition;
                }
                else
                {
                    objetoActualEnMano.transform.localPosition = Vector3.zero;
                }

                if (pesca != null)
                {
                    if (itemActual.esCanaDePescar)
                    {
                        animadorPropio = objetoActualEnMano.GetComponent<Animator>();
                        if (animadorPropio != null)
                        {
                            pesca.animadorCana = animadorPropio;
                        }
                    }
                    else
                    {
                        pesca.animadorCana = null;
                    }
                }

                if (UIManager.Instance != null)
                {
                    UIManager.Instance.MostrarTooltipTemporal(itemActual.nombreDisplay, 2.5f);
                }
            }
        }
        else
        {
            Debug.LogWarning("No se puede equipar un índice mayor a 4 en la barra rápida.");
        }
    }

    /// <summary>
    /// Agrega un nuevo elemento en el primer espacio libre disponible y sincroniza con Firestore.
    /// </summary>
    public bool AnadirObjeto(ItemData nuevoItem)
    {
        bool anadido = false;
        for (int i = 0; i < inventario.Length && !anadido; i++)
        {
            if (inventario[i] == null)
            {
                inventario[i] = nuevoItem;
                ActualizarTodaLaUI();

                if (slotSeleccionado == i)
                {
                    EquiparSlot(i);
                }

                GuardarInventarioEnNube();
                anadido = true;
            }
        }

        if (!anadido)
        {
            Debug.LogWarning("El inventario está lleno, no se pudo añadir el objeto.");
        }

        return anadido;
    }

    /// <summary>
    /// Elimina el objeto equipado y actualiza la interfaz y base de datos.
    /// </summary>
    public void ConsumirItemEnMano()
    {
        inventario[slotSeleccionado] = null;
        if (objetoActualEnMano != null)
        {
            Destroy(objetoActualEnMano);
        }

        ActualizarTodaLaUI();
        GuardarInventarioEnNube();
    }

    /// <summary>
    /// Vacía el contenido de una casilla específica y sincroniza con Firestore.
    /// </summary>
    public void LimpiarCasilla(int indice)
    {
        inventario[indice] = null;
        if (slotSeleccionado == indice && objetoActualEnMano != null)
        {
            Destroy(objetoActualEnMano);
        }

        ActualizarTodaLaUI();
        GuardarInventarioEnNube();
    }

    /// <summary>
    /// Devuelve la información del objeto que el jugador sostiene actualmente.
    /// </summary>
    public ItemData ObtenerItemEnMano()
    {
        return inventario[slotSeleccionado];
    }

    /// <summary>
    /// Devuelve los datos del elemento almacenado en una posición concreta.
    /// </summary>
    public ItemData ObtenerItemEnSlot(int indice)
    {
        return inventario[indice];
    }

    /// <summary>
    /// Refresca los íconos de todas las casillas del inventario para coincidir con los datos internos.
    /// </summary>
    private void ActualizarTodaLaUI()
    {
        int idx;
        for (int i = 0; i < slotsHotbar.Length; i++)
        {
            if (i < inventario.Length)
            {
                slotsHotbar[i].ActualizarSlot(inventario[i]);
            }
        }

        for (int i = 0; i < slotsMochila.Length; i++)
        {
            idx = i + 5;
            if (idx < inventario.Length)
            {
                slotsMochila[i].ActualizarSlot(inventario[idx]);
            }
        }
    }

    /// <summary>
    /// Comprueba si el objeto equipado actualmente funciona como una caña de pescar.
    /// </summary>
    public bool TieneCanaEnMano()
    {
        ItemData item;
        bool resultado = false;
        
        item = inventario[slotSeleccionado];

        if (item != null && item.esCanaDePescar)
        {
            resultado = true;
        }

        return resultado;
    }

    /// <summary>
    /// [BD] Sobrescribe el inventario local en la cuenta del jugador en Firestore.
    /// </summary>
    public void GuardarInventarioEnNube()
    {
        FirebaseFirestore db;
        DocumentReference docRef;
        Dictionary<string, object> dictInventario;
        Dictionary<string, object> data;

        if (!string.IsNullOrEmpty(idUsuario))
        {
            db = FirebaseFirestore.DefaultInstance;
            docRef = db.Collection("users")
                .Document(idUsuario)
                .Collection("save_slots")
                .Document(idSaveSlot);

            dictInventario = new Dictionary<string, object>();

            for (int i = 0; i < inventario.Length; i++)
            {
                if (inventario[i] != null)
                {
                    dictInventario["slot_" + i.ToString("D2")] = inventario[i].ID;
                }
                else
                {
                    dictInventario["slot_" + i.ToString("D2")] = "";
                }
            }

            data = new Dictionary<string, object> { { "inventory", dictInventario } };
            docRef.SetAsync(data, SetOptions.MergeAll);
        }
        else
        {
            Debug.LogWarning("No se guardó el inventario porque no hay usuario activo.");
        }
    }

    /// <summary>
    /// [BD] Descarga los objetos previamente guardados o asigna equipo inicial por defecto.
    /// </summary>
    public void CargarInventarioDesdeNube()
    {
        FirebaseFirestore db;
        DocumentReference docRef;

        if (!string.IsNullOrEmpty(idUsuario))
        {
            db = FirebaseFirestore.DefaultInstance;
            docRef = db.Collection("users")
                .Document(idUsuario)
                .Collection("save_slots")
                .Document(idSaveSlot);

            docRef
                .GetSnapshotAsync()
                .ContinueWithOnMainThread(task =>
                {
                    Dictionary<string, object> dictInyectado;
                    string clave;
                    string idItem;

                    if (
                        task.IsCompleted
                        && task.Result.Exists
                        && task.Result.ContainsField("inventory")
                    )
                    {
                        dictInyectado = task.Result.GetValue<Dictionary<string, object>>(
                            "inventory"
                        );

                        for (int i = 0; i < inventario.Length; i++)
                        {
                            clave = "slot_" + i.ToString("D2");
                            if (dictInyectado.ContainsKey(clave))
                            {
                                idItem = dictInyectado[clave].ToString();
                                if (!string.IsNullOrEmpty(idItem))
                                {
                                    inventario[i] = BuscarItemPorID(idItem);
                                }
                                else
                                {
                                    inventario[i] = null;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (datosCana != null)
                        {
                            inventario[0] = datosCana;
                        }
                        GuardarInventarioEnNube();
                        Debug.LogWarning(
                            "No había inventario guardado, se entregó la caña básica."
                        );
                    }

                    ActualizarTodaLaUI();
                    EquiparSlot(0);
                });
        }
        else
        {
            Debug.LogWarning("No se puede descargar el inventario sin un usuario activo.");
        }
    }

    /// <summary>
    /// Localiza los datos de un objeto consultando los distintos catálogos disponibles.
    /// </summary>
    private ItemData BuscarItemPorID(string id)
    {
        ItemData resultado = null;
        if (!string.IsNullOrEmpty(id))
        {
            if (datosCana != null && datosCana.ID == id)
            {
                resultado = datosCana;
            }
            else
            {
                if (FishManager.Instance != null && resultado == null)
                {
                    foreach (ItemData pez in FishManager.Instance.todosLosPeces)
                    {
                        if (pez.ID == id)
                        {
                            resultado = pez;
                        }
                    }
                }

                if (MarketManager.Instance != null && resultado == null)
                {
                    foreach (ItemData item in MarketManager.Instance.todosLosItems)
                    {
                        if (item.ID == id)
                        {
                            resultado = item;
                        }
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("Se intentó buscar un objeto con un ID vacío.");
        }

        return resultado;
    }
}
