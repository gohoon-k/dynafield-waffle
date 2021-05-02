using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CalibrationDialog : MonoBehaviour {
    public Text currentCalibration;
    public Button calibrationDown;
    
    [HideInInspector] public bool isOpen;
    [HideInInspector] public bool isActive;
    
    private GameObject _dialogs;
    
    private RectTransform _content;
    private CanvasGroup _group;
    
    private bool _init;

    public void Open() {
        if (!_init) {
            _dialogs ??= transform.parent.gameObject;
            _content ??= transform.GetChild(0).GetComponent<RectTransform>();
            _group ??= GetComponent<CanvasGroup>();

            _init = true;
        }
        
        isActive = isOpen = true;

        currentCalibration.text = $"{G.PlaySettings.DisplaySync}";

        _group.alpha = 0;
        _content.localScale = new Vector3(1.5f, 1.5f, 1);

        gameObject.SetActive(true);
        
        StartCoroutine(Interpolators.Linear(0, 1, 0.4f, step => {
            _group.alpha = step;
            
            var localScale = 1.5f - step / 2f;
            _content.localScale = new Vector3(localScale, localScale, 1);
        }, () => { }));
    }

    public void Close(bool deactivateDialogs) {
        isActive = false;
        
        StartCoroutine(Interpolators.Linear(1, 0, 0.25f, step => {
            _group.alpha = step;
            
            var localScale = 1.5f - step / 2f;
            _content.localScale = new Vector3(localScale, localScale, 1);
        }, () => {
            gameObject.SetActive(false);
            isOpen = false;
            if (deactivateDialogs)
                _dialogs.SetActive(false);
        }));
    }

    public void SetCalibrationUp() {
        G.PlaySettings.DisplaySync++;
        currentCalibration.text = $"{G.PlaySettings.DisplaySync}";
    }

    public void SetCalibrationDown() {
        if (G.PlaySettings.DisplaySync > -165)
            G.PlaySettings.DisplaySync--;

        if (G.PlaySettings.DisplaySync <= -165) {
            G.PlaySettings.DisplaySync = -165;
            calibrationDown.interactable = false;   
        }
        
        currentCalibration.text = $"{G.PlaySettings.DisplaySync}";
    }
    
}
