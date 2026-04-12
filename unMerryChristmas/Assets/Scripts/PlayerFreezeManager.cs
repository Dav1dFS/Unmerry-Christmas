using UnityEngine;

public class PlayerFreezeManager : MonoBehaviour
{
    public static PlayerFreezeManager Instance { get; private set; }

    private int _watcherCount = 0;

    public bool isFrozen => _watcherCount > 0;

    void Awake()
    {
       if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void Freeze()
    {
        _watcherCount++;
    }

    public void UnFreeze()
    {
        // Only unfreezes when all npcs have stopped watching the player
        _watcherCount = Mathf.Max(0, _watcherCount - 1);
    }
}