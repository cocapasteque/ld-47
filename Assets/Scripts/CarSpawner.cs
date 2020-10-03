using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class CarSpawner : MonoBehaviour
{
    public int Lanes;
    public Vector2 RadiusBounds;
    public float Speed;
    public int Cars;
    public List<GameObject> CarPrefabs;
    public Transform CarParent;

    public Dictionary<int, List<GameObject>> CarsPerLane;

    private List<float> laneRadii;
    private List<float> laneProbabilities;

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
        CarsPerLane = new Dictionary<int, List<GameObject>>();
        for(int i = 0; i < Lanes; i++)
        {
            CarsPerLane.Add(i, new List<GameObject>());
        }
        for (int i = 0; i < Cars; i++)
        {
            int prefabIndex = UnityEngine.Random.Range(0, CarPrefabs.Count);
            GameObject car = Instantiate(CarPrefabs[prefabIndex], CarParent);
            Tuple<int, Vector3, float> tuple = FindSpawnPos(car.GetComponent<NpcCarControls>().minDistance);
            if (tuple.Item1 >= 0)
            {
                car.transform.position = tuple.Item2;
                car.transform.rotation = Quaternion.LookRotation(Vector3.Cross(tuple.Item2, Vector3.up));
                CarsPerLane[tuple.Item1].Add(car);
                car.GetComponent<NpcCarControls>().Init(5, tuple.Item1, tuple.Item3);
            }
            else
            {
                Destroy(car);
            }
        }
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
        Debug.Log("can switch = " + canSwitch);
        return canSwitch;
    }  
    
    public void SwitchLane(GameObject car, int oldLane, int newLane)
    {
        Debug.Log("Switch");
        CarsPerLane[oldLane].Remove(car);
        CarsPerLane[newLane].Add(car);
    }

    private Vector3 GetPosFromAngle(float radius, float angle)
    {
        return new Vector3(radius * Mathf.Sin(angle), 0 , radius * Mathf.Cos(angle));
    }
}