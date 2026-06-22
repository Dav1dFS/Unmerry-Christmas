using UnityEngine;

/// <summary>
/// Place one per scene that owns tasks. On Awake it ensures the
/// <see cref="TaskService"/> exists and registers this scene's <see cref="TaskSet"/>
/// (under the scene's name) so the drawing-book Task List shows the right tasks while
/// the player is in this scene.
/// </summary>
public class SceneTaskRegistrar : MonoBehaviour
{
    [SerializeField] private TaskSet _taskSet;

    private void Awake()
    {
        if (_taskSet == null)
        {
            Debug.LogWarning("[SceneTaskRegistrar] No TaskSet assigned — this scene's " +
                             "tasks won't register. Assign one in the Inspector.", this);
            return;
        }

        TaskService.EnsureExists();
        TaskService.Register(_taskSet, gameObject.scene.name);
    }
}
