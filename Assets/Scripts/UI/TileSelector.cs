using System;
using System.Collections.Generic;
using TileSystem;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace UI
{
    public class TileSelector : MonoBehaviour
    {
        private                  TileSet      tileSet;
        [SerializeField] private TabGroup     tabGroup;
        [SerializeField] private ShipDesigner shipDesigner;
        [SerializeField] private Color        hoverColor    = Color.green;
        [SerializeField] private Color        selectedColor = Color.white;
        [SerializeField] private Color        defaultColor  = Color.gray;
        

        private TileButton       selected;
        private List<TileButton> buttons = new List<TileButton>();

        private void Awake()
        {
            tileSet = TileSet.Instance;
        }

        private void OnEnable()
        {
            foreach (BasePart variant in tileSet.TileVariants)
            {
                var tab = tabGroup.AddTab(variant.category);
                var button = tab.AddTile(variant.previewImg, this);
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
            foreach (TileButton button in buttons)
            {
                button.Image.color = defaultColor;
            }
        }
    }
}
