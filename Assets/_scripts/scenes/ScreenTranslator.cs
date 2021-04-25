using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenTranslator : MonoBehaviour {

    public GameObject transition;

    public float translateInSpeed = 1f;
    public float translateOutSpeed = 1f;

    private Image _transitionImage;
    private Animator _transitionAnimator;
    
    void Start() {
        _transitionImage = transition.GetComponent<Image>();
        _transitionAnimator = transition.GetComponent<Animator>();

        _transitionImage.color =
            new Color(_transitionImage.color.r, _transitionImage.color.g, _transitionImage.color.b, 1f);
        _transitionAnimator.speed = translateInSpeed;
        _transitionAnimator.Play("ui_fade_out");

        StartCoroutine(HideTransition());
    }

    private IEnumerator HideTransition() {
        yield return new WaitForSeconds(0.75f * (1 / translateInSpeed));
        _transitionImage.rectTransform.anchoredPosition = new Vector2(0f, -1440f);
    }

    public IEnumerator ShowTransition(Action finishAction) {
        _transitionImage.rectTransform.anchoredPosition = new Vector2(0, 0);
        _transitionAnimator.speed = translateOutSpeed;
        _transitionAnimator.Play("ui_fade_in");
        yield return new WaitForSeconds(0.75f);
        finishAction();
    }
    
}
