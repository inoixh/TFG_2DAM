using UnityEngine;

/// <summary>
/// Gestiona el movimiento libre (sin NavMesh), animaciones y flotabilidad.
/// Los gatos dan pasitos aleatorios alrededor de sus zonas de patrulla.
/// Asume que 'Idle' es el estado por defecto en el Animator y no requiere booleanos.
/// </summary>
public class CatBehavior : MonoBehaviour
{
    public enum Comportamiento
    {
        Quieto,
        Patrullando,
    }

    [Header("Tipo de Movimiento")]
    public Comportamiento comportamientoBase;
    public bool esFlotante = false;

    [Header("Configuración Paseo (Sin NavMesh)")]
    public float velocidadCaminar = 3f;
    public float radioDeMerodeo = 3f;
    public float esperaMinima = 1f;
    public float esperaMaxima = 4f;
    public int paseosPorZona = 3;

    [Header("Configuración Flotante")]
    public Transform modelo3D;
    public float velocidadFlote = 2f;
    public float alturaFlote = 0.2f;
    public float elevacionExtra = 1f;
    private float posYOriginalModelo;

    [Header("Rutas de Patrulla")]
    public Transform[] waypoints;
    private int puntoActual = 0;
    private int contadorPaseos = 0;

    [Header("Animaciones Disponibles")]
    public Animator animator;
    public bool tieneAnimWalk = true;
    public bool tieneAnimTalk = false;

    [Header("Interacción")]
    public bool mirarAlJugador = true;
    private bool estaInteractuando = false;
    private Transform jugadorTransform;

    private Vector3 posicionBase;
    private Vector3 destinoActual;
    private bool estaCaminando = false;
    private float temporizadorEspera;

    private void Start()
    {
        if (modelo3D != null)
            posYOriginalModelo = modelo3D.localPosition.y;
        posicionBase = transform.position;

        if (comportamientoBase == Comportamiento.Patrullando)
        {
            if (waypoints == null || waypoints.Length == 0)
            {
                GameObject objetoRuta = GameObject.Find("RutaGeneral");
                if (objetoRuta != null && objetoRuta.transform.childCount > 0)
                {
                    waypoints = new Transform[objetoRuta.transform.childCount];
                    for (int i = 0; i < objetoRuta.transform.childCount; i++)
                    {
                        waypoints[i] = objetoRuta.transform.GetChild(i);
                    }
                }
            }

            if (waypoints != null && waypoints.Length > 0)
            {
                puntoActual = Random.Range(0, waypoints.Length);
            }

            FijarNuevoDestino();
        }
    }

    private void Update()
    {
        GestionarFlote();

        if (estaInteractuando && jugadorTransform != null)
        {
            if (mirarAlJugador)
                GirarHacia(jugadorTransform.position);
        }
        else if (comportamientoBase == Comportamiento.Patrullando)
        {
            GestionarPaseoLibre();
        }

        GestionarAnimaciones();
    }

    /// <summary>
    /// Aplica un efecto de levitación suave si el NPC está configurado como flotante, y le suma altura.
    /// </summary>
    private void GestionarFlote()
    {
        if (esFlotante && modelo3D != null)
        {
            float nuevaY =
                posYOriginalModelo
                + elevacionExtra
                + Mathf.Sin(Time.time * velocidadFlote) * alturaFlote;
            modelo3D.localPosition = new Vector3(
                modelo3D.localPosition.x,
                nuevaY,
                modelo3D.localPosition.z
            );
        }
    }

    /// <summary>
    /// Controla la lógica de buscar un punto aleatorio, desplazarse y esperar.
    /// </summary>
    private void GestionarPaseoLibre()
    {
        if (!estaCaminando)
        {
            temporizadorEspera -= Time.deltaTime;
            if (temporizadorEspera <= 0)
            {
                FijarNuevoDestino();
            }
        }
        else
        {
            GirarHacia(destinoActual);
            transform.position = Vector3.MoveTowards(
                transform.position,
                destinoActual,
                velocidadCaminar * Time.deltaTime
            );

            float distancia = Vector3.Distance(
                new Vector3(transform.position.x, 0, transform.position.z),
                new Vector3(destinoActual.x, 0, destinoActual.z)
            );
            if (distancia < 0.1f)
            {
                LlegarADestino();
            }
        }
    }

    /// <summary>
    /// Calcula un punto aleatorio cerca de la zona base o del waypoint actual.
    /// </summary>
    private void FijarNuevoDestino()
    {
        Vector3 centroZona = transform.position;

        if (waypoints != null && waypoints.Length > 0)
        {
            centroZona = waypoints[puntoActual].position;
        }
        else
        {
            centroZona = posicionBase;
        }

        Vector3 offsetAleatorio = new Vector3(
            Random.Range(-radioDeMerodeo, radioDeMerodeo),
            0,
            Random.Range(-radioDeMerodeo, radioDeMerodeo)
        );
        destinoActual = centroZona + offsetAleatorio;
        destinoActual.y = transform.position.y;

        estaCaminando = true;
    }

    /// <summary>
    /// Detiene al NPC y calcula el tiempo de espera antes del siguiente paseo.
    /// </summary>
    private void LlegarADestino()
    {
        estaCaminando = false;
        temporizadorEspera = Random.Range(esperaMinima, esperaMaxima);

        contadorPaseos++;
        if (contadorPaseos >= paseosPorZona && waypoints != null && waypoints.Length > 0)
        {
            contadorPaseos = 0;
            puntoActual++;
            if (puntoActual >= waypoints.Length)
                puntoActual = 0;
        }
    }

    /// <summary>
    /// Rota suavemente el modelo hacia una dirección específica.
    /// </summary>
    private void GirarHacia(Vector3 objetivo)
    {
        Vector3 direccion = (objetivo - transform.position).normalized;
        direccion.y = 0;
        if (direccion != Vector3.zero)
        {
            Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rotacionObjetivo,
                Time.deltaTime * 5f
            );
        }
    }

    /// <summary>
    /// Envía los booleanos al Animator solo si el modelo dispone de dichas animaciones.
    /// </summary>
    private void GestionarAnimaciones()
    {
        if (animator == null)
            return;

        if (tieneAnimWalk)
        {
            animator.SetBool("Walk", estaCaminando && !estaInteractuando);
        }

        if (tieneAnimTalk)
        {
            animator.SetBool("Talk", estaInteractuando);
        }
    }

    /// <summary>
    /// Detiene la rutina de paseo y orienta al NPC hacia el jugador.
    /// </summary>
    public void IniciarInteraccion(Transform jugador)
    {
        estaInteractuando = true;
        jugadorTransform = jugador;
    }

    /// <summary>
    /// Libera al NPC para que reanude sus rutinas de patrulla o espera.
    /// </summary>
    public void FinalizarInteraccion()
    {
        estaInteractuando = false;
        jugadorTransform = null;
        temporizadorEspera = Random.Range(1f, 3f);
    }
}
