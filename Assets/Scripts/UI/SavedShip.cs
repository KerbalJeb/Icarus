using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class SavedShip : MonoBehaviour, IPointerClickHandler
    {
        private string       text;
        public  ShipDesigner ShipDesigner { private get; set; }

        private void Awake()
        {
            text = GetComponentInChildren<TextMeshProUGUI>().text;
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            ShipDesigner.LoadDesign(text);
        }
    }
}
