using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class QTEManager : MonoBehaviour
{
    [SerializeField] private GameObject[] touchsPrefab;
    [SerializeField] private float platformLenghtY = 100f;
    [SerializeField] private int numSpawning = 12;

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
        GameObject newUI = Instantiate(PickTouch(), new Vector3(1870, i*platformLenghtY+140, 0), Quaternion.identity, transform.Find("Line"));
    }

    public void Checkmark(float y)
    {
        GameObject check = Instantiate(touchsPrefab[6], new Vector3(1870, y+540, 0), Quaternion.identity, transform.Find("Line"));
    }

    private GameObject PickTouch()
    {
        int randomIndex = Random.Range(0, touchsPrefab.Length-1);
        return touchsPrefab[randomIndex];
    }
}
