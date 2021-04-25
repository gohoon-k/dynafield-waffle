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

        public static int Energy = 15;

        public static int MaxEnergy = 15;
        public static long CoolDown = -1;

    }

    public static class PlaySettings {
        public static int TrackId = 0;

        public static int Difficulty = 1;

        public static int DisplaySync = 5;
        public static int DisplaySpeed = 3;

        public static bool AutoPlay = false;

        public static float Speed => DisplaySpeed / 200f;

        public static int Sync => (int) ((DisplaySync / 165f + 1) * 165f);
    }

    public static class InternalSettings {

        public static readonly float[] JudgeOfClick = { 0.2f, 0.08f, 0.06f };
        public static readonly float[] JudgeOfSlide = { 0.2f, 0.08f, 0.08f };
        public static readonly float[] JudgeOfHold = { 0.50f, 0.80f, 0.90f };
        public static readonly float[] JudgeOfSwipe = { 0.3f, 0.15f, 0.1f };
        public static readonly float[] JudgeOfCounter = { 0.60f, 1f };

        public static readonly float[] ScoreRatioByJudges = { 1f, 1f, 0.75f, 0.25f, 0f };
        public static readonly float[] AccuracyRatioByJudges = { 1f, 0.75f, 0.3f, 0.1f, 0f };

    }

    public static class InGame {

        public static bool PreparePause = false;
        public static bool Paused = false;
        public static bool CanBePaused = true;
        public static bool CanBeResumed = false;

        public static bool ReadyAnimated = false;

        public static float Time;

        public static int CountOfNotes = 0;

        public static float ScoreByJudge;
        public static float ScoreByCombo;
        
        public static float TotalScore => ScoreByJudge + ScoreByCombo;

        public static float Accuracy;
        
        public static int Combo;
        public static int MaxCombo;

        public static int CountOfAccuracyPerfect;
        public static int CountOfPerfect;
        public static int CountOfGreat;
        public static int CountOfBad;
        public static int CountOfError;

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

    public static class TracksRecord        //기록 저장용 클래스
    {
        public static float[,] tracksrecord = new float[G.Tracks.Length,4];
        //y축 = 곡별로 구분(TrackId와 동일)
        //x축 = 0=easy최대스코어  1=easy최대정확도 2=hard최대스코어 3=hard최대정확도
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
