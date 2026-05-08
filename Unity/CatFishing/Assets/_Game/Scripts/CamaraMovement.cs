using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controla el movimiento visual de la cámara según los movimientos del ratón.
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
    /// Oculta el cursor del sistema operativo al empezar a jugar.
    /// </summary>
    void Start()
    {
        BloquearCursor();
    }

    /// <summary>
    /// Calcula cuánto se mueve el ratón y aplica la rotación al jugador y a la cámara.
    /// </summary>
    void Update()
    {
        if (rotacionBloqueada || Mouse.current == null || jugador == null)
        {
            return;
        }

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

    /// <summary>
    /// Deja el ratón fijo en el centro y lo vuelve invisible.
    /// </summary>
    public void BloquearCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// Libera el ratón para que el jugador pueda hacer clics en los menús.
    /// </summary>
    public void DesbloquearCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
