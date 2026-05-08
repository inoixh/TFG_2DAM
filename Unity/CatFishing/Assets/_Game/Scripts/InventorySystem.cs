using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controla la mochila del jugador, el cambio de objetos en la mano y la sincronización con la base de datos.
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

    /// <summary>
    /// Nombra a este script como el gestor principal del inventario y prepara los identificadores de guardado.
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
    /// Espera a que los peces y el mercado estén cargados para descargar los objetos del jugador.
    /// </summary>
    IEnumerator Start()
    {
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
    /// Escucha los números del teclado para cambiar de objeto y las teclas I o Tab para abrir la mochila.
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
    /// Enumera todas las casillas gráficas para que cada una sepa qué posición ocupa en la lista.
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
    /// Vende el objeto si el mercado está abierto, o lo mueve de lugar si estamos ordenando la mochila.
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
    /// Abre o cierra la pantalla del inventario y pausa el movimiento del jugador para navegar con el ratón.
    /// </summary>
    private void ToggleMochila()
    {
        CamaraMovement camara;
        PlayerMovement movimiento;

        mochilaAbierta = !mochilaAbierta;

        if (panelMochila != null)
        {
            panelMochila.SetActive(mochilaAbierta);
        }

        Cursor.visible = mochilaAbierta;
        Cursor.lockState = mochilaAbierta ? CursorLockMode.None : CursorLockMode.Locked;

        camara = FindFirstObjectByType<CamaraMovement>();
        if (camara != null)
        {
            camara.rotacionBloqueada = mochilaAbierta;
        }

        movimiento = FindFirstObjectByType<PlayerMovement>();
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
    /// Pone el objeto en la mano del jugador, ajustando su tamaño visual y conectando sus animaciones si es una caña.
    /// </summary>
    public void EquiparSlot(int indice)
    {
        ItemData itemActual;
        Vector3 escalaDeseada;
        Vector3 escalaDelHueso;
        PescaController pesca;
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

                pesca = FindFirstObjectByType<PescaController>();

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
    /// Busca el primer hueco vacío de la mochila y guarda el objeto nuevo en él.
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
    /// Elimina el objeto que el jugador sostiene actualmente en la mano.
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
    /// Elimina un objeto específico usando su número de posición en la lista.
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
    /// Devuelve los datos del objeto que está actualmente equipado.
    /// </summary>
    public ItemData ObtenerItemEnMano()
    {
        return inventario[slotSeleccionado];
    }

    /// <summary>
    /// Devuelve los datos de un objeto consultando un número específico.
    /// </summary>
    public ItemData ObtenerItemEnSlot(int indice)
    {
        return inventario[indice];
    }

    /// <summary>
    /// Refresca los iconos visuales de todos los huecos para que coincidan con la lista interna.
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
    /// Comprueba si el objeto en la mano sirve para pescar.
    /// </summary>
    public bool TieneCanaEnMano()
    {
        ItemData item;
        item = inventario[slotSeleccionado];

        if (item != null && item.esCanaDePescar)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Empaqueta los textos de los objetos y los sube a la base de datos del jugador.
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
    /// Baja la lista de nombres desde la nube y los transforma en los objetos reales dentro del juego.
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
    /// Revisa todos los catálogos del juego para encontrar los datos completos de un objeto usando su texto identificador.
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
