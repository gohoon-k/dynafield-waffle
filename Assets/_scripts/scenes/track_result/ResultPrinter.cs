using System;
using System.Collections;
using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultPrinter : MonoBehaviour {
    public Animator uiAnimator;

    public Image background;
    public Image foreground;

    public Text trackInfo;
    public Text score;
    public Text scoreDiff;
    public Text accuracy;
    public Text accuracyDiff;
    public Text judgeP;
    public Text judgeG;
    public Text judgeB;
    public Text judgeE;
    public Text maxCombo;
    public Image rank;
    public Text difficulty;

    public Button retry;
    public GameObject energyDes;

    public GameObject autoPlayBarrier;

    public Sprite[] ranks;

    private AudioSource audioSource;

    private InterstitialAd _trackFinishedAd;
    private bool _adClosed;

    // Start is called before the first frame update
    void Start() {
        G.InitTracks();

        G.PlaySettings.FromTrackResult = true;
        G.PlaySettings.FromTrackPlay = false;

        if (G.Items.Energy <= 0) {
            retry.interactable = false;
            energyDes.SetActive(true);
        }

        autoPlayBarrier.SetActive(G.PlaySettings.AutoPlay);
        if (G.PlaySettings.AutoPlay) {
            var barrierGroup = autoPlayBarrier.GetComponent<CanvasGroup>();
            StartCoroutine(Interpolators.Linear(0, 1, 0.3f, step => { barrierGroup.alpha = step; }, () => { }));
        }

        background.sprite =
            (Sprite) Resources.Load($"textures/tracks/normal/{G.Tracks[G.PlaySettings.TrackId].id}",
                typeof(Sprite));

        audioSource = GetComponent<AudioSource>();
        audioSource.clip =
            (AudioClip) Resources.Load($"tracks/preview/{G.Tracks[G.PlaySettings.TrackId].id}", typeof(AudioClip));
        audioSource.Play();

        if (Math.Abs(G.InGame.Accuracy - 100f) < 0.001f) {
            rank.sprite = ranks[6];
        } else if ((int) Math.Ceiling(G.InGame.TotalScore) == 1000000) {
            rank.sprite = ranks[5];
        } else if ((int) Math.Ceiling(G.InGame.TotalScore) >= 950000) {
            rank.sprite = ranks[4];
        } else if ((int) Math.Ceiling(G.InGame.TotalScore) >= 900000) {
            rank.sprite = ranks[3];
        } else if ((int) Math.Ceiling(G.InGame.TotalScore) >= 800000) {
            rank.sprite = ranks[2];
        } else if ((int) Math.Ceiling(G.InGame.TotalScore) >= 700000) {
            rank.sprite = ranks[1];
        } else {
            rank.sprite = ranks[0];
        }

        var isFC = "";
        if (G.InGame.MaxCombo == G.InGame.CountOfNotes) {
            isFC = "  FC";
        }

        maxCombo.text = $"0{isFC}";

        trackInfo.text =
            $"{G.Tracks[G.PlaySettings.TrackId].title}  <size=100>{G.Tracks[G.PlaySettings.TrackId].artist}</size>";
        difficulty.text = G.PlaySettings.Difficulty == 0 ? "[easy]" : "[hard]";

        uiAnimator.Play("result_intro", -1, 0);
        StartCoroutine(PrintResult());

        if (G.Items.Energy == 0 && G.Items.CoolDown == -1)
            G.Items.CoolDown = DateTime.Now.AddMinutes(G.InternalSettings.CooldownInMinute).ToBinary();
        
        LoadAd();
    }

    void Update() {
        if (_adClosed) {
            _adClosed = false;
            StartCoroutine(LeaveScene());
        }
    }

    private void LoadAd() {
        _trackFinishedAd?.Destroy();

        _trackFinishedAd = new InterstitialAd(G.AD.TestMode ? G.AD.TestId : G.AD.TrackFinishedId);
        _trackFinishedAd.OnAdClosed += HandleAdClosed;
        
        _trackFinishedAd.LoadAd(new AdRequest.Builder().Build());
    }

    private IEnumerator PrintResult() {
        yield return new WaitForSeconds(0.75f);

        StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, 0, G.InGame.TotalScore, 0.75f,
            step => { score.text = $"{(int) step:0000000}"; }, () => { }));

        var scoreSign = G.InGame.TotalScore >= G.InGame.BestScore ? "+" : "";
        StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, 0, G.InGame.TotalScore - G.InGame.BestScore,
            0.75f,
            step => { scoreDiff.text = $"{scoreSign}{(int) step}"; }, () => { }));

        yield return new WaitForSeconds(0.25f);

        StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, 0, G.InGame.Accuracy, 0.75f,
            step => { accuracy.text = $"{step:F2}%"; }, () => { }));

        var accuracySign = G.InGame.Accuracy >= G.InGame.BestAccuracy ? "+" : "";
        StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, 0, G.InGame.Accuracy - G.InGame.BestAccuracy,
            0.75f,
            step => { accuracyDiff.text = $"{accuracySign}{step:F2}%"; }, () => { }));

        yield return new WaitForSeconds(0.25f);

        StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, 0,
            G.InGame.CountOfPerfect + G.InGame.CountOfAccuracyPerfect, 0.75f,
            step => { judgeP.text = $"{(int) step}"; }, () => { }));

        StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, 0, G.InGame.CountOfGreat, 0.75f,
            step => { judgeG.text = $"{(int) step}"; }, () => { }));

        StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, 0, G.InGame.CountOfBad, 0.75f,
            step => { judgeB.text = $"{(int) step}"; }, () => { }));

        StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, 0, G.InGame.CountOfError, 0.75f,
            step => { judgeE.text = $"{(int) step}"; }, () => { }));

        yield return new WaitForSeconds(0.25f);

        var isFC = "";
        if (G.InGame.MaxCombo == G.InGame.CountOfNotes) {
            isFC = "  FC";
        }

        StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, 0, G.InGame.MaxCombo, 0.75f,
            step => { maxCombo.text = $"{(int) step}{isFC}"; }, () => { }));

        G.InGame.Reset();
    }

    private IEnumerator Outro(bool animateForeground, bool showAd) {
        uiAnimator.SetFloat("Speed", -2);
        uiAnimator.Play("result_intro", -1, 1);

        yield return new WaitForSeconds(1f);

        if (animateForeground) {
            StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, foreground.color.a, 0, 0.75f,
                step => { foreground.color = new Color(0, 0, 0, step); }, () => { }));
        }
        
        if (G.PlaySettings.AutoPlay) {
            var barrierGroup = autoPlayBarrier.GetComponent<CanvasGroup>();
            StartCoroutine(Interpolators.Linear(1, 0, 0.3f, step => { barrierGroup.alpha = step; }, () => { autoPlayBarrier.SetActive(false); }));
        }

        yield return StartCoroutine(Interpolators.Linear(1, 0, 1f,
            step => { audioSource.volume = step; }, () => { }
        ));

        if (showAd && _trackFinishedAd.IsLoaded()) {
            _trackFinishedAd.Show();
        }
    }

    private void HandleAdClosed(object sender, EventArgs args) {
        _adClosed = true;
    }

    private IEnumerator LeaveScene() {
        yield return new WaitForSeconds(0.75f);
        SceneManager.LoadScene("_scenes/TrackSelect");
    }
    
    public void Retry() {
        StartCoroutine(Outro(false, false));
    }

    public void Next() {
        StartCoroutine(Outro(true, true));
    }
}