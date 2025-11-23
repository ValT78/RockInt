using UnityEngine;

public class PassantsSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] Passants passantPrefab;

    [Header("Spawn control")]
    [SerializeField] int initialMaxPassants = 3;
    [Tooltip("Increase of allowed concurrent passants per second (float).")]
    [SerializeField] float passantsGrowthPerSecond = 0.02f; // example: 0.02 => +1 every 50s
    [SerializeField] float cooldown = 1f; // minimal time between spawns
    [SerializeField] float spawnRandomJitter = 0.3f; // optional jitter
    [SerializeField] float spawnMarginY = 1f;

    float cooldownTimer = 0f;
    Vector3[] spawnPoints;
    float elapsed = 0f;

    void Start()
    {
        spawnPoints = SpawnPoints();
        cooldownTimer = 0f;
    }

    void Update()
    {
        elapsed += Time.deltaTime;

        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        // compute dynamic target
        float dynamicMax = initialMaxPassants + elapsed * passantsGrowthPerSecond;
        int targetConcurrent = Mathf.FloorToInt(dynamicMax);

        int currentCount = GameManager.Instance.passants.Count;

        if (currentCount < targetConcurrent && cooldownTimer <= 0f)
        {
            Vector3 spawnPos = spawnPoints[Random.Range(0, spawnPoints.Length)];
            // small jitter inside border so they don't spawn exactly on corner
            spawnPos.x += Random.Range(-spawnRandomJitter, spawnRandomJitter);
            spawnPos.z += Random.Range(-spawnRandomJitter, spawnRandomJitter);
            spawnPos.y = spawnMarginY;

            Passants newP = Instantiate(passantPrefab, spawnPos, Quaternion.identity);
            newP.name = "Passant_" + Time.frameCount;
            GameManager.Instance.AddPassant(newP);

            // reset cooldown
            cooldownTimer = cooldown;
        }
    }

    Vector3[] SpawnPoints()
    {
        float[] border = new float[] { GameManager.minX, GameManager.maxX, GameManager.minZ, GameManager.maxZ };
        Vector3[] points = new Vector3[]
        {
            new Vector3(border[0], 1f, border[2]),
            new Vector3(border[0], 1f, border[3]),
            new Vector3(border[1], 1f, border[2]),
            new Vector3(border[1], 1f, border[3])
        };
        return points;
    }
}
