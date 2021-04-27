using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TrackSelect : MonoBehaviour
{

    #region UIObj

    public Sprite[] backgrounds;
    public Sprite[] brightBackgrounds;

    public Image trackBackground;
    public Image trackBackgroundBright;
    public Image virtualBackground;
    public Image virtualBackgroundBright;
    
    public Text trackInfo;       //곡 정보
    public Text bastScore;        //최고 스코어
    public Text bestAccuracy;     //최고 정확도
    public Text remainingEnergy;      //현재 에너지
    public Text maxEnergy;       //최대 에너지
    public Text energyTimer;     //에너지 충전 대기시간
    public Text difficulty;       //난이도 숫자
    public Text[] difficultyDescriptions;  //난이도 easy
    public Text speed;            //채보속도 숫자
    public Button speedIncrease;
    public Text speedIncreaseText;
    public Button speedDecrease;
    public Text speedDecreaseText;
    public Image start_toggle;
    public Text start_message;
    public Image prepareplay;
    
    #endregion

    public RectTransform scalableArea;
    public RectTransform scalableAreaPrepare;
    public Animator prepareBack;
    public Animator preparePlay;
    public Image startGameEffectImage;

    private bool _trackSelectable = true;
    private bool _canStartGame = false;
    private bool _prepareAnimating = false;
    private bool _starting = false;
    
    void Start()
    {
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

        G.Items.MaxEnergy = PlayerPrefs.GetInt(G.Keys.MaxEnergy);
        G.Items.Energy = PlayerPrefs.GetInt(G.Keys.Energy);
        G.Items.CoolDown = (long) PlayerPrefs.GetFloat(G.Keys.CoolDown);
        
        PlayerPrefs.Save();

        backgrounds = Resources.LoadAll<Sprite>("textures/tracks/normal");
        Array.Sort(backgrounds, (a, b) => int.Parse(a.name) - int.Parse(b.name));
        brightBackgrounds = Resources.LoadAll<Sprite>("textures/tracks/bright");
        Array.Sort(brightBackgrounds, (a, b) => int.Parse(a.name) - int.Parse(b.name));

        SelectTrack();

        CheckCooldownFinished();
        UpdateEnergyUI();
        
        speed.text = G.PlaySettings.DisplaySpeed.ToString();
        
        startGameEffectImage = preparePlay.gameObject.transform.GetChild(0).gameObject.GetComponent<Image>();

        #endregion
        
        
        
        StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, 2, 1, 1f, step => {
            scalableArea.localScale = new Vector3(step, step, 1);
            scalableAreaPrepare.localScale = new Vector3(step, step, 1);
        }, () => { }));
        
        if (G.Items.Energy == 0 && G.Items.CoolDown == -1)
        {
            G.Items.CoolDown = DateTime.Now.AddMinutes(5).Ticks;
        }
    }
    
    void Update()
    {
        if (G.Items.CoolDown != -1) {
            CheckCooldownFinished();
            UpdateEnergyUI();
        }
    }
    
    private void UpdateEnergyUI()
    {
        if (G.Items.CoolDown != -1) {
            var difference = new DateTime(G.Items.CoolDown - DateTime.Now.Ticks);
            energyTimer.text = difference.ToString("mm:ss");
        }

        remainingEnergy.text = $"{G.Items.Energy}";
        
        remainingEnergy.color = new Color(remainingEnergy.color.r, remainingEnergy.color.g,
            remainingEnergy.color.b, G.Items.CoolDown == -1 ? 1.0f : 0.15f);
        
        maxEnergy.color = new Color(maxEnergy.color.r, maxEnergy.color.g,
            maxEnergy.color.b, G.Items.CoolDown == -1 ? 1.0f : 0.15f);

        maxEnergy.text = $"/{G.Items.MaxEnergy}";
        
        energyTimer.color = new Color(energyTimer.color.r, energyTimer.color.g,
            energyTimer.color.b, G.Items.CoolDown == -1 ? 0.0f : 1.0f);
    }
    
    private void CheckCooldownFinished() {
        if (G.Items.CoolDown == -1 || G.Items.CoolDown - DateTime.Now.Ticks >= 0) return;
        
        G.Items.Energy = G.Items.MaxEnergy;
        G.Items.CoolDown = -1;

        PlayerPrefs.SetInt(G.Keys.Energy, G.Items.MaxEnergy);
        PlayerPrefs.SetFloat(G.Keys.CoolDown, G.Items.CoolDown);
        PlayerPrefs.Save();

        UpdateEnergyUI();
    }

    public void PreparePlay()
    {
        // prepareplay.gameObject.SetActive(true);
        // start_toggle.color = new Color(start_toggle.color.r, start_toggle.color.g,
        //     start_toggle.color.b, 0.3f);
        
        if (_prepareAnimating || _starting) return;
        
        _prepareAnimating = true;
        
        prepareBack.gameObject.SetActive(true);
        preparePlay.gameObject.SetActive(true);
        prepareBack.SetFloat("speed", 1f);
        preparePlay.SetFloat("speed", 1f);
        prepareBack.Play("track_select_prepare_back", -1, 0);
        preparePlay.Play("track_select_prepare_play", -1, 0);
        StartCoroutine(SetGameCanStart(0.5f));
    }

    public void CancelPrepare(bool slow = false) {
        if (_prepareAnimating) return;
        _prepareAnimating = true;
        
        
        _canStartGame = false;
        
        prepareBack.Play("track_select_prepare_back", -1, 1);
        preparePlay.Play("track_select_prepare_play", -1, 1);
        prepareBack.SetFloat("speed", slow ? -0.75f : -2f);
        preparePlay.SetFloat("speed", slow ? -0.75f : -2f);
        StartCoroutine(DeactivatePrepare(1));
    }

    private IEnumerator SetGameCanStart(float length) {
        yield return new WaitForSeconds(length);
        _canStartGame = true;
        _prepareAnimating = false;
    }

    private IEnumerator DeactivatePrepare(float length) {
        yield return new WaitForSeconds(length);
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

        StartCoroutine(Interpolators.Linear(1, 0, 0.15f, step => {
            startGameEffectImage.color = new Color(1, 1, 1, step);
            trackBackgroundBright.color = new Color(1, 1, 1, step);
        }, () => { }));
        
        StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, 1, 2f, 1f, step => {
            scalableArea.localScale = new Vector3(step, step, 1);
            scalableAreaPrepare.localScale = new Vector3(step, step, 1);
        }, () => { }));

        CancelPrepare(true);

        yield return new WaitForSeconds(2f);


        SceneManager.LoadScene("TrackPlay");
    }
    
    private void InitializePlayerPrefs()
    {
        PlayerPrefs.SetInt(G.Keys.Speed, 5);
        PlayerPrefs.SetInt(G.Keys.Sync, 3);
        PlayerPrefs.SetInt(G.Keys.MaxEnergy, 10);
        PlayerPrefs.SetInt(G.Keys.Energy, 10);
        PlayerPrefs.SetInt(G.Keys.CoolDown, -1);
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
            }, () => {}));
        }
        
        trackBackground.sprite = backgrounds[G.PlaySettings.TrackId];
        trackBackgroundBright.sprite = brightBackgrounds[G.PlaySettings.TrackId];

        trackInfo.GetComponent<Text>().text =
            $"{G.Tracks[G.PlaySettings.TrackId].title}   <size=40>{G.Tracks[G.PlaySettings.TrackId].artist}</size>";
        difficulty.text = $"{G.Tracks[G.PlaySettings.TrackId].difficulty[G.PlaySettings.Difficulty]}";

        bastScore.text = $"{PlayerPrefs.GetInt(G.Keys.FormatKey(G.Keys.BestScore), 0):000000}";
        bestAccuracy.text = $"{PlayerPrefs.GetFloat(G.Keys.FormatKey(G.Keys.BestAccuracy), 0):F2}%";
    }
    
    public void SelectTrack(int dir)        //트랙변경
    {
        if (!_trackSelectable) return;

        trackBackground.sprite = backgrounds[G.PlaySettings.TrackId];
        trackBackgroundBright.sprite = brightBackgrounds[G.PlaySettings.TrackId];

        G.PlaySettings.TrackId += dir;
        if (G.PlaySettings.TrackId == -1) G.PlaySettings.TrackId = G.Tracks.Length - 1;
        if (G.PlaySettings.TrackId == G.Tracks.Length) G.PlaySettings.TrackId = 0;

        virtualBackground.sprite = backgrounds[G.PlaySettings.TrackId];
        virtualBackgroundBright.sprite = brightBackgrounds[G.PlaySettings.TrackId];
        
        trackInfo.GetComponent<Text>().text =
            $"{G.Tracks[G.PlaySettings.TrackId].title}   <size=40>{G.Tracks[G.PlaySettings.TrackId].artist}</size>";
        difficulty.text = $"{G.Tracks[G.PlaySettings.TrackId].difficulty[G.PlaySettings.Difficulty]}";

        bastScore.text = $"{PlayerPrefs.GetInt(G.Keys.FormatKey(G.Keys.BestScore), 0):000000}";
        bestAccuracy.text = $"{PlayerPrefs.GetFloat(G.Keys.FormatKey(G.Keys.BestAccuracy), 0):F2}%";

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
    }
    
    public void ToggleDifficulty()              //난이도 변경
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
        
        difficulty.GetComponent<Text>().text = $"{G.Tracks[G.PlaySettings.TrackId].difficulty[G.PlaySettings.Difficulty]}";
        
        PlayerPrefs.SetInt(G.Keys.Difficulty, G.PlaySettings.Difficulty);
    }
    
    public void SetDisplaySpeed(int delta)      //DisplaySpeed 변경   
    {
        G.PlaySettings.DisplaySpeed += delta;
        speed.text = $"{G.PlaySettings.DisplaySpeed}";

        PlayerPrefs.SetInt(G.Keys.Speed, G.PlaySettings.DisplaySpeed);
        
        speedIncrease.interactable = G.PlaySettings.DisplaySpeed != 9;
        speedDecrease.interactable = G.PlaySettings.DisplaySpeed != 1;

        speedIncreaseText.color = new Color(1, 1, 1, speedIncrease.interactable ? 1f : 0.5f);
        speedDecreaseText.color = new Color(1, 1, 1, speedDecrease.interactable ? 1f : 0.5f);
    }

    public void Back()                          //뒤로가기 버튼
    {
        PlayerPrefs.SetInt(G.Keys.SelectedTrack,G.PlaySettings.TrackId);
        PlayerPrefs.SetInt(G.Keys.Speed, G.PlaySettings.DisplaySpeed);
        PlayerPrefs.SetInt(G.Keys.Sync, G.PlaySettings.DisplaySync);
        PlayerPrefs.SetInt(G.Keys.Energy, G.Items.Energy);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Intro");
    }

}
