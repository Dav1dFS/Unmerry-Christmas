using UnityEngine;

public partial class Controller : MonoBehaviour
{
    [Header("Camera Control")]
    [SerializeField] private SmoothCameraFollow _cameraFollow;

    private void ToggleCameraOffset()
    {
        if (_cameraFollow != null)
        {
            _cameraFollow.ToggleOffset();
        }
    }
}