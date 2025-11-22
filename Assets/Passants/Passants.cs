using UnityEngine;

public class Passants : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float speed = 1;
    [SerializeField] public Vector3 direction;
    [SerializeField] public int type = 1;
    [SerializeField] int damage = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        direction = new Vector3(Random.Range(-1f, 1f),
                                0f,
                                Random.Range(-1f, 1f)).normalized;;
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

    void OnTriggerEnter(Collider other)
    {
        
        Player player = other.GetComponent<Player>();

        if (player != null)
        {
            print("touché");
            GameManager.Instance.TakeDamage(damage);
        }
    }

}
