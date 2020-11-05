using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class LineDmgTester : MonoBehaviour
{
    private readonly RaycastHit2D[] hits = new RaycastHit2D[20];
    private          Camera         cam;
    private          Vector3        endPos;
    private          bool           pressed;
    private          Vector3        startPos;

    private void Start()
    {
        cam = Camera.main;
    }


    public void DrawLine(InputAction.CallbackContext ctx)
    {
        if (ctx.phase == InputActionPhase.Started && !pressed)
        {
            Vector2Control mousePos = Mouse.current.position;
            startPos = cam.ScreenToWorldPoint(new Vector3(mousePos.x.ReadValue(), mousePos.y.ReadValue()));
            pressed  = true;
        }

        if (ctx.phase == InputActionPhase.Canceled)
        {
            Vector2Control mousePos = Mouse.current.position;
            endPos  = cam.ScreenToWorldPoint(new Vector3(mousePos.x.ReadValue(), mousePos.y.ReadValue()));
            pressed = false;
            var dmg = new Damage(startPos, endPos, 5000f);
            dmg.ApplyDamage();
        }
    }
}
