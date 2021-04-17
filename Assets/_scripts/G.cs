using System;
using UnityEngine;

public static class G {

    public static Track[] Tracks;
    
    public static void InitTracks() {
        if (Tracks != null) return;
        Tracks = JsonUtility.FromJson<TrackList>(Resources.Load("data/tracks", typeof(TextAsset)).ToString()).tracks;
    }
    
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
        public const string PlayTimes = "play_time_{0}_{1}";

        public static string FormatKey(string key) {
            return string.Format(key, PlaySettings.TrackId, PlaySettings.Difficulty);
        }

    }

    public static class Items {

        public static int Energy = 3;
        public static long CoolDown = -1;

    }

    public static class PlaySettings {

        public static int TrackId = 0;

        public static int Difficulty = 1;
        public static bool NotifyLineMove = false;
        public static bool ShowEarlyLate = false;

        public static float Sync = 165;
        public static int DisplaySpeed = 3;

        public static bool AutoPlay = false;

        public static float Speed => DisplaySpeed / 200f;
    }

    public static class InternalSettings {

        public static float[] JudgeOfClick = { 0.2f, 0.08f, 0.06f };
        public static float[] JudgeOfSlide = { 0.2f, 0.08f, 0.08f };
        public static float[] JudgeOfHold = { 0.50f, 0.80f, 0.90f };

        public static float[] ScoreRatioByJudges = { 1f, 1f, 0.75f, 0.25f, 0f };
        public static float[] AccuracyRatioByJudges = { 1f, 0.75f, 0.3f, 0.1f, 0f };

    }

    public static class InGame {

        public static bool PreparePause = false;
        public static bool Paused = false;
        public static bool CanBePaused = true;
        public static bool CanBeResumed = false;

        public static bool ReadyAnimated = false;

        public static float Time = 0;

        public static int CountOfNotes = 0;
        public static int ScorePerNote = 0;

        public static float ScoreByJudge = 0;
        public static float ScoreByCombo = 0;
        
        public static float TotalScore => ScoreByJudge + ScoreByCombo;

        public static float Accuracy = 0;
        
        public static int Combo = 0;
        public static int MaxCombo = 0;

        public static int CountOfAccuracyPerfect = 0;
        public static int CountOfPerfect = 0;
        public static int CountOfGreat = 0;
        public static int CountOfBad = 0;
        public static int CountOfError = 0;

        public static void Init() {
            Time = 0;
            ScoreByJudge = 0;
            ScoreByCombo = 0;
            Accuracy = 0;
            Combo = 0;
            MaxCombo = 0;
            CountOfAccuracyPerfect = 0;
            CountOfPerfect = 0;
            CountOfGreat = 0;
            CountOfBad = 0;
            CountOfError = 0;
        }

    }

    [Serializable]
    public class TrackList {
        public Track[] tracks;
    }

    [Serializable]
    public class Track {
        public string internal_name;
        public string title;
        public string artist;
        public string id;
        public int[] difficulty;
        public string length;
    }
    
}
