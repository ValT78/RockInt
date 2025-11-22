using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class QTEManager : MonoBehaviour
{
    [SerializeField] private GameObject[] touchsPrefab;
    [SerializeField] private int numSpawning = 6;
    [SerializeField] private float touchLenghtY = 100f;

    public static QTEManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        for (int i = 0; i < numSpawning; i++)
        {
            SpawnTouch(i);
        }
    }

    public void SpawnTouch()
    {
        SpawnTouch(numSpawning - 1);
    }

    private void SpawnTouch(int i)
    {
        GameObject newUI = Instantiate(PickTouch(), new Vector3(391, 300, 0), Quaternion.identity);
    }

    private GameObject PickTouch()
    {
        int randomIndex = Random.Range(0, touchsPrefab.Length);
        return touchsPrefab[randomIndex];
    }
}
