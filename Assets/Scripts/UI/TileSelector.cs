﻿using System;
using System.Collections.Generic;
using TileSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UI
{
    /// <summary>
    ///     Manages the UI elements related to selecting the tile to draw with when designing a ship
    /// </summary>
    public class TileSelector : MonoBehaviour
    {
        [SerializeField] private TabGroup                            tabGroup;
        [SerializeField] private ShipDesigner                        shipDesigner;
        [SerializeField] private Color                               hoverColor    = Color.green;
        [SerializeField] private Color                               selectedColor = Color.white;
        [SerializeField] private Color                               defaultColor  = Color.gray;
        private readonly         List<TileButton>                    buttons       = new List<TileButton>();
        private                  Action<InputAction.CallbackContext> reset;


        private TileButton selected;
        private TileSet    tileSet;

        private void Awake()
        {
            tileSet = TileSet.Instance;
            reset   = context => ResetButtons();
        }

        private void OnEnable()
        {
            foreach (BasePart variant in tileSet.TileVariants)
            {
                if (variant == null || !variant.showInPartSelector) continue;
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

            InputManager.PlayerActions.Escape.performed += reset;
        }

        private void OnDisable()
        {
            InputManager.PlayerActions.Escape.performed -= reset;
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

        public void ResetButtons()
        {
            foreach (TileButton button in buttons) button.Image.color = defaultColor;
        }
    }
}
