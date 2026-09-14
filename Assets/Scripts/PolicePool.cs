using System.Collections.Generic;
using UnityEngine;

public class PolicePool : MonoBehaviour
{
    public static PolicePool Instance;
    public GameObject policePrefab;
    public int initialPoolSize = 200;

    private Queue<GameObject> pool = new Queue<GameObject>();

    void Awake()
    {
        Instance = this;
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject obj = Instantiate(policePrefab, transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject Get(Vector3 position)
    {
        GameObject obj = pool.Count > 0 ? pool.Dequeue() : Instantiate(policePrefab, transform);
        obj.transform.position = position;
        obj.SetActive(true);
        return obj;
    }

    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}