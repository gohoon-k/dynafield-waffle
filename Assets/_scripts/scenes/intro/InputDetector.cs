using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InputDetector : MonoBehaviour {

    public ScreenTranslator translator;
    
    private void Update() {

        if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }

        if (Input.GetMouseButtonDown(0)) {
            TranslateToSelect();
        }

        if (Input.touches.Length > 0) {
            TranslateToSelect();
        }

    }

    private void TranslateToSelect() {
        StartCoroutine(translator.ShowTransition(() => { SceneManager.LoadScene("TrackSelect"); }));
    }
    
}
