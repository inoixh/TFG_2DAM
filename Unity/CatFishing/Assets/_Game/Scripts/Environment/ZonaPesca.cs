using UnityEngine;

/// <summary>
/// Controla la entrada y salida del jugador en las zonas habilitadas para pescar.
/// [Relaciones: PescaController]
/// </summary>
public class ZonaPesca : MonoBehaviour
{
    /// <summary>
    /// Habilita la capacidad de pescar cuando el jugador entra en el trigger de zona pesca.
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
    /// Deshabilita la capacidad de pescar al salir de la zona de agua.
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
                Debug.LogWarning("El jugador salió de zona pesca pero la caña da error.");
            }
        }
    }
}
