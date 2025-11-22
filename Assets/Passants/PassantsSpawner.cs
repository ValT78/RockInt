using UnityEngine;


public class PassantsSpawner : MonoBehaviour
{
    [SerializeField] Passants passant;
    //[SerializeField] Vector3[] directions;
    [SerializeField] int nbPassantsDepart;
    int nbPassants = 0;
    int currentSpawnIndex = 0;
    int nbspawnpts = 4;
    Vector3[] spawnPoints;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spawnPoints = SpawnPoints();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.passants.Count<nbPassantsDepart)
        {
            Passants curentPassant = Instantiate(passant, spawnPoints[Random.Range(0,nbspawnpts)], Quaternion.identity);
            curentPassant.name = "Passant"+nbPassants;
            GameManager.Instance.AddPassant(curentPassant);
        }
    }

    Vector3[] SpawnPoints()
    {
        //border = [minX,maxX,minY,maxY]
        float[] border = new float[]{GameManager.minX,GameManager.maxX,GameManager.minZ,GameManager.maxZ};
        Vector3[] points = new Vector3[]
        {
            new Vector3(border[0], 1f, border[2]),
            new Vector3(border[0], 1f, border[3]),
            new Vector3(border[1], 1f, border[2]),
            new Vector3(border[1], 1f, border[3])};
        return points;
    }
}
