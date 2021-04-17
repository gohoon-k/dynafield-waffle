using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperPlayEffect : MonoBehaviour {

    public Sprite perfect;
    public Sprite ac;

    private Animator _animator;
    
    private SpriteRenderer _mainRenderer;
    private SpriteRenderer _effectRenderer;

    private bool _played = false;
    
    void Start() {
        _animator = GetComponent<Animator>();
        
        _mainRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        _effectRenderer = transform.GetChild(1).GetComponent<SpriteRenderer>();
    }

    public void Play(int type) {
        if (_played) return;

        _played = true;

        var target = type switch {
            0 => perfect,
            1 => ac,
            _ => perfect
        };

        _mainRenderer.sprite = target;
        _effectRenderer.sprite = target;
        
        _animator.Play("super_play_effect", -1, 0);

    }


}
