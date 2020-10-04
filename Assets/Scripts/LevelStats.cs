using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelStats", menuName = "ScriptableObjects/LevelStats", order = 1)]
public class LevelStats : ScriptableObject
{
    [Header("Cars")]
    public int FirstLevelCars;
    public int MaxCars;
    public int MaxCarLevel;
    public AnimationCurve CarIncrease;
    [Header("Lane Change")]
    public Vector2 FirstLevelLaneChangeCooldown;
    public Vector2 MinLaneChangeCooldown;
    public int MinLaneChangeCooldownLevel;
    public AnimationCurve LaneChangeCooldownDecrease;
    [Header("Speed")]
    public float FirstLevelSpeed;
    public float FirstLevelPlayerMaxSpeed;
    public float FirstLevelPlayerAccaleration;
    public float SpeedIncreaseFactor;
}
