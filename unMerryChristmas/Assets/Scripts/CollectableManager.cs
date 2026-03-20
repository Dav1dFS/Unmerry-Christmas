using UnityEngine;

public class CollectableManager : MonoBehaviour
{

    public static CollectableManager Instance { get; private set; }

    [SerializeField] private int TotalCollectables = 3;
    private int CollectableCollected = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void Collect()
    { CollectableCollected++;
        Debug.Log($"Collectable collected! Total collected: {CollectableCollected}/{TotalCollectables}");
        if (CollectableCollected >= TotalCollectables)
        {
            Debug.Log("All collectables collected!");
            // You can trigger any event or action here when all collectables are collected.
        }
    }

}
