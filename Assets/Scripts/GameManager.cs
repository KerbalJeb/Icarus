using System;
using UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public  PopUp                               closePopUp;
    private Action<InputAction.CallbackContext> quitAction;
    private ShipDesigner                        shipDesigner;

    private void Awake()
    {
        quitAction = context => Quit();
    }

    private void Start()
    {
        shipDesigner = GetComponent<ShipDesigner>();
    }

    private void OnEnable()
    {
        InputManager.PlayerActions.Escape.performed += quitAction;
    }

    private void OnDisable()
    {
        InputManager.PlayerActions.Escape.performed -= quitAction;
    }

    public void LoadDesigner()
    {
        SceneManager.LoadScene("Scenes/ShipBuildTesting");
    }

    public void Quit()
    {
        closePopUp.Open();
    }

    public void ExitGameNow()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
