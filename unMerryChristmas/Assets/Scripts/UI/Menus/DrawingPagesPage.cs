using TMPro;
using UnityEngine;
public class DrawingPagesPage : MonoBehaviour
{
    [SerializeField] private TMP_Text _countLabel;

    public void Refresh()
    {
        int collected = CollectableManager.Instance != null
            ? CollectableManager.Instance.CollectedCount
            : 0;
        _countLabel.text = $"{collected} / {CollectableManager.TotalPages}";
    }
}
