using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona la generación y posicionamiento de los gatos en el mundo de forma procedimental.
/// [Relaciones: GatoNPC, CatBehavior, GameManager]
/// </summary>
public class CatSpawner : MonoBehaviour
{
    [System.Serializable]
    public class RarityGroup
    {
        public string nombreRareza;

        [Range(0f, 100f)]
        public float probabilidad;
        public GameObject[] prefabsGatos;
    }

    [System.Serializable]
    public class SpawnEspecifico
    {
        public enum TipoRegla
        {
            PorID,
            GatosQuietos,
        }

        public TipoRegla regla;
        public string idGato;
        public Transform puntoExclusivo;
    }

    [Header("Configuración de Gatos")]
    public RarityGroup[] gruposDeRareza;

    [Header("Spawns Especiales")]
    public SpawnEspecifico[] spawnsExclusivos;

    [Header("Lugares de Aparición Comunes")]
    public Transform[] puntosDeSpawnGenerales;

    [Header("Rutas de Patrulla Generales")]
    public Transform[] waypointsGenerales;

    [Header("Ritmo y Límites")]
    public float segundosEntreSpawns = 60f;

    private List<GameObject> gatosActivos = new List<GameObject>();
    private float temporizador;

    /// <summary>
    /// Asigna el contador de tiempo e intenta crear al primer habitante de la isla.
    /// </summary>
    void Start()
    {
        temporizador = segundosEntreSpawns;
        IntentarSpawnearGato();
    }

    /// <summary>
    /// Disminuye el tiempo de espera y efectúa un intento de creación al finalizar el ciclo.
    /// </summary>
    void Update()
    {
        temporizador -= Time.deltaTime;
        if (temporizador <= 0)
        {
            IntentarSpawnearGato();
            temporizador = segundosEntreSpawns + Random.Range(-10f, 20f);
        }
    }

    /// <summary>
    /// Comprueba los límites de la isla y posiciona un gato aleatorio si hay espacios disponibles.
    /// </summary>
    private void IntentarSpawnearGato()
    {
        int nivelJugador;
        int maxGatosPermitidos;
        GameObject gatoElegido;
        Transform puntoElegido;

        gatosActivos.RemoveAll(gato => gato == null);

        nivelJugador = PlayerPrefs.GetInt("CurrentLevel", 1);
        maxGatosPermitidos = Mathf.Clamp(1 + (nivelJugador / 10), 1, 6);

        if (gatosActivos.Count >= maxGatosPermitidos || puntosDeSpawnGenerales.Length == 0)
        {
            Debug.LogWarning("Límite de gatos alcanzado o no hay puntos de creación válidos.");
        }
        else
        {
            gatoElegido = ObtenerGatoPorRareza();

            if (gatoElegido == null)
            {
                Debug.LogWarning("No se pudo elegir ningún gato para aparecer.");
            }
            else
            {
                puntoElegido = BuscarPuntoEspecial(gatoElegido);

                if (puntoElegido == null)
                {
                    puntoElegido = ObtenerPuntoGeneralLibre();
                }

                if (puntoElegido != null)
                {
                    InstanciarGato(gatoElegido, puntoElegido);
                }
                else
                {
                    Debug.LogWarning("No hay ningún punto libre para colocar al gato.");
                }
            }
        }
    }

    /// <summary>
    /// Asigna puntos designados para gatos inmóviles o de IDs concretas, evitando solapamientos.
    /// </summary>
    private Transform BuscarPuntoEspecial(GameObject gatoElegido)
    {
        GatoNPC scriptGato = gatoElegido.GetComponent<GatoNPC>();
        CatBehavior scriptBehavior = gatoElegido.GetComponent<CatBehavior>();
        List<SpawnEspecifico> spawnsDisponibles;
        bool esMatchPorID;
        bool esMatchQuieto;
        Transform puntoEncontrado = null;

        if (scriptGato != null && scriptBehavior != null)
        {
            spawnsDisponibles = new List<SpawnEspecifico>(spawnsExclusivos);

            for (int i = 0; i < spawnsDisponibles.Count; i++)
            {
                SpawnEspecifico temp = spawnsDisponibles[i];
                int randomIndex = Random.Range(i, spawnsDisponibles.Count);
                spawnsDisponibles[i] = spawnsDisponibles[randomIndex];
                spawnsDisponibles[randomIndex] = temp;
            }

            foreach (SpawnEspecifico spawnEsp in spawnsDisponibles)
            {
                if (puntoEncontrado == null)
                {
                    esMatchPorID =
                        spawnEsp.regla == SpawnEspecifico.TipoRegla.PorID
                        && spawnEsp.idGato == scriptGato.idGatoDB;
                    esMatchQuieto =
                        spawnEsp.regla == SpawnEspecifico.TipoRegla.GatosQuietos
                        && scriptBehavior.comportamientoBase == CatBehavior.Comportamiento.Quieto;

                    if ((esMatchPorID || esMatchQuieto) && !PuntoOcupado(spawnEsp.puntoExclusivo))
                    {
                        puntoEncontrado = spawnEsp.puntoExclusivo;
                    }
                }
            }
        }
        return puntoEncontrado;
    }

