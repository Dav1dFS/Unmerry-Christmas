using UnityEngine;
using UnityEngine.InputSystem;
public class DrawingBookMenu : MonoBehaviour
{
    [SerializeField] private GameObject       _bookPanel;
    [SerializeField] private TutorialPage     _tutorialPage;
    [SerializeField] private DrawingPagesPage _drawingPagesPage;
    [SerializeField] private TaskListPage     _taskListPage;
    [SerializeField] private InputAction      _toggleBookAction;


    private bool _open;

    private void OnEnable()
    {
        _toggleBookAction.Enable();
        _toggleBookAction.started += _ => Toggle();
    }

    private void OnDisable()
    {
        _toggleBookAction.started -= _ => Toggle();
        _toggleBookAction.Disable();
    }

    private void Start() => _bookPanel.SetActive(false);

    public void Toggle()
    {
        _open = !_open;
        _bookPanel.SetActive(_open);
        PlayerFreezeManager.Instance?.SetMenuFrozen(_open);

        if (_open)
        {
            _tutorialPage?.Refresh();
            _drawingPagesPage?.Refresh();
            _taskListPage?.Refresh();
        }
    }
}
