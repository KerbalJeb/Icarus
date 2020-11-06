using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


public class InputManager
{
    private static InputManager instance;

    public static    InputManager              Instance      => instance ?? (instance = new InputManager());
    public static    PlayerInput               PlayerInput   => Instance.playerInput;
    public static    PlayerInput.PlayerActions PlayerActions => PlayerInput.Player;
    private readonly PlayerInput               playerInput;

    private InputManager()
    {
        playerInput = new PlayerInput();
        playerInput.Enable();
    }

    public static bool IsMouseOverClickableUI()
    {
        var pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Mouse.current.position.ReadValue();
        
        var raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData,raycastResults);

        return raycastResults.Any(raycastResult => !raycastResult.gameObject.CompareTag("Unclickable"));
    }
}

