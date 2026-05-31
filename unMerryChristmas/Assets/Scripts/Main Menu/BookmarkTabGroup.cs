using UnityEngine;
using System.Collections.Generic;

public class BookmarkTabGroup : MonoBehaviour
{
    [SerializeField] private List<BookmarkTab> _tabs;
    [SerializeField] private PageFlipController _pageFlip;

    private int _activeIndex = 0;
    private int _pendingIndex = -1;
    private int _visualIndex = 0; // tracks which tab looks selected independently

    private void Start()
    {
        for (int i = 0; i < _tabs.Count; i++)
        {
            int idx = i;
            _tabs[i].Init(idx, OnTabClicked);
        }
        _tabs[0].SetSelected(true);
        _pageFlip.ShowPageImmediate(0);
    }

    public void SelectTab(int index) => OnTabClicked(index);

    public void FocusCurrentPage() { } // intentionally empty — blink fix

    private void OnTabClicked(int index)
    {
        if (index == _visualIndex) return;

        // Update tab visuals immediately for responsiveness
        _tabs[_visualIndex].SetSelected(false);
        _visualIndex = index;
        _tabs[_visualIndex].SetSelected(true);

        if (_pageFlip.IsFlipping)
        {
            // Queue — flip will execute this when done
            _pendingIndex = index;
            return;
        }

        ExecuteFlip(index);
    }

    private void ExecuteFlip(int index)
    {
        _pendingIndex = -1;
        _activeIndex = index;
        _pageFlip.FlipToPage(index, OnFlipComplete);
    }

    private void OnFlipComplete()
    {
        if (_pendingIndex != -1 && _pendingIndex != _activeIndex)
            ExecuteFlip(_pendingIndex);
        else
            _pendingIndex = -1;
    }
}