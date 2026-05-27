using System;
using UnityEngine;

public class DrawingPageCollectable : MonoBehaviour
{
    // Subscribe to react when any drawing page is collected (e.g. HUD "New note added" toast)
    public static event Action OnDrawingPageCollected;
    public void OnCollect()
    {
        OnDrawingPageCollected?.Invoke();
    }
}
