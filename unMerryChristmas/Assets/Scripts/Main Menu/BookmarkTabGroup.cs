using UnityEngine;
using System.Collections.Generic;

public class BookmarkTabGroup : MonoBehaviour
{
    [SerializeField] private List<BookmarkTab> _tabs;
    [SerializeField] private PageFlipController _pageFlip;

    private int _activeIndex = 0;

    private void Start()
    {
        for (int i = 0; i < _tabs.Count; i++)
        {
            int idx = i;
            _tabs[i].Init(idx, OnTabClicked);
        }
        // Mostra a primeira página sem animação
        _tabs[0].SetSelected(true);
        _pageFlip.ShowPageImmediate(0);
    }

    public void SelectTab(int index)
    {
        OnTabClicked(index);
    }

    private void OnTabClicked(int index)
    {
        if (index == _activeIndex) return;
        _tabs[_activeIndex].SetSelected(false);
        _activeIndex = index;
        _tabs[_activeIndex].SetSelected(true);
        _pageFlip.FlipToPage(index);
    }
}