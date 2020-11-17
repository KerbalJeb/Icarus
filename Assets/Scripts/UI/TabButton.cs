using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    ///     The button for a UI tab
    /// </summary>
    public class TabButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
    {
        public                    Image         Image              = null;
        [HideInInspector] public  TabGroup      tabGroup           = null;
        [HideInInspector] public  string        tabName            = null;
        [SerializeField]  private GameObject    tileButtonTemplate = null;
        [HideInInspector] public  bool          selected           = false;
        public                    GameObject    content              { get; private set; }
        public                    RectTransform ContentRectTransform { get; private set; }


        private void Start()
        {
            tabGroup.Subscribe(this);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            tabGroup.OnTabSelect(this);
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            tabGroup.OnTabEnter(this);
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

        /// <summary>
        ///     Adds a new button the the content for this tab
        /// </summary>
        /// <param name="sprite">The sprite to use</param>
        /// <param name="tileSelector">The main tile selector</param>
        /// <returns></returns>
        public TileButton AddTile(Sprite sprite, TileSelector tileSelector)
        {
            GameObject button     = Instantiate(tileButtonTemplate, content.transform);
            var        tileButton = button.GetComponent<TileButton>();
            tileButton.SetSprite(sprite);
            tileButton.tileSelector = tileSelector;
            return tileButton;
        }
    }
}
