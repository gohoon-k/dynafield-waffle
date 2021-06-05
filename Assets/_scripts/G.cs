using System;
using System.Collections.Generic;
using UnityEngine;

public static class G {
    public static Track[] Tracks;
    public static bool[] TrackUnlockData;

    public static bool AdInitializeStatus;

    public static void InitTracks() {
        if (Tracks != null) return;
        Tracks = JsonUtility.FromJson<TrackList>(Resources.Load("data/tracks", typeof(TextAsset)).ToString()).tracks;
        if (PlayerPrefs.GetString(Keys.UnlockedTracks, "") == string.Empty) {
            TrackUnlockData = new bool[Tracks.Length];
            for (var i = 0; i < 2; i++) {
                TrackUnlockData[i] = true;
            }

            SaveTrackUnlockData();
        } else {
            TrackUnlockData = JsonUtility.FromJson<TrackUnlockDataRaw>(PlayerPrefs.GetString(Keys.UnlockedTracks, ""))
                .data;
        }
    }

    public static void SaveTrackUnlockData() {
        var dat = new TrackUnlockDataRaw {data = TrackUnlockData};
        PlayerPrefs.SetString(Keys.UnlockedTracks, JsonUtility.ToJson(dat));
        PlayerPrefs.Save();
    }

    public static class AD {
        public const bool TestMode = true;

        public static readonly List<string> TestDevices = new List<string>(
            new[] {
                "52b9d1fd-95a0-4e67-b7c7-0e4a7d09a9c7"
            }
        );

        public const string TestRewardedId = "ca-app-pub-3940256099942544/5224354917";
        public const string TestInterstitialId = "ca-app-pub-3940256099942544/1033173712";

        public const string RefillEnergyId = "ca-app-pub-7103199490015871/9958279056";
        public const string TrackFinishedId = "ca-app-pub-7103199490015871/4844132290";
    }

    public static class Keys {
        public const string FirstExecution = "first_execution";

        public const string SelectedTrack = "selected_track";

        public const string Difficulty = "difficulty";

        public const string Sync = "sync";
        public const string Speed = "speed";

        public const string MaxEnergyStep = "max_energy_step";
        public const string Energy = "energy";
        public const string CoolDown = "cool_down";
        public const string Key = "key";

        public const string ReceivedPreviousReward = "received_date";
        public const string CheckedDate = "checked_date";
        public const string RewardIndex = "reward_index";

        public const string UnlockedTracks = "track_unlock_data";

        public const string BestScore = "score_{0}_{1}";
        public const string BestAccuracy = "accuracy_{0}_{1}";
        public const string PlayType = "super_play_{0}_{1}";
        private const string PlayTypeRewards = "super_play_reward_{0}_{1}_{2}";
        public const string PlayTimes = "play_time_{0}_{1}";

        public static string FormatKey(string key) {
            return string.Format(key, PlaySettings.TrackId, PlaySettings.Difficulty);
        }

        public static string FormatPlayTypeRewards(int playType) {
            return string.Format(PlayTypeRewards, PlaySettings.TrackId, PlaySettings.Difficulty, playType);
        }
    }

    public static class Items {
        public static int Energy {
            get => _energy;
            set {
                _energy = value;
                PlayerPrefs.SetInt(Keys.Energy, _energy);
            }
        }

        private static int _energy = 10;

        public static readonly int[] MaxEnergy = {5, 7, 10, 25};

        public static int MaxEnergyStep {
            get => _maxEnergyStep;
            set {
                _maxEnergyStep = value;
                PlayerPrefs.SetInt(Keys.MaxEnergyStep, _maxEnergyStep);
            }
        }

        private static int _maxEnergyStep;

        public static long CoolDown {
            get => _coolDown;
            set {
                _coolDown = value;
                PlayerPrefs.SetString(Keys.CoolDown, $"{_coolDown}");
            }
        }

        private static long _coolDown = -1;

        public static int Key {
            get => _key;
            set {
                _key = value;
                PlayerPrefs.SetInt(Keys.Key, _key);
            }
        }

        private static int _key = 0;
    }

    public static class PlaySettings {
        public static int TrackId {
            get => _trackId;
            set {
                _trackId = value;
                PlayerPrefs.SetInt(Keys.SelectedTrack, _trackId);
            }
        }

        private static int _trackId = 0;

        public static int Difficulty {
            get => _difficulty;
            set {
                _difficulty = value;
                PlayerPrefs.SetInt(Keys.Difficulty, _difficulty);
            }
        }

        private static int _difficulty = 0;

        public static int DisplaySync {
            get => _displaySync;
            set {
                _displaySync = value;
                PlayerPrefs.SetInt(Keys.Sync, _displaySync);
            }
        }

        private static int _displaySync = 3;

        public static int DisplaySpeed {
            get => _displaySpeed;
            set {
                _displaySpeed = value;
                PlayerPrefs.SetInt(Keys.Speed, _displaySpeed);
            }
        }

        private static int _displaySpeed = 6;

        public static bool AutoPlay = false;

        public static float Speed => DisplaySpeed / 400f;

        public static int Sync => (int) ((DisplaySync / 165f + 1) * 165f);


        public static bool FromTrackPlay = false;
        public static bool FromTrackResult = false;
    }

    public static class InternalSettings {
        public static readonly float[] JudgeOfClick = {0.2f, 0.08f, 0.06f};
        public static readonly float[] JudgeOfSlide = {0.2f, 0.08f, 0.08f};
        public static readonly float[] JudgeOfHold = {0.50f, 0.80f, 0.90f};
        public static readonly float[] JudgeOfSwipe = {0.3f, 0.15f, 0.1f};
        public static readonly float[] JudgeOfCounter = {0.60f, 1f};

        public static readonly float[] ScoreRatioByJudges = {1f, 1f, 0.75f, 0.25f, 0f};
        public static readonly float[] AccuracyRatioByJudges = {1f, 0.75f, 0.3f, 0.1f, 0f};

        public static readonly int[] DailyRewardAmount = {2, 1, 3, 1, 4, 1};
        public static readonly int[] DailyRewardType = {0, 1, 0, 1, 0, 1};

        public static readonly string[] PlayTypeNames = {"", "FC", "P", "K"};

        public static readonly int[][] PlayTypeRewards = {
            new[] {0, 1, 2, 3}, new[] {0, 2, 5, 10}
        };

        public const int CooldownInMinute = 10;
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

        public static int BestScore;
        public static float BestAccuracy;

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

        public static void Reset() {
            Init();
            BestScore = 0;
            BestAccuracy = 0;
            PreparePause = false;
            Paused = false;
            CanBePaused = true;
            CanBeResumed = false;
            ReadyAnimated = false;
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

    public class TrackUnlockDataRaw {
        public bool[] data;
    }
}