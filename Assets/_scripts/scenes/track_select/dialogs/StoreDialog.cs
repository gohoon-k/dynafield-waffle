using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreDialog : MonoBehaviour {

    [HideInInspector] public bool isOpen;
    [HideInInspector] public bool isActive;

    public Text extendMaxEnergyDescription;
    public Button extendMaxEnergyButton;
    
    private GameObject _dialogs;
    
    private CanvasGroup _group;
    private RectTransform _content;

    private bool _init;

    public void Open() {
        if (!_init) {
            _dialogs ??= transform.parent.gameObject;
            _content ??= transform.GetChild(0).GetComponent<RectTransform>();
            _group ??= GetComponent<CanvasGroup>();

            _init = true;
        }
        
        isActive = isOpen = true;

        _group.alpha = 0;
        _content.localScale = new Vector3(1.5f, 1.5f, 1);

        if (G.Items.MaxEnergyStep + 1 < G.Items.MaxEnergy.Length)
            extendMaxEnergyDescription.text = $"extend max energy <size=60>to</size> {G.Items.MaxEnergy[G.Items.MaxEnergyStep + 1]}";
        else {
            extendMaxEnergyDescription.text = "max energy is fully extended.";
            extendMaxEnergyButton.interactable = false;
        }
        
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

}
