using UnityEngine;
public class BoxStackTracker : MonoBehaviour
{
    [SerializeField] private MovingBox[] _boxes;

    private int _opened;
    private bool _completed;

    private void OnEnable()
    {
        foreach (var box in _boxes)
            box.OnOpened += OnBoxOpened;
    }

    private void OnDisable()
    {
        foreach (var box in _boxes)
            box.OnOpened -= OnBoxOpened;
    }

    private void OnBoxOpened()
    {
        if (_completed) return;
        _opened++;
        if (_opened >= _boxes.Length)
        {
            _completed = true;
            BackYardTaskTracker.ReportTask(BackYardTaskTracker.TaskId.UnearthThePast);
        }
    }
}
