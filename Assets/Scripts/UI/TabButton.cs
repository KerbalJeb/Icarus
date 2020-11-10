using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TabButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    public                    Image         Image;
    [HideInInspector] public  TabGroup      tabGroup;
    public                    GameObject    content              { get; private set; }
    public                    RectTransform ContentRectTransform { get; private set; }
    [HideInInspector] public  string        tabName;
    [SerializeField]  private GameObject    tileButtonTemplate;
    [HideInInspector] public  bool          selected = false;


    private void Start()
    {
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

    public void SetContent(GameObject go)
    {
        content              = go;
        ContentRectTransform = content.GetComponent<RectTransform>();
    }

    public TileButton AddTile(Sprite sprite, TileSelector tileSelector)
    {
        var button     = Instantiate(tileButtonTemplate, content.transform);
        var tileButton = button.GetComponent<TileButton>();
        tileButton.SetSprite(sprite);
        tileButton.tileSelector = tileSelector;
        return tileButton;
    }
}
