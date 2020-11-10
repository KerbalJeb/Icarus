using System;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class TabGroup : MonoBehaviour
{
    private                  List<TabButton> tabButtons    = new List<TabButton>();
    [SerializeField] private Color           hoverColor    = Color.green;
    [SerializeField] private Color           selectedColor = Color.white;
    [SerializeField] private Color           defaultColor  = Color.gray;
    [SerializeField] private ScrollRect      scrollRect;
    [SerializeField] private GameObject      buttonTemplate;
    [SerializeField] private GameObject      contentTemplate;
    [SerializeField] private Transform       viewPort;


    private                  TabButton                     selected =null;
    private                  Dictionary<string, TabButton> tabs = new Dictionary<string, TabButton>();
    

    public void Subscribe(TabButton button)
    {
        if (button.content is null)
        {
            Debug.LogWarning("Null Content for: " + button.gameObject.name);
        }
        tabButtons.Add(button);
        if (selected is null)
        {
            selected           = button;
            button.Image.color = selectedColor;
            selected.content.SetActive(true);
            scrollRect.content = selected.ContentRectTransform;
            return;
        }
        button.Image.color = defaultColor;
        button.content.SetActive(false);

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
        selected?.content.SetActive(false);
        selected           = button;
        selected.content.SetActive(true);
        scrollRect.content = selected.ContentRectTransform;
    }

    public TabButton AddTab(string tabName)
    {
        GameObject content = Instantiate(contentTemplate, viewPort);
        if (tabs.ContainsKey(tabName))
        {
            return tabs[tabName];
        }
        var button    = Instantiate(buttonTemplate, transform);
        var tabButton = button.GetComponent<TabButton>();
        tabButton.SetContent(content);
        tabButton.tabGroup = this;
        tabButton.tabName  = tabName;
        tabs[tabName]      = tabButton;
        return tabButton;
    }

    public void ResetTabs()
    {
        foreach (TabButton button in tabButtons)
        {
            button.Image.color = defaultColor;
        }
    }

}

