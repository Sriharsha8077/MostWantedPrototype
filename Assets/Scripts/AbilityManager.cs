using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AbilityDefinition
{
    public string abilityName;
    public float cost;
    public float cooldown;
    public float effectRadius;
    public int maxPoliceRemoved;
    public float disableDuration;
    public bool isNuke;
    public int requiredWantedLevel;
    [HideInInspector] public float cooldownTimer;
}

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager Instance;
    public List<AbilityDefinition> abilities = new List<AbilityDefinition>();
    public GameObject explosionEffectPrefab;
    public float clusterSearchRadius = 20f;
    public int clusterSampleCount = 20;

    void Awake() { Instance = this; }

    void Update()
    {
        foreach (var a in abilities)
            if (a.cooldownTimer > 0) a.cooldownTimer -= Time.deltaTime;
    }

    public bool TryActivate(int index, Vector3 searchOrigin)
    {
        if (index < 0 || index >= abilities.Count) return false;
        var ability = abilities[index];
        if (ability.cooldownTimer > 0) return false;
        if (!MoneyManager.Instance.TrySpend(ability.cost)) return false;

        ability.cooldownTimer = ability.cooldown;
        GameManager.Instance.ReportAbilityUsed();

        Vector3 targetPosition = FindNearestPoliceClusterCenter(searchOrigin);

        if (ability.isNuke) ActivateNuke();
        else ApplyAreaEffect(targetPosition, ability);

        if (explosionEffectPrefab != null)
            Instantiate(explosionEffectPrefab, targetPosition, Quaternion.identity);

        return true;
    }

    Vector3 FindNearestPoliceClusterCenter(Vector3 searchOrigin)
    {
        PoliceUnit[] allPolice = FindObjectsOfType<PoliceUnit>();
        if (allPolice.Length == 0) return searchOrigin;

        List<PoliceUnit> nearby = new List<PoliceUnit>();
        foreach (var p in allPolice)
            if (Vector2.Distance(searchOrigin, p.transform.position) <= clusterSearchRadius)
                nearby.Add(p);
        if (nearby.Count == 0) return searchOrigin;

        nearby.Sort((a, b) =>
            Vector2.Distance(searchOrigin, a.transform.position)
                .CompareTo(Vector2.Distance(searchOrigin, b.transform.position)));

        int take = Mathf.Min(clusterSampleCount, nearby.Count);
        Vector3 sum = Vector3.zero;
        for (int i = 0; i < take; i++) sum += nearby[i].transform.position;
        return sum / take;
    }

    void ApplyAreaEffect(Vector3 position, AbilityDefinition ability)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, ability.effectRadius);
        int removed = 0;

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Police")) continue;
            PoliceUnit unit = hit.GetComponent<PoliceUnit>();
            if (unit == null) continue;

            if (ability.maxPoliceRemoved > 0)
            {
                if (removed >= ability.maxPoliceRemoved) continue;
                PolicePool.Instance.Return(hit.gameObject);
                removed++;
                GameManager.Instance.ReportPoliceKilled();
            }
            else if (ability.disableDuration > 0)
            {
                unit.Disable(ability.disableDuration);
            }
        }
    }

    void ActivateNuke()
    {
        foreach (var p in FindObjectsOfType<PoliceUnit>())
        {
            PolicePool.Instance.Return(p.gameObject);
            GameManager.Instance.ReportPoliceKilled();
        }
        GameManager.Instance.EndRun("Nuclear Detonation");
    }
}