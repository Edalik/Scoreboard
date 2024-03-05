namespace Scoreboard.Modules.Main.Models.Data
{
    public class ScoreboardData
    {
        string[] StatName =
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

        string[] StatValue = new string[19];

        public string GetStatName(int id)
        {
            return StatName[id];
        }

        public string GetStatValue(int id)
        {
            return StatValue[id];
        }
        public void SetStatValue(int id, string value)
        {
            StatValue[id] = value;
        }
    }
}
