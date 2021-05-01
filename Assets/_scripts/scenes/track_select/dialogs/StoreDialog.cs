using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreDialog : MonoBehaviour {

    public GameObject dialogs;
    
    public RectTransform content;
    public Image background;
    public Text[] texts;
    public Image[] images;

    public bool isOpen;

    public bool isActive;

    public void Open() {
        isActive = isOpen = true;
        
        background.color = new Color(0, 0, 0, 0);
        foreach (var text in texts) {
            text.color = new Color(1, 1, 1, 0);
        }

        foreach (var image in images) {
            image.color = new Color(1, 1, 1, 0);
        }
        content.localScale = new Vector3(1.5f, 1.5f, 1);

        if (G.Items.MaxEnergyStep + 1 < G.Items.MaxEnergy.Length)
            texts[3].text = $"extend max energy <size=60>to</size> {G.Items.MaxEnergy[G.Items.MaxEnergyStep + 1]}";
        else {
            texts[3].text = "max energy is fully extended.";
            texts[4].gameObject.GetComponent<Button>().interactable = false;
        }
        
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

}
