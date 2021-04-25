using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InputDetector : MonoBehaviour {
    public Animator backgroundEffect;
    public ScreenTranslator translator;

    public RectTransform trackSource;
    public Text trackSourceText;
    public Image trackSourceLinkUnderline;
    public Text trackSourceLinkText;
    public Button trackSourceLink;

    public AudioSource introAudioPlayer;

    private bool _hasInput;
    private bool _trackSourceShowing;

    private void Start() {
        StartCoroutine(PlayIntroAudio());
    }

    void Update() {
        if (Time.timeSinceLevelLoad <= 5 || _hasInput) return;

        if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }
    }

    private IEnumerator TranslateToSelect() {
        backgroundEffect.Play("intro_effect_clicked", -1, 0);
        yield return new WaitForSeconds(0.15f);
        StartCoroutine(Interpolators.Linear(1, 0, 0.65f, step => {
            introAudioPlayer.volume = step;
        }, () => { introAudioPlayer.Stop(); }));
        StartCoroutine(translator.ShowTransition(() => { SceneManager.LoadScene("TrackSelect"); }));
    }

    private IEnumerator PlayIntroAudio() {
        yield return new WaitForSeconds(0.75f);
        StartCoroutine(Interpolators.Linear(0, 1, 5f, step => {
            introAudioPlayer.volume = step;
        }, () => { }));
        introAudioPlayer.time = 20.2f;
        introAudioPlayer.Play();
    }

    public void ToggleTrackSource() {
        if (!_trackSourceShowing) {
            trackSource.anchoredPosition = new Vector2(-70, 170);
            trackSourceLink.interactable = true;
        }

        StartCoroutine(Interpolators.Linear(!_trackSourceShowing ? 0 : 1f, !_trackSourceShowing ? 1 : 0, 0.25f,
                step => {
                    trackSourceLinkText.color = new Color(1, 1, 1, step * 0.8f);
                    trackSourceText.color = new Color(1, 1, 1, step * 0.95f);
                    trackSourceLinkUnderline.color = new Color(1, 1, 1, step * 0.8f);
                },
                () => {
                    if (_trackSourceShowing) return;
                    
                    trackSourceLink.interactable = false;
                    trackSource.anchoredPosition = new Vector2(1070, 170);
                }
            )
        );

        _trackSourceShowing = !_trackSourceShowing;
    }

    public void ScreenTouched() {
        if (Time.timeSinceLevelLoad <= 5 || _hasInput) return;
        
        _hasInput = true;
        StartCoroutine(TranslateToSelect());
    }
    
    public void OpenSoundCloud() {
        Application.OpenURL("https://soundcloud.com/jake-francis-south/the-invasion");
    }
}