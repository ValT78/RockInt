using UnityEngine;
using TMPro;


public class Health : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI healthText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateHealth();
    }
    

    // Update is called once per frame
    public void UpdateHealth()
    {
        healthText.text = ""+GameManager.Instance.Health;
    }
}
