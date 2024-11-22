using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;



public class FatoQueueUI : MonoBehaviour
{
    public FatoManager fatoManager; // Reference to the FatoManager script

    // TextMeshProUGUI fields to display queue length, max queue length, and average wait time
    public TextMeshProUGUI queueLengthText; 
    public TextMeshProUGUI maxQueueLengthText;
    public TextMeshProUGUI avgWaitTimeText;

    void Update()
    {
        // Update the UI with the latest data from FatoManager
        queueLengthText.text = $"Queue Length: {fatoManager.GetQueueLength()}";
        maxQueueLengthText.text = $"Max Queue Length: {fatoManager.MaxQueueLength}";
        avgWaitTimeText.text = $"Average Wait Time: {fatoManager.GetAverageWaitTime():F2}";
    }
}