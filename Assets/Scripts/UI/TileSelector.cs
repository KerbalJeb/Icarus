using System.Collections.Generic;
using TileSystem;
using UnityEngine;

namespace UI
{
    /// <summary>
    ///     Manages the UI elements related to selecting the tile to draw with when designing a ship
    /// </summary>
    public class TileSelector : MonoBehaviour
    {
        [SerializeField] private TabGroup         tabGroup      = null;
        [SerializeField] private ShipDesigner     shipDesigner  = null;
        [SerializeField] private Color            hoverColor    = Color.green;
        [SerializeField] private Color            selectedColor = Color.white;
        [SerializeField] private Color            defaultColor  = Color.gray;
        private readonly         List<TileButton> buttons       = new List<TileButton>();


        private TileButton selected;
        private TileSet    tileSet;

        private void Awake()
        {
            tileSet = TileSet.Instance;
        }

        private void OnEnable()
        {
            foreach (BasePart variant in tileSet.TileVariants)
            {
                if (variant == null) continue;
                TabButton  tab    = tabGroup.AddTab(variant.category);
                TileButton button = tab.AddTile(variant.previewImg, this);
                button.tileID = variant.partID;
                buttons.Add(button);

                if (selected is null)
                {
                    selected             = button;
                    selected.Image.color = selectedColor;
                    continue;
                }

                button.Image.color = defaultColor;
            }
        }

        public void OnButtonEnter(TileButton button)
        {
            button.Image.color = hoverColor;
        }

        public void OnButtonExit(TileButton button)
        {
            button.Image.color = selected != button ? defaultColor : selectedColor;
        }

        public void OnButtonPress(TileButton button)
        {
            ResetButtons();
            button.Image.color         = selectedColor;
            selected                   = button;
            shipDesigner.CurrentTileID = selected.tileID;
        }

        private void ResetButtons()
        {
            foreach (TileButton button in buttons) button.Image.color = defaultColor;
        }
    }
}