    /// <summary>
    /// Crea el modelo en escena y configura su ruta de movimiento automática.
    /// </summary>
    private void InstanciarGato(GameObject prefabGato, Transform puntoElegido)
    {
        GameObject nuevoGato;
        CatBehavior behavior;

        nuevoGato = Instantiate(prefabGato, puntoElegido.position, puntoElegido.rotation);
        behavior = nuevoGato.GetComponent<CatBehavior>();

        if (behavior != null && (behavior.waypoints == null || behavior.waypoints.Length == 0))
        {
            behavior.waypoints = waypointsGenerales;
        }

        gatosActivos.Add(nuevoGato);
    }

    /// <summary>
    /// Examina si un punto de aparición dado está demasiado cerca de un gato ya existente.
    /// </summary>
    private bool PuntoOcupado(Transform punto)
    {
        bool ocupado = false;
        foreach (GameObject gato in gatosActivos)
        {
            if (!ocupado && gato != null && Vector3.Distance(gato.transform.position, punto.position) < 1f)
            {
                ocupado = true;
            }
        }
        return ocupado;
    }

    /// <summary>
    /// Intenta localizar de forma reiterada una ubicación general vacía para alojar una entidad.
    /// </summary>
    private Transform ObtenerPuntoGeneralLibre()
    {
        Transform punto = null;
        for (int i = 0; i < 15 && punto == null; i++)
        {
            Transform puntoCandidato = puntosDeSpawnGenerales[Random.Range(0, puntosDeSpawnGenerales.Length)];
            if (!PuntoOcupado(puntoCandidato))
            {
                punto = puntoCandidato;
            }
        }
        return punto;
    }

    /// <summary>
    /// Calcula matemáticamente qué grupo de rareza invocar dependiendo de los pesos porcentuales.
    /// </summary>
    private GameObject ObtenerGatoPorRareza()
    {
        float pesoTotalRareza = 0;
        float tiradaRareza;
        float pesoAcumuladoRareza = 0;
        GameObject gatoElegido = null;

        foreach (RarityGroup grupo in gruposDeRareza)
        {
            if (grupo.prefabsGatos.Length > 0)
            {
                pesoTotalRareza += grupo.probabilidad;
            }
        }

        tiradaRareza = Random.Range(0, pesoTotalRareza);

        foreach (RarityGroup grupo in gruposDeRareza)
        {
            if (gatoElegido == null && grupo.prefabsGatos.Length > 0)
            {
                pesoAcumuladoRareza += grupo.probabilidad;
                if (tiradaRareza <= pesoAcumuladoRareza)
                {
                    gatoElegido = ElegirGatoPorBajaAfinidad(grupo.prefabsGatos);
                }
            }
        }
        return gatoElegido;
    }

    /// <summary>
    /// Prioriza la aparición de individuos con niveles de amistad inferiores.
    /// </summary>
    private GameObject ElegirGatoPorBajaAfinidad(GameObject[] prefabsDisponibles)
    {
        float pesoTotalGatos = 0f;
        List<float> pesosInversos = new List<float>();
        GatoNPC scriptNPC;
        int afinidadActual;
        float pesoAsignado;
        float tiradaGato;
        float acumuladoGato = 0f;
        GameObject gatoElegido = null;

        foreach (GameObject prefab in prefabsDisponibles)
        {
            scriptNPC = prefab.GetComponent<GatoNPC>();
            afinidadActual = 0;

            if (
                scriptNPC != null
                && GameManager.Instance != null
                && GameManager.Instance.afinidadGatos.ContainsKey(scriptNPC.idGatoDB)
            )
            {
                afinidadActual = GameManager.Instance.afinidadGatos[scriptNPC.idGatoDB];
            }

            pesoAsignado = 1000f / (afinidadActual + 100f);
            pesosInversos.Add(pesoAsignado);
            pesoTotalGatos += pesoAsignado;
        }

        tiradaGato = Random.Range(0f, pesoTotalGatos);

        for (int i = 0; i < prefabsDisponibles.Length && gatoElegido == null; i++)
        {
            acumuladoGato += pesosInversos[i];
            if (tiradaGato <= acumuladoGato)
            {
                gatoElegido = prefabsDisponibles[i];
            }
        }

        if (gatoElegido == null && prefabsDisponibles.Length > 0)
        {
            gatoElegido = prefabsDisponibles[0];
        }

        return gatoElegido;
    }
}
