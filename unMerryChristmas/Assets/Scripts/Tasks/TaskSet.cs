using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An ordered collection of <see cref="TaskDefinition"/>s owned by one scene.
/// Drives the order (and membership) of the drawing-book Task List for that scene,
/// and is what a <see cref="SceneTaskRegistrar"/> registers with the
/// <see cref="TaskService"/> on load.
/// </summary>
[CreateAssetMenu(menuName = "UnMerry/Task Set", fileName = "TaskSet_")]
public class TaskSet : ScriptableObject
{
    [SerializeField] private List<TaskDefinition> _tasks = new();

    public IReadOnlyList<TaskDefinition> Tasks => _tasks;
}
