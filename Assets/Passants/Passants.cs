using UnityEngine;

public class Passants : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float speed;
    [SerializeField] private Vector3 direction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        direction = direction.normalized;
    }

    // Update is called once per frame
    void Update()
    {
        rb.linearVelocity = direction * speed;
    }
}
