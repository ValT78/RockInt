using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public float Score { get; set; }

    public int Health { get; set;} = 5;

    [Header("Dancefloor Size")]
    public static float minX = -10f;
    public static float maxX = 10f;
    public static float minZ = -8f;
    public static float maxZ = 8f;

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

    public void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            print("Game Over");
            ResetGame();
        }
    }
}
