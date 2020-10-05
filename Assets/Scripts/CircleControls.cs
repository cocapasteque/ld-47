using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cinemachine;
using Newtonsoft.Json;
using TMPro;
using Utils;

public class CircleControls : MonoBehaviour
{
    public GameObject explosionPrefab;

    public Vector2 RadiusBounds;

    public AnimationCurve AccelerationCurve;
    public AnimationCurve BreakingCurve;
    public AnimationCurve TurningCurve;

    public float ExitTurningDuration;
    public AnimationCurve ExitTurningX;
    public AnimationCurve ExitTurningY;
    public AnimationCurve ExitTurningZ;

    public float GameOverSpinDuration;
    public AnimationCurve GameOverSpinX;
    public AnimationCurve GameOverSpinY;
    public AnimationCurve GameOverSpinZ;

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
    public bool exited = false;

    [Tooltip("0 - 1st value:\tnegative\n1st - 2nd value:\tneutral\n2nd value - 1:\tpositive")]
    public Vector2 HonkEffectProbabilities;

    public float HonkCooldown = 1f;
    private float currentHonkCooldown = 10f;

    public float HitCooldown = 0.3f;
    private float _currentCooldown = 1;

    [ColorUsage(false, true)] public Color _blinkColor;

    public Vector2 EnginePitch;

    public AudioSource honkAudio;
    public AudioClip honkClip;
    public AudioSource source;
    public AudioClip exploClip;
    public GameOverScreen gameOver;

    public TMP_Text totalLoops;
    public TMP_Text levelLoops;
    private int _totalLoops;
    private int _levelLoops;

    private void Start()
    {
        Init();
        source = GetComponent<AudioSource>();
    }

    public void Init()
    {
        _totalLoops = PlayerPrefs.GetInt("TotalLoops", 0);
        angle = 0;
        GetComponent<Collider>().enabled = true;
        exited = false;
        var transposer = vcam.GetCinemachineComponent<CinemachineTransposer>();
        transposer.m_FollowOffset =
            new Vector3(ExitTurningX.Evaluate(0), ExitTurningY.Evaluate(0), ExitTurningZ.Evaluate(0));
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
        var noise = vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        noise.m_AmplitudeGain = 0;
        source.Play();
    }

    void Update()
    {
        totalLoops.text = $"Total loops: {_totalLoops}";
        levelLoops.text = $"Loops: {_levelLoops}";
        PlayerPrefs.SetInt("TotalLoops", _totalLoops);
        source.pitch = Mathf.Lerp(EnginePitch[0], EnginePitch[1], speed / maxSpeed);

        if (exited)
        {
            source.Stop();
            return;
        }

        angle -= speed * Time.deltaTime / radius;
        baseRot = Quaternion.LookRotation(Vector3.Cross(transform.position, Vector3.up));
        if (Input.GetAxis("Accelerating") > 0)
        {
            var speedGain = AccelerationCurve.Evaluate(Mathf.InverseLerp(0, maxSpeed, speed)) *
                            Acceleration * Time.deltaTime;
            speed = Mathf.Clamp(speed + speedGain, 0, maxSpeed);
        }

        if (Input.GetAxis("Breaking") > 0)
        {
            var speedLose = BreakingCurve.Evaluate(Mathf.InverseLerp(0, maxSpeed, speed)) *
                            Acceleration * Time.deltaTime;
            speed = Mathf.Clamp(speed - speedLose, 0, maxSpeed);
        }

        speedRatio = speed / maxSpeed;
        if (Input.GetAxis("Horizontal") < 0)
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

        if (Input.GetAxis("Horizontal") > 0)
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

        if (Input.GetButton("Honk"))
        {
            if (currentHonkCooldown > HonkCooldown)
                Honk();
        }

        if (turningRightTime == 0f && turningLeftTime == 0f)
        {
            turningRot = Quaternion.Slerp(turningRot, Quaternion.identity, Time.time - lastTurnTime);
        }

        transform.position = new Vector3(radius * Mathf.Sin(angle), 0f, radius * Mathf.Cos(angle));
        transform.rotation = baseRot * turningRot;

        currentHonkCooldown += Time.deltaTime;
        _currentCooldown += Time.deltaTime;
    }

