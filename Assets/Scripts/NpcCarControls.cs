using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcCarControls : MonoBehaviour
{
    public float minDistance;

    private int lane;
    private float speed;
    private float radius;
    private float angle;
    
    // Update is called once per frame
    void Update()
    {
        angle -= speed * Time.deltaTime / radius;
        transform.position = new Vector3(radius * Mathf.Sin(angle), 0f, radius * Mathf.Cos(angle));
        transform.rotation = Quaternion.LookRotation(Vector3.Cross(transform.position, Vector3.up));
    }

    public void Init(float s, int l, float a)
    {
        speed = s;
        lane = l;
        radius = CarSpawner.Instance.GetRadius(lane);
        angle = a;
    }

    public void ChangeLane(bool left)
    {

    }
}
