using System;
using UnityEngine;

public class CollectableManager : MonoBehaviour
{
    public static CollectableManager Instance { get; private set; }

    public static event Action<int, int> OnPageCollected;   // (collected, total)
    public static event Action OnAllPagesCollected;

    public const int TotalPages = 20;
    public int CollectedCount { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Collect()
    {
        if (CollectedCount >= TotalPages) return;

        CollectedCount++;
        OnPageCollected?.Invoke(CollectedCount, TotalPages);

        if (CollectedCount >= TotalPages)
            OnAllPagesCollected?.Invoke();
    }
}