    private void Honk()
    {
        currentHonkCooldown = 0;
        honkAudio.PlayOneShot(honkClip);
        // Get Npc around the player
        var hits = Physics.OverlapSphere(transform.position, 2).ToList();
        hits = hits.Distinct(new ColliderComparer()).ToList();
        foreach (var hit in hits)
        {
            var npc = hit.GetComponent<NpcCarControls>();
            if (npc == null) continue;
            var diff = npc.transform.position - transform.position;
            var dot = Vector3.Dot(transform.right, diff);
            var rnd = Random.Range(0f, 1f);
            //Change to your lane
            if (rnd < HonkEffectProbabilities[0])
            {
                npc.ChangeLane(dot > 0);
                Debug.Log("negative");
            }
            //Change to other lane
            else if (rnd > HonkEffectProbabilities[1])
            {
                npc.ChangeLane(dot < 0);
                Debug.Log("positive");
            }
            else
                Debug.Log("neutral");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Exit"))
        {
            if (!exited)
            {
                other.GetComponent<Exit>().StartExiting(gameObject);
                Exit();
            }
        }
        else if (other.CompareTag("LoopCounter"))
        {
            _levelLoops++;
            _totalLoops++;
        }
        else
        {
            if (_currentCooldown <= HitCooldown) return;
            _currentCooldown = 0;
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

        while (_currentCooldown <= HitCooldown)
        {
            matPropBlock.SetColor("_EmissionColor", _blinkColor);
            renderer.SetPropertyBlock(matPropBlock);
            Debug.Log(_blinkColor);
            yield return new WaitForSeconds(0.2f * HitCooldown);
            matPropBlock.SetColor("_EmissionColor", baseColor);
            renderer.SetPropertyBlock(matPropBlock);
            Debug.Log(baseColor);
            yield return new WaitForSeconds(0.2f * HitCooldown);
        }

        matPropBlock.SetColor("_EmissionColor", baseColor);
        renderer.SetPropertyBlock(matPropBlock);
    }


    public void PlayerGoBrr()
    {
        exited = true;
        fracturedState.SetActive(true);
        var explosion = Instantiate(explosionPrefab, fracturedState.transform.position, Quaternion.identity);
        Destroy(explosion, 5);
        CameraShake(.8f);
        foreach (Transform child in fracturedState.transform)
        {
            child.GetComponent<Rigidbody>()
                .AddExplosionForce(600, fracturedState.transform.position - Vector3.up, 2, 50);
        }

        gameOver.GameOver(CarSpawner.Instance.CurrentLevel + 1);

        StartCoroutine(CameraPan());

        // Send Scores
        var level = CarSpawner.Instance.CurrentLevel + 1;
        var name = PlayerPrefs.GetString("Playername");

        LeaderboardController.Instance.PostEntry(new LeaderboardEntry
        {
            Key = name,
            Metadata = JsonConvert.SerializeObject(new LeaderboardMeta {Level = level})
        }, level, "level");

        IEnumerator CameraPan()
        {
            float startTime = Time.time;
            var t = 0f;
            var transposer = vcam.GetCinemachineComponent<CinemachineTransposer>();
            while (true)
            {
                t = (Time.time - startTime) / GameOverSpinDuration;
                transposer.m_FollowOffset = new Vector3(GameOverSpinX.Evaluate(t), GameOverSpinY.Evaluate(t),
                    GameOverSpinZ.Evaluate(t));
                yield return null;
            }
        }
    }

    private void Exit()
    {
        GetComponent<Collider>().enabled = false;
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

            _levelLoops = 0;
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

    public void CameraShake(float duration)
    {
        StartCoroutine(Work());

        IEnumerator Work()
        {
            var noise = vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            noise.m_AmplitudeGain = 5;
            yield return new WaitForSeconds(duration);
            noise.m_AmplitudeGain = 0;
        }
    }
}