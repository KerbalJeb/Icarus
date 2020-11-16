using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
///     A singleton used to store a reference to the input sytem
/// </summary>
public class InputManager
{
    private static   InputManager instance;
    private readonly PlayerInput  playerInput;

    private InputManager()
    {
        playerInput = new PlayerInput();
        playerInput.Enable();
    }

    public static InputManager              Instance      => instance ?? (instance = new InputManager());
    public static PlayerInput               PlayerInput   => Instance.playerInput;
    public static PlayerInput.PlayerActions PlayerActions => PlayerInput.Player;

    /// <summary>
    ///     Checks if the mouse is over a UI element (excluding ones with the tag "Unclickable")
    /// </summary>
    /// <returns>True if the mouse is over a UI element, false otherwise</returns>
    public static bool IsMouseOverClickableUI()
    {
        var pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Mouse.current.position.ReadValue();

        var raycastResults = new List<RaycastResult>();

        if (EventSystem.current == null)
        {
            return false;
        }
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);

        return raycastResults.Any(raycastResult => !raycastResult.gameObject.CompareTag("Unclickable"));
    }
}
