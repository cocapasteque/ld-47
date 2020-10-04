using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cinemachine;
using Utils;

public class CircleControls : MonoBehaviour
{
    public GameObject explosionPrefab;

    public Vector2 RadiusBounds;

    public AnimationCurve AccelerationCurve;
    public AnimationCurve TurningCurve;

    public float ExitTurningDuration;
    public AnimationCurve ExitTurningX;
    public AnimationCurve ExitTurningY;
    public AnimationCurve ExitTurningZ;

    public float Acceleration;
    public float BaseTurningSpeed;
    public float MaxTurningDuration;

    public float MaxTurningAngle;

    public float maxSpeed;
    private float angle;
    private float radius;
    public float speed;
    public CinemachineVirtualCamera vcam;

    private float speedRatio;
    private float turningSpeed;
    private float turningLeftTime, turningRightTime;
    private bool turningLeft, turningRight;

    private Quaternion baseRot;
    private Quaternion turningRot;
    private float lastTurnTime = 0f;

    public GameObject[] playerStates;
    public GameObject fracturedState;
    private int _currentPlayerState = 0;
    private bool exited = false;

    private float _hitCooldown = 1;
    private float _currentCooldown = 1;

    [ColorUsage(false, true)] public Color _blinkColor;

    public AudioClip honkClip;
    public AudioSource source;

    private void Start()
    {
        Init();
        source = GetComponent<AudioSource>();
    }

    private void Init()
    {
        angle = 0;
        var levelStats = CarSpawner.Instance.levelStats;
        var currentLevel = CarSpawner.Instance.CurrentLevel;
        maxSpeed = levelStats.FirstLevelPlayerMaxSpeed * Mathf.Pow(levelStats.SpeedIncreaseFactor, currentLevel);
        Acceleration = levelStats.FirstLevelPlayerAccaleration *
                       Mathf.Pow(levelStats.SpeedIncreaseFactor, currentLevel);
        turningSpeed = BaseTurningSpeed * Mathf.Pow(levelStats.SpeedIncreaseFactor, currentLevel);
        speed = 0.1f * maxSpeed;
        radius = RadiusBounds[0];
        transform.position = new Vector3(radius * Mathf.Sin(angle), 0f, radius * Mathf.Cos(angle));
        transform.rotation = Quaternion.LookRotation(Vector3.Cross(transform.position, Vector3.up));
        turningRot = Quaternion.identity;
    }

