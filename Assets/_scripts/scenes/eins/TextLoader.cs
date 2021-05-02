using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TextLoader : MonoBehaviour {
    
    public Text text;
    private Animator _textAnimator;

    void Start() {
        Application.targetFrameRate = 60;
        
        LoadingTexts texts = JsonUtility.FromJson<LoadingTexts>(
            ((TextAsset) Resources.Load("texts/eins", typeof(TextAsset))).ToString()
        );
        text.text = texts.texts[Random.Range(0, texts.texts.Length)];

        _textAnimator = text.GetComponent<Animator>();

        StartCoroutine(Animate());
    }

    private IEnumerator Animate() {
        yield return new WaitForSeconds(2f);
        _textAnimator.Play("loading_text");
        text.color = new Color(text.color.r, text.color.g, text.color.b, 1f);
        yield return new WaitForSeconds(5f);
        text.color = new Color(text.color.r, text.color.g, text.color.b, 0f);
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("Intro");
    }

    [Serializable]
    class LoadingTexts {
        public string[] texts;
    }
}