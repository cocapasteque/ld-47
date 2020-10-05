using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class CarSpawner : MonoBehaviour
{
    public int Lanes;
    public Vector2 RadiusBounds;
    public List<GameObject> CarPrefabs;
    public Transform CarParent;

    public Action CarSpawned;
    
    public Dictionary<int, List<GameObject>> CarsPerLane;

    public LevelStats levelStats;

    private List<float> laneRadii;
    private List<float> laneProbabilities;

    public int CurrentLevel = 0;
    public CanvasGroup Cg;
    public float FadeDuration;
    
    public List<GameObject> Exits;

    private static CarSpawner _instance;
   
    
    public static CarSpawner Instance { get { return _instance; } }


    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    private void Start()
    {
        CalculateLaneProbabilities();
        SpawnCars();
    }

    public void SpawnCars()
    {
        Vector2 laneChangeCooldown = GetCurrentLaneChangeCooldown();
        int cars = GetCurrentNumberOfCars();
        float speed = GetCurrentSpeed();
        foreach (var exit in Exits)
        {
            exit.SetActive(false);
        }
        if (Exits.Count > 0)
        {
            var rnd = UnityEngine.Random.Range(0, Exits.Count);
            Exits[rnd].SetActive(true);
        }
        CarsPerLane = new Dictionary<int, List<GameObject>>();
        for(int i = 0; i < Lanes; i++)
        {
            CarsPerLane.Add(i, new List<GameObject>());
        }
        for (int i = 0; i < cars; i++)
        {
            int prefabIndex = UnityEngine.Random.Range(0, CarPrefabs.Count);
            GameObject car = Instantiate(CarPrefabs[prefabIndex], CarParent);
            Tuple<int, Vector3, float> tuple = FindSpawnPos(car.GetComponent<NpcCarControls>().minDistance);
            if (tuple.Item1 >= 0)
            {
                car.transform.position = tuple.Item2;
                car.transform.rotation = Quaternion.LookRotation(Vector3.Cross(tuple.Item2, Vector3.up));
                CarsPerLane[tuple.Item1].Add(car);
                car.GetComponent<NpcCarControls>().Init(speed, tuple.Item1, tuple.Item3, laneChangeCooldown);
            }
            else
            {
                Destroy(car);
            }
        }

        Debug.Log("Car spawned");
        CarSpawned?.Invoke();
    }

    private Vector2 GetCurrentLaneChangeCooldown()
    {
        var currentAnimPos = (float)CurrentLevel / levelStats.MinLaneChangeCooldownLevel;
        var t = levelStats.LaneChangeCooldownDecrease.Evaluate(currentAnimPos);
        Vector2 result = t * (levelStats.MinLaneChangeCooldown - levelStats.FirstLevelLaneChangeCooldown) + levelStats.FirstLevelLaneChangeCooldown;
        return result;
    }

    private int GetCurrentNumberOfCars()
    {
        var currentAnimPos = (float)CurrentLevel /levelStats.MaxCarLevel;
        var t = levelStats.CarIncrease.Evaluate(currentAnimPos);
        int result = Mathf.FloorToInt(t * (levelStats.MaxCars - levelStats.FirstLevelCars) + levelStats.FirstLevelCars);
        return result;
    }

    private float GetCurrentSpeed()
    {
        var result = levelStats.FirstLevelSpeed * Mathf.Pow(levelStats.SpeedIncreaseFactor, CurrentLevel);
        return result;
    }

    private Tuple<int, Vector3, float> FindSpawnPos(float dist)
    {
        
        float laneProb = UnityEngine.Random.Range(0f, 1f);
        int lane = 0;
        for(int i = 0; i < laneProbabilities.Count; i++)
        {
            if (laneProb <= laneProbabilities[i])
            {
                break;
            }
            else
            {
                lane++;
            }
        }
        float radius = laneRadii[lane];
        bool posFound = true;
        Vector3 pos = Vector3.zero;
        float angle = 0;
        for (int i = 0; i < 10; i++)
        {
            angle = UnityEngine.Random.Range(0f, 360f);
            pos = GetPosFromAngle(radius, angle);
            foreach(var car in CarsPerLane[lane])
            {
                if (Vector3.Distance(pos, car.transform.position) < Mathf.Max(dist, car.GetComponent<NpcCarControls>().minDistance))
                {
                    posFound = false;
                    break;
                }
            }
            if (posFound)
            {
                break;
            }
        }
        if (posFound)
        {
            return new Tuple<int, Vector3, float>(lane, pos, angle);
        }
        else
        {
            return new Tuple<int, Vector3, float>(-1, Vector3.zero, -1f);
        }
    }

    private void CalculateLaneProbabilities()
    {
        laneRadii = new List<float>();
        for (int i = 0; i < Lanes; i++)
        {
            laneRadii.Add(RadiusBounds[0] + i * ((RadiusBounds[1] - RadiusBounds[0]) / (Lanes - 1)));
        }
        var sum = laneRadii.Sum();
        laneProbabilities = new List<float>();
        foreach (var radius in laneRadii)
        {
            laneProbabilities.Add((laneProbabilities.Count > 0 ? laneProbabilities.Last() : 0f) + (radius / sum));
        }
    }

    public float GetRadius(int lane)
    {
        if (lane >= laneRadii.Count || lane < 0)
        {
            return -1;
        }
        else
        {
            return laneRadii[lane];
        }
    }

    public bool CheckFreeSpotInLane(int lane, float angle, float dist)
    {
        bool canSwitch = true;
        Vector3 pos = GetPosFromAngle(laneRadii[lane], angle);
        foreach (var car in CarsPerLane[lane])
        {
            var npc = car.GetComponent<NpcCarControls>();
            Vector3 carPos = GetPosFromAngle(laneRadii[lane], npc.Angle);
            if (Vector3.Distance(pos, carPos) < Mathf.Max(dist, npc.minDistance))
            {
                canSwitch = false;
                break;
            }
        }
        return canSwitch;
    }  
    
    public void SwitchLane(GameObject car, int oldLane, int newLane)
    {
        CarsPerLane[oldLane].Remove(car);
        CarsPerLane[newLane].Add(car);
    }

    private Vector3 GetPosFromAngle(float radius, float angle)
    {
        return new Vector3(radius * Mathf.Sin(angle), 0 , radius * Mathf.Cos(angle));
    }

    public void NextLevel()
    {
        CurrentLevel++;
        foreach(Transform car in CarParent)
        {
            Destroy(car.gameObject);         
        }
        SpawnCars();
        var player = FindObjectOfType<CircleControls>();
        player.Init();

        StartCoroutine(FadeIn());

        IEnumerator FadeIn()
        {
            yield return new WaitForSeconds(0.2f);
            float startTime = Time.time;
            while (Time.time - startTime < FadeDuration)
            {
                yield return null;
                Cg.alpha = 1f - (Time.time - startTime) / FadeDuration;
            }
            Cg.alpha = 0f;
        }
    }
}