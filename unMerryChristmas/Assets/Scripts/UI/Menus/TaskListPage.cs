using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public class TaskListPage : MonoBehaviour
{
    [SerializeField] private TMP_Text _listLabel;

    private static readonly (BackYardTaskTracker.TaskId id, string label)[] TaskLabels =
    {
        (BackYardTaskTracker.TaskId.MainClimbWindow,    "Climb through the Kitchen Window"),
        (BackYardTaskTracker.TaskId.GardenChaos,        "Garden Chaos"),
        (BackYardTaskTracker.TaskId.GnomeParade,        "Gnome Parade"),
        (BackYardTaskTracker.TaskId.UnearthThePast,     "Unearth the Past"),
        (BackYardTaskTracker.TaskId.LightsOut,          "Lights Out"),
        (BackYardTaskTracker.TaskId.BurstPipe,          "Burst Pipe"),
        (BackYardTaskTracker.TaskId.IceSlide,           "Ice Slide"),
        (BackYardTaskTracker.TaskId.BirdbathDemolition, "Birdbath Demolition"),
        (BackYardTaskTracker.TaskId.ShedHeist,          "Shed Heist"),
        (BackYardTaskTracker.TaskId.TheHighWall,        "The High Wall"),
    };

    public void Refresh()
    {
        // Scenes on the new scene-agnostic task system (e.g. the Kitchen) register a
        // TaskSet for their scene; render that instead of the Backyard table. When no
        // set is registered for the active scene, fall through to the Backyard path
        // below, unchanged.
        var set = TaskService.GetSetForScene(SceneManager.GetActiveScene().name);
        if (set != null)
        {
            RefreshFromSet(set);
            return;
        }

        var sb = new StringBuilder();
        foreach (var (id, label) in TaskLabels)
        {
            bool done   = BackYardTaskTracker.IsCompleted(id);
            bool locked = !done && !BackYardTaskTracker.IsAvailable(id);
            string prefix = done ? "✓" : locked ? "○" : "—";
            string color  = done ? "green" : locked ? "grey" : "white";
            sb.AppendLine($"<color={color}>{prefix}  {label}</color>");
        }
        _listLabel.text = sb.ToString();
    }

    // Render a scene's TaskSet from the scene-agnostic TaskService, using the same
    // glyphs/colours as the Backyard list above.
    private void RefreshFromSet(TaskSet set)
    {
        var sb = new StringBuilder();
        foreach (var def in set.Tasks)
        {
            if (def == null) continue;
            bool done   = TaskService.IsCompleted(def);
            bool locked = !done && !TaskService.IsAvailable(def);
            string prefix = done ? "✓" : locked ? "○" : "—";
            string color  = done ? "green" : locked ? "grey" : "white";
            sb.AppendLine($"<color={color}>{prefix}  {def.DisplayName}</color>");
        }
        _listLabel.text = sb.ToString();
    }
}
