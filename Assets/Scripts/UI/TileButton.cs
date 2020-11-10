using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class TileButton: MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
    {
        [SerializeField]  private Image        graphic =null;
        [HideInInspector] public  TileSelector tileSelector;
        public                    Image        Image;
        [HideInInspector] public  string       tileID;

        public void SetSprite(Sprite sprite)
        {
            graphic.sprite = sprite;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            tileSelector.OnButtonEnter(this);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            tileSelector.OnButtonPress(this);
        }

        public void OnPointerExit(PointerEventData  eventData)
        {
            tileSelector.OnButtonExit(this);
        }
    }
}
