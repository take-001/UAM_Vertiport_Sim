using System.Collections;
using UnityEngine;


public class ExponentialTimeSpawner : MonoBehaviour
{
    public GameObject aircraftPrefab; // Prefab of the aircraft to spawn
    public Transform[] fatoTransforms;
    public Vector3 spawnAreaSize = new Vector3(300, 0, 300); // Size of the area where aircraft will be spawned
    public float lambda = 1f / 30f; // Rate parameter for exponential distribution of time intervals
    private int aircraftCounter = 1; // Counter for spawned aircraft

    void Start()
    {
        // Check if the aircraftPrefab is correctly assigned
        if (aircraftPrefab == null)
        {
            //Debug.LogError("Aircraft prefab is not assigned in the Inspector.");
            return; // Exit if no prefab is assigned
        }

        // Start the coroutine to spawn aircraft over time with exponential intervals
        StartCoroutine(SpawnAircraftsWithExponentialIntervals());
    }

    IEnumerator SpawnAircraftsWithExponentialIntervals()
    {
        while (true) // Loop indefinitely or set a condition to stop
        {
            // Generate a random wait time based on an exponential distribution
            
            float waitTime = Mathf.Min(50,GenerateExponential(lambda));

            // Log the wait time
            //Debug.Log($"Waiting for {waitTime} seconds before spawning next aircraft.");

            // Wait for the calculated amount of time before spawning the next aircraft
            yield return new WaitForSeconds(waitTime);

            // Generate a random position within the specified spawn area
            Vector3 randomPosition = new Vector3(
                Random.Range(-spawnAreaSize.x / 2-500, spawnAreaSize.x / 2-500),
                300, // Set Y-axis at ground level
                Random.Range(-spawnAreaSize.z / 2-500, spawnAreaSize.z / 2-500)
            );

            // Instantiate the aircraft at the random position with a random rotation
            GameObject spawnedAircraft = Instantiate(aircraftPrefab, randomPosition, Quaternion.Euler(-90,0,0));

            if (spawnedAircraft != null)
            {
                spawnedAircraft.name = $"Aircraft{aircraftCounter}";
                aircraftCounter++; // Increment the counter for the next spawn
                AircraftController controller = spawnedAircraft.AddComponent<AircraftController>();
                FatoManager fatoManagerInstance = FindObjectOfType<FatoManager>();
                GateManager gatemanagerInstance = FindObjectOfType<GateManager>();
                controller.fatomanager = fatoManagerInstance;
                controller.gatemanager = gatemanagerInstance;
                //spawnedAircraft.AddComponent<AircraftController>();
                
              
                //Debug.Log($"{spawnedAircraft.name} spawned at {randomPosition} after {waitTime} seconds."); 

            }
        }
    }

    // Method to generate a random value based on exponential distribution
    float GenerateExponential(float lambda)
    {
        // Generate a uniformly distributed random number U between 0 (inclusive) and 1 (exclusive)
        float U = Random.value; // This is in the range [0, 1)

        // Apply inverse transform sampling to generate an exponential random variable
        return -Mathf.Log(U) / lambda;
    }
}