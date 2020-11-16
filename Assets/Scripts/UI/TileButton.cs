using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    ///     A button used to select the tile to use for 'drawing' with the ship designer
    /// </summary>
    public class TileButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
    {
        [SerializeField]  private Image        graphic = null;
        [HideInInspector] public  TileSelector tileSelector;
        public                    Image        Image;
        [HideInInspector] public  string       tileID;

        public void OnPointerClick(PointerEventData eventData)
        {
            tileSelector.OnButtonPress(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            tileSelector.OnButtonEnter(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            tileSelector.OnButtonExit(this);
        }

        public void SetSprite(Sprite sprite)
        {
            graphic.sprite = sprite;
        }
    }
}
