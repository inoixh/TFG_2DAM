using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Representa un espacio en la colección para mostrar un gato o un pez desbloqueado.
/// [Relaciones: CatData, ItemData, CollectionManager]
/// </summary>
public class CollectionSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("Componentes Visuales")]
    public Image iconoItem;

    private CatData datosGato;
    private ItemData datosPez;
    private bool esGato;
    private bool estaDesbloqueado;

    /// <summary>
    /// Configura la ranura para mostrar la información de un gato específico.
    /// </summary>
    public void ConfigurarGato(CatData gato, bool desbloqueado, Sprite iconoIncognita)
    {
        datosGato = gato;
        esGato = true;
        estaDesbloqueado = desbloqueado;

        if (iconoItem != null)
        {
            iconoItem.sprite = desbloqueado ? gato.icono : iconoIncognita;
        }
        else
        {
            Debug.LogWarning("No existe el componente Image para el icono en CollectionSlot.");
        }
    }

    /// <summary>
    /// Configura la ranura para mostrar la información de un pez específico.
    /// </summary>
    public void ConfigurarPez(ItemData pez, bool desbloqueado, Sprite iconoIncognita)
    {
        datosPez = pez;
        esGato = false;
        estaDesbloqueado = desbloqueado;

        if (iconoItem != null)
        {
            iconoItem.sprite = desbloqueado ? pez.icono : iconoIncognita;
        }
        else
        {
            Debug.LogWarning("No existe el componente Image para el icono en CollectionSlot.");
        }
    }

    /// <summary>
    /// Detecta la pulsación sobre la ranura y notifica al gestor de la colección.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (estaDesbloqueado)
        {
            if (esGato && datosGato != null && CollectionManager.Instance != null)
            {
                CollectionManager.Instance.SeleccionarGato(datosGato);
            }
            else if (!esGato && datosPez != null && CollectionManager.Instance != null)
            {
                CollectionManager.Instance.SeleccionarPez(datosPez);
            }
            else
            {
                Debug.LogWarning(
                    "Faltan datos o el CollectionManager no está disponible para procesar el clic."
                );
            }
        }
        else
        {
            Debug.LogWarning("El jugador ha clicado un objeto no desbloqueado, se ignora.");
        }
    }
}
