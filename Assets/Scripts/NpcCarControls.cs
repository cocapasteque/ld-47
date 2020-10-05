using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cinemachine;

public class NpcCarControls : MonoBehaviour
{
    public float minDistance;
    private Vector2 changeLaneCooldown;
    [HideInInspector]
    public float Angle;

    public AudioClip exploClip;
    public GameObject explosionPrefab;

    private int lane;
    private float speed;
    private float radius;

    public AudioClip[] honks;
    public AudioSource source;

    private Rigidbody rb;
    private CinemachineVirtualCamera vcam;

    void Start()
    {
        source = GetComponent<AudioSource>();
        source.pitch = Random.Range(1, 2);
        rb = GetComponent<Rigidbody>();
        vcam = FindObjectOfType<CinemachineVirtualCamera>();
    }
    
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
            Honk();
        }
    }

    private void Honk()
    {
        source.PlayOneShot(honks[Random.Range(0, honks.Length)]);
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Npc"))
        {
            Rigidbody otherRb = collision.rigidbody;

            source.PlayOneShot(exploClip, 1);
            var explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 5);
            GetComponent<NpcCarControls>().enabled = false;
            GetComponent<CarMovement>().enabled = false;
            rb.isKinematic = false;
            rb.AddExplosionForce(200, rb.transform.position - Vector3.up, 2, 2);
            collision.gameObject.GetComponent<NpcCarControls>().enabled = false;
            collision.gameObject.GetComponent<CarMovement>().enabled = false;
            otherRb.isKinematic = false;
            otherRb.AddExplosionForce(200, rb.transform.position - Vector3.up, 2, 2);
            CameraShake(0.2f);
        }        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            source.PlayOneShot(exploClip, 1);
            var explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 5);
            GetComponent<NpcCarControls>().enabled = false;
            GetComponent<CarMovement>().enabled = false;
            rb.isKinematic = false;
            rb.AddExplosionForce(200, rb.transform.position - Vector3.up, 2, 2);
        }
    }

    public void CameraShake(float duration)
    {
        StartCoroutine(Work());
        IEnumerator Work()
        {
            var noise = vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            noise.m_AmplitudeGain = 3;
            yield return new WaitForSeconds(duration);
            noise.m_AmplitudeGain = 0;
        }
    }
}
