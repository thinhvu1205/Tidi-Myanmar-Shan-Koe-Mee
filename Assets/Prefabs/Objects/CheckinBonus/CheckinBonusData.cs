using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[Serializable]
public class OnlinePolicyData
{
    public List<int> timeWaiting;
    public List<int> chipBonus;
}
public class WeeklyBonusData
{
    public List<int> listDP;
    public int OD;

    public static WeeklyBonusData FromJson(JObject json)
    {
        var weekly = new WeeklyBonusData
        {
            listDP = new List<int>(),
            OD = json["OD"]?.ToObject<int>() ?? 0
        };

        string listDPString = json["ListDP"]?.ToString();
        if (!string.IsNullOrEmpty(listDPString))
        {
            var parts = listDPString.Split(';')
                .Where(p => !string.IsNullOrEmpty(p));

            foreach (var part in parts)
            {
                var valueStr = part.Split('_')[0];
                if (int.TryParse(valueStr, out int value))
                {
                    weekly.listDP.Add(value);
                }
            }
        }

        return weekly;
    }
}

[Serializable]
public class CheckinBonusData
{
    public int OC;   // lần nhận hiện tại
    public int OM;   // số lần tối đa trong ngày
    public int T;    // thời gian còn lại (giây)
    public OnlinePolicyData OnlinePolicy;

    // Parse từ JObject server gửi
    public static CheckinBonusData FromJson(JObject json)
    {
        var data = new CheckinBonusData
        {
            OC = json["OC"]?.ToObject<int>() ?? 0,
            OM = json["OM"]?.ToObject<int>() ?? 0,
            T = json["T"]?.ToObject<int>() ?? 0,
        };

        if (json.ContainsKey("OnlinePolicy"))
        {
            string onlinePolicyStr = json["OnlinePolicy"].ToString();
            data.OnlinePolicy = JsonConvert.DeserializeObject<OnlinePolicyData>(onlinePolicyStr);
        }

        return data;
    }

    // Đổi T sang hh:mm:ss
    public string GetTimeRemainFormatted(int time)
    {
        TimeSpan ts = TimeSpan.FromSeconds(time);
        return $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
    }

    // Lấy thưởng lần kế tiếp dựa theo OC
    public int GetNextBonus()
    {
        if (OnlinePolicy?.chipBonus == null) return 0;
        if (OC < OnlinePolicy.chipBonus.Count)
            return OnlinePolicy.chipBonus[OC];
        return 0;
    }

    // Lấy thời gian chờ lần kế tiếp
    public string GetNextWaitingTimeString()
    {
        if (OnlinePolicy?.timeWaiting == null || OnlinePolicy.timeWaiting.Count == 0)
            return "00:00:00";

        int nextIndex = OC + 1;
        if (nextIndex < OnlinePolicy.timeWaiting.Count)
        {
            int minutes = OnlinePolicy.timeWaiting[nextIndex];
            TimeSpan t = TimeSpan.FromMinutes(minutes);
            return t.ToString(@"hh\:mm\:ss");
        }

        return "00:00:00";
    }


}
