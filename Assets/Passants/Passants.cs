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
    [SerializeField] float delaiAvantBourre = 6f;
    [SerializeField] Vector3 centreScene = Vector3.zero;
    float timer = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        Vector3 dir = (centreScene - transform.position).normalized;

        // Limiter l’angle pour qu’il reste dans le bon quart
        float maxAngle = 45f; 
        float angle = Random.Range(-maxAngle, maxAngle);

        direction = Quaternion.Euler(0, angle, 0) * dir;
        direction.y = 0f;
    
        float alea=Random.Range(0f,1f);
        float cumul = 0f;
        for(int i = 0; i < probaType.Length; i++)
        {
            cumul+=probaType[i];
            if (alea <= cumul)
            {
                type = i+1;
                break;
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
            if (timer >= delaiAvantBourre)
            {
                NewDirection();
                timer -= changeDirInterval;
            }
            rb.linearVelocity= direction*speed;
        }
        if(transform.position.x < GameManager.minX || transform.position.x > GameManager.maxX || transform.position.z > GameManager.maxZ|| transform.position.z < GameManager.minZ)
        {
            GameManager.Instance.RemovePassant(this);
            Destroy(gameObject,0f);
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
