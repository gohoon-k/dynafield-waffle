using System;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundBlur : MonoBehaviour {

    public int blurRadius = 15;
    
    public DialogManager manager;

    private Image _image;
    private bool _beforeDialogState;

    private void Update() {
        if (_beforeDialogState != manager.DialogShowing) {
            _beforeDialogState = manager.DialogShowing;

            if (manager.DialogShowing) {
                BlurBackground();
            } else {
                ReturnBackground();
            }
        }
    }

    private void BlurBackground() {
        _image ??= GetComponent<Image>();
        
        _image.color = new Color(1, 1, 1, 1);
        
        StartCoroutine(Interpolators.Linear(0, blurRadius, 0.4f, step => {
            _image.material.SetInteger(Shader.PropertyToID("_Radius"), (int) step);
        }, () => { }));
    }

    private void ReturnBackground() {
        _image ??= GetComponent<Image>();
        
        StartCoroutine(Interpolators.Linear(blurRadius, 0, 0.25f, step => {
            _image.material.SetInteger(Shader.PropertyToID("_Radius"), (int) step);
        }, () => {
            _image.color = new Color(1, 1, 1, 0);
        }));
    }
    
}