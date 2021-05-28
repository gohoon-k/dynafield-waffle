using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HowToDialog : MonoBehaviour
{
    public List<GameObject> pages;
    public Text pageIndicator;
    public Button prev, next;
    
    [HideInInspector] public bool isOpen;
    [HideInInspector] public bool isActive;

    private GameObject _dialogs;
    
    private RectTransform _content;
    private CanvasGroup _group;

    private readonly List<RectTransform> _pageTransforms = new List<RectTransform>(); 
    private readonly List<CanvasGroup> _pageGroups = new List<CanvasGroup>(); 

    private int _pageSize;
    
    private bool _init;
    private int _page;
    private bool _animating;

    private Action _closeAction;

    public void Open(Action closeAction = null) {
        if (!_init) {
            _dialogs = transform.parent.gameObject;
            _content = transform.GetChild(0).GetComponent<RectTransform>();
            _group = GetComponent<CanvasGroup>();

            _pageSize = pages.Count;
            
            _pageTransforms.Clear();
            _pageGroups.Clear();
            
            pages.ForEach(page => {
                _pageTransforms.Add(page.GetComponent<RectTransform>());
                _pageGroups.Add(page.GetComponent<CanvasGroup>());
            });
            
            _init = true;
        }
        
        isActive = isOpen = true;
        
        prev.interactable = false;
        next.interactable = true;

        _closeAction = closeAction;

        _page = 0;
        pageIndicator.text = $"{_page + 1}/{_pageSize}";
        
        _group.alpha = 0;
        _content.localScale = new Vector3(1.5f, 1.5f, 1);

        gameObject.SetActive(true);
        pages[0].gameObject.SetActive(true);
        _pageGroups[0].alpha = 1;
        _pageTransforms[0].anchoredPosition = new Vector2(0, _pageTransforms[0].anchoredPosition.y);

        for (var i = 1; i < _pageSize; i++) {
            pages[i].gameObject.SetActive(false);
            _pageGroups[i].alpha = 0;
            _pageTransforms[i].anchoredPosition = new Vector2(300, _pageTransforms[0].anchoredPosition.y);
        }
        
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
            
            _closeAction?.Invoke();
            _closeAction = null;
        }));
    }

    public void NextPage() {
        if (_page >= _pageSize - 1) return;
        
        if (_animating) return;
        _animating = true;

        var beforePage = _page;
        _page++;

        if (_page >= _pageSize - 1) next.interactable = false;
        prev.interactable = true;

        pageIndicator.text = $"{_page + 1}/{_pageSize}";
        
        pages[_page].gameObject.SetActive(true);
        
        StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, 300, 0, 0.25f, step => {
            _pageTransforms[beforePage].anchoredPosition = new Vector2(step - 300, _pageTransforms[beforePage].anchoredPosition.y);
            _pageGroups[beforePage].alpha = step / 300f;
            
            _pageTransforms[_page].anchoredPosition = new Vector2(step, _pageTransforms[_page].anchoredPosition.y);
            _pageGroups[_page].alpha = (300 - step) / 300f;
        }, () => {
            _animating = false;
            pages[beforePage].gameObject.SetActive(false);
        }));

    }

    public void PreviousPage() {
        if (_page <= 0) return;
        
        if (_animating) return;
        _animating = true;

        var beforePage = _page;
        _page--;
        
        if (_page <= 0) prev.interactable = false;
        next.interactable = true;
        
        pageIndicator.text = $"{_page + 1}/{_pageSize}";
        
        pages[_page].gameObject.SetActive(true);

        StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, 300, 0, 0.25f, step => {
            _pageTransforms[beforePage].anchoredPosition = new Vector2(300 - step, _pageTransforms[beforePage].anchoredPosition.y);
            _pageGroups[beforePage].alpha = step / 300f;
            
            _pageTransforms[_page].anchoredPosition = new Vector2(-step, _pageTransforms[_page].anchoredPosition.y);
            _pageGroups[_page].alpha = (300 - step) / 300f;
        }, () => {
            _animating = false;
            pages[beforePage].gameObject.SetActive(false);
        }));
    }
    
}
