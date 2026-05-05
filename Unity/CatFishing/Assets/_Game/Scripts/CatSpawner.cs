using System.Collections.Generic;
using UnityEngine;

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

        [Tooltip(
            "Elige si este punto es para un gato concreto o para cualquiera que esté 'Quieto'."
        )]
        public TipoRegla regla;

        [Tooltip("Solo hace falta rellenarlo si la regla es 'PorID' (Ej: cat_leia)")]
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

    void Start()
    {
        temporizador = segundosEntreSpawns;
        IntentarSpawnearGato();
    }

    void Update()
    {
        temporizador -= Time.deltaTime;
        if (temporizador <= 0)
        {
            IntentarSpawnearGato();
            temporizador = segundosEntreSpawns + Random.Range(-10f, 20f);
        }
    }

    private void IntentarSpawnearGato()
    {
        gatosActivos.RemoveAll(gato => gato == null);

        int nivelJugador = PlayerPrefs.GetInt("CurrentLevel", 1);
        int maxGatosPermitidos = Mathf.Clamp(1 + (nivelJugador / 10), 1, 6);

        if (gatosActivos.Count >= maxGatosPermitidos)
            return;
        if (puntosDeSpawnGenerales.Length == 0)
            return;

        GameObject gatoElegido = ObtenerGatoPorRareza();
        if (gatoElegido == null)
            return;

        Transform puntoElegido = null;
        GatoNPC scriptGato = gatoElegido.GetComponent<GatoNPC>();
        CatBehavior scriptBehavior = gatoElegido.GetComponent<CatBehavior>();

        if (scriptGato != null && scriptBehavior != null)
        {
            // Mezclamos la lista de spawns especiales para que sea aleatorio si hay varios "Quietos" libres
            List<SpawnEspecifico> spawnsDisponibles = new List<SpawnEspecifico>(spawnsExclusivos);
            for (int i = 0; i < spawnsDisponibles.Count; i++)
            {
                SpawnEspecifico temp = spawnsDisponibles[i];
                int randomIndex = Random.Range(i, spawnsDisponibles.Count);
                spawnsDisponibles[i] = spawnsDisponibles[randomIndex];
                spawnsDisponibles[randomIndex] = temp;
            }

            foreach (var spawnEsp in spawnsDisponibles)
            {
                bool esMatch = false;

                // Opción A: Match exacto por ID
                if (
                    spawnEsp.regla == SpawnEspecifico.TipoRegla.PorID
                    && spawnEsp.idGato == scriptGato.idGatoDB
                )
                {
                    esMatch = true;
                }
                // Opción B: Cualquier gato con el bool "Quieto"
                else if (
                    spawnEsp.regla == SpawnEspecifico.TipoRegla.GatosQuietos
                    && scriptBehavior.comportamientoBase == CatBehavior.Comportamiento.Quieto
                )
                {
                    esMatch = true;
                }

                // Verificamos que no haya ya un gato subido en ese punto
                if (esMatch && !PuntoOcupado(spawnEsp.puntoExclusivo))
                {
                    puntoElegido = spawnEsp.puntoExclusivo;
                    break;
                }
            }
        }

        // Si no se encontró un punto especial libre (o no le correspondía), busca uno general libre
        if (puntoElegido == null)
        {
            puntoElegido = ObtenerPuntoGeneralLibre();
        }

        if (puntoElegido == null)
            return; // Si todo está lleno, no spawnea

        // --- INSTANCIAR ---
        GameObject nuevoGato = Instantiate(
            gatoElegido,
            puntoElegido.position,
            puntoElegido.rotation
        );

        CatBehavior behavior = nuevoGato.GetComponent<CatBehavior>();
        if (behavior != null && (behavior.waypoints == null || behavior.waypoints.Length == 0))
        {
            behavior.waypoints = waypointsGenerales;
        }

        gatosActivos.Add(nuevoGato);
    }

    private bool PuntoOcupado(Transform punto)
    {
        foreach (var gato in gatosActivos)
        {
            // Si hay un gato a menos de 1 metro de ese punto de spawn, se considera ocupado
            if (gato != null && Vector3.Distance(gato.transform.position, punto.position) < 1f)
                return true;
        }
        return false;
    }

    private Transform ObtenerPuntoGeneralLibre()
    {
        for (int i = 0; i < 15; i++) // Intenta buscar un punto libre 15 veces
        {
            Transform punto = puntosDeSpawnGenerales[
                Random.Range(0, puntosDeSpawnGenerales.Length)
            ];
            if (!PuntoOcupado(punto))
                return punto;
        }
        return null;
    }

    private GameObject ObtenerGatoPorRareza()
    {
        float pesoTotal = 0;
        foreach (var grupo in gruposDeRareza)
            if (grupo.prefabsGatos.Length > 0)
                pesoTotal += grupo.probabilidad;

        float tirada = Random.Range(0, pesoTotal);
        float pesoAcumulado = 0;

        foreach (var grupo in gruposDeRareza)
        {
            if (grupo.prefabsGatos.Length == 0)
                continue;
            pesoAcumulado += grupo.probabilidad;
            if (tirada <= pesoAcumulado)
                return grupo.prefabsGatos[Random.Range(0, grupo.prefabsGatos.Length)];
        }
        return null;
    }
}
