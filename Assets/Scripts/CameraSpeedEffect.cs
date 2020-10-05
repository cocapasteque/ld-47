using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraSpeedEffect : MonoBehaviour
{
    public CinemachineVirtualCamera vCam;
    public CircleControls player;
    public AnimationCurve effectCurve;
    public AnimationCurve offsetCurve;

    void Update()
    {
        if (player.exited) return;
        var vel = Remap(player.speed, 0, player.maxSpeed, 0, 1);
        vCam.m_Lens.FieldOfView = effectCurve.Evaluate(vel);
        vCam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset =
            new Vector3(0, 5, offsetCurve.Evaluate(vel));
    }


    public static float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}