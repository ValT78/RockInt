using UnityEngine;
using System.Collections;

public class SpotlightController : MonoBehaviour
{
    [Header("Spotlight Visual")]
    public Light spotlightVisual;   // le Quad ou Sprite au sol
    public float oscillationAmplitude = 160f;
    public float oscillationSpeed = 2f;
    public float detectionTime = 20f;    // durée d'oscillation

    [Header("Score Settings")]
    public float pointsPerSecond = 10f; // points gagnés par seconde
    public float maxPoints = 50f;       // max points que le faisceau peut donner
    public float bonusPoints = 20f;     // bonus si max atteint

    private bool isOscillating = false;
    private float currentPoints = 0f;
    private bool playerInside = false;

    void Start()
    {
        SpawnAtRandomPosition();
    }

    void SpawnAtRandomPosition()
    {
        float x = Random.Range(GameManager.minX, GameManager.maxX);
        float z = Random.Range(GameManager.minZ, GameManager.maxZ);
        transform.position = new Vector3(x, transform.position.y, z);
        spotlightVisual.intensity = 160f; // reset intensity 
        currentPoints = 0f;
        playerInside = false;
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Player>(out var player))
        {
            playerInside = true;
            if (!isOscillating)
                StartCoroutine(OscillateThenRespawn(player));
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<Player>(out var player))
        {
            playerInside = false;
        }
    }

    IEnumerator OscillateThenRespawn(Player player)
    {
        isOscillating = true;
        float elapsed = 0f;
        float originalIntensity = spotlightVisual.intensity;

        while (elapsed < detectionTime && playerInside)
        {
            // Oscillation du faisceau
            float normalized = (Mathf.Sin(elapsed * oscillationSpeed) + 1f) / 2f;
            spotlightVisual.intensity = Mathf.Lerp(0f, originalIntensity * 2f, normalized);
            print(spotlightVisual.intensity);

            // Ajout de points progressif
            currentPoints += pointsPerSecond * Time.deltaTime;
            if (currentPoints >= maxPoints)
            {
                GameManager.Instance.AddScore(maxPoints);   // ajoute les points max
                GameManager.Instance.AddScore(bonusPoints); // bonus pour max atteint
                break; // fin du faisceau
            }
            else
            {
                GameManager.Instance.AddScore(pointsPerSecond * Time.deltaTime);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Disparition instantanée
        spotlightVisual.intensity = 0f;

        // Petite pause avant réapparition
        yield return new WaitForSeconds(0.5f);

        // Réapparition ailleurs

        SpawnAtRandomPosition();
        spotlightVisual.intensity = 160f;
        isOscillating = false;
    }
}
