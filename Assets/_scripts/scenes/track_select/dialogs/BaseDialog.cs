using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseDialog : MonoBehaviour {

    public GameObject dialogs;

    public RectTransform content;
    public Image background;
    public Text message;
    public Text positive;
    public Text negative;

    public bool isOpen;

    public bool isActive;

    [HideInInspector]
    public float backgroundAlpha = 0.6f;

    public virtual void Open() {
        isActive = isOpen = true;
        
        background.color = new Color(0, 0, 0, 0);
        message.color = new Color(1, 1, 1, 0);
        if (!(positive is null)) {
            positive.color = new Color(1, 1, 1, 0);
        }

        if (!(negative is null)) {
            negative.color = new Color(1, 1, 1, 0);
        }
        content.localScale = new Vector3(1.5f, 1.5f, 1);
        
        gameObject.SetActive(true);
        
        StartCoroutine(Interpolators.Linear(0, 1, 0.4f, step => {
            background.color = new Color(0, 0, 0, step * backgroundAlpha);
            message.color = new Color(1, 1, 1, step);
            if (!(positive is null)) {
                positive.color = new Color(1, 1, 1, step);
            }

            if (!(negative is null)) {
                negative.color = new Color(1, 1, 1, step);
            }
            content.localScale = new Vector3(1.5f - step / 2f, 1.5f - step / 2f, 1);
        }, () => { }));
    }

    public virtual void Close(bool deactivateDialogs) {
        isActive = false;
        
        StartCoroutine(Interpolators.Linear(1, 0, 0.25f, step => {
            background.color = new Color(0, 0, 0, step * backgroundAlpha);
            message.color = new Color(1, 1, 1, step);
            if (!(positive is null)) {
                positive.color = new Color(1, 1, 1, step);
            }

            if (!(negative is null)) {
                negative.color = new Color(1, 1, 1, step);
            }
            content.localScale = new Vector3(1.5f - step / 2f, 1.5f - step / 2f, 1);
        }, () => {
            gameObject.SetActive(false);
            isOpen = false;
            if (deactivateDialogs)
                dialogs.SetActive(false);
        }));
    }
    
}
