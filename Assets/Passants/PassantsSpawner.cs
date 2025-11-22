using UnityEngine;


public class PassantsSpawner : MonoBehaviour
{
    [SerializeField] GameObject passant;
    [SerializeField] Vector3[] spawnPoints;
    //[SerializeField] Vector3[] directions;
    [SerializeField] int nbPassantsDepart;
    int nbPassants = 0;
    int currentSpawnIndex = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (nbPassants<nbPassantsDepart)
        {
            GameObject curentPassant = Instantiate(passant, spawnPoints[currentSpawnIndex], Quaternion.identity);
            curentPassant.name = "Passant"+nbPassants;
            Passants controller = passant.GetComponent<Passants>(); 
            //controller.direction = directions[currentSpawnIndex % directions.Length];
            currentSpawnIndex = (currentSpawnIndex + 1) % spawnPoints.Length;
            nbPassants++;
        }
    }
}
