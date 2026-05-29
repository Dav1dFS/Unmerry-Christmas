using UnityEngine;
using System.Collections.Generic;

public class BookmarkTabGroup : MonoBehaviour
{
    [SerializeField] private List<BookmarkTab> _tabs;
    [SerializeField] private PageFlipController _pageFlip;

    private int _activeIndex = -1;

    private void Start()
    {
        for (int i = 0; i < _tabs.Count; i++)
        {
            int index = i;
            _tabs[i].Init(index, OnTabSelected);
        }

        SelectTab(0, instant: true);
    }

    public void SelectTab(int index)
    {
        SelectTab(index, false);
    }
    public void FocusCurrentPage()
    {
        if (_activeIndex >= 0 && _activeIndex < _tabs.Count)
            _pageFlip?.FocusCurrentPage();
    }

    private void OnTabSelected(int index)
    {
        SelectTab(index, false);
    }

    private void SelectTab(int index, bool instant)
    {
        if (index == _activeIndex)
            return;

        if (_activeIndex >= 0 && _activeIndex < _tabs.Count)
            _tabs[_activeIndex].SetSelected(false);

        _activeIndex = index;

        _tabs[_activeIndex].SetSelected(true);

        if (_pageFlip != null)
        {
            if (instant)
                _pageFlip.ShowPageImmediate(index);
            else
                _pageFlip.FlipToPage(index);
        }
    }
}