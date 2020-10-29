using UnityEngine;
using UnityEngine.InputSystem;

public class LineDmgTester : MonoBehaviour
{
    private readonly RaycastHit2D[] hits = new RaycastHit2D[20];
    private Camera cam;
    private Vector3 endPos;
    private bool pressed;
    private Vector3 startPos;

    private void Start()
    {
        cam = Camera.main;
    }


    public void DrawLine(InputAction.CallbackContext ctx)
    {
        if (ctx.phase == InputActionPhase.Started && !pressed)
        {
            var mousePos = Mouse.current.position;
            startPos = cam.ScreenToWorldPoint(new Vector3(mousePos.x.ReadValue(), mousePos.y.ReadValue()));
            pressed = true;
        }

        if (ctx.phase == InputActionPhase.Canceled)
        {
            var mousePos = Mouse.current.position;
            endPos = cam.ScreenToWorldPoint(new Vector3(mousePos.x.ReadValue(), mousePos.y.ReadValue()));
            pressed = false;

            var dir = endPos - startPos;

            var numHits = Physics2D.RaycastNonAlloc(startPos, dir, hits);
            var dmg = new Damage(startPos, endPos, 1250f);

            for (var i = 0; i < numHits; i++)
            {
                var hit = hits[i];

                var ship = hit.transform?.gameObject.GetComponentInParent<ShipManager>();
                if (ship != null)
                {
                    ship.ApplyDamage(dmg);
                }
            }
        }
    }
}