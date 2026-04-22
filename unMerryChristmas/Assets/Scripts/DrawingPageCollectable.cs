using System;
using UnityEngine;

public class DrawingPageCollectable : MonoBehaviour
{
    // Subscribe to react when any drawing page is collected (e.g. HUD "New note added" toast)
    public static event Action OnDrawingPageCollected;

    /// <summary>
    /// Called by Controller when this object is collected.
    /// </summary>
    public void OnCollect()
    {
        OnDrawingPageCollected?.Invoke();
    }
}
