using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class AbilityTokenManager : MonoBehaviour
{
    public static AbilityTokenManager Instance { get; private set; }

    [SerializeField] private List<PlayerAbility> _unlockedByDefault = new();

    private readonly HashSet<PlayerAbility> _unlockedAbilities = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        foreach (var ability in _unlockedByDefault)
        {
            _unlockedAbilities.Add(ability);
        }
    }
    public bool IsUnlocked(PlayerAbility ability) => _unlockedAbilities.Contains(ability);

    public void Unlock(PlayerAbility ability)
    {
        if (_unlockedAbilities.Add(ability))
        {
            Debug.Log($"Unlocked ability: {ability}");
        }
        else
        {
            Debug.LogWarning($"Ability {ability} is already unlocked.");
        }
    }
}
