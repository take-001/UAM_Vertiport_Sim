using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro; // Add this to use TextMeshPro

public class GateStatusUI : MonoBehaviour
{
    public GateManager gateManager; // Reference to the GateManager script
    public TextMeshProUGUI[] gateStatusTexts; // Array of TextMeshProUGUI UI elements for each gate's status and queue length

    void Update()
    {
        // Loop through all gates to update the status and queue length
        for (int i = 0; i < gateManager.gates.Length; i++)
        {
            Transform gate = gateManager.gates[i];

            // Get the current gate occupancy status
            string status = gateManager.IsGateOccupied(gate) ? "Occupied" : "Free";

            // Get the current queue length for the gate
            int currentQueueLength = gateManager.gateQueues[gate].Count;
            
            // Get the maximum queue length for the gate
            int maxQueueLength = gateManager.GetMaxQueueLength(gate);

            // Update the UI text for this gate
            gateStatusTexts[i].text = $"Gate {i + 1}: {status}\nQueue Length: {currentQueueLength}\nMax Queue Length: {maxQueueLength}";
        }
    }
}