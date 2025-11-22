using UnityEngine;
using System.Collections;

public class SpotlightController : MonoBehaviour
{
    [Header("Spotlight Visual")]
    public Light spotlightVisual;
    public float oscillationSpeed = 2f;
    public float detectionTime = 20f;
    public GameObject floatingTextPrefab;
    public Transform parentCanvas;


    [Header("Score Settings")]
    public float pointsPerSecond = 10f;
    public float maxPoints = 50f;
    public float bonusPoints = 20f;

    private bool isOscillating = false;
    private float currentPoints = 0f;

    // Nouveau : nombre de joueurs dans l'orbe
    private int playersInside = 0;

    void Start()
    {
        SpawnAtRandomPosition();
    }

    void SpawnAtRandomPosition()
    {
        float x = Random.Range(GameManager.minX, GameManager.maxX);
        float z = Random.Range(GameManager.minZ, GameManager.maxZ);

        transform.position = new Vector3(x, transform.position.y, z);

        spotlightVisual.intensity = 160f;
        currentPoints = 0f;
        playersInside = 0;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Player>(out var player))
        {
            playersInside++;
            print(playersInside);
            // 1. Spawner le +5 visuel
            Instantiate(floatingTextPrefab, parentCanvas).GetComponent<FloatingText>().Initialize(Camera.main.WorldToScreenPoint(player.transform.position));

            // 2. Ajouter réellement +5 au score
            GameManager.Instance.AddScore(5);


            if (!isOscillating)
                StartCoroutine(OscillateThenRespawn());
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<Player>(out var player))
        {
            playersInside = Mathf.Max(0, playersInside - 1);
        }
    }

    IEnumerator OscillateThenRespawn()
    {
        isOscillating = true;
        float elapsed = 0f;
        float originalIntensity = spotlightVisual.intensity;

        while (elapsed < detectionTime && playersInside > 0)
        {
            // Variation très marquée entre 0 et x2
            float normalized = (Mathf.Sin(elapsed * oscillationSpeed) + 1f) / 2f;

            spotlightVisual.intensity = Mathf.Lerp(0f, originalIntensity * 2f, normalized);
            // Debug
            // Debug.Log("Intensity: " + spotlightVisual.intensity);

            // Ajout de points progressifs
            currentPoints += pointsPerSecond * Time.deltaTime;

            if (currentPoints >= maxPoints)
            {
                GameManager.Instance.AddScore(maxPoints);
                GameManager.Instance.AddScore(bonusPoints);
                break;
            }
            else
            {
                GameManager.Instance.AddScore(pointsPerSecond * Time.deltaTime);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Disparition
        spotlightVisual.intensity = 0f;

        // Pause avant respawn
        yield return new WaitForSeconds(0.5f);

        SpawnAtRandomPosition();
        spotlightVisual.intensity = 160f;

        isOscillating = false;
    }
}
