using UnityEngine;
using System.Collections.Generic;

public class GateManager : MonoBehaviour
{
    public Transform[] gates; // Array of gates (parking spots)
    public AndonLightController[] andonLights; // Array of Andon lights for each gate
    public int NofGates = 3;
    public Dictionary<Transform, Transform> occupiedGates = new Dictionary<Transform, Transform>(); // Dictionary to track gate occupancy

    // Dictionary to manage individual queues for each gate
    public Dictionary<Transform, Queue<Transform>> gateQueues = new Dictionary<Transform, Queue<Transform>>();

    // Dictionary to track max queue length for each gate
    private Dictionary<Transform, int> maxQueueLengths = new Dictionary<Transform, int>();

    // Method to initialize queues and max lengths for each gate
    void Start()
    {
        // Initialize a queue for each gate and set max queue length to 0
        foreach (Transform gate in gates)
        {
            gateQueues[gate] = new Queue<Transform>();
            maxQueueLengths[gate] = 0;
        }
    }

    // Method to check if a specific gate is occupied
    public bool IsGateOccupied(Transform gate)
    {
        return occupiedGates.ContainsKey(gate);
    }

    // Method to request an available gate; returns the gate if available, otherwise null
    public Transform RequestGate(Transform aircraft)
    {
        for (int i = 0; i < gates.Length; i++)
        {
            Transform gate = gates[i];
            //Debug.Log($"Checking gate: {gate.name} for aircraft: {aircraft.name}");

            if (!IsGateOccupied(gate))  // Using IsGateOccupied to check if the gate is free
            {
                occupiedGates[gate] = aircraft; // Assign the gate to the aircraft
                //Debug.Log($"{aircraft.name} assigned to {gate.name}");

                // Update the Andon light for this gate to show "Occupied"
                if (andonLights != null && i < andonLights.Length)
                {
                    andonLights[i].SetLightStatus(true); // Red for occupied
                }

                return gate; // Return the available gate
            }
        }

        // If no gate is available, add the aircraft to the shortest queue
        Transform chosenGate = FindGateWithShortestQueue();
        //Debug.Log($"{aircraft.name} found no available gates. Added to queue for {chosenGate.name}.");
        
        gateQueues[chosenGate].Enqueue(aircraft);
        maxQueueLengths[chosenGate] = Mathf.Max(maxQueueLengths[chosenGate], gateQueues[chosenGate].Count); // Track max queue length

        return null; // No gate was available
    }

    // Method to release a gate when the aircraft leaves
    public void ReleaseGate(Transform aircraft)
    {
        foreach (var entry in occupiedGates)
        {
            if (entry.Value == aircraft)
            {
                Transform gate = entry.Key;
                occupiedGates.Remove(gate);  // Release the gate
                UpdateAndonLight(gate, false);  // Update the Andon light to green (free)

                // If there is an aircraft waiting in the queue for this gate, assign it to the gate
                if (gateQueues[gate].Count > 0)
                {
                    Transform nextAircraft = gateQueues[gate].Dequeue(); // Dequeue the next aircraft for this gate
                    occupiedGates[gate] = nextAircraft; // Assign gate to next aircraft
                    //Debug.Log($"{nextAircraft.name} is assigned to {gate.name} from the queue.");
                    UpdateAndonLight(gate, true);  // Update the Andon light to red (occupied)
                }

                //Debug.Log($"{aircraft.name} left {gate.name}, gate is now free.");
                break;
            }
        }
    }

    // Method to find the gate with the shortest queue
    private Transform FindGateWithShortestQueue()
    {
        Transform chosenGate = gates[0];
        int minQueueLength = gateQueues[gates[0]].Count;

        foreach (Transform gate in gates)
        {
            if (gateQueues[gate].Count < minQueueLength)
            {
                chosenGate = gate;
                minQueueLength = gateQueues[gate].Count;
            }
        }

        return chosenGate;
    }

    // Method to update the Andon light for a gate
    private void UpdateAndonLight(Transform gate, bool isOccupied)
    {
        for (int i = 0; i < gates.Length; i++)
        {
            if (gates[i] == gate && i < andonLights.Length)
            {
                andonLights[i].SetLightStatus(isOccupied); // Red if occupied, green if free
                break;
            }
        }
    }

    // Method to get the max queue length for a specific gate
    public int GetMaxQueueLength(Transform gate)
    {
        return maxQueueLengths[gate];
    }
}