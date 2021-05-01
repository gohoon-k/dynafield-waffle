using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LockedBarrier : MonoBehaviour {
    
    public Image background;
    public Image locker;
    public Text description;
    public Button unlockButton;
    public Text unlockText;

    [HideInInspector]
    public bool showing;

    private RectTransform _rect;

    public void Show() {
        gameObject.SetActive(true);
        
        _rect ??= GetComponent<RectTransform>();

        background.color = new Color(0, 0, 0, 0);
        locker.color = description.color = unlockText.color = new Color(1, 1, 1, 0);
            
        unlockButton.interactable = true;

        showing = true;

        StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, 0, 1, 0.25f, step => {
            background.color = new Color(0, 0, 0, step * 0.7f);
            locker.color = description.color = unlockText.color = new Color(1, 1, 1, step * 0.8f);

            _rect.localScale = new Vector3(1.5f - step / 2f, 1.5f - step / 2f, 1);
        }, () => { }));
    }

    public void Hide() {
        unlockButton.interactable = false;

        StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, 1, 0, 0.05f, step => {
            background.color = new Color(0, 0, 0, step * 0.7f);
            locker.color = description.color = unlockText.color = new Color(1, 1, 1, step * 0.8f);
        }, () => {
            gameObject.SetActive(false);
            showing = false;
        }));
    }

    public IEnumerator Unlock() {
        yield return new WaitForSeconds(0.5f);
        
        unlockButton.interactable = false;

        StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, 1, 0, 0.2f, step => {
            background.color = new Color(0, 0, 0, step * 0.7f);
            locker.color = description.color = unlockText.color = new Color(1, 1, 1, step * 0.8f);
            
            _rect.localScale = new Vector3(1.5f - step / 2f, 1.5f - step / 2f, 1);
        }, () => {
            gameObject.SetActive(false);
            showing = false;
        }));
    }
    
}
