using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class QTEManager : MonoBehaviour
{
    [SerializeField] private GameObject[] touchsPrefab;
    [SerializeField] private int numSpawning = 4;

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
        int rd = PickTouch();
        GameObject newUI = Instantiate(touchsPrefab[rd], transform.GetChild(0));
        newUI.transform.position = new Vector3(832, 700, 0);
        touchsPrefab[rd] = newUI;
    }

    private int PickTouch()
    {
        int randomIndex = Random.Range(0, touchsPrefab.Length);
        return randomIndex;
    }
}