    void Update()
    {
        if (exited)
        {
            return;
        }

        angle -= speed * Time.deltaTime / radius;
        baseRot = Quaternion.LookRotation(Vector3.Cross(transform.position, Vector3.up));
        if (Input.GetKey(KeyCode.UpArrow))
        {
            var speedGain = AccelerationCurve.Evaluate(Mathf.InverseLerp(0, maxSpeed, speed)) *
                            Acceleration * Time.deltaTime;
            speed = Mathf.Clamp(speed + speedGain, 0, maxSpeed);
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            speed = Mathf.Clamp(speed - Acceleration * Time.deltaTime, 0, maxSpeed);
        }

        speedRatio = speed / maxSpeed;
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            if (radius > RadiusBounds[0])
            {
                lastTurnTime = Time.time;
                turningLeftTime += Time.deltaTime;
                var turningModifier = TurningCurve.Evaluate(turningLeftTime / MaxTurningDuration);
                radius = Mathf.Clamp(radius - turningSpeed * turningModifier * speedRatio * Time.deltaTime,
                    RadiusBounds[0], RadiusBounds[1]);
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
                radius = Mathf.Clamp(radius + turningSpeed * turningModifier * speedRatio * Time.deltaTime,
                    RadiusBounds[0], RadiusBounds[1]);
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

        if (Input.GetKeyDown(KeyCode.Space)) Honk();

        if (turningRightTime == 0f && turningLeftTime == 0f)
        {
            turningRot = Quaternion.Slerp(turningRot, Quaternion.identity, Time.time - lastTurnTime);
        }

        transform.position = new Vector3(radius * Mathf.Sin(angle), 0f, radius * Mathf.Cos(angle));
        transform.rotation = baseRot * turningRot;

        _currentCooldown += Time.deltaTime;
    }

    private void Honk()
    {
        source.PlayOneShot(honkClip);
        // Get Npc around the player
        var hits = Physics.OverlapSphere(transform.position, 2).ToList();
        hits = hits.Distinct(new ColliderComparer()).ToList();
        foreach (var hit in hits)
        {
            var npc = hit.GetComponent<NpcCarControls>();
            if (npc == null) continue;
            var diff = npc.transform.position - transform.position;
            var dot = Vector3.Dot(transform.right, diff);
            npc.ChangeLane(dot < 0);
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Exit")
        {
            if (!exited)
            {
                other.GetComponent<Exit>().StartExiting(gameObject);
                Exit();
            }
        }
        else
        {
            if (_currentCooldown <= _hitCooldown) return;
            _currentCooldown = 0;
            var explosion = Instantiate(explosionPrefab, other.transform.position, Quaternion.identity);
            Destroy(explosion, 5);
            other.GetComponent<NpcCarControls>().enabled = false;
            other.GetComponent<CarMovement>().enabled = false;
            var rb = other.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.AddExplosionForce(500, rb.transform.position - Vector3.up, 2, 2);

            playerStates[_currentPlayerState].SetActive(false);
            if (++_currentPlayerState > playerStates.Length - 1)
            {
                Debug.Log("Game over");
                PlayerGoBrr();
            }
            else
            {
                playerStates[_currentPlayerState].SetActive(true);
                StartCoroutine(BlinkRoutine());
            }

            Debug.Log("HIT");
        }
    }

    private IEnumerator BlinkRoutine()
    {
        var renderer = playerStates[_currentPlayerState].GetComponentInChildren<MeshRenderer>();
        var matPropBlock = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(matPropBlock);
        renderer.material.EnableKeyword("_EMISSION");

        var baseColor = matPropBlock.GetColor("_EmissionColor");

        while (_currentCooldown <= _hitCooldown)
        {
            matPropBlock.SetColor("_EmissionColor", _blinkColor);
            renderer.SetPropertyBlock(matPropBlock);
            Debug.Log(_blinkColor);
            yield return new WaitForSeconds(0.2f);
            matPropBlock.SetColor("_EmissionColor", baseColor);
            renderer.SetPropertyBlock(matPropBlock);
            Debug.Log(baseColor);
            yield return new WaitForSeconds(0.2f);
        }

        matPropBlock.SetColor("_EmissionColor", baseColor);
        renderer.SetPropertyBlock(matPropBlock);
    }


    public void PlayerGoBrr()
    {
        fracturedState.SetActive(true);
        var explosion = Instantiate(explosionPrefab, fracturedState.transform.position, Quaternion.identity);
        Destroy(explosion, 5);

        foreach (Transform child in fracturedState.transform)
        {
            child.GetComponent<Rigidbody>()
                .AddExplosionForce(600, fracturedState.transform.position - Vector3.up, 2, 50);
        }

        enabled = false;
    }

    private void Exit()
    {
        exited = true;
        var transposer = vcam.GetCinemachineComponent<CinemachineTransposer>();
        StartCoroutine(Turning());

        IEnumerator Turning()
        {
            float startTime = Time.time;
            float t = 0;
            while (Time.time - startTime <= ExitTurningDuration)
            {
                t = (Time.time - startTime) / ExitTurningDuration;
                transposer.m_FollowOffset = new Vector3(ExitTurningX.Evaluate(t), ExitTurningY.Evaluate(t),
                    ExitTurningZ.Evaluate(t));
                yield return null;
            }
        }
    }
    
    class ColliderComparer : IEqualityComparer<Collider>
    {
        // Products are equal if their names and product numbers are equal.
        public bool Equals(Collider x, Collider y)
        {

            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;

            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            //Check whether the products' properties are equal.
            return x.gameObject == y.gameObject;
        }

        public int GetHashCode(Collider obj)
        {
            return obj.GetHashCode();
        }
    }
}