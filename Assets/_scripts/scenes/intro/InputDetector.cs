using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InputDetector : MonoBehaviour {

    public Animator backgroundEffect;
    public ScreenTranslator translator;

    private bool _hasInput = false;
    
    void Update() {

        if (Time.timeSinceLevelLoad <= 5 || _hasInput) return;

        if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }

        if (Input.GetMouseButtonDown(0) || Input.touches.Length > 0) {
            StartCoroutine(TranslateToSelect());
        }

    }

    private IEnumerator TranslateToSelect() {
        _hasInput = true;
        backgroundEffect.Play("intro_effect_clicked", -1, 0);
        yield return new WaitForSeconds(0.75f);
        StartCoroutine(translator.ShowTransition(() => { SceneManager.LoadScene("TrackSelect"); }));
    }
    
}
