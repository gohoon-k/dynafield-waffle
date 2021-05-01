using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CalibrationDialog : MonoBehaviour
{
    public GameObject dialogs;
    
    public RectTransform content;
    public Image background;
    public Text[] texts;
    public Image[] images;

    public Button calibrationDown;

    public bool isOpen;

    [HideInInspector] public bool isActive;

    public void Open() {
        isActive = isOpen = true;

        texts[1].text = $"{G.PlaySettings.DisplaySync}";
        
        background.color = new Color(0, 0, 0, 0);
        foreach (var text in texts) {
            text.color = new Color(1, 1, 1, 0);
        }

        foreach (var image in images) {
            image.color = new Color(1, 1, 1, 0);
        }
        content.localScale = new Vector3(1.5f, 1.5f, 1);

        gameObject.SetActive(true);
        
        StartCoroutine(Interpolators.Linear(0, 1, 0.4f, step => {
            background.color = new Color(0, 0, 0, step * 0.8f);
            foreach (var text in texts) {
                text.color = new Color(1, 1, 1, step);
            }

            foreach (var image in images) {
                image.color = new Color(1, 1, 1, step);
            }
            content.localScale = new Vector3(1.5f - step / 2f, 1.5f - step / 2f, 1);
        }, () => { }));
    }

    public void Close(bool deactivateDialogs) {
        isActive = false;
        
        StartCoroutine(Interpolators.Linear(1, 0, 0.25f, step => {
            background.color = new Color(0, 0, 0, step * 0.8f);
            foreach (var text in texts) {
                text.color = new Color(1, 1, 1, step);
            }

            foreach (var image in images) {
                image.color = new Color(1, 1, 1, step);
            }
            content.localScale = new Vector3(1.5f - step / 2f, 1.5f - step / 2f, 1);
        }, () => {
            gameObject.SetActive(false);
            isOpen = false;
            if (deactivateDialogs)
                dialogs.SetActive(false);
        }));
    }

    public void SetCalibrationUp() {
        G.PlaySettings.DisplaySync++;
        texts[1].text = $"{G.PlaySettings.DisplaySync}";
    }

    public void SetCalibrationDown() {
        if (G.PlaySettings.DisplaySync > -165)
            G.PlaySettings.DisplaySync--;

        if (G.PlaySettings.DisplaySync <= -165) {
            G.PlaySettings.DisplaySync = -165;
            calibrationDown.interactable = false;   
        }
        
        texts[1].text = $"{G.PlaySettings.DisplaySync}";
    }
    
}
