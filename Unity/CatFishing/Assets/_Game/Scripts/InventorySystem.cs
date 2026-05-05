using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Gestiona la mochila y la barra rápida del jugador.
/// Instancia los objetos 3D manteniendo su rotación y escala original, conectando sus componentes.
/// Sincroniza dinámicamente los datos con el usuario logueado.
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

    private string idUsuario;
    private string idSaveSlot;
    private ItemData[] inventario = new ItemData[15];
    private int slotSeleccionado = 0;
    private GameObject objetoActualEnMano;
    private bool mochilaAbierta = false;
    private int slotOrigen = -1;

    /// <summary>
    /// Configura la instancia global y extrae los datos de sesión de Firebase y PlayerPrefs.
    /// </summary>
    void Awake()
    {
        Instance = this;

        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            idUsuario = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        }
        else
        {
            Debug.LogError("Error de seguridad: Ningún usuario logueado en el juego.");
        }

        int slotIndex = PlayerPrefs.GetInt("CurrentSaveSlot", 0);
        idSaveSlot = "slot_" + slotIndex;
    }

    /// <summary>
    /// Inicia el proceso de carga esperando a que las bases de datos de peces y del mercado estén listas en memoria.
    /// </summary>
    IEnumerator Start()
    {
        if (panelMochila != null)
            panelMochila.SetActive(false);
        ConfigurarIndicesSlots();

        yield return new WaitUntil(() =>
            FishManager.Instance != null && FishManager.Instance.pecesCargados
        );
        yield return new WaitUntil(() =>
            MarketManager.Instance != null && MarketManager.Instance.itemsCargados
        );

        CargarInventarioDesdeNube();
    }

    void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            EquiparSlot(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame)
            EquiparSlot(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame)
            EquiparSlot(2);
        if (Keyboard.current.digit4Key.wasPressedThisFrame)
            EquiparSlot(3);
        if (Keyboard.current.digit5Key.wasPressedThisFrame)
            EquiparSlot(4);

        if (
            Keyboard.current.iKey.wasPressedThisFrame || Keyboard.current.tabKey.wasPressedThisFrame
        )
        {
            ToggleMochila();
        }
    }

    /// <summary>
    /// Inicializa y enumera todas las casillas gráficas de la barra rápida y el inventario.
    /// </summary>
    private void ConfigurarIndicesSlots()
    {
        for (int i = 0; i < slotsHotbar.Length; i++)
            if (slotsHotbar[i] != null)
                slotsHotbar[i].ConfigurarSlot(i);

        for (int i = 0; i < slotsMochila.Length; i++)
            if (slotsMochila[i] != null)
                slotsMochila[i].ConfigurarSlot(i + 5);
    }

    /// <summary>
    /// Intercambia objetos entre casillas, o los selecciona para vender si el mercado está abierto.
    /// </summary>
    public void ClickEnSlot(int indiceClicado)
    {
        ItemData itemClicado = inventario[indiceClicado];

        if (MarketManager.Instance != null && MarketManager.Instance.mercadoAbierto)
        {
            MarketManager.Instance.SeleccionarItemInventario(itemClicado, indiceClicado);
            return;
        }

        if (slotOrigen == -1)
        {
            if (inventario[indiceClicado] != null)
                slotOrigen = indiceClicado;
        }
        else
        {
            ItemData temp = inventario[indiceClicado];
            inventario[indiceClicado] = inventario[slotOrigen];
            inventario[slotOrigen] = temp;

            slotOrigen = -1;
            ActualizarTodaLaUI();
            EquiparSlot(slotSeleccionado);
            GuardarInventarioEnNube();
        }
    }

    /// <summary>
    /// Alterna el estado visual de la mochila, ajustando el cursor y bloqueando o liberando
    /// los controles de movimiento y cámara del jugador.
    /// </summary>
    private void ToggleMochila()
    {
        mochilaAbierta = !mochilaAbierta;
        if (panelMochila != null)
            panelMochila.SetActive(mochilaAbierta);

        Cursor.visible = mochilaAbierta;
        Cursor.lockState = mochilaAbierta ? CursorLockMode.None : CursorLockMode.Locked;

        CamaraMovement camara = FindFirstObjectByType<CamaraMovement>();
        if (camara != null)
        {
            camara.rotacionBloqueada = mochilaAbierta;
        }

        PlayerMovement movimiento = FindFirstObjectByType<PlayerMovement>();
        if (movimiento != null)
        {
            movimiento.movimientoBloqueado = mochilaAbierta;
        }

        if (!mochilaAbierta)
            slotOrigen = -1;
    }

    /// <summary>
    /// Instancia el modelo 3D en la mano neutralizando escalas deformadas, aplicando
    /// la rotación original del Prefab y enlazando el Animator si es una caña.
    /// Diferencia el ajuste posicional dependiendo del tipo de objeto equipado.
    /// </summary>
    public void EquiparSlot(int indice)
    {
        if (indice >= 5)
            return;
        slotSeleccionado = indice;

        for (int i = 0; i < slotsHotbar.Length; i++)
            if (slotsHotbar[i] != null)
                slotsHotbar[i].Seleccionar(i == slotSeleccionado);

        ItemData itemActual = inventario[slotSeleccionado];

        if (objetoActualEnMano != null)
            Destroy(objetoActualEnMano);

        if (itemActual != null && itemActual.modelo3D != null)
        {
            objetoActualEnMano = Instantiate(itemActual.modelo3D, manoDelJugador);

            Vector3 escalaDeseada = itemActual.modelo3D.transform.localScale;
            Vector3 escalaDelHueso = manoDelJugador.lossyScale;

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

            PescaController pesca = FindFirstObjectByType<PescaController>();

            if (pesca != null)
            {
                if (itemActual.esCanaDePescar)
                {
                    Animator animadorPropio = objetoActualEnMano.GetComponent<Animator>();
                    if (animadorPropio != null)
                        pesca.animadorCana = animadorPropio;
                }
                else
                {
                    pesca.animadorCana = null;
                }
            }

            if (UIManager.Instance != null)
                UIManager.Instance.MostrarTooltipTemporal(itemActual.nombreDisplay, 2.5f);
        }
    }

    /// <summary>
    /// Añade un nuevo objeto al primer hueco libre del inventario.
    /// </summary>
    public bool AnadirObjeto(ItemData nuevoItem)
    {
        for (int i = 0; i < inventario.Length; i++)
        {
            if (inventario[i] == null)
            {
                inventario[i] = nuevoItem;
                ActualizarTodaLaUI();
                if (slotSeleccionado == i)
                    EquiparSlot(i);
                GuardarInventarioEnNube();
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Destruye el objeto equipado actualmente y limpia su casilla.
    /// </summary>
    public void ConsumirItemEnMano()
    {
        inventario[slotSeleccionado] = null;
        if (objetoActualEnMano != null)
            Destroy(objetoActualEnMano);
        ActualizarTodaLaUI();
        GuardarInventarioEnNube();
    }

    /// <summary>
    /// Vacía una casilla del inventario, destruye el objeto si estaba en la mano y guarda los cambios en la nube.
    /// </summary>
    public void LimpiarCasilla(int indice)
    {
        inventario[indice] = null;
        if (slotSeleccionado == indice && objetoActualEnMano != null)
            Destroy(objetoActualEnMano);
        ActualizarTodaLaUI();
        GuardarInventarioEnNube();
    }

    /// <summary>
    /// Retorna los datos del objeto que el jugador sostiene actualmente.
    /// </summary>
    public ItemData ObtenerItemEnMano()
    {
        return inventario[slotSeleccionado];
    }

    /// <summary>
    /// Retorna los datos del objeto en un índice específico del inventario.
    /// </summary>
    public ItemData ObtenerItemEnSlot(int indice)
    {
        return inventario[indice];
    }

    /// <summary>
    /// Sincroniza visualmente todas las casillas gráficas con el array de datos actual.
    /// </summary>
    private void ActualizarTodaLaUI()
    {
        for (int i = 0; i < slotsHotbar.Length; i++)
            if (i < inventario.Length)
                slotsHotbar[i].ActualizarSlot(inventario[i]);

        for (int i = 0; i < slotsMochila.Length; i++)
        {
            int idx = i + 5;
            if (idx < inventario.Length)
                slotsMochila[i].ActualizarSlot(inventario[idx]);
        }
    }

    /// <summary>
    /// Verifica si el jugador tiene una caña de pescar funcional equipada.
    /// </summary>
    public bool TieneCanaEnMano()
    {
        ItemData item = inventario[slotSeleccionado];
        return item != null && item.esCanaDePescar;
    }

    // =========================================================================
    // LÓGICA DE FIREBASE (DICCIONARIO)
    // =========================================================================

    /// <summary>
    /// Convierte el array del inventario a un diccionario y lo sincroniza con Firestore.
    /// </summary>
    public async void GuardarInventarioEnNube()
    {
        if (string.IsNullOrEmpty(idUsuario))
            return;

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        DocumentReference docRef = db.Collection("users")
            .Document(idUsuario)
            .Collection("save_slots")
            .Document(idSaveSlot);

        Dictionary<string, object> dictInventario = new Dictionary<string, object>();
        for (int i = 0; i < inventario.Length; i++)
        {
            if (inventario[i] != null)
                dictInventario["slot_" + i.ToString("D2")] = inventario[i].ID;
            else
                dictInventario["slot_" + i.ToString("D2")] = "";
        }

        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "inventory", dictInventario },
        };
        await docRef.SetAsync(data, SetOptions.MergeAll);
    }

    /// <summary>
    /// Descarga el diccionario del inventario de Firestore e inyecta los objetos correspondientes en memoria.
    /// </summary>
    public async void CargarInventarioDesdeNube()
    {
        if (string.IsNullOrEmpty(idUsuario))
            return;

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        DocumentReference docRef = db.Collection("users")
            .Document(idUsuario)
            .Collection("save_slots")
            .Document(idSaveSlot);

        try
        {
            DocumentSnapshot snap = await docRef.GetSnapshotAsync();
            if (snap.Exists && snap.ContainsField("inventory"))
            {
                Dictionary<string, object> dictInyectado = snap.GetValue<
                    Dictionary<string, object>
                >("inventory");

                for (int i = 0; i < inventario.Length; i++)
                {
                    string clave = "slot_" + i.ToString("D2");
                    if (dictInyectado.ContainsKey(clave))
                    {
                        string idItem = dictInyectado[clave].ToString();
                        inventario[i] = BuscarItemPorID(idItem);
                    }
                }
            }
            else
            {
                if (datosCana != null)
                    inventario[0] = datosCana;
                GuardarInventarioEnNube();
            }
        }
        catch
        {
            if (datosCana != null)
                inventario[0] = datosCana;
        }

        ActualizarTodaLaUI();
        EquiparSlot(0);
    }

    /// <summary>
    /// Busca un objeto por su ID consultando primero la caña base, luego el catálogo de peces y finalmente el mercado.
    /// </summary>
    private ItemData BuscarItemPorID(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;
        if (datosCana != null && datosCana.ID == id)
            return datosCana;

        if (FishManager.Instance != null)
        {
            foreach (ItemData pez in FishManager.Instance.todosLosPeces)
                if (pez.ID == id)
                    return pez;
        }

        if (MarketManager.Instance != null)
        {
            foreach (ItemData item in MarketManager.Instance.todosLosItems)
                if (item.ID == id)
                    return item;
        }

        return null;
    }
}
