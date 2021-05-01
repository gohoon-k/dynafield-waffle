using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HowToDialog : MonoBehaviour
{
    public GameObject dialogs;
    
    public RectTransform content;
    public Image background;

    public List<Text> otherTexts;
    
    public List<Text> textsIn00;
    public List<Text> textsIn01;
    public List<Text> textsIn02;
    public List<Text> textsIn03;
    public List<Text> textsIn04;
    public List<Text> textsIn05;
    public List<Text> textsIn06;
    public List<Text> textsIn07;

    public List<Image> imagesIn00;
    public List<Image> imagesIn01;
    public List<Image> imagesIn02;
    public List<Image> imagesIn03;
    public List<Image> imagesIn04;
    public List<Image> imagesIn05;
    public List<Image> imagesIn06;
    public List<Image> imagesIn07;
    
    private readonly List<List<Text>> _texts = new List<List<Text>>();
    private readonly List<List<Image>> _images = new List<List<Image>>();
    public List<RectTransform> tips;

    public Text pageIndicator;
    
    public Button prev, next;
    
    private bool _init;
    
    public int currentPage;
    public bool isOpen;

    public bool isActive;

    private bool _animating;

    public int tipSize = 8;

    private Action _closeAction;

    public void Open(Action closeAction = null) {
        if (!_init) {
            _texts.Add(textsIn00);
            _texts.Add(textsIn06);
            _texts.Add(textsIn07);
            _texts.Add(textsIn01);
            _texts.Add(textsIn02);
            _texts.Add(textsIn03);
            _texts.Add(textsIn04);
            _texts.Add(textsIn05);
            
            _images.Add(imagesIn00);
            _images.Add(imagesIn06);
            _images.Add(imagesIn07);
            _images.Add(imagesIn01);
            _images.Add(imagesIn02);
            _images.Add(imagesIn03);
            _images.Add(imagesIn04);
            _images.Add(imagesIn05);
            
            _init = true;
        }

        _closeAction = closeAction;

        isActive = isOpen = true;

        currentPage = 0;
        pageIndicator.text = $"{currentPage + 1}/{tipSize}";
        
        prev.interactable = false;
        next.interactable = true;

        background.color = new Color(0, 0, 0, 0);

        foreach (var text in otherTexts) {
            text.color = new Color(1, 1, 1, 0);
        }
        
        foreach (var text in _texts.SelectMany(texts => texts)) {
            text.color = new Color(1, 1, 1, 0);
        }

        foreach (var image in _images.SelectMany(images => images)) {
            image.color = new Color(1, 1, 1, 0);
        }
        content.localScale = new Vector3(1.5f, 1.5f, 1);

        gameObject.SetActive(true);
        tips[0].gameObject.SetActive(true);
        tips[0].anchoredPosition = new Vector2(0, tips[0].anchoredPosition.y);
        
        StartCoroutine(Interpolators.Linear(0, 1, 0.4f, step => {
            background.color = new Color(0, 0, 0, step * 0.8f);
            
            foreach (var text in otherTexts) {
                text.color = new Color(1, 1, 1, step);
            }

            foreach (var text in _texts[0]) {
                text.color = new Color(1, 1, 1, step);
            }

            foreach (var image in _images[0]) {
                image.color = new Color(1, 1, 1, step);
            }
            content.localScale = new Vector3(1.5f - step / 2f, 1.5f - step / 2f, 1);
        }, () => { }));
    }

    public void Close(bool deactivateDialogs) {
        isActive = false;
        
        StartCoroutine(Interpolators.Linear(1, 0, 0.25f, step => {
            background.color = new Color(0, 0, 0, step * 0.8f);
            
            foreach (var text in otherTexts) {
                text.color = new Color(1, 1, 1, step);
            }
            
            foreach (var text in _texts[currentPage]) {
                text.color = new Color(1, 1, 1, step);
            }

            foreach (var image in _images[currentPage]) {
                image.color = new Color(1, 1, 1, step);
            }
            content.localScale = new Vector3(1.5f - step / 2f, 1.5f - step / 2f, 1);
        }, () => {
            gameObject.SetActive(false);
            isOpen = false;
            if (deactivateDialogs)
                dialogs.SetActive(false);
            
            _closeAction?.Invoke();
            _closeAction = null;
        }));
    }

    public void NextTip() {
        if (currentPage >= tipSize - 1) return;
        
        if (_animating) return;
        _animating = true;

        var beforePage = currentPage;
        currentPage++;

        if (currentPage >= tipSize - 1) next.interactable = false;
        prev.interactable = true;

        pageIndicator.text = $"{currentPage + 1}/{tipSize}";
        
        tips[currentPage].gameObject.SetActive(true);
        
        StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, 300, 0, 0.25f, step => {
            tips[beforePage].anchoredPosition = new Vector2(step - 300, tips[beforePage].anchoredPosition.y);
            _texts[beforePage].ForEach(text => text.color = new Color(1, 1, 1, step / 300f));
            _images[beforePage].ForEach(text => text.color = new Color(1, 1, 1, step / 300f));
            tips[currentPage].anchoredPosition = new Vector2(step, tips[currentPage].anchoredPosition.y);
            _texts[currentPage].ForEach(text => text.color = new Color(1, 1, 1, (300 - step) / 300f));
            _images[currentPage].ForEach(text => text.color = new Color(1, 1, 1, (300 - step) / 300f));
        }, () => {
            _animating = false;
            tips[beforePage].gameObject.SetActive(false);
        }));

    }

    public void PreviousTip() {
        if (currentPage <= 0) return;
        
        if (_animating) return;
        _animating = true;

        var beforePage = currentPage;
        currentPage--;
        
        if (currentPage <= 0) prev.interactable = false;
        next.interactable = true;
        
        pageIndicator.text = $"{currentPage + 1}/{tipSize}";
        
        tips[currentPage].gameObject.SetActive(true);

        StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, 300, 0, 0.25f, step => {
            tips[beforePage].anchoredPosition = new Vector2(300 - step, tips[beforePage].anchoredPosition.y);
            _texts[beforePage].ForEach(text => text.color = new Color(1, 1, 1, step / 300f));
            _images[beforePage].ForEach(text => text.color = new Color(1, 1, 1, step / 300f));
            tips[currentPage].anchoredPosition = new Vector2(-step, tips[currentPage].anchoredPosition.y);
            _texts[currentPage].ForEach(text => text.color = new Color(1, 1, 1, (300 - step) / 300f));
            _images[currentPage].ForEach(text => text.color = new Color(1, 1, 1, (300 - step) / 300f));
        }, () => {
            _animating = false;
            tips[beforePage].gameObject.SetActive(false);
        }));
    }
    
}
