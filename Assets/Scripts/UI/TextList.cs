using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UI
{
    public class TextList : MonoBehaviour
    {
        [SerializeField] private GameObject       elementTemplate;
        [SerializeField] private Transform        viewPort;
        private                  List<GameObject> elements = new List<GameObject>();

        public void AddElement(string elementName, ShipDesigner shipDesigner)
        {
            var go = Instantiate(elementTemplate, viewPort);
            go.GetComponentInChildren<TextMeshProUGUI>().text = elementName;
            go.GetComponent<SavedShip>().ShipDesigner         = shipDesigner;
            elements.Add(go);
        }

        public void Empty()
        {
            foreach (GameObject element in elements)
            {
                Destroy(element);
            }
            elements.Clear();
        }
        
        
    }
}
