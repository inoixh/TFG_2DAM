using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Añade un efecto de sonido a cualquier botón de la interfaz al ser pulsado.
/// [Relaciones: SoundManager]
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonSound : MonoBehaviour
{
    private Button btn;

    /// <summary>
    /// Asigna el evento de clic del botón para reproducir el sonido correspondiente.
    /// </summary>
    void Start()
    {
        btn = GetComponent<Button>();

        if (btn != null)
        {
            btn.onClick.AddListener(ReproducirSonidoClick);
        }
        else
        {
            Debug.LogWarning("No se encontró el componente Button en el objeto.");
        }
    }

    /// <summary>
    /// Emite el sonido de interacción usando el sistema de audio global.
    /// </summary>
    private void ReproducirSonidoClick()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.ReproducirClick();
        }
        else
        {
            Debug.LogWarning("SoundManager no existe para reproducir el clic.");
        }
    }
}
