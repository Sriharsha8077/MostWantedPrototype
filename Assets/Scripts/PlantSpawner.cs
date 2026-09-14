using UnityEngine;

public class PlantSpawner : MonoBehaviour
{
    [Header("Ground")]
    public SpriteRenderer ground;

    [Header("Plants")]
    public GameObject[] plantPrefabs;
    public int plantCount = 150;

    [Header("Plant Size")]
    public float minScale = 0.7f;
    public float maxScale = 1.2f;

    void Start()
    {
        SpawnPlants();
    }

    void SpawnPlants()
    {
        if (ground == null)
        {
            Debug.LogError("Ground SpriteRenderer is not assigned!");
            return;
        }

        if (plantPrefabs.Length == 0)
        {
            Debug.LogError("No plant prefabs assigned!");
            return;
        }

        Bounds bounds = ground.bounds;

        for (int i = 0; i < plantCount; i++)
        {
            float x = Random.Range(bounds.min.x, bounds.max.x);
            float y = Random.Range(bounds.min.y, bounds.max.y);

            Vector3 position = new Vector3(x, y, 0);

            GameObject prefab =
                plantPrefabs[Random.Range(0, plantPrefabs.Length)];

            GameObject plant = Instantiate(
                prefab,
                position,
                Quaternion.Euler(0, 0, Random.Range(0f, 360f))
            );

            float scale = Random.Range(minScale, maxScale);
            plant.transform.localScale = prefab.transform.localScale * scale;

            // Make sure plants render above the ground
            SpriteRenderer sprite = plant.GetComponent<SpriteRenderer>();

            if (sprite != null)
            {
                sprite.sortingOrder = -1;
            }
        }
    }
}