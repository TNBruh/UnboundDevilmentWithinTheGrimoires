using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BenchMB : MonoBehaviour
{
    const string url = "http://localhost:3000";
    internal static SpellStats stats = new SpellStats();

    static int currentUnixTime
    {
        get
        {
            System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
            int cur_time = (int)(System.DateTime.UtcNow - epochStart).TotalSeconds;
            return cur_time;
        }
    }

    internal static void PostData(int spell, float delta, long mem)
    {


        string unix_time = BenchMB.currentUnixTime.ToString();
        string data = $"\"spell\":{spell},\"delta\":{delta},\"time\":{unix_time},\"memory\":{mem}";

        UnityWebRequest uwr = new UnityWebRequest(url + "/amogus", "POST");

        byte[] jsonByte = new System.Text.UTF8Encoding().GetBytes(data);

        uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonByte);
        uwr.SetRequestHeader("Content-Type", "application/json");


    }

    internal static void Store(int spell, float delta, long mem)
    {
        SpellStat stat = new SpellStat(spell.ToString(), delta.ToString(), mem.ToString());

        stats.stats.Add(stat);

        Debug.Log(stats.stats.Count);
    }

    internal static void SendStats(bool resetStats = true)
    {
        Debug.LogWarning(stats.Convert());

        byte[] jsonByte = new System.Text.UTF8Encoding().GetBytes(stats.Convert());
        Debug.Log(jsonByte.Length);

        UnityWebRequest uwr = new UnityWebRequest(url + "/amogus", "POST");
        uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonByte);
        uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");

        uwr.SendWebRequest();

        if (resetStats) stats = new SpellStats();
    }

    internal class SpellStats
    {
        internal List<SpellStat> stats = new List<SpellStat>();

        public string Convert()
        {
            string res = "{\"crewmates\":[";
            res += stats[0].Convert();
            for (int i = 1; i < stats.Count; i++)
            {
                res += ",";
                res += stats[i].Convert();
            }
            res += "]}";

            return res;
        }
    }

    internal class SpellStat
    {
        string spell;
        string delta;
        int time;
        string mem;

        public SpellStat(string spell, string delta, string mem)
        {
            this.spell = spell;
            this.delta = delta;
            this.time = BenchMB.currentUnixTime;
            this.mem = mem;
        }

        public string Convert()
        {

            return $"{{\"spell\":{spell},\"delta\":{delta},\"time\":{time},\"mem\":{mem}}}";
        }
    }
}
