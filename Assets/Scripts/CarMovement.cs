using System.Collections.Generic;
using UnityEngine;

public class CarMovement : MonoBehaviour
{
    public Vector2 rollSpeed;
    public Vector2 pitchSpeed;

    private float _roll;
    private float _pitch;

    private void Start()
    {
        _roll = Random.Range(rollSpeed.x, rollSpeed.y);
        _pitch = Random.Range(pitchSpeed.x, pitchSpeed.y);
    }

    private void Update()
    {
        var roll = Mathf.Sin(Time.time * _roll);
        var pitch = Mathf.Sin(Time.time * _pitch);
        var r = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(r.x + pitch, r.y, r.z + roll);
    }
}
