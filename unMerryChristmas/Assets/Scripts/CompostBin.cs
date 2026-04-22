using UnityEngine;

public class CompostBin : MonoBehaviour, IInteractable
{
    [SerializeField] private DrawingPageCollectable _drawingPage;
    [SerializeField] private Transform _lidTransform;
    [SerializeField] private float _openAngle = 90f;

    private bool _isOpen;

    // Called by Controller when player presses Interact near this object.
    // Requires: this GameObject is on the Pickup layer.
    public void Interact()
    {
        if (_isOpen) return;
        Open();
    }

    // Called when the ice-slide (Task 7) sends the elf into the bin.
    private void OnCollisionEnter(Collision collision)
    {
        if (!_isOpen) Open();
    }

    private void Open()
    {
        _isOpen = true;

        if (_lidTransform != null)
            _lidTransform.localRotation = Quaternion.Euler(_openAngle, 0f, 0f);

        if (_drawingPage != null)
            _drawingPage.gameObject.SetActive(true);
    }
}
