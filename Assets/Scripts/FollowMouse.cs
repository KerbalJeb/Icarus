using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;


public class FollowMouse : MonoBehaviour
{
    private Camera cam;

    private void Start()
    {
        cam=Camera.main;
    }

    private void Update()
    {
        Vector2Control mousePos = Mouse.current.position;
        Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(mousePos.x.ReadValue(), mousePos.y.ReadValue()));
        worldPos.z         = 0;
        transform.position = worldPos;
    }
}

