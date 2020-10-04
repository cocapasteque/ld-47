using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class Exit : MonoBehaviour
{
    public Transform RotationObject;
    public Transform FinalDirection;

    public float TurningSpeed;
    public float LeaveStartSpeed;
    public float LeaveDuration;

    public CanvasGroup Cg;

    private GameObject player;

    public void Init()
    {
        StopAllCoroutines();
    }

    public void StartExiting(GameObject player)
    {
        this.player = player;
        player.transform.parent = RotationObject;
        StartCoroutine(Turning());

        IEnumerator Turning()
        {         
            while(Quaternion.Angle(player.transform.rotation, FinalDirection.rotation) > 3f)
            {
                RotationObject.Rotate(Vector3.up, TurningSpeed * Time.deltaTime);
                yield return null;
            }
            player.transform.parent = null;
            player.transform.DORotateQuaternion(FinalDirection.transform.rotation, 0.2f);
            StartCoroutine(Leave());
        }


        IEnumerator Leave()
        {
            float startTime = Time.time;
            float t = 0;
            while (Time.time - startTime < LeaveDuration)
            {
                yield return null;
                t = (Time.time - startTime) / LeaveDuration;
                player.transform.Translate(Vector3.forward * Mathf.Lerp(LeaveStartSpeed * Time.deltaTime, 0f, t));
                Cg.alpha = t;          
            }
            CarSpawner.Instance.NextLevel();
        }
    }
}
