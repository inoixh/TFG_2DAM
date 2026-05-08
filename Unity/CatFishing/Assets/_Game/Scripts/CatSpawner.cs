using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona la cantidad y el lugar donde nacen los gatos.
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
    /// Carga el tiempo y lanza el primer gato.
    /// </summary>
    void Start()
    {
        temporizador = segundosEntreSpawns;
        IntentarSpawnearGato();
    }

    /// <summary>
    /// Revisa cuánto tiempo falta para traer a otro gato nuevo a la isla.
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
    /// Verifica si hay sitio para más gatos y los crea según el nivel del jugador.
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
            return;
        }

        gatoElegido = ObtenerGatoPorRareza();

        if (gatoElegido == null)
        {
            Debug.LogWarning("No se pudo elegir ningún gato para aparecer.");
            return;
        }

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

    /// <summary>
    /// Comprueba si el gato elegido debe ir obligatoriamente a una zona especial de la isla.
    /// </summary>
    private Transform BuscarPuntoEspecial(GameObject gatoElegido)
    {
        GatoNPC scriptGato;
        CatBehavior scriptBehavior;
        List<SpawnEspecifico> spawnsDisponibles;
        bool esMatchPorID;
        bool esMatchQuieto;

        scriptGato = gatoElegido.GetComponent<GatoNPC>();
        scriptBehavior = gatoElegido.GetComponent<CatBehavior>();

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
                esMatchPorID =
                    spawnEsp.regla == SpawnEspecifico.TipoRegla.PorID
                    && spawnEsp.idGato == scriptGato.idGatoDB;

                esMatchQuieto =
                    spawnEsp.regla == SpawnEspecifico.TipoRegla.GatosQuietos
                    && scriptBehavior.comportamientoBase == CatBehavior.Comportamiento.Quieto;

                if ((esMatchPorID || esMatchQuieto) && !PuntoOcupado(spawnEsp.puntoExclusivo))
                {
                    return spawnEsp.puntoExclusivo;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Coloca físicamente el modelo del gato en la escena y le marca la ruta.
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
    /// Compara posiciones para que no nazcan gatos uno encima del otro.
    /// </summary>
    private bool PuntoOcupado(Transform punto)
    {
        foreach (GameObject gato in gatosActivos)
        {
            if (gato != null && Vector3.Distance(gato.transform.position, punto.position) < 1f)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Busca un lugar aleatorio y normal que no esté ocupado por otros gatos.
    /// </summary>
    private Transform ObtenerPuntoGeneralLibre()
    {
        Transform punto;

        for (int i = 0; i < 15; i++)
        {
            punto = puntosDeSpawnGenerales[Random.Range(0, puntosDeSpawnGenerales.Length)];

            if (!PuntoOcupado(punto))
            {
                return punto;
            }
        }
        return null;
    }

    /// <summary>
    /// Sortea qué tipo de rareza de gato va a nacer usando probabilidades.
    /// </summary>
    private GameObject ObtenerGatoPorRareza()
    {
        float pesoTotalRareza = 0;
        float tiradaRareza;
        float pesoAcumuladoRareza = 0;

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
            if (grupo.prefabsGatos.Length == 0)
            {
                continue;
            }

            pesoAcumuladoRareza += grupo.probabilidad;

            if (tiradaRareza <= pesoAcumuladoRareza)
            {
                return ElegirGatoPorBajaAfinidad(grupo.prefabsGatos);
            }
        }
        return null;
    }

    /// <summary>
    /// Da más posibilidad de nacer a los gatos que el jugador no tiene descubiertos.
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

        for (int i = 0; i < prefabsDisponibles.Length; i++)
        {
            acumuladoGato += pesosInversos[i];

            if (tiradaGato <= acumuladoGato)
            {
                return prefabsDisponibles[i];
            }
        }

        return prefabsDisponibles[0];
    }
}
