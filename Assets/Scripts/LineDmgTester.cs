using TileSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/// <summary>
///     A class to test applying damage without having weapons
/// </summary>
public class LineDmgTester : MonoBehaviour
{
    private Camera  cam;
    private Vector3 endPos;
    private Vector3 startPos;

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
    }

    public void StopDrawingLine(InputAction.CallbackContext context)
    {
        Vector2Control mousePos = Mouse.current.position;
        endPos = cam.ScreenToWorldPoint(new Vector3(mousePos.x.ReadValue(), mousePos.y.ReadValue()));
        var dmg = new Damage(startPos, endPos, 5000f);
        dmg.ApplyDamage(new TileManager[] { });
    }
}
