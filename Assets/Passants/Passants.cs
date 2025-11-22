using UnityEngine;

public class Passants : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float speed = 1;
    [SerializeField] public Vector3 direction;
    [SerializeField] public int type;
    [SerializeField] int damage = 1;
    [SerializeField] float changeDirInterval = 2f;
    [SerializeField] float[] probaType = {0.9f, 0.1f};
    [SerializeField] Material materialBourre;
    float timer = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        direction = new Vector3(Random.Range(-1f, 1f),
                                0f,
                                Random.Range(-1f, 1f)).normalized;
        float alea=Random.Range(0f,1f);
        print(alea);
        float cumul = 0f;
        for(int i = 0; i < probaType.Length; i++)
        {
            cumul+=probaType[i];
            print(cumul);
            if (alea <= cumul)
            {
                type = i+1;
                
            }
            
        }
        if (type == 2)
        {
            GetComponent<Renderer>().material = materialBourre;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (type == 1)
        {
            rb.linearVelocity= direction*speed;
        }
        else if (type == 2)
        {
            timer += Time.deltaTime;

            if (timer >= changeDirInterval)
    {
                NewDirection();
                timer = 0f;
            rb.linearVelocity= direction*speed;
    }
        }
        if(transform.position.x < GameManager.minX || transform.position.x > GameManager.maxX || transform.position.z > GameManager.maxZ|| transform.position.z < GameManager.minZ)
        {
            print("out");
            GameManager.Instance.RemovePassant(this);
            Destroy(gameObject,2f);
        }
    }

    void NewDirection()
    {
        direction = new Vector3(
            Random.Range(-1f, 1f),
            0f,
            Random.Range(-1f, 1f)).normalized;
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
