using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteSwipe : Note {
    public sbyte ad;
    public int arrowRotation;
    
    private SpriteRenderer _renderer;
    private SpriteRenderer _arrowRenderer;

    private float _judgeTime = -1;

    new void Start() {
        base.Start();

        _arrowRenderer = transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
        
        transform.GetChild(0).localRotation = Quaternion.Euler(0, 0, arrowRotation);
        _arrowRenderer.gameObject.transform.localScale =
            new Vector3(_renderer.size.x / 7, _renderer.size.x / 7, 1);
    }

    new void Update() {
        if (G.PlaySettings.AutoPlay && G.InGame.Time >= time) {
            _judgeTime = G.InGame.Time;
            Judge();
        }

        base.Update();
    }

    public override void SetRenderer(SpriteRenderer noteRenderer) {
        base.SetRenderer(noteRenderer);
        _renderer = noteRenderer;
    }
    
    public override List<SpriteRenderer> GetRenderers() {
        return new List<SpriteRenderer> { _renderer };
    }

    protected override void PlayErrorAnim() {
        AlphaAnim(_renderer, false);
        AlphaAnim(_arrowRenderer);
        // AlphaAnim(_arrowRenderer);
    }

    protected override void PlayDestroyAnim() {
        CustomScaleAnim();
        ArrowAnim();
        AlphaAnim(_renderer, false);
        StartCoroutine(
            Interpolators.Linear(
                _arrowRenderer.color.a, 0, 0.15f,
                step => { _arrowRenderer.color = new Color(1f, 1f, 1f, step); },
                () => { }
            )
        );
    }

    private void ArrowAnim() {
        var arrowObject = _arrowRenderer.gameObject;
        var beforePos = arrowObject.transform.localPosition.y;
        var arrowTransform = arrowObject.transform;

        StartCoroutine(
            Interpolators.Curve(
                Interpolators.EaseOutCurve,
                beforePos, beforePos + 1.25f,
                0.25f,
                step => {
                    arrowTransform.localPosition = new Vector3(0, step, 0);
                }, () => {}
            )
        );
    }

    private void CustomScaleAnim() {
        var beforeScale = _renderer.size;
        StartCoroutine(
            Interpolators.Curve(
                Interpolators.EaseOutCurve,
                beforeScale, beforeScale * 1.5f,
                0.25f,
                step => {
                    _renderer.size = step;
                }, () => { }
            )
        );
    }

    protected override bool IsPending() {
        return !destroying;
    }

    protected override void HandleInput(Touch touch) {
        if (touch.phase != TouchPhase.Began && touch.phase != TouchPhase.Moved) return;

        var inputPosition = GetInputPosition(touch);

        if (!IsTargeted(inputPosition)) return;
        if (IsHiddenByOtherNote(inputPosition)) return;

        if (touch.phase == TouchPhase.Began && !hasInput) {
            StartCoroutine(GiveInput());
            _judgeTime = G.InGame.Time;
        }

        if (touch.phase == TouchPhase.Moved && _judgeTime > 0) {
            Judge();
        }
    }

    protected override float GetTimeDifference() {
        return Math.Abs(_judgeTime - time);
    }

    protected override int TimeDifferenceToJudge(float diff) {
        for (var i = 0; i < 3; i++)
            if (diff > G.InternalSettings.JudgeOfSwipe[i])
                return i;

        return 3;
    }
}