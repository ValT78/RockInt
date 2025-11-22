using UnityEngine;
using System.Collections;

public class SpotlightController : MonoBehaviour
{
    [Header("Zone de spawn")]
    public float minX = -5f;
    public float maxX = 5f;
    public float minZ = -5f;
    public float maxZ = 5f;

    [Header("Spotlight Visual")]
    public Transform spotlightVisual;   // le Quad ou Sprite au sol
    public float oscillationAmplitude = 0.3f;
    public float oscillationSpeed = 4f;
    public float detectionTime = 5f;    // durée d'oscillation

    private bool isOscillating = false;

    void Start()
    {
        SpawnAtRandomPosition();
    }

    void SpawnAtRandomPosition()
    {
        float x = Random.Range(minX, maxX);
        float z = Random.Range(minZ, maxZ);
        transform.position = new Vector3(x, transform.position.y, z);
        spotlightVisual.localScale = Vector3.one; // reset taille
        gameObject.SetActive(true);
    }

    void OnTriggerEnter(Collider other)
    {
        // Vérifie si le collider a un composant PlayerScore
        PlayerScore player = other.GetComponent<PlayerScore>();
        if (player != null && !isOscillating)
        {
            StartCoroutine(OscillateThenRespawn());
        }
    }
    IEnumerator OscillateThenRespawn()
    {
        isOscillating = true;

        float elapsed = 0f;
        Vector3 originalScale = spotlightVisual.localScale;

        while (elapsed < detectionTime)
        {
            float scaleFactor = 1 + Mathf.Sin(elapsed * oscillationSpeed) * oscillationAmplitude;
            spotlightVisual.localScale = originalScale * scaleFactor;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Disparition instantanée
        gameObject.SetActive(false);

        // Petite pause avant réapparition
        yield return new WaitForSeconds(0.5f);

        // Réapparition ailleurs
        SpawnAtRandomPosition();
        isOscillating = false;
    }
}
