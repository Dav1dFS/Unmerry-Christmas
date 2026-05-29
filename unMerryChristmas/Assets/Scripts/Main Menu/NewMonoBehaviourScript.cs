using UnityEngine;
using System.Collections.Generic;
using System;

public class TabGroup : MonoBehaviour
{
    [SerializeField] private List<BookmarkTab> tabs = new List<BookmarkTab>();

    public int CurrentIndex { get; private set; } = -1;

    public event Action<int> OnTabSelected;

    private void Awake()
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            tabs[i].Init(i, OnTabClicked);
        }
    }

    public void SelectTab(int index)
    {
        OnTabClicked(index);
    }

    private void OnTabClicked(int index)
    {
        if (index == CurrentIndex) return;

        CurrentIndex = index;

        for (int i = 0; i < tabs.Count; i++)
        {
            tabs[i].SetSelected(i == index);
        }

        OnTabSelected?.Invoke(index);
    }

    public void SelectFirst()
    {
        if (tabs.Count > 0)
            OnTabClicked(0);
    }
}