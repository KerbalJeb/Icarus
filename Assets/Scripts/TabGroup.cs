using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabGroup : MonoBehaviour
{
    [SerializeField] private List<TabButton> tabButtons    =new List<TabButton>();
    [SerializeField] private Color           hoverColor    = Color.green;
    [SerializeField] private Color           selectedColor = Color.white;
    [SerializeField] private Color           defaultColor  = Color.gray;
    [SerializeField] private ScrollRect      scrollRect;
    private                  TabButton       selected=null;

    public void Subscribe(TabButton button)
    {
        if (button.Content is null)
        {
            Debug.LogWarning("Null Content for: " + button.gameObject.name);
        }
        tabButtons.Add(button);
        if (selected is null)
        {
            selected           = button;
            button.Image.color = selectedColor;
            selected.Content.SetActive(true);
            scrollRect.content = selected.ContentRectTransform;
            return;
        }
        button.Image.color = defaultColor;
        button.Content.SetActive(false);

    }

    public void OnTabEnter(TabButton button)
    {
        button.Image.color = hoverColor;
    }

    public void OnTabExit(TabButton button)
    {
        button.Image.color = selected != button ? defaultColor : selectedColor;
    }

    public void OnTabSelect(TabButton button)
    {
        ResetTabs();
        button.Image.color = selectedColor;
        selected?.Content.SetActive(false);
        selected           = button;
        selected.Content.SetActive(true);
        scrollRect.content = selected.ContentRectTransform;
    }

    public void ResetTabs()
    {
        foreach (TabButton button in tabButtons)
        {
            button.Image.color = defaultColor;
        }
    }
}

