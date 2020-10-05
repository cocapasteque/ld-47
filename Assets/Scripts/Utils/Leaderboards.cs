using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using Utils;

public class Leaderboards : MonoBehaviour
{
    public GameObject leaderboardPrefab;
    public Transform[] leaderboardParents;
    
    private LeaderboardEntry[] _top3;
    private bool _init;
    private void Start()
    {
        GetTop3();
    }
    
    private void GetTop3()
    {
        LeaderboardController.Instance.FetchLeaderboard("level").Then(x =>
        {
            var entries = JsonConvert.DeserializeObject<LeaderboardEntry[]>(x.Text);
            _top3 = entries.Take(3).ToArray();

            for (var index = 0; index < _top3.Length; index++)
            {
                var entry = _top3[index];
                if (entry == null) continue;

                foreach (var parent in leaderboardParents)
                {
                    var lb = Instantiate(leaderboardPrefab, parent);
                    lb.GetComponent<LeaderboardItem>().SetEntry(entry, index + 1);
                }
            }
        });
    }
}