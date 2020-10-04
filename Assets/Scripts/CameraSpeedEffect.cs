using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraSpeedEffect : MonoBehaviour
{
    public CinemachineVirtualCamera vCam;
    public CircleControls player;
    public AnimationCurve effectCurve;
    void Update()
    {
        var vel = Remap(player.speed, player.SpeedBounds.x, player.SpeedBounds.y, 0, 1);
        vCam.m_Lens.FieldOfView = effectCurve.Evaluate(vel);
    }
    
    public static float Remap (float value, float from1, float to1, float from2, float to2) {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}
