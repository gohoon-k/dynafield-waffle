using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class IntroArea {
    public TextMesh trackInformation;
    public TextMesh playInformation;
    public TextMesh retryInformation;
}

[Serializable]
public class ScoreArea {
    public Text combo;
    public Text score;
}

[Serializable]
public class TrackInformationArea {
    public Text trackInformation;
    public Text difficulty;
}

[Serializable]
public class BackgroundArea {
    public Image background;
    public Image brightBackground;
    public Animator brightBackgroundAnimator;
}

[Serializable]
public class MenuArea {

    public Pause pause;
    public Retry retry;
    public Stop stop;

    [Serializable]
    public class Retry {
        public Image image;
        public Button button;
        public Text remainingEnergyDesc;
        public Text remainingEnergy;
    }

    [Serializable]
    public class Stop {
        public Image image;
        public Button button;
    }

    [Serializable]
    public class Pause {
        public Button button;
        public Text help;
    }
    
}

public class UserInterfaceUpdater : MonoBehaviour {
    public IntroArea introArea;
    
    public ScoreArea scoreArea;
    public TrackInformationArea trackInformationArea;
    public BackgroundArea backgroundArea;
    public MenuArea menuArea;

    private Animator _retryInfoAnimator;
    
    private int _animatedScore;
    private int _scoreAnimatedCount;

    void Start() {
        G.InitTracks();

        trackInformationArea.trackInformation.text =
            $"<size=60>{G.Tracks[G.PlaySettings.TrackId].artist}</size>   {G.Tracks[G.PlaySettings.TrackId].title}";
        trackInformationArea.difficulty.text = G.PlaySettings.Difficulty == 0 ? "easy" : "hard";

        backgroundArea.background.sprite =
            (Sprite) Resources.Load($"textures/tracks/normal/{G.Tracks[G.PlaySettings.TrackId].id}",
                typeof(Sprite));
        
        backgroundArea.brightBackground.sprite =
            (Sprite) Resources.Load($"textures/tracks/bright/{G.Tracks[G.PlaySettings.TrackId].id}",
                typeof(Sprite));

        _retryInfoAnimator = introArea.retryInformation.gameObject.GetComponent<Animator>();
    }

    void Update() {
        var intScore = (int) Math.Ceiling(G.InGame.TotalScore);
        if (_animatedScore != intScore) {
            _scoreAnimatedCount++;
            AnimateScore(_animatedScore, intScore, _scoreAnimatedCount);
            _animatedScore = intScore;
        }
        scoreArea.combo.text = $"{G.InGame.Combo}";

        menuArea.pause.help.color = new Color(1, 1, 1, G.InGame.PreparePause ? 1 : 0);
        
        if (menuArea.pause.button.interactable != G.InGame.CanBePaused)
            menuArea.pause.button.interactable = G.InGame.CanBePaused;
    }

    public void ShowPauseMenu(bool show) {
        if (!show) {
            menuArea.stop.button.interactable = false;
            menuArea.retry.button.interactable = false;
        }

        StartCoroutine(AnimateMenu(show, () => {
            if (show) {
                menuArea.stop.button.interactable = true;
                menuArea.retry.button.interactable = true;
            }
        }));
    }

    private void AnimateScore(int from, int to, int animatedCount) {
        StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, from, to, 0.2f,
                step => {
                    if (animatedCount != _scoreAnimatedCount) return;
                    scoreArea.score.text = $"{step:000000}";
                },
                () => { }
            )
        );
    }

    private IEnumerator AnimateMenu(bool show, Action finishAction) {
        if (show) menuArea.retry.remainingEnergy.text = $"{G.Items.Energy}";
        
        var from = show ? 0 : 0.568f;
        var to = show ? 0.568f : 0;
        while (from < to) {
            from += 0.02f;
            menuArea.stop.image.color = new Color(1, 1, 1, from);
            menuArea.retry.image.color = new Color(1, 1, 1, from);
            menuArea.retry.remainingEnergy.color = new Color(1, 1, 1, from);
            menuArea.retry.remainingEnergyDesc.color = new Color(1, 1, 1, from);
            yield return new WaitForEndOfFrame();
        }

        menuArea.stop.image.color = new Color(1, 1, 1, to);
        menuArea.retry.image.color = new Color(1, 1, 1, to);
        menuArea.retry.remainingEnergy.color = new Color(1, 1, 1, to);
        menuArea.retry.remainingEnergyDesc.color = new Color(1, 1, 1, to);

        finishAction();
    }

    public void Intro() {
        var playCounts = PlayerPrefs.GetInt(G.Keys.FormatKey(G.Keys.PlayTimes), 0);
        playCounts++;
        var unit = (playCounts % 10) switch {
            1 => "st",
            2 => "nd",
            3 => "rd",
            _ => "th"
        };
        
        introArea.trackInformation.text =
            $"{G.Tracks[G.PlaySettings.TrackId].title}\n<size=200>{G.Tracks[G.PlaySettings.TrackId].artist}</size>";
        introArea.playInformation.text =
            $"<size=150>play time:</size> {G.Tracks[G.PlaySettings.TrackId].length} <size=150>/ energy left:</size> {G.Items.Energy}\n{PlayerPrefs.GetInt(G.Keys.FormatKey(G.Keys.PlayTimes), 0)}<size=150>{unit}</size> play";
    }
    
    public void Retry() {
        introArea.retryInformation.text = $"<size=180>energy left:</size> {G.Items.Energy}";
                _retryInfoAnimator.Play("ingame_effect_retry_blink", -1, 0);
    }

    public void NotEnoughEnergy() {
        introArea.retryInformation.text = "<size=180>you don't have enough energy!</size>";
        _retryInfoAnimator.Play("ingame_effect_retry_blink", -1, 0);
    }
    
}