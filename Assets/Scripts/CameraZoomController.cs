﻿using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
///     Basic camera zoom control using Cinemachine
/// </summary>
public class CameraZoomController : MonoBehaviour
{
    private const float                    ZoomInc     = -0.05f;
    private const float                    ZoomDamping = 0.1f;
    public        float                    MinFOV;
    public        float                    MaxFOV;
    private       float                    goalZoom;
    private       CinemachineVirtualCamera virtualCamera;
    private       float                    zoomLevel;
    private       float                    zoomVel;

    // Todo add panning (probably remove Cinemachine)

    private void Awake()
    {
        virtualCamera                             =  GetComponent<CinemachineVirtualCamera>();
        zoomLevel                                 =  MinFOV;
        goalZoom                                  =  MinFOV;
        InputManager.PlayerActions.Zoom.performed += Zoom;
    }

    private void Update()
    {
        zoomLevel                             = Mathf.SmoothDamp(zoomLevel, goalZoom, ref zoomVel, ZoomDamping);
        virtualCamera.m_Lens.OrthographicSize = zoomLevel;
    }

    private void OnDestroy()
    {
        InputManager.PlayerActions.Zoom.performed -= Zoom;
    }

    private void Zoom(InputAction.CallbackContext ctx)
    {
        if (InputManager.IsMouseOverClickableUI()) return;
        var deltaZoom = ctx.ReadValue<float>();
        goalZoom += deltaZoom * ZoomInc * (goalZoom / MaxFOV);
        goalZoom =  Mathf.Clamp(goalZoom, MinFOV, MaxFOV);
    }
}
