using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UI
{
    /// <summary>
    ///     A list of UI text elements (used for selecting save)
    /// </summary>
    public class TextList : MonoBehaviour
    {
        [SerializeField] private GameObject       elementTemplate;
        [SerializeField] private Transform        viewPort;
        private readonly         List<GameObject> elements = new List<GameObject>();

        public void AddElement(string elementName, ShipDesigner shipDesigner)
        {
            GameObject go = Instantiate(elementTemplate, viewPort);
            go.GetComponentInChildren<TextMeshProUGUI>().text = elementName;
            go.GetComponent<SavedShip>().ShipDesigner         = shipDesigner;
            elements.Add(go);
        }

        public void Empty()
        {
            foreach (GameObject element in elements) Destroy(element);
            elements.Clear();
        }
    }
}
