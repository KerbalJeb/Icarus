using UnityEngine;

namespace UI
{
    /// <summary>
    /// Used to create 'pop up' windows that are closed by default
    /// </summary>
    public class PopUp : MonoBehaviour
    {
        private void Awake()
        {
            gameObject.SetActive(false);
        }

        public void Open()
        {
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}
