using System;
using System.Text;
using TMPro;
using UnityEngine;
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
}
