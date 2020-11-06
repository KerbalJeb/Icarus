using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TabButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    [SerializeField] private GameObject    content;
    public                   Image         Image { get; private set; }
    [SerializeField] private TabGroup      tabGroup;
    public                   GameObject    Content       => content;
    public                   RectTransform ContentRectTransform { get; private set; }

    private void Start()
    {
        Image = GetComponent<Image>();
        ContentRectTransform = content.GetComponent<RectTransform>();
        tabGroup.Subscribe(this);
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        tabGroup.OnTabEnter(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        tabGroup.OnTabSelect(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tabGroup.OnTabExit(this);
    }
}
