using UnityEngine;
using UnityEngine.InputSystem;

public class LineDmgTester : MonoBehaviour
{
    private Camera cam;
    private Vector3 endPos;
    private RaycastHit2D[] hits = new RaycastHit2D[20];
    private bool pressed = false;
    private Vector3 startPos;

    void Start()
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

            for (int i = 0; i < numHits; i++)
            {
                var hit = hits[i];

                var ship = hit.transform?.gameObject.GetComponentInParent<ShipManager>();
                ship?.ApplyDamage(dmg);
            }
        }
    }
}