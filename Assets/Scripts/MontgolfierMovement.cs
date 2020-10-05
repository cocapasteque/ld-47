using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MontgolfierMovement : MonoBehaviour
{
    public float rotationSpeed;
    public float amplitude;
    public float movementSpeed;

    private Vector3 basePosition;

    void Start()
    {
        basePosition = transform.position;
    }
    
    void Update()
    {
        transform.Rotate(Vector3.forward, rotationSpeed);
        transform.position = new Vector3(basePosition.x, basePosition.y + Mathf.Sin(Time.time) * amplitude, basePosition.z);
    }
}
