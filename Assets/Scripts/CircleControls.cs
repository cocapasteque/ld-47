using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleControls : MonoBehaviour
{
    public GameObject explosionPrefab;
    
    public Vector2 RadiusBounds;
    public Vector2 SpeedBounds;

    public AnimationCurve AccelerationCurve;
    public AnimationCurve BreakingCurve;
    public AnimationCurve TurningCurve;

    public float Acceleration;
    public float TurningSpeed;
    public float MaxTurningDuration;

    public float MaxTurningAngle;

    private float angle;
    private float radius;
    public float speed;

    private float speedRatio;

    private float turningLeftTime, turningRightTime;
    private bool turningLeft, turningRight;

    private Quaternion baseRot;
    private Quaternion turningRot;
    private float lastTurnTime = 0f;

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        angle = 0;
        speed = SpeedBounds[0];
        radius = RadiusBounds[0];
        transform.position = new Vector3(radius * Mathf.Sin(angle), 0f, radius * Mathf.Cos(angle));
        transform.rotation = Quaternion.LookRotation(Vector3.Cross(transform.position, Vector3.up));
        turningRot = Quaternion.identity;
    }

    void Update()
    {
        angle -= speed * Time.deltaTime / radius;
        baseRot = Quaternion.LookRotation(Vector3.Cross(transform.position, Vector3.up));     
        if (Input.GetKey(KeyCode.UpArrow))
        {
            var speedGain = AccelerationCurve.Evaluate(Mathf.InverseLerp(SpeedBounds[0], SpeedBounds[1], speed)) * Acceleration * Time.deltaTime;
            speed = Mathf.Clamp(speed + speedGain, SpeedBounds[0], SpeedBounds[1]);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            speed = Mathf.Clamp(speed - Acceleration * Time.deltaTime, SpeedBounds[0], SpeedBounds[1]);
        }
        speedRatio = speed / SpeedBounds[1];
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            if (radius > RadiusBounds[0])
            {
                lastTurnTime = Time.time;
                turningLeftTime += Time.deltaTime;
                var turningModifier = TurningCurve.Evaluate(turningLeftTime / MaxTurningDuration);
                radius = Mathf.Clamp(radius - TurningSpeed * turningModifier * speedRatio * Time.deltaTime, RadiusBounds[0], RadiusBounds[1]);
                turningRot = Quaternion.AngleAxis(-turningModifier * MaxTurningAngle * speedRatio, Vector3.up);
            }
            else
            {
                turningLeftTime = 0f;
            }
        }
        else
        {
            turningLeftTime = 0f;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            if (radius < RadiusBounds[1])
            {
                lastTurnTime = Time.time;
                turningRightTime += Time.deltaTime;
                var turningModifier = TurningCurve.Evaluate(turningRightTime / MaxTurningDuration);
                radius = Mathf.Clamp(radius + TurningSpeed * turningModifier * speedRatio * Time.deltaTime, RadiusBounds[0], RadiusBounds[1]);
                turningRot = Quaternion.AngleAxis(turningModifier * MaxTurningAngle * speedRatio, Vector3.up);
            }
            else
            {
                turningRightTime = 0f;
            }
        }
        else
        {
            turningRightTime = 0f;
        }
        if (turningRightTime == 0f && turningLeftTime == 0f)
        {
            turningRot = Quaternion.Slerp(turningRot, Quaternion.identity, Time.time - lastTurnTime);
        }
        transform.position = new Vector3(radius * Mathf.Sin(angle), 0f, radius * Mathf.Cos(angle));
        transform.rotation = baseRot * turningRot;     
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Exit")
        {
            Debug.Log("Exit");
        }
        else
        {
            var explosion = Instantiate(explosionPrefab, other.transform.position, Quaternion.identity);
            Destroy(explosion, 5);
            other.GetComponent<NpcCarControls>().enabled = false;
            other.GetComponent<CarMovement>().enabled = false;
            var rb = other.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.AddExplosionForce(300, rb.transform.position - Vector3.up, 2, 2);
            Debug.Log("HIT");
        }
    }
}
