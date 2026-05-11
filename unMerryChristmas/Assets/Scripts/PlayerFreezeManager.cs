using UnityEngine;

public class PlayerFreezeManager : MonoBehaviour
{
    public static PlayerFreezeManager Instance { get; private set; }

    private int  _watcherCount = 0;
    private bool _menuFrozen;

    // True when NPCs are watching OR a menu is open
    public bool isFrozen => _watcherCount > 0 || _menuFrozen;

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

    // Called by NPC detection system
    public void Freeze()   => _watcherCount++;
    public void UnFreeze() => _watcherCount = Mathf.Max(0, _watcherCount - 1);

    // Called by menus to block player input while open
    public void SetMenuFrozen(bool value) => _menuFrozen = value;
}
