using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controla la rotación de la cámara basada en el movimiento del ratón con efecto suave.
/// Permite el bloqueo temporal desde otros sistemas como diálogos o minijuegos.
/// </summary>
public class CamaraMovement : MonoBehaviour
{
    [Header("Sensibilidad y Suavizado")]
    public float velocidad = 200f;

    [Range(0.01f, 0.2f)]
    public float tiempoSuavizado = 0.05f;

    public Transform jugador;

    [HideInInspector]
    public bool rotacionBloqueada = false;

    private float rotacionX = 0f;
    private Vector2 deltaRatonActual;
    private Vector2 velocidadRatonSuavizado;

    void Start()
    {
        BloquearCursor();
    }

    void Update()
    {
        if (rotacionBloqueada || Mouse.current == null || jugador == null)
            return;
        if (Time.timeScale == 0f)
            Time.timeScale = 1f;

        Vector2 deltaObjetivo = Mouse.current.delta.ReadValue();
        deltaRatonActual = Vector2.SmoothDamp(
            deltaRatonActual,
            deltaObjetivo,
            ref velocidadRatonSuavizado,
            tiempoSuavizado
        );

        float mouseX = deltaRatonActual.x * velocidad * Time.deltaTime;
        float mouseY = deltaRatonActual.y * velocidad * Time.deltaTime;

        rotacionX -= mouseY;
        rotacionX = Mathf.Clamp(rotacionX, -60f, 60f);
        transform.localRotation = Quaternion.Euler(rotacionX, 0f, 0f);

        jugador.Rotate(Vector3.up * mouseX);
    }

    /// <summary>
    /// Oculta y bloquea el cursor del ratón en el centro de la pantalla.
    /// </summary>
    public void BloquearCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
    }

    /// <summary>
    /// Muestra y libera el cursor del ratón para interactuar con interfaces.
    /// </summary>
    public void DesbloquearCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
