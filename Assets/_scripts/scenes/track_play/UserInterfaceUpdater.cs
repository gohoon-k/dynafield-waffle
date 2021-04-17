using System;
using System.Collections;
using System.Net.Http.Headers;
using UnityEngine;
using UnityEngine.UI;

public class UserInterfaceUpdater : MonoBehaviour {
    public Text combo;
    public Text score;

    public Text trackInfo;
    public Text difficulty;

    public Image background;
    public Image backgroundBright;

    public Text preparePauseInfo;

    public Image stopButtonImage;
    public Image retryButtonImage;
    public Button pauseButton;
    public Button stopButton;
    public Button retryButton;
    public Text retryEnergyLeftDes;
    public Text retryEnergyLeft;

    public TextMesh retryInfo;
    public TextMesh introTrackInfo;
    public TextMesh introPlayInfo;

    private Animator _retryInfoAnimator; 

    void Start() {
        G.InitTracks();

        trackInfo.text =
            $"<size=60>{G.Tracks[G.PlaySettings.TrackId].artist}</size>   {G.Tracks[G.PlaySettings.TrackId].title}";
        difficulty.text = G.PlaySettings.Difficulty == 0 ? "easy" : "hard";

        background.sprite =
            (Sprite) Resources.Load($"textures/tracks/track_{G.Tracks[G.PlaySettings.TrackId].internal_name}",
                typeof(Sprite));
        
        backgroundBright.sprite =
            (Sprite) Resources.Load($"textures/tracks/track_{G.Tracks[G.PlaySettings.TrackId].internal_name}_bright",
                typeof(Sprite));

        _retryInfoAnimator = retryInfo.gameObject.GetComponent<Animator>();
    }

    void Update() {
        combo.text = $"{G.InGame.Combo}";
        score.text = $"{Math.Ceiling(G.InGame.TotalScore):000000}";

        preparePauseInfo.color = new Color(1, 1, 1, G.InGame.PreparePause ? 1 : 0);
        
        if (pauseButton.interactable != G.InGame.CanBePaused)
            pauseButton.interactable = G.InGame.CanBePaused;
    }

    public void ShowPauseMenu(bool show) {
        if (!show) {
            stopButton.interactable = false;
            retryButton.interactable = false;
        }

        StartCoroutine(AnimateMenu(show, () => {
            if (show) {
                stopButton.interactable = true;
                retryButton.interactable = true;
            }
        }));
    }

    private IEnumerator AnimateMenu(bool show, Action finishAction) {
        if (show) retryEnergyLeft.text = $"{G.Items.Energy}";
        
        var from = show ? 0 : 0.568f;
        var to = show ? 0.568f : 0;
        while (from < to) {
            from += 0.02f;
            stopButtonImage.color = new Color(1, 1, 1, from);
            retryButtonImage.color = new Color(1, 1, 1, from);
            retryEnergyLeft.color = new Color(1, 1, 1, from);
            retryEnergyLeftDes.color = new Color(1, 1, 1, from);
            yield return new WaitForEndOfFrame();
        }

        stopButtonImage.color = new Color(1, 1, 1, to);
        retryButtonImage.color = new Color(1, 1, 1, to);
        
        retryEnergyLeft.color = new Color(1, 1, 1, to);
        retryEnergyLeftDes.color = new Color(1, 1, 1, to);

        finishAction();
    }

    public void Intro() {
        int playCounts = PlayerPrefs.GetInt(G.Keys.FormatKey(G.Keys.PlayTimes), 1);
        string unit = (playCounts % 10) switch {
            1 => "st",
            2 => "nd",
            3 => "rd",
            _ => "th"
        };
        
        introTrackInfo.text =
            $"{G.Tracks[G.PlaySettings.TrackId].title}\n<size=200>{G.Tracks[G.PlaySettings.TrackId].artist}</size>";
        introPlayInfo.text =
            $"<size=150>play time:</size> {G.Tracks[G.PlaySettings.TrackId].length} <size=150>/ energy left:</size> {G.Items.Energy}\n{PlayerPrefs.GetInt(G.Keys.FormatKey(G.Keys.PlayTimes), 0) + 1}<size=150>{unit}</size> play";
    }
    
    public void Retry() {
        if (G.Items.Energy > 0)
            retryInfo.text = $"<size=180>energy left:</size> {G.Items.Energy - 1}";
        else
            retryInfo.text = "<size=180>you don't have enough energy!</size>";
        _retryInfoAnimator.Play("ingame_effect_retry_blink", -1, 0);
    }
    
}