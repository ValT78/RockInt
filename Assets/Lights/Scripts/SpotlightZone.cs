using Unity.VisualScripting;
using UnityEngine;

public class SpotlightZone : MonoBehaviour {
  

    private bool p1Inside = false;
    private bool p2Inside = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player1"))
            p1Inside = true;

        if (other.CompareTag("Player2"))
            p2Inside = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player1"))
            p1Inside = false;

        if (other.CompareTag("Player2"))
            p2Inside = false;
    }

    void Update()
    {
    
    }
}
