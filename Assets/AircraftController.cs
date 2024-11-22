using System.Collections;
using System.Collections.Generic;
using System; 
using UnityEngine;

public class AircraftController : MonoBehaviour
{
    public float speed = 100f; // Speed of the aircraft
    public Vector3 areaSize = new Vector3(1000, 300, 1000); // Size of the area within which the aircraft can move

    public FatoManager fatomanager;
    public GateManager gatemanager;

    private Vector3 Randomtarget; // Current target position
    private Vector3 Heightadjuster = new Vector3(0, 10, 0);
    private bool hasArrivedAtFato = false;
    private bool hasArrivedAtGate = false;
    private bool hasDepartedAtFato = false;
    private bool hasDepartedAtGate = false;
    private bool isExiting = false;
    private Transform AssignedGate = null;
    private Transform AssignedFato = null;
    private Quaternion initialRotation;
    private float DepartFatoTime = 1f;
    private float LandingFatoTime = 1f;
    private float intervalTime = 1f;
    private float MinOccupyingTimeGate;
    
    
    private float TowingMachineTime;
    private DateTime Fatoarrivaltime;
    private DateTime Fatoarrivaltime2;
    private DateTime Gatearrivaltime;
    private DateTime GatedepartTime;
    private DateTime TowingStart;
    private DateTime entitygentime;

    // Define an exit point outside the scene
    private Vector3 exitPoint = new Vector3(1500, 100, 1500); // Adjust this as needed to represent leaving the scene

    void Start()
    {
        // Set an initial random target position
        fatomanager.FatoQueue.Enqueue(transform);
        MinOccupyingTimeGate = UnityEngine.Random.Range(8.2f, 12.6f);
        initialRotation = transform.rotation;
        Randomtarget = SetRandomTargetPosition();
        entitygentime = DateTime.Now;

    }

    void Update()
    {   
        //////////////////////////////  Request FATO and Gate  //////////////////////////////
        if (AssignedGate == null && AssignedFato == null && !hasArrivedAtGate)
        {
            MoveTowardsTarget(Randomtarget);

            // Request a FATO and a Gate if none is assigned
            //print($"total usage{fatomanager.UsageDictionary.Count + gatemanager.occupiedGates.Count}");
            if (fatomanager.UsageDictionary.Count + gatemanager.occupiedGates.Count < 3)
            {
                AssignedFato = fatomanager.RequestFato(transform);
        
            }

            // Check if the aircraft has reached close enough to the random target position
            if (Vector3.Distance(transform.position, Randomtarget) < 1f)
            {
                Randomtarget = SetRandomTargetPosition();
                //Debug.Log("New Random Target: " + Randomtarget);
            }
            fatomanager.waitingTimes[transform] = (float)(DateTime.Now-entitygentime).TotalSeconds;
            fatomanager.MaxQueueLength = Mathf.Max(fatomanager.MaxQueueLength, fatomanager.FatoQueue.Count);

        }

        if (AssignedFato != null && AssignedGate == null)
        {
            RequestGate(); 

            
        }

        //////////////////////////////// Proceed to FATO if FATO is assigned and not yet arrived //////////////////////////////
        if (AssignedFato != null && !hasArrivedAtFato && AssignedGate != null && !hasArrivedAtGate)
        {
            MoveTowardsTarget(AssignedFato.position + Heightadjuster);

            if (Vector3.Distance(transform.position, AssignedFato.position + Heightadjuster) < 0.1f)
            {
                hasArrivedAtFato = true;
                //Debug.Log("Arrived at FATO.");
                transform.rotation = Quaternion.Euler(-90, 90, 0);
                Fatoarrivaltime = DateTime.Now;
                TowingMachineTime = UnityEngine.Random.Range(0.0f, 1.0f);
                TowingStart =DateTime.Now;

            }
        }

        //////////////////////////////// Move towards the Gate if FATO has been reached and Gate assigned//////////////////////////////
        if (hasArrivedAtFato && AssignedGate != null && !hasArrivedAtGate && (DateTime.Now - Fatoarrivaltime).TotalSeconds >= LandingFatoTime+intervalTime)
        {  
            
            if((DateTime.Now-TowingStart).TotalSeconds>=TowingMachineTime){ 
            ReleaseFato();
            hasDepartedAtFato = true;
            MoveTowardsTarget(AssignedGate.position + Heightadjuster);

            if (Vector3.Distance(transform.position, AssignedGate.position + Heightadjuster) < 0.1f)
            {
                Gatearrivaltime = DateTime.Now;
                hasArrivedAtGate = true;
                //Debug.Log("Arrived at Gate.");
                
                // Now set to return to FATO
                AssignedFato = fatomanager.RequestFato(transform);
                hasArrivedAtFato = false; // Reset for returning to FATO
            }
            }
        }

        // After the aircraft arrives at the gate, set the FATO assignment to happen after a delay (occupying gate time)
        if (hasArrivedAtGate && AssignedFato == null && (DateTime.Now - Gatearrivaltime).TotalSeconds >= MinOccupyingTimeGate)
        {
            AssignedFato = fatomanager.RequestFato(transform); // Request the FATO after the aircraft has stayed at the gate for the required time
        }

        // Logic to depart from the gate and move to FATO
        if (!hasArrivedAtFato&&hasArrivedAtGate && AssignedFato != null && (DateTime.Now - Gatearrivaltime).TotalSeconds >= MinOccupyingTimeGate)
        {
            hasDepartedAtGate = true; // Mark that the aircraft has departed the gate
            MoveTowardsTarget(AssignedFato.position + Heightadjuster);

            if (Vector3.Distance(transform.position, AssignedFato.position + Heightadjuster) < 1f)
            {
                hasArrivedAtFato = true;
                Fatoarrivaltime2 = DateTime.Now;
                // Prepare to exit after arriving back at the FATO
            }
        }

        // Check if ready to exit
        if (hasDepartedAtGate && hasArrivedAtFato &&(DateTime.Now - Fatoarrivaltime2).TotalSeconds >= 2)
        {
            //Debug.Log("Exiting the system.");
            ReleaseFato();
            ReleaseGate();
            Destroy(gameObject);
        }
    }

    // Method to request a gate after FATO arrival
    void RequestGate()
    {
        if (AssignedGate == null)
        {
            AssignedGate = gatemanager.RequestGate(transform);
        }
    }

    // Method to set a new random target position within the defined area
    Vector3 SetRandomTargetPosition()
    {
        return new Vector3(
            UnityEngine.Random.Range(-areaSize.x / 2, areaSize.x / 2),
            UnityEngine.Random.Range(areaSize.y - 100, areaSize.y + 100),
            UnityEngine.Random.Range(-areaSize.z / 2, areaSize.z / 2)
        );
    }

    // Method to move the aircraft towards the current target position
    void MoveTowardsTarget(Vector3 target)
    {
        // Skip movement if aircraft has already exited
        if (isExiting) return;

        // Calculate the direction and move the aircraft
        Vector3 direction = (target - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        // Rotate the aircraft to face the target position
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            targetRotation *= Quaternion.Euler(-90, 90, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speed);
        }
    }

    public void ReleaseGate()
    {
        if (AssignedGate != null)
        {
            gatemanager.ReleaseGate(transform);
            AssignedGate = null; // Reset gate assignment
        }
    }

    public void ReleaseFato()
    {
        if (AssignedFato != null)
        {
            fatomanager.ReleaseFato(transform);
            AssignedFato = null; // Reset FATO assignment
        }
    }
}