using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Gestiona la interacción del jugador con los distintos edificios del entorno.
/// [Relaciones: UIManager, MarketManager, TavernManager, ChurchManager]
/// </summary>
public class BuildingInteraction : MonoBehaviour
{
    public enum TipoEdificio
    {
        Mercado,
        Taberna,
        Iglesia,
    }

    [Header("Configuración del Edificio")]
    public TipoEdificio tipoDeEdificio;
    private bool jugadorCerca = false;

    /// <summary>
    /// Comprueba si el jugador interactúa con el edificio cercano para abrir su panel correspondiente.
    /// </summary>
    private void Update()
    {
        if (jugadorCerca && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (UIManager.Instance != null && UIManager.Instance.ConsumirInteraccion())
            {
                UIManager.Instance.OcultarInteraccion();

                switch (tipoDeEdificio)
                {
                    case TipoEdificio.Mercado:
                        if (
                            MarketManager.Instance != null
                            && !MarketManager.Instance.mercadoAbierto
                        )
                            MarketManager.Instance.AbrirMercado();
                        break;
                    case TipoEdificio.Taberna:
                        if (
                            TavernManager.Instance != null
                            && !TavernManager.Instance.tabernaAbierta
                        )
                            TavernManager.Instance.AbrirTaberna();
                        break;
                    case TipoEdificio.Iglesia:
                        if (
                            ChurchManager.Instance != null
                            && !ChurchManager.Instance.iglesiaAbierta
                        )
                            ChurchManager.Instance.AbrirIglesia();
                        break;
                }
            }
            else
            {
                Debug.LogWarning("Interacción ignorada: Ya hay un panel abierto.");
            }
        }
    }

    /// <summary>
    /// Detecta cuando el jugador entra en la zona de interacción del edificio y muestra el mensaje en pantalla.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = true;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.MostrarInteraccion(
                    "Pulsa [ESPACIO] para entrar al " + tipoDeEdificio.ToString()
                );
            }
        }
    }

    /// <summary>
    /// Detecta cuando el jugador sale de la zona de interacción y oculta el mensaje correspondiente.
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = false;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.OcultarInteraccion();
            }
        }
    }
}
