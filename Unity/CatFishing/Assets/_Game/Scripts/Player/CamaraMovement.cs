using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controla la rotación de la cámara basada en el movimiento del ratón.
/// [Relaciones: Ninguna]
/// </summary>
public class CamaraMovement : MonoBehaviour
{
    [Header("Configuración de Control")]
    public float velocidad = 200f;
    public bool invertirY = false;

    [Header("Suavizado")]
    [Range(0.01f, 0.2f)]
    public float tiempoSuavizado = 0.05f;

    [Header("Referencias")]
    public Transform jugador;

    [HideInInspector]
    public bool rotacionBloqueada = false;

    private float rotacionX = 0f;
    private Vector2 deltaRatonActual;
    private Vector2 velocidadRatonSuavizado;

    /// <summary>
    /// Configura el estado inicial del cursor al iniciar la escena.
    /// </summary>
    void Start()
    {
        BloquearCursor();
    }

    /// <summary>
    /// Lee la entrada del ratón y aplica las transformaciones de rotación pertinentes.
    /// </summary>
    void Update()
    {
        if (!rotacionBloqueada && Mouse.current != null && jugador != null)
        {
            Vector2 deltaObjetivo = Mouse.current.delta.ReadValue();

            deltaRatonActual = Vector2.SmoothDamp(
                deltaRatonActual,
                deltaObjetivo,
                ref velocidadRatonSuavizado,
                tiempoSuavizado
            );

            float mouseX = deltaRatonActual.x * velocidad * Time.deltaTime;
            float mouseY = deltaRatonActual.y * velocidad * Time.deltaTime;

            if (invertirY)
            {
                rotacionX += mouseY;
            }
            else
            {
                rotacionX -= mouseY;
            }

            rotacionX = Mathf.Clamp(rotacionX, -60f, 60f);

            transform.localRotation = Quaternion.Euler(rotacionX, 0f, 0f);
            jugador.Rotate(Vector3.up * mouseX);
        }
    }

    /// <summary>
    /// Oculta el cursor del sistema operativo y lo centra en la ventana para permitir la vista en primera persona.
    /// </summary>
    public void BloquearCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// Libera el cursor para permitir interacciones con la interfaz gráfica.
    /// </summary>
    public void DesbloquearCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
