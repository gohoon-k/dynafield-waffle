public static class G {

    public static Track[] tracks;
    
    public static class Keys {

        public const string SelectedTrack = "selected_track";
        
        public const string Difficulty = "difficulty";
        public const string NotifyLineMove = "notify_line_move";
        public const string ShowEarlyLate = "show_early_late";
        
        public const string Sync = "sync";
        public const string Speed = "speed";

        public const string Energy = "energy";
        public const string CoolDown = "cool_down";

        public const string BestScore = "score_{0}_{1}";
        public const string BestAccuracy = "accuracy_{0}_{1}";
        public const string SuperPlay = "super_play_{0}_{1}";

        public static string FormatKey(string key) {
            return string.Format(key, PlaySettings.TrackId, PlaySettings.Difficulty);
        }

    }

    public static class Items {

        public static int Energy = 0;
        public static long CoolDown = -1;

    }

    public static class PlaySettings {

        public static int TrackId = -1;

        public static int Difficulty = 0;
        public static bool NotifyLineMove = false;
        public static bool ShowEarlyLate = false;

        public static float Sync = 0;
        public static int Speed = 4;

    }

    public static class InternalSettings {

        public static float[] JudgeOfClick = { 0.5f, 0.67f, 0.73f, 0.85f };
        public static float[] JudgeOfSlide = { 0.73f, 0.73f, 0.9f, 0.9f };
        public static float[] JudgeOfHold = { 0.67f, 0.73f, 0.85f, 0.92f };

        public static float[] ScoreRatioByJudges = { 1f, 1f, 0.75f, 0.25f, 0f };
        public static float[] AccuracyRatioByJudges = { 1f, 0.75f, 0.3f, 0.1f, 0f };

    }

    public static class InGame {

        public static bool Paused = false;
        public static bool CanBePaused = true;

        public static bool ReadyAnimated = false;

        public static int Time = 0;

        public static int CountOfNotes = 0;
        public static int ScorePerNote = 0;

        public static float ScoreByJudge = 0;
        public static float ScoreByCombo = 0;
        public static float TotalScore = 0;

        public static float Accuracy = 0;
        
        public static int Combo = 0;
        public static int MaxCombo = 0;

        public static int CountOfAccuracyPerfect = 0;
        public static int CountOfPerfect = 0;
        public static int CountOfGreat = 0;
        public static int CountOfBad = 0;
        public static int CountOfError = 0;

    }

    public class Track {
        public string Title;
        public string Artist;
        public string Id;
        public int[] Difficulty;
    }
    
}
