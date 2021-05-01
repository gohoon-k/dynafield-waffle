using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TrackSelect : MonoBehaviour {
    #region UIObj

    public Image trackBackground;
    public Image trackBackgroundBright;
    public Animator trackBackgroundBrightAnimator;
    public Image virtualBackground;
    public Image virtualBackgroundBright;

    public Text trackInfo; //곡 정보
    public Text bastScore; //최고 스코어
    public Text bestAccuracyInteger; //최고 정확도
    public Text bestAccuracyFloat; //최고 정확도
    public Text remainingEnergy; //현재 에너지
    public Text maxEnergy; //최대 에너지
    public Text energyTimer; //에너지 충전 대기시간
    public Text difficulty; //난이도 숫자
    public Text[] difficultyDescriptions; //난이도 easy
    public Text speed; //채보속도 숫자
    public Button speedIncrease;
    public Text speedIncreaseText;
    public Button speedDecrease;
    public Text speedDecreaseText;

    public Text keysUI;

    #endregion

    public RectTransform scalableArea;
    public RectTransform scalableAreaPrepare;
    public Animator prepareBack;
    public Animator preparePlay;

    public LockedBarrier barrier;
    
    private Image _startGameEffectImage;

    private AudioSource _previewPlayer;
    private AudioClip[] _previewClips;

    private Sprite[] _backgrounds;
    private Sprite[] _brightBackgrounds;

    private bool _trackSelectable = true;
    private bool _canStartGame;
    private bool _prepareAnimating;
    private bool _starting;

    void Start() {
        #region Initialization

        if (!PlayerPrefs.HasKey("initialized"))
            InitializePlayerPrefs();

        G.InitTracks();

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

        speed.text = G.PlaySettings.DisplaySpeed.ToString();

        _startGameEffectImage = preparePlay.gameObject.transform.GetChild(0).gameObject.GetComponent<Image>();

        #endregion

        _previewPlayer = GetComponent<AudioSource>();
        _previewClips = Resources.LoadAll<AudioClip>("tracks/preview");

        SelectTrack();

        CheckCooldownFinished();
        UpdateEnergyUI(G.Items.Energy);

        StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, 2, 1, 1f, step => {
            scalableArea.localScale = new Vector3(step, step, 1);
            scalableAreaPrepare.localScale = new Vector3(step, step, 1);
        }, () => { }));

        if (G.Items.Energy == 0 && G.Items.CoolDown == -1)
            G.Items.CoolDown = DateTime.Now.AddMinutes(5).ToBinary();

        UpdateKeyUI(0);
    }

    void Update() {
        if (G.Items.CoolDown != -1) {
            CheckCooldownFinished();
            UpdateEnergyUI(0, false);
        }
    }

    public void UpdateEnergyUI(int delta, bool energyChanged = true) {
        if (G.Items.CoolDown != -1) {
            var difference = new DateTime(G.Items.CoolDown - DateTime.Now.ToBinary());
            energyTimer.text = difference.ToString("m:ss");
        }

        if (energyChanged)
            StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, G.Items.Energy - delta, G.Items.Energy,
                0.35f,
                step => { remainingEnergy.text = $"{(int) step}"; }, () => { })
            );

        remainingEnergy.color = new Color(remainingEnergy.color.r, remainingEnergy.color.g,
            remainingEnergy.color.b, G.Items.CoolDown == -1 ? 1.0f : 0.15f);

        maxEnergy.color = new Color(maxEnergy.color.r, maxEnergy.color.g,
            maxEnergy.color.b, G.Items.CoolDown == -1 ? 1.0f : 0.15f);

        maxEnergy.text = $"/{G.Items.MaxEnergy[G.Items.MaxEnergyStep]}";

        energyTimer.color = new Color(energyTimer.color.r, energyTimer.color.g,
            energyTimer.color.b, G.Items.CoolDown == -1 ? 0.0f : 1.0f);
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
            step => { keysUI.text = $"<size=50>you have</size>\n{(int) step} key(s)"; }, () => { }));
    }

    public void PreparePlay() {
        if (_prepareAnimating || _starting) return;

        _prepareAnimating = true;

        prepareBack.gameObject.SetActive(true);
        preparePlay.gameObject.SetActive(true);

        preparePlay.transform.GetChild(0).gameObject.SetActive(G.Items.Energy != 0);
        preparePlay.transform.GetChild(1).gameObject.SetActive(G.Items.Energy != 0);
        preparePlay.transform.GetChild(2).gameObject.SetActive(G.Items.Energy != 0);
        preparePlay.transform.GetChild(3).gameObject.SetActive(G.Items.Energy != 0);

        preparePlay.transform.GetChild(4).gameObject.SetActive(G.Items.Energy == 0);

        prepareBack.SetFloat("speed", 3f);
        preparePlay.SetFloat("speed", 3f);
        prepareBack.Play("track_select_prepare_back", -1, 0);
        preparePlay.Play("track_select_prepare_play", -1, 0);
        StartCoroutine(SetGameCanStart(0.2f));
    }

    public void CancelPrepare(bool slow = false) {
        if (_prepareAnimating) return;
        _prepareAnimating = true;


        _canStartGame = false;

        prepareBack.Play("track_select_prepare_back", -1, 1);
        preparePlay.Play("track_select_prepare_play", -1, 1);
        prepareBack.SetFloat("speed", slow ? -0.75f : -2f);
        preparePlay.SetFloat("speed", slow ? -0.75f : -2f);
        StartCoroutine(DeactivatePrepare(slow ? 0.6f : 0.2f));
    }

    private IEnumerator SetGameCanStart(float length) {
        yield return new WaitForSeconds(length);
        _canStartGame = true;
        _prepareAnimating = false;
    }

    private IEnumerator DeactivatePrepare(float length) {
        yield return new WaitForSeconds(length);

        preparePlay.transform.GetChild(0).gameObject.SetActive(false);
        preparePlay.transform.GetChild(1).gameObject.SetActive(false);
        preparePlay.transform.GetChild(2).gameObject.SetActive(false);
        preparePlay.transform.GetChild(3).gameObject.SetActive(false);

        preparePlay.transform.GetChild(4).gameObject.SetActive(false);

        prepareBack.gameObject.SetActive(false);
        preparePlay.gameObject.SetActive(false);
        _prepareAnimating = false;
    }

    public void StartGame() {
        if (_starting || !_canStartGame) return;
        _starting = true;

        StartCoroutine(Enter());
    }

    private IEnumerator Enter() {
        var backgroundBrightAnimator = trackBackgroundBright.GetComponent<Animator>();
        backgroundBrightAnimator.enabled = false;

        trackBackgroundBrightAnimator.enabled = false;

        StartCoroutine(Interpolators.Linear(1, 0, 0.15f, step => {
            _startGameEffectImage.color = new Color(1, 1, 1, step);
            trackBackgroundBright.color = new Color(1, 1, 1, step * trackBackgroundBright.color.a);
        }, () => { }));

        StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, 1, 2f, 1f, step => {
            scalableArea.localScale = new Vector3(step, step, 1);
            scalableAreaPrepare.localScale = new Vector3(step, step, 1);
        }, () => { }));

        CancelPrepare(true);

        yield return new WaitForSeconds(2f);


        SceneManager.LoadScene("TrackPlay");
    }

    private void InitializePlayerPrefs() {
        G.PlaySettings.DisplaySpeed = 4;
        G.PlaySettings.DisplaySync = 0;
        G.Items.MaxEnergyStep = 0;
        G.Items.Energy = G.Items.MaxEnergy[G.Items.MaxEnergyStep];
        G.Items.CoolDown = -1;
        G.Items.Key = 0;
        PlayerPrefs.SetInt("initialized", 1);
        PlayerPrefs.Save();
    }

    private void SelectTrack() {
        if (!G.PlaySettings.FromTrackPlay) {
            trackBackground.color = new Color(1, 1, 1, 0);
            trackBackgroundBright.color = new Color(1, 1, 1, 0);

            StartCoroutine(Interpolators.Linear(0, 1, 1f, step => {
                trackBackground.color = new Color(1, 1, 1, step);
                trackBackgroundBright.color = new Color(1, 1, 1, step);
            }, () => { }));
        }
        
        if (!G.TrackUnlockData[G.PlaySettings.TrackId])
            barrier.Show();

        trackBackground.sprite = _backgrounds[G.PlaySettings.TrackId];
        trackBackgroundBright.sprite = _brightBackgrounds[G.PlaySettings.TrackId];

        trackInfo.GetComponent<Text>().text =
            $"{G.Tracks[G.PlaySettings.TrackId].title}   <size=40>{G.Tracks[G.PlaySettings.TrackId].artist}</size>";
        difficulty.text = $"{G.Tracks[G.PlaySettings.TrackId].difficulty[G.PlaySettings.Difficulty]}";

        bastScore.text = $"{PlayerPrefs.GetInt(G.Keys.FormatKey(G.Keys.BestScore), 0):000000}";
        var bestAc = PlayerPrefs.GetFloat(G.Keys.FormatKey(G.Keys.BestAccuracy), 0);
        var bestAcInt = Math.Floor(bestAc);
        var bestAcFloat = Math.Floor((bestAc - bestAcInt) * 100);
        bestAccuracyInteger.text = $"{bestAcInt:00}";
        bestAccuracyFloat.text = $"{bestAcFloat:00}";

        _previewPlayer.clip = _previewClips[G.PlaySettings.TrackId];
        _previewPlayer.PlayDelayed(1.0f);
    }

    public void SelectTrack(int dir) //트랙변경
    {
        if (!_trackSelectable) return;

        trackBackground.sprite = _backgrounds[G.PlaySettings.TrackId];
        trackBackgroundBright.sprite = _brightBackgrounds[G.PlaySettings.TrackId];

        G.PlaySettings.TrackId += dir;
        if (G.PlaySettings.TrackId == -1) G.PlaySettings.TrackId = G.Tracks.Length - 1;
        if (G.PlaySettings.TrackId == G.Tracks.Length) G.PlaySettings.TrackId = 0;
        
        if (!G.TrackUnlockData[G.PlaySettings.TrackId] && !barrier.showing)
            barrier.Show();
        else if(G.TrackUnlockData[G.PlaySettings.TrackId] && barrier.showing)
            barrier.Hide();

        virtualBackground.sprite = _backgrounds[G.PlaySettings.TrackId];
        virtualBackgroundBright.sprite = _brightBackgrounds[G.PlaySettings.TrackId];

        trackInfo.GetComponent<Text>().text =
            $"{G.Tracks[G.PlaySettings.TrackId].title}   <size=40>{G.Tracks[G.PlaySettings.TrackId].artist}</size>";
        difficulty.text = $"{G.Tracks[G.PlaySettings.TrackId].difficulty[G.PlaySettings.Difficulty]}";

        bastScore.text = $"{PlayerPrefs.GetInt(G.Keys.FormatKey(G.Keys.BestScore), 0):000000}";
        var bestAc = PlayerPrefs.GetFloat(G.Keys.FormatKey(G.Keys.BestAccuracy), 0);
        var bestAcInt = Math.Floor(bestAc);
        var bestAcFloat = Math.Floor((bestAc - bestAcInt) * 100);
        bestAccuracyInteger.text = $"{bestAcInt:00}";
        bestAccuracyFloat.text = $"{bestAcFloat:00}";
        
        _trackSelectable = false;

        StartCoroutine(Interpolators.Linear(0, 1, 0.15f, step => {
            trackBackground.color = new Color(1, 1, 1, 1 - step);
            trackBackgroundBright.color = new Color(1, 1, 1, 1 - step);
            virtualBackground.color = new Color(1, 1, 1, step);
            virtualBackgroundBright.color = new Color(1, 1, 1, step);
        }, () => {
            trackBackground.sprite = virtualBackground.sprite;
            trackBackgroundBright.sprite = virtualBackgroundBright.sprite;

            trackBackground.color = new Color(1, 1, 1, 1);
            trackBackgroundBright.color = new Color(1, 1, 1, 1);
            virtualBackground.color = new Color(1, 1, 1, 0);
            virtualBackgroundBright.color = new Color(1, 1, 1, 0);

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
        var previousDifficulty = difficultyDescriptions[G.PlaySettings.Difficulty];
        previousDifficulty.color = new Color(
            previousDifficulty.color.r, previousDifficulty.color.g, previousDifficulty.color.b, 0.4f
        );
        G.PlaySettings.Difficulty = G.PlaySettings.Difficulty == 0 ? 1 : 0;
        var currentDifficulty = difficultyDescriptions[G.PlaySettings.Difficulty];
        currentDifficulty.color = new Color(
            currentDifficulty.color.r, currentDifficulty.color.g, currentDifficulty.color.b, 1f
        );

        difficulty.GetComponent<Text>().text =
            $"{G.Tracks[G.PlaySettings.TrackId].difficulty[G.PlaySettings.Difficulty]}";
    }

    public void SetDisplaySpeed(int delta) //DisplaySpeed 변경   
    {
        G.PlaySettings.DisplaySpeed += delta;
        speed.text = $"{G.PlaySettings.DisplaySpeed}";

        speedIncrease.interactable = G.PlaySettings.DisplaySpeed != 9;
        speedDecrease.interactable = G.PlaySettings.DisplaySpeed != 1;

        speedIncreaseText.color = new Color(1, 1, 1, speedIncrease.interactable ? 1f : 0.5f);
        speedDecreaseText.color = new Color(1, 1, 1, speedDecrease.interactable ? 1f : 0.5f);
    }

    public void RefillEnergy() {
        G.Items.Energy = G.Items.MaxEnergy[G.Items.MaxEnergyStep];
        G.Items.CoolDown = -1;
        UpdateEnergyUI(G.Items.MaxEnergy[G.Items.MaxEnergyStep]);
    }

    public void StartCooldownNow() {
        G.Items.Energy = 0;
        G.Items.CoolDown = DateTime.Now.AddMinutes(5).ToBinary();
    }

    public void Back() //뒤로가기 버튼
    {
        PlayerPrefs.SetInt(G.Keys.SelectedTrack, G.PlaySettings.TrackId);
        PlayerPrefs.SetInt(G.Keys.Speed, G.PlaySettings.DisplaySpeed);
        PlayerPrefs.SetInt(G.Keys.Sync, G.PlaySettings.DisplaySync);
        PlayerPrefs.SetInt(G.Keys.Energy, G.Items.Energy);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Intro");
    }
}