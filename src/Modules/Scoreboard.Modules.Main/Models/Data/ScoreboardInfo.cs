using System.IO;

namespace Scoreboard.Modules.Main.Models.Data;

public class ScoreboardInfo
{
    string[] Name =
        {"HOME_TEAM",
        "GUEST_TEAM",
        "PERIOD",
        "TIME_MINUTES",
        "TIME_SECONDS",
        "HOME_SCORE",
        "GUEST_SCORE",
        "HOME_PENALTY1_NUM",
        "HOME_PENALTY1_TIME_MINUTES",
        "HOME_PENALTY1_TIME_SECONDS",
        "HOME_PENALTY2_NUM",
        "HOME_PENALTY2_TIME_MINUTES",
        "HOME_PENALTY2_TIME_SECONDS",
        "GUEST_PENALTY1_NUM",
        "GUEST_PENALTY1_TIME_MINUTES",
        "GUEST_PENALTY1_TIME_SECONDS",
        "GUEST_PENALTY2_NUM",
        "GUEST_PENALTY2_TIME_MINUTES",
        "GUEST_PENALTY2_TIME_SECONDS",};

    string[] Value = new string[19];

    public string GetName(int id)
    {
        return Name[id];
    }

    public string GetValue(int id)
    {
        return Value[id];
    }

    public void SetValue(int id, string value)
    {
        Value[id] = value;
    }

    public void SaveValue(int id, string path)
    {
        if (Value[id] == null)
            return;

        using (StreamWriter logWriter = new StreamWriter($"{path}\\{Name[id]}.txt"))
        {
            logWriter.Write(Value[id]);
            logWriter.Close();
        }
    }
}
