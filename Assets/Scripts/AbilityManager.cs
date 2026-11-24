using System;
using System.Collections.Generic;
using UnityEngine;

public enum AbilityEffect
{
    Auto,    // old behaviour: removes police if "Max Police Removed" > 0, otherwise stuns
    Remove,  // bombs: remove police at the chasing group
    Stun,    // police freeze completely (goes off on the player)
    Slow,    // police move slower (goes off on the player)
    Blind    // police lose track of you and walk to where you were (goes off on the player)
}

[Serializable]
public class AbilityDefinition
{
    public string abilityName;
    public float cost;
    public float cooldown;
    public float effectRadius;
    public int maxPoliceRemoved;
    [Tooltip("How long a Stun / Slow / Blind lasts, in seconds.")]
    public float disableDuration;
    public bool isNuke;
    public int requiredWantedLevel;

    [Header("Effect type")]
    public AbilityEffect effect = AbilityEffect.Auto;
    [Tooltip("Slow only. 0.4 means police move at 40% speed.")]
    [Range(0.05f, 1f)] public float slowMultiplier = 0.4f;
    [Tooltip("Untick on crowd-control abilities if you don't want the explosion effect on the player.")]
    public bool showExplosionEffect = true;

    [Header("Audio")]                                  // AUDIO
    public AudioClip activationSound;                  // AUDIO
    [Range(0f, 1f)] public float soundVolume = 1f;     // AUDIO

    [HideInInspector] public float cooldownTimer;
}

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager Instance;
    public List<AbilityDefinition> abilities = new List<AbilityDefinition>();
    public GameObject explosionEffectPrefab;

    [Tooltip("Optional. A circle-sprite prefab with the AbilityPulse script. Shows the ability's radius.")]
    public GameObject pulsePrefab;

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

        if (AudioManager.Instance != null)                                // AUDIO
            AudioManager.Instance.PlayAbility(ability);                   // AUDIO

        AbilityEffect effect = ResolveEffect(ability);

        // Bombs strike where the chasing group is.
        // Stun / Slow / Blind always go off ON the player, so the officers closest to you are always caught.
        Vector3 targetPosition = effect == AbilityEffect.Remove
            ? FindNearestPoliceClusterCenter(searchOrigin)
            : searchOrigin;

        if (ability.isNuke) ActivateNuke();
        else ApplyAreaEffect(targetPosition, ability, effect);

        if (ability.showExplosionEffect && explosionEffectPrefab != null)
            Instantiate(explosionEffectPrefab, targetPosition, Quaternion.identity);

        if (!ability.isNuke) SpawnPulse(targetPosition, ability.effectRadius, effect);

        return true;
    }

    // Old rows (Effect = Auto) keep working: removes if it has a removal count, otherwise stuns.
    static AbilityEffect ResolveEffect(AbilityDefinition a)
    {
        if (a.effect != AbilityEffect.Auto) return a.effect;
        if (a.maxPoliceRemoved > 0) return AbilityEffect.Remove;
        if (a.disableDuration > 0f) return AbilityEffect.Stun;
        return AbilityEffect.Remove;
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

    void ApplyAreaEffect(Vector3 position, AbilityDefinition ability, AbilityEffect effect)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, ability.effectRadius);
        int removed = 0;

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Police")) continue;
            PoliceUnit unit = hit.GetComponent<PoliceUnit>();
            if (unit == null) continue;

            switch (effect)
            {
                case AbilityEffect.Remove:
                    if (ability.maxPoliceRemoved <= 0 || removed >= ability.maxPoliceRemoved) break;
                    PolicePool.Instance.Return(hit.gameObject);
                    removed++;
                    GameManager.Instance.ReportPoliceKilled();
                    break;

                case AbilityEffect.Stun:
                    unit.Disable(ability.disableDuration);
                    break;

                case AbilityEffect.Slow:
                    unit.Slow(ability.slowMultiplier, ability.disableDuration);
                    break;

                case AbilityEffect.Blind:
                    unit.Blind(position, ability.disableDuration);
                    break;
            }
        }
    }

    void SpawnPulse(Vector3 position, float radius, AbilityEffect effect)
    {
        if (pulsePrefab == null) return;
        GameObject go = Instantiate(pulsePrefab, position, Quaternion.identity);
        AbilityPulse pulse = go.GetComponent<AbilityPulse>();
        if (pulse != null) pulse.Init(radius, PulseColor(effect));
    }

    static Color PulseColor(AbilityEffect effect)
    {
        switch (effect)
        {
            case AbilityEffect.Stun:  return new Color(1f, 0.92f, 0.2f);
            case AbilityEffect.Slow:  return new Color(0.4f, 0.9f, 0.5f);
            case AbilityEffect.Blind: return new Color(0.75f, 0.75f, 0.75f);
            default:                  return new Color(1f, 0.55f, 0.2f);
        }
    }

    void ActivateNuke()
    {
        foreach (var p in FindObjectsOfType<PoliceUnit>())
        {
            PolicePool.Instance.Return(p.gameObject);
            GameManager.Instance.ReportPoliceKilled();
        }
        GameManager.Instance.EndRun(GameManager.NukeCause);
    }
}