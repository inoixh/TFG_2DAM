using UnityEngine;

/// <summary>
/// Avisa al juego cuando el jugador toca el agua para dejarle usar la caña.
/// </summary>
public class ZonaPesca : MonoBehaviour
{
    /// <summary>
    /// Analiza el componente colisionado y habilita las virtudes lógicas que dan potestad acuática al usuario.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        PescaController cana;

        if (other.CompareTag("Player"))
        {
            cana = other.GetComponentInChildren<PescaController>(true);
            if (cana != null)
            {
                cana.EstablecerZonaDePesca(true);
            }
            else
            {
                Debug.LogWarning(
                    "El jugador tocó el agua pero no se encontró su controlador de pesca."
                );
            }
        }
    }

    /// <summary>
    /// Cierra el grifo permitiendo que las restricciones se instalen formalmente en los márgenes terrestres de la costa.
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        PescaController cana;

        if (other.CompareTag("Player"))
        {
            cana = other.GetComponentInChildren<PescaController>(true);
            if (cana != null)
            {
                cana.EstablecerZonaDePesca(false);
            }
            else
            {
                Debug.LogWarning(
                    "El jugador salió del agua pero el sistema ignoraba la presencia real de la caña."
                );
            }
        }
    }
}
