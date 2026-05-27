using System.Collections.Generic;
using UnityEngine;
public class TutorialPage : MonoBehaviour
{
    [SerializeField] private Transform   _entryContainer;
    [SerializeField] private AbilityEntry _entryPrefab;

    private static readonly PlayerAbility[] Starting = {
        PlayerAbility.Moving, PlayerAbility.Running, PlayerAbility.Jumping,
        PlayerAbility.Grabbing, PlayerAbility.Interacting, PlayerAbility.Releasing,
    };

    private static readonly PlayerAbility[] Unlockable = {
        PlayerAbility.Pushing, PlayerAbility.Rolling, PlayerAbility.Throwing,
        PlayerAbility.ChangingCostumes, PlayerAbility.ThrowingCandy, PlayerAbility.Ziplining,
        PlayerAbility.AnimatingElves, PlayerAbility.Hiding, PlayerAbility.ExplosingPresents,
        PlayerAbility.SequencedDance,
    };

    private readonly Dictionary<PlayerAbility, AbilityEntry> _entries = new();

    private void Start()
    {
        foreach (var ability in Starting)
            CreateEntry(ability, faded: true);

        foreach (var ability in Unlockable)
        {
            var entry = CreateEntry(ability, faded: false);
            entry.gameObject.SetActive(AbilityTokenManager.Instance.IsUnlocked(ability));
        }

        AbilityTokenManager.OnAbilityUnlocked += OnAbilityUnlocked;
    }

    private void OnDestroy() => AbilityTokenManager.OnAbilityUnlocked -= OnAbilityUnlocked;

    private AbilityEntry CreateEntry(PlayerAbility ability, bool faded)
    {
        var entry = Instantiate(_entryPrefab, _entryContainer);
        entry.Setup(ability, faded);
        _entries[ability] = entry;
        return entry;
    }

    private void OnAbilityUnlocked(PlayerAbility ability)
    {
        if (_entries.TryGetValue(ability, out var entry))
            entry.gameObject.SetActive(true);
    }

    public void Refresh()
    {
        foreach (var ability in Unlockable)
        {
            if (_entries.TryGetValue(ability, out var entry))
                entry.gameObject.SetActive(AbilityTokenManager.Instance.IsUnlocked(ability));
        }
    }
}
