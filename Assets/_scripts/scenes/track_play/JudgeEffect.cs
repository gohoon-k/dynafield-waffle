using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgeEffect : MonoBehaviour {
    public sbyte d;

    public Sprite[] judgeSprites;

    private bool _startFadeOut = false;
    private SpriteRenderer _mainRenderer;
    private TextMesh _earlyLateMesh;

    void Start() {
        if (d == -1)
            gameObject.transform.Rotate(0, 0, 180);

        StartCoroutine(WaitForFadeOut());
        
        StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, d * 1.1f, d * 1.5f, 0.4f,
            step => {
                var currentTransform = transform;
                var pos = currentTransform.localPosition;
                currentTransform.localPosition = new Vector2(pos.x, step);
            }, () => { })
        );
        StartCoroutine(Interpolators.Linear(0, 1, 0.25f,
                step => {
                    _mainRenderer.color = new Color(1, 1, 1, step);
                    _earlyLateMesh.color = new Color(1, 1, 1, step);
                },
                () => { }
            )
        );

    }

    void Update() {
        if (_startFadeOut) {
            _startFadeOut = false;
            StartCoroutine(Interpolators.Linear(1, 0, 0.25f,
                step => { _mainRenderer.color = new Color(1, 1, 1, step); },
                    () => { Destroy(gameObject); }
                )
            );
        }
    }

    private IEnumerator WaitForFadeOut() {
        yield return new WaitForSeconds(0.5f);
        _startFadeOut = true;
    }

    public void Set(int type, sbyte direction, bool early) {
        d = direction;
        _mainRenderer = GetComponent<SpriteRenderer>();
        _earlyLateMesh = transform.GetChild(0).GetComponent<TextMesh>();
        _mainRenderer.sprite = judgeSprites[type];
        if (type == 2 && early) {
            _earlyLateMesh.text = "early";
        }else if (type == 2 && !early) {
            _earlyLateMesh.text = "late";
        }
    }

}