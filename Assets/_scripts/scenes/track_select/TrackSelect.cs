using System;
using System.Collections;
using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[Serializable]
public class UIElements {
    public Scalable scalable;

    public Backgrounds backgrounds;
    public TrackInformation trackInformation;
    public Records records;
    public PlaySettings playSettings;
    public PreparePlay preparePlay;
    public Keys keys;

    [Serializable]
    public class Scalable {
        public RectTransform main;
        public CanvasGroup mainGroup;
        public RectTransform prepare;
    }

    [Serializable]
    public class Backgrounds {
        public Main main;
        public Transition transition;

        [Serializable]
        public class Main {
            public Image normal;
            public Image bright;
            public Image blur;
        }

        [Serializable]
        public class Transition {
            public Image normal;
            public Image bright;
        }
    }

    [Serializable]
    public class TrackInformation {
        public Text text;
    }

    [Serializable]
    public class Records {
        public Text score;
        public Text accuracyInt;
        public Text accuracyFloat;
        public Text playType;
    }

    [Serializable]
    public class PlaySettings {
        public Energy energy;
        public Difficulty difficulty;
        public Speed speed;
        public AutoPlay autoPlay;

        [Serializable]
        public class Energy {
            public Text remaining;
            public Text max;
            public Text timer;
        }

        [Serializable]
        public class Difficulty {
            public Text value;
            public Text[] names;
        }

        [Serializable]
        public class Speed {
            public Text value;
            public Button increase;
            public Button decrease;
        }

        [Serializable]
        public class AutoPlay {
            public Button toggle;
            public Text value;
        }
    }

    [Serializable]
    public class PreparePlay {
        public RectTransform start;
        public CanvasGroup startGroup;
    }

    [Serializable]
    public class Keys {
        public Text text;
    }
}

[Serializable]
public class Others {
    public DialogManager dialogManager;

    public Animator bgBrightAnimator;
    public Animator prepareBack;
    public Animator preparePlay;

    public LockedBarrier barrier;
}

public class TrackSelect : MonoBehaviour {
    public UIElements uiElements;
    public Others others;

    private Image _startGameEffectImage;

    private AudioSource _previewPlayer;
    private AudioClip[] _previewClips;

    private Sprite[] _backgrounds;
    private Sprite[] _brightBackgrounds;
    private Sprite[] _blurBackgrounds;

    public bool preparing;
    
    private bool _canPrepare;
    private bool _trackSelectable = true;
    private bool _canStartGame;
    private bool _prepareAnimating;
    private bool _starting;
    private bool _backAnimating;

    private RewardedAd _energyRefillAd;
    private bool _energyRefillPending;
    private bool _energyRefillAdClosed;

    void Start() {
        #region Initialization

        if (!G.AdInitializeStatus) {
            MobileAds.Initialize(initStatus => { G.AdInitializeStatus = true; });
        }

        if (!PlayerPrefs.HasKey("initialized"))
            InitializePlayerPrefs();

        G.InitTracks();

        G.PlaySettings.AutoPlay = false;

        G.PlaySettings.TrackId = PlayerPrefs.GetInt(G.Keys.SelectedTrack);
        G.PlaySettings.DisplaySpeed = PlayerPrefs.GetInt(G.Keys.Speed);
        G.PlaySettings.DisplaySync = PlayerPrefs.GetInt(G.Keys.Sync);
        G.PlaySettings.Difficulty = PlayerPrefs.GetInt(G.Keys.Difficulty);
        ToggleDifficulty();
        ToggleDifficulty();

        G.Items.MaxEnergyStep = PlayerPrefs.GetInt(G.Keys.MaxEnergyStep);
        G.Items.Energy = PlayerPrefs.GetInt(G.Keys.Energy);
        G.Items.CoolDown = long.Parse(PlayerPrefs.GetString(G.Keys.CoolDown));
        G.Items.Key = PlayerPrefs.GetInt(G.Keys.Key);

        _backgrounds = Resources.LoadAll<Sprite>("textures/tracks/normal");
        Array.Sort(_backgrounds, (a, b) => int.Parse(a.name) - int.Parse(b.name));
        _brightBackgrounds = Resources.LoadAll<Sprite>("textures/tracks/bright");
        Array.Sort(_brightBackgrounds, (a, b) => int.Parse(a.name) - int.Parse(b.name));
        _blurBackgrounds = Resources.LoadAll<Sprite>("textures/tracks/blur");
        Array.Sort(_blurBackgrounds, (a, b) => int.Parse(a.name) - int.Parse(b.name));

        uiElements.playSettings.speed.value.text = $"{G.PlaySettings.DisplaySpeed}";

        _startGameEffectImage = others.preparePlay.gameObject.transform.GetChild(0).GetChild(0).gameObject
            .GetComponent<Image>();

        #endregion

        _previewPlayer = GetComponent<AudioSource>();
        _previewClips = Resources.LoadAll<AudioClip>("tracks/preview");

        SelectTrack();

        CheckCooldownFinished();
        UpdateEnergyUI(G.Items.Energy);

        StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, 2, 1, 1f, step => {
            uiElements.scalable.main.localScale = new Vector3(step, step, 1);
            uiElements.scalable.prepare.localScale = new Vector3(step, step, 1);
        }, () => { _canPrepare = true; }));

        if (G.Items.Energy == 0 && G.Items.CoolDown == -1)
            G.Items.CoolDown = DateTime.Now.AddMinutes(G.InternalSettings.CooldownInMinute).ToBinary();

        UpdateKeyUI(0);

        G.PlaySettings.FromTrackPlay = false;
        G.PlaySettings.FromTrackResult = false;

        LoadEnergyRefillAd();
    }

    void Update() {
        if (G.Items.CoolDown != -1) {
            CheckCooldownFinished();
            UpdateEnergyUI(0, false);
        }

        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (_canStartGame) {
                CancelPrepare();
            } else if (others.dialogManager.DialogShowing) {
                others.dialogManager.CloseAllDialogs();
            } else {
                Back();
            }
        }

        if (_energyRefillAdClosed && _energyRefillPending) {
            others.dialogManager.adDialog.message.text = "성공!";
            
            _energyRefillPending = false;
            _energyRefillAdClosed = false;
            
            RefillEnergy();
            
            StartCoroutine(CloseDialog());
        } else if (_energyRefillAdClosed) {
            others.dialogManager.adDialog.message.text = "실패했습니다. 광고가 로드되지 않은 것 같네요.\n나중에 다시 시도해주세요.";
            _energyRefillAdClosed = false;

            StartCoroutine(CloseDialogDelay());
        }
    }

    private IEnumerator CloseDialogDelay() {
        yield return new WaitForSeconds(3);
        
        StartCoroutine(CloseDialog());
    }
    
    private void LoadEnergyRefillAd() {
        _energyRefillAd = new RewardedAd(G.AD.TestMode ? G.AD.TestRewardedId : G.AD.RefillEnergyId);

        _energyRefillAd.OnUserEarnedReward += HandleUserEarnedReward;
        _energyRefillAd.OnAdClosed += HandleRewardAdClosed;
        _energyRefillAd.OnAdFailedToLoad += HandleOnAdFailedToLoad;

        _energyRefillAd.LoadAd(new AdRequest.Builder().Build());
    }

    public void UpdateEnergyUI(int delta, bool energyChanged = true) {
        if (G.Items.CoolDown != -1) {
            var difference = new DateTime(G.Items.CoolDown - DateTime.Now.ToBinary());
            uiElements.playSettings.energy.timer.text = difference.ToString("m:ss");
        }

        if (energyChanged)
            StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, G.Items.Energy - delta, G.Items.Energy,
                0.35f,
                step => { uiElements.playSettings.energy.remaining.text = $"{(int) step}"; }, () => { })
            );

        uiElements.playSettings.energy.remaining.color = uiElements.playSettings.energy.max.color =
            new Color(1, 1, 1, G.Items.CoolDown == -1 ? 1.0f : 0.15f);

        uiElements.playSettings.energy.max.text = $"/{G.Items.MaxEnergy[G.Items.MaxEnergyStep]}";

        uiElements.playSettings.energy.timer.color =
            new Color(1, 1, 1, G.Items.CoolDown == -1 ? 0.0f : 1.0f);
    }

    private void CheckCooldownFinished() {
        if (G.Items.CoolDown == -1 || G.Items.CoolDown - DateTime.Now.ToBinary() >= 0) return;

        G.Items.Energy = G.Items.MaxEnergy[G.Items.MaxEnergyStep];
        G.Items.CoolDown = -1;

        PlayerPrefs.Save();

        UpdateEnergyUI(G.Items.MaxEnergy[G.Items.MaxEnergyStep]);
    }

    public void UpdateKeyUI(int before) {
        StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, before, G.Items.Key, 0.5f,
            step => { uiElements.keys.text.text = $"<size=50>you have</size>\n{(int) step} key(s)"; }, () => { }));
    }

    public void PreparePlay() {
        if (_prepareAnimating || _starting || !_canPrepare) return;

        preparing = true;

        _prepareAnimating = true;

        others.prepareBack.gameObject.SetActive(true);
        others.preparePlay.gameObject.SetActive(true);

        others.preparePlay.transform.GetChild(0).gameObject.SetActive(G.Items.Energy != 0);
        others.preparePlay.transform.GetChild(1).gameObject.SetActive(G.Items.Energy != 0);
        others.preparePlay.transform.GetChild(2).gameObject.SetActive(G.Items.Energy != 0);

        others.preparePlay.transform.GetChild(3).gameObject.SetActive(G.Items.Energy == 0);

        others.prepareBack.SetFloat(others.prepareBack.GetParameter(0).name, 3f);
        others.preparePlay.SetFloat(others.preparePlay.GetParameter(0).name, 3f);
        others.prepareBack.Play("track_select_prepare_back", -1, 0);
        others.preparePlay.Play("track_select_prepare_play", -1, 0);

        StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, 1.5f, 1f, 0.35f, step => {
                    uiElements.preparePlay.start.localScale = new Vector3(step, step, 1);
                    uiElements.preparePlay.startGroup.alpha = 1 - (step - 1) * 2;
                },
                () => { }
            )
        );

        StartCoroutine(SetGameCanStart(0.2f));
    }

    public void CancelPrepare(bool slow = false) {
        if (_prepareAnimating) return;
        _prepareAnimating = true;

        preparing = false;

            _canStartGame = false;

        others.prepareBack.Play("track_select_prepare_back", -1, 1);
        others.preparePlay.Play("track_select_prepare_play", -1, 1);
        others.prepareBack.SetFloat(others.prepareBack.GetParameter(0).name, slow ? -0.75f : -2f);
        others.preparePlay.SetFloat(others.preparePlay.GetParameter(0).name, slow ? -0.75f : -2f);

        StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, 1f, 0f, 0.5f,
                step => { uiElements.preparePlay.startGroup.alpha = step; },
                () => { }
            )
        );

        StartCoroutine(DeactivatePrepare(slow ? 0.6f : 0.2f));
    }

    private IEnumerator SetGameCanStart(float length) {
        yield return new WaitForSeconds(length);
        _canStartGame = true;
        _prepareAnimating = false;
    }

    private IEnumerator DeactivatePrepare(float length) {
        yield return new WaitForSeconds(length);

        others.preparePlay.transform.GetChild(0).gameObject.SetActive(false);
        others.preparePlay.transform.GetChild(1).gameObject.SetActive(false);
        others.preparePlay.transform.GetChild(2).gameObject.SetActive(false);
        others.preparePlay.transform.GetChild(3).gameObject.SetActive(false);

        others.prepareBack.gameObject.SetActive(false);
        others.preparePlay.gameObject.SetActive(false);
        _prepareAnimating = false;
    }

    public void StartGame() {
        if (_starting || !_canStartGame) return;
        _starting = true;

        StartCoroutine(Enter());
    }

    private IEnumerator Enter() {
        others.bgBrightAnimator.enabled = false;

        var beforeAlpha = uiElements.backgrounds.main.bright.color.a;

        StartCoroutine(Interpolators.Linear(1, 0, 0.15f, step => {
            _startGameEffectImage.color = new Color(1, 1, 1, step);
            uiElements.backgrounds.main.bright.color = new Color(1, 1, 1, step * beforeAlpha);
        }, () => { }));

        StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, 1, 2f, 1f, step => {
            uiElements.scalable.main.localScale = new Vector3(step, step, 1);
            uiElements.scalable.prepare.localScale = new Vector3(step, step, 1);

            _previewPlayer.volume = 2 - step;
        }, () => { }));

        CancelPrepare(true);

        yield return new WaitForSeconds(2f);


        SceneManager.LoadScene("TrackPlay");
    }

    private void InitializePlayerPrefs() {
        G.PlaySettings.DisplaySpeed = 6;
        G.PlaySettings.DisplaySync = 0;
        G.Items.MaxEnergyStep = 0;
        G.Items.Energy = G.Items.MaxEnergy[G.Items.MaxEnergyStep];
        G.Items.CoolDown = -1;
        G.Items.Key = 0;
        PlayerPrefs.SetInt("initialized", 1);
        PlayerPrefs.Save();
    }

    private void UpdateScoreUI() {
        uiElements.records.score.text = $"{PlayerPrefs.GetInt(G.Keys.FormatKey(G.Keys.BestScore), 0):000000}";
        var bestAc = PlayerPrefs.GetFloat(G.Keys.FormatKey(G.Keys.BestAccuracy), 0);
        var bestAcInt = Math.Floor(bestAc);
        var bestAcFloat = Math.Round((bestAc - bestAcInt) * 100);
        uiElements.records.accuracyInt.text = $"{bestAcInt:00}";
        uiElements.records.accuracyFloat.text = $"{bestAcFloat:00}";

        var playType = PlayerPrefs.GetInt(G.Keys.FormatKey(G.Keys.PlayType), 0);
        uiElements.records.playType.text = G.InternalSettings.PlayTypeNames[playType];
    }

    private void SelectTrack() {
        if (!(G.PlaySettings.FromTrackPlay || G.PlaySettings.FromTrackResult)) {
            uiElements.backgrounds.main.normal.color = new Color(1, 1, 1, 0);
            uiElements.backgrounds.main.bright.color = new Color(1, 1, 1, 0);
            uiElements.backgrounds.main.blur.color = new Color(1, 1, 1, 0);

            StartCoroutine(Interpolators.Linear(0, 1, 1f, step => {
                uiElements.backgrounds.main.normal.color = new Color(1, 1, 1, step);
                uiElements.backgrounds.main.bright.color = new Color(1, 1, 1, step);
            }, () => { }));
        }

        if (!G.TrackUnlockData[G.PlaySettings.TrackId])
            others.barrier.Show();

        uiElements.backgrounds.main.normal.sprite = _backgrounds[G.PlaySettings.TrackId];
        uiElements.backgrounds.main.bright.sprite = _brightBackgrounds[G.PlaySettings.TrackId];
        uiElements.backgrounds.main.blur.sprite = _blurBackgrounds[G.PlaySettings.TrackId];

        uiElements.trackInformation.text.text =
            $"{G.Tracks[G.PlaySettings.TrackId].title}   <size=40>{G.Tracks[G.PlaySettings.TrackId].artist}</size>";
        uiElements.playSettings.difficulty.value.text =
            $"{G.Tracks[G.PlaySettings.TrackId].difficulty[G.PlaySettings.Difficulty]}";

        UpdateScoreUI();

        _previewPlayer.clip = _previewClips[G.PlaySettings.TrackId];
        _previewPlayer.PlayDelayed(1.0f);
    }

    public void SelectTrack(int dir) //트랙변경
    {
        if (!_trackSelectable) return;

        uiElements.backgrounds.main.normal.sprite = _backgrounds[G.PlaySettings.TrackId];
        uiElements.backgrounds.main.bright.sprite = _brightBackgrounds[G.PlaySettings.TrackId];
        uiElements.backgrounds.main.blur.sprite = _blurBackgrounds[G.PlaySettings.TrackId];

        G.PlaySettings.TrackId += dir;
        if (G.PlaySettings.TrackId == -1) G.PlaySettings.TrackId = G.Tracks.Length - 1;
        if (G.PlaySettings.TrackId == G.Tracks.Length) G.PlaySettings.TrackId = 0;

        if (!G.TrackUnlockData[G.PlaySettings.TrackId] && !others.barrier.showing)
            others.barrier.Show();
        else if (G.TrackUnlockData[G.PlaySettings.TrackId] && others.barrier.showing)
            others.barrier.Hide();

        uiElements.backgrounds.transition.normal.sprite = _backgrounds[G.PlaySettings.TrackId];
        uiElements.backgrounds.transition.bright.sprite = _brightBackgrounds[G.PlaySettings.TrackId];

        uiElements.trackInformation.text.text =
            $"{G.Tracks[G.PlaySettings.TrackId].title}   <size=40>{G.Tracks[G.PlaySettings.TrackId].artist}</size>";
        uiElements.playSettings.difficulty.value.text =
            $"{G.Tracks[G.PlaySettings.TrackId].difficulty[G.PlaySettings.Difficulty]}";

        UpdateScoreUI();

        _trackSelectable = false;

        StartCoroutine(Interpolators.Linear(0, 1, 0.15f, step => {
            uiElements.backgrounds.main.normal.color = new Color(1, 1, 1, 1 - step);
            uiElements.backgrounds.main.bright.color = new Color(1, 1, 1, 1 - step);
            uiElements.backgrounds.transition.normal.color = new Color(1, 1, 1, step);
            uiElements.backgrounds.transition.bright.color = new Color(1, 1, 1, step);
        }, () => {
            uiElements.backgrounds.main.normal.sprite = uiElements.backgrounds.transition.normal.sprite;
            uiElements.backgrounds.main.bright.sprite = uiElements.backgrounds.transition.bright.sprite;
            uiElements.backgrounds.main.blur.sprite = _blurBackgrounds[G.PlaySettings.TrackId];

            uiElements.backgrounds.main.normal.color = new Color(1, 1, 1, 1);
            uiElements.backgrounds.main.bright.color = new Color(1, 1, 1, 1);
            uiElements.backgrounds.transition.normal.color = new Color(1, 1, 1, 0);
            uiElements.backgrounds.transition.bright.color = new Color(1, 1, 1, 0);

            _trackSelectable = true;
        }));

        StartCoroutine(Interpolators.Linear(1, 0, 0.3f, step => { _previewPlayer.volume = step; }, () => {
            _previewPlayer.clip = _previewClips[G.PlaySettings.TrackId];
            _previewPlayer.volume = 1f;
            _previewPlayer.Play();
        }));
    }

    public void ToggleDifficulty() //난이도 변경
    {
        var previousDifficulty = uiElements.playSettings.difficulty.names[G.PlaySettings.Difficulty];
        previousDifficulty.color = new Color(
            previousDifficulty.color.r, previousDifficulty.color.g, previousDifficulty.color.b, 0.4f
        );
        G.PlaySettings.Difficulty = G.PlaySettings.Difficulty == 0 ? 1 : 0;
        var currentDifficulty = uiElements.playSettings.difficulty.names[G.PlaySettings.Difficulty];
        currentDifficulty.color = new Color(
            currentDifficulty.color.r, currentDifficulty.color.g, currentDifficulty.color.b, 1f
        );

        uiElements.playSettings.difficulty.value.text =
            $"{G.Tracks[G.PlaySettings.TrackId].difficulty[G.PlaySettings.Difficulty]}";

        UpdateScoreUI();
    }

    public void SetDisplaySpeed(int delta) //DisplaySpeed 변경   
    {
        G.PlaySettings.DisplaySpeed += delta;
        uiElements.playSettings.speed.value.text = $"{G.PlaySettings.DisplaySpeed}";

        uiElements.playSettings.speed.increase.interactable = G.PlaySettings.DisplaySpeed < 9;
        uiElements.playSettings.speed.decrease.interactable = G.PlaySettings.DisplaySpeed > 1;
    }

    public IEnumerator StartEnergyRefillAd() {
        yield return new WaitForSeconds(0.75f);
        if (_energyRefillAd.IsLoaded()) {
            _energyRefillAd.Show();
        } else {
            _energyRefillAdClosed = true;
        }
    }
    
    private void HandleRewardAdClosed(object sender, EventArgs args) {
        LoadEnergyRefillAd();

        _energyRefillAdClosed = true;
    }

    private void HandleUserEarnedReward(object sender, Reward args) {
        if (G.AD.TestMode) {
            _energyRefillPending = true;
        } else {
            if (args.Type.Equals("ENERGY")) {
                _energyRefillPending = true;
            }
        }
    }
    
    void HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        Debug.Log("HandleFailedToReceiveAd event received with message: "
                            + args.LoadAdError.GetMessage());
    }

    private IEnumerator CloseDialog() {
        yield return new WaitForSeconds(0.35f);
        others.dialogManager.adDialog.Close(true);
        MuteAudio(false);
    }

    public void MuteAudio(bool mute) {
        StartCoroutine(Interpolators.Linear(mute ? 1 : 0, mute ? 0 : 1, 0.35f, step => { _previewPlayer.volume = step; }, () => { }));
    }

    private void RefillEnergy() {
        G.Items.Energy = G.Items.MaxEnergy[G.Items.MaxEnergyStep];
        G.Items.CoolDown = -1;
        UpdateEnergyUI(G.Items.MaxEnergy[G.Items.MaxEnergyStep]);
    }

    public void StartCooldownNow() {
        G.Items.Energy = 0;
        G.Items.CoolDown = DateTime.Now.AddMinutes(G.InternalSettings.CooldownInMinute).ToBinary();
    }

    public void Back() //뒤로가기 버튼
    {
        if (_backAnimating) return;
        _backAnimating = true;

        StartCoroutine(AnimateBack());
    }

    public void ToggleAutoPlay() {
        G.PlaySettings.AutoPlay = !G.PlaySettings.AutoPlay;
        uiElements.playSettings.autoPlay.value.text = G.PlaySettings.AutoPlay ? "on" : "off";
        uiElements.playSettings.autoPlay.value.color = new Color(1, 1, 1, G.PlaySettings.AutoPlay ? 1 : 0.35f);
    }

    private IEnumerator AnimateBack() {
        var beforeColor = uiElements.backgrounds.main.bright.color.a;

        yield return StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, 0, 1, 1f,
                step => {
                    var newScale = 1 + step / 2f;
                    uiElements.scalable.main.localScale = new Vector3(newScale, newScale, 1);
                    uiElements.scalable.mainGroup.alpha = 1 - step;

                    others.bgBrightAnimator.enabled = false;

                    uiElements.backgrounds.main.bright.color = new Color(1, 1, 1, beforeColor * (1 - step));
                    uiElements.backgrounds.main.normal.color = new Color(1, 1, 1, 1 - step);

                    _previewPlayer.volume = 1 - step;
                },
                () => { }
            )
        );

        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene("Intro");
    }
}