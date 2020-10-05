using Newtonsoft.Json;
using TMPro;
using UnityEngine;

namespace Utils
{
    public class LeaderboardItem : MonoBehaviour
    {
        public TMP_Text position;
        public TMP_Text name;

        public void SetEntry(LeaderboardEntry entry, int pos)
        {
            var meta = JsonConvert.DeserializeObject<LeaderboardMeta>(entry.Metadata);
            position.text = $"#{pos}";
            name.text = $"{entry.Key} - Level {meta?.Level}";
        }
    }
}