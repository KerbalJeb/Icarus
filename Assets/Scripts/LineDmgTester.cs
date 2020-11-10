using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/// <summary>
///     A class to test applying damage without having weapons
/// </summary>
public class LineDmgTester : MonoBehaviour
{
    private readonly RaycastHit2D[] hits = new RaycastHit2D[20];
    private          Camera         cam;
    private          Vector3        endPos;
    private          bool           pressed;
    private          Vector3        startPos;

    private void Start()
    {
        cam                                         =  Camera.main;
        InputManager.PlayerActions.DmgTest.started  += StartDrawingLine;
        InputManager.PlayerActions.DmgTest.canceled += StopDrawingLine;
    }

    private void OnDestroy()
    {
        InputManager.PlayerActions.DmgTest.started  -= StartDrawingLine;
        InputManager.PlayerActions.DmgTest.canceled -= StopDrawingLine;
    }


    public void StartDrawingLine(InputAction.CallbackContext context)
    {
        Vector2Control mousePos = Mouse.current.position;
        startPos = cam.ScreenToWorldPoint(new Vector3(mousePos.x.ReadValue(), mousePos.y.ReadValue()));
        pressed  = true;
    }

    public void StopDrawingLine(InputAction.CallbackContext context)
    {
        Vector2Control mousePos = Mouse.current.position;
        endPos  = cam.ScreenToWorldPoint(new Vector3(mousePos.x.ReadValue(), mousePos.y.ReadValue()));
        pressed = false;
        var dmg = new Damage(startPos, endPos, 5000f);
        dmg.ApplyDamage();
    }
}
