using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AndonLightController : MonoBehaviour
{
    private Renderer andonRenderer;

    void Start()
    {
        andonRenderer = GetComponent<Renderer>();
        SetLightStatus(false); // Initialize to "free"
    }

    // Call this method to change the light color
    public void SetLightStatus(bool isOccupied)
    {
        if (isOccupied)
        {
            andonRenderer.material.color = Color.red; // Occupied
        }
        else
        {
            andonRenderer.material.color = Color.green; // Free
        }
    }
}