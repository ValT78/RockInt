using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public float Score { get; set; }

    [Header("Dancefloor Size")]
    public static float minX = -5f;
    public static float maxX = 5f;
    public static float minZ = -5f;
    public static float maxZ = 5f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ResetGame()
    {
        Score = 0;
        // Additional reset logic can be added here
    }

    public void AddScore(float points)
    {
        Score += points;
        ScoreManager.Instance.UpdateScore((int)Score);
    }
}
