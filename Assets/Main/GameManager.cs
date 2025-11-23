using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    //[SerializeField] GameObject gameOverPanel;
    public static GameManager Instance { get; private set; }

    public float Score { get; set; }
    public int maxHP = 5;
    public int health { get; set;} = 5;

    public List<Passants> passants;

    [Header("Dancefloor Size")]
    public static float minX = -14f;
    public static float maxX = 8f;
    public static float minZ = -11f;
    public static float maxZ = 10f;

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
        health = maxHP;
        SceneManager.LoadScene(0);
        // Additional reset logic can be added here
    }

    public void AddScore(float points)
    {
        Score += points;
        if (ScoreManager.Instance != null) ScoreManager.Instance.UpdateScore((int)Score);
    }

    public void TakeDamage(int damage)
    {
        print("touché");
        health -= damage;
        if (Health.Instance != null) Health.Instance.UpdateHealth(health);

        if (health <= 0)
        {
            print("Game Over");
            GameOver.Instance.gameOverPanel.SetActive(true);
        }
    }

    public void AddPassant(Passants passant)
    {
        passants.Add(passant);
    }

    public void RemovePassant(Passants passant)
    {
        passants.Remove(passant);
    }
}
