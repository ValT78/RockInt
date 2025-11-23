using UnityEngine;
using TMPro;


public class Health : MonoBehaviour
{
    public static Health Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI healthText;
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
    }
    
    void Start()
    {
        UpdateHealth(0);
    }
    

    // Update is called once per frame
    public void UpdateHealth(int health)
    {
        healthText.text = ""+health;
    }
}
