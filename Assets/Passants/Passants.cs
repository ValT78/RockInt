using UnityEngine;

public class Passants : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float speed;
    [SerializeField] public Vector3 direction;
    [SerializeField] public int type = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        direction = direction.normalized;
    }

    // Update is called once per frame
    void Update()
    {
        rb.linearVelocity = move(type);
    }

    Vector3 move(int type)
    {
        if(type==1){
        return direction * speed;
        }
        return direction * speed;
    }

}
