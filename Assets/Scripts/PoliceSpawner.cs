using UnityEngine;

public class PoliceSpawner : MonoBehaviour
{
    public Transform player;
    public float minSpawnRadius = 9f;
    public float maxSpawnRadius = 12f;
    private float timer = 0f;

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = DifficultyManager.Instance.GetSpawnInterval();
            int count = DifficultyManager.Instance.GetSpawnCountPerWave();
            for (int i = 0; i < count; i++) SpawnOne();
        }
    }

    void SpawnOne()
    {
        float angle;
        int biasedSector = AdaptivePlayerProfile.Instance.GetBiasedSectorForSpawning();

        // Only kicks in at higher wanted levels — the "it noticed" moment, not from run 1
        if (biasedSector >= 0 && DifficultyManager.Instance.WantedLevel >= 3 && Random.value < 0.4f)
            angle = biasedSector * 45f + Random.Range(-20f, 20f);
        else
            angle = Random.Range(0f, 360f);

        float radius = Random.Range(minSpawnRadius, maxSpawnRadius);
        float rad = angle * Mathf.Deg2Rad;
        Vector3 spawnPos = player.position + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * radius;

        GameObject obj = PolicePool.Instance.Get(spawnPos);
        PoliceUnit unit = obj.GetComponent<PoliceUnit>();
        unit.playerTransform = player;
        unit.playerRb = player.GetComponent<Rigidbody2D>();

        GameManager.Instance.ReportPoliceCount(GameObject.FindGameObjectsWithTag("Police").Length);
    }
}