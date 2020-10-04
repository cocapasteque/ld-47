using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class NpcCarControls : MonoBehaviour
{
    public float minDistance;
    private Vector2 changeLaneCooldown;
    [HideInInspector]
    public float Angle;

    private int lane;
    private float speed;
    private float radius;
    
    
    // Update is called once per frame
    void Update()
    {
        Angle -= speed * Time.deltaTime / radius;
        transform.position = new Vector3(radius * Mathf.Sin(Angle), 0f, radius * Mathf.Cos(Angle));
        transform.rotation = Quaternion.LookRotation(Vector3.Cross(transform.position, Vector3.up));
    }

    public void Init(float s, int l, float a, Vector2 changeLaneCD)
    {
        speed = s;
        lane = l;
        radius = CarSpawner.Instance.GetRadius(lane);
        Angle = a;
        changeLaneCooldown = changeLaneCD;
        StartCoroutine(StartChangeLaneCooldown());
    }

    public void ChangeLane(bool left)
    {
        if ((left && lane == 0) ||(!left && lane == CarSpawner.Instance.Lanes - 1))
        {
            return;
        }
        int oldLane = lane;
        int newLane = lane + (left ? -1 : 1);
        bool canSwitch = CarSpawner.Instance.CheckFreeSpotInLane(newLane, Angle, minDistance);
        if (canSwitch)
        {
            lane = newLane;
            CarSpawner.Instance.SwitchLane(gameObject, oldLane, newLane);       
            DOTween.To(() => radius, x => radius = x, CarSpawner.Instance.GetRadius(newLane), 1.5f);
        }
    }

    private IEnumerator StartChangeLaneCooldown()
    {
        while (true)
        {
            float waitTime = Random.Range(changeLaneCooldown[0], changeLaneCooldown[1]);
            yield return new WaitForSeconds(waitTime);
            bool left = System.Convert.ToBoolean(Random.Range(0, 100) % 2);
            ChangeLane(left);
        }
    }
}
