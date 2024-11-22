using System.Collections.Generic;
using UnityEngine;

public class FatoManager : MonoBehaviour
{
    public Transform[] Fato; // Array of FATO positions
    public Dictionary<Transform, Transform> UsageDictionary = new Dictionary<Transform, Transform>(); // Tracks which FATO is occupied by which aircraft
    public GateManager gateManager;

    public  Queue<Transform> FatoQueue = new Queue<Transform>(); // Queue for tracking aircraft waiting for FATO
    public Dictionary<Transform, float> waitingTimes = new Dictionary<Transform, float>(); // Track waiting times for each aircraft

    public float totalWaitTime = 0; // Sum of all wait times
    private int totalProcessedAirplanes = 0; // Total number of airplanes processed
    public int MaxQueueLength { get;  set; } = 0; // Track maximum queue length

    // Method to set the Fato when spawning
    public void SetFato(Transform[] fatoTransforms)
    {
        Fato = fatoTransforms;
        // Perform any initialization or setup needed here
    }

    // Method to check if a specific FATO is occupied
    public bool IsFatoOccupied(Transform fato)
    {
        return UsageDictionary.ContainsKey(fato);
    }

    // Method to request an available FATO; returns the FATO if available, otherwise adds the aircraft to the queue
    public Transform RequestFato(Transform aircraft)
    {   
        foreach (Transform fato in Fato)
        {   
            if (!IsFatoOccupied(fato)) // Check if the FATO is free
            {
                UsageDictionary[fato] = aircraft; // Assign the FATO to the aircraft
                print(UsageDictionary[fato]);
                //Debug.Log($"{aircraft.name} assigned to {fato.name}");
                return fato; // Return the assigned FATO
            }
            if (UsageDictionary[fato] == aircraft.transform)
            {
                return fato;
            }
            
            
        }
        

        // No FATO available, add the aircraft to the queue if it's not already in it


        return null; // No FATO was available
    }

    // Method to release a FATO when the aircraft leaves
    public void ReleaseFato(Transform aircraft)
    {
        // Find the FATO being used by the aircraft
        Transform fatoToRelease = null;

        foreach (var entry in UsageDictionary)
        {
            if (entry.Value == aircraft)
            {
                fatoToRelease = entry.Key; // Store the FATO being used
                break;
            }
        }

        if (fatoToRelease != null)
        {
            // Free the FATO
            UsageDictionary.Remove(fatoToRelease);
            //Debug.Log($"{aircraft.name} has left {fatoToRelease.name} FATO");

            // Process the next aircraft in the queue, if any
            if (FatoQueue.Count > 0)
            {
                Transform nextAircraft = FatoQueue.Dequeue(); // Get the next aircraft from the queue
                float waitTime =  waitingTimes[nextAircraft]; // Calculate the wait time
                totalWaitTime += waitTime;
               
                totalProcessedAirplanes++;
                 print($"total:{waitTime}, wait: {totalWaitTime}, airplanes: {totalProcessedAirplanes}"); // Increment the number of processed airplanes
                waitingTimes.Remove(nextAircraft); // Remove the aircraft from the waiting times dictionary

                //Debug.Log($"{nextAircraft.name} waited {waitTime:F2} seconds and is now assigned to {fatoToRelease.name}");
                if(gateManager.occupiedGates.Count==0){
                UsageDictionary[fatoToRelease] = nextAircraft;
                } // Assign the FATO to the next aircraft
            }
        }
        else
        {
            //Debug.LogWarning("Attempted to release FATO, but aircraft was not found in the UsageDictionary.");
        }
    }

    // Method to get the current queue length
    public int GetQueueLength()
    {
        return FatoQueue.Count;
    }

    // Method to calculate the average wait time
    public float GetAverageWaitTime()
    {
        if (totalProcessedAirplanes == 0) return 0;
        return (totalWaitTime / totalProcessedAirplanes)/5f;
    }
}