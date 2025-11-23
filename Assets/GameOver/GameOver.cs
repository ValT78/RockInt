using UnityEngine;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    public static GameOver Instance { get; private set; }
    [SerializeField] Button restartButton;
    public GameObject gameOverPanel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
        restartButton.onClick.AddListener(RestartGame);
    }



    void RestartGame()
    {
        gameOverPanel.SetActive(false);
        GameManager.Instance.ResetGame();
    }

}
