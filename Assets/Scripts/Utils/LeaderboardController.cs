using System;
using Doozy.Engine.UI.Nodes;
using Newtonsoft.Json;
using Proyecto26;
using RSG;
using UnityEngine;
using Utils;

public class LeaderboardController : Singleton<LeaderboardController>
{
    public string lboardUrl = "https://lboard.cocapasteque.tech";

    public IPromise<ResponseHelper> FetchLeaderboard(string level)
    {
        Debug.Log("Fetching leaderboard");
        RequestHelper request = new RequestHelper();
        request.Uri = $"{lboardUrl}/api/entries/{level}";

        var promise = RestClient.Get(request);
        return promise;
    }
    
    public void PostEntry(LeaderboardEntry entry, double score, string board)
    {
        Debug.Log("Posting entry " + entry);
        RequestHelper request = new RequestHelper();
        var reqBody = new LeaderboardEntryRequest()
        {
            Entry = entry,
            Score = score
        };
        
        request.Uri = $"{lboardUrl}/api/entries/{board}";
        request.BodyString = JsonConvert.SerializeObject(reqBody);
        
        var promise = RestClient.Post(request).Then(response => { Debug.Log(response.Text); });
    }
}

[Serializable]
public class LeaderboardEntry
{
    [JsonProperty("key")] public string Key { get; set; }

    [JsonProperty("metadata")] public string Metadata { get; set; }

    public override string ToString()
    {
        return $"[key={Key}, metadata={Metadata}]";
    }
}

[Serializable]
public class LeaderboardEntryRequest
{
    [JsonProperty("entry")] public LeaderboardEntry Entry { get; set; }

    [JsonProperty("score")] public double Score { get; set; }
}

[Serializable]
public class LeaderboardMeta
{
    [JsonProperty("level")] public int Level { get; set; }
}