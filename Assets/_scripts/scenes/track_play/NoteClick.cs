using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteClick : Note {

    private SpriteRenderer _renderer;
    
    new void Start() {
        base.Start();
    }

    new void Update() {
        if (G.PlaySettings.AutoPlay && G.InGame.Time >= time) {
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
        AlphaAnim(_renderer);
    }

    protected override void PlayDestroyAnim() {
        ScaleAnim();
        AlphaAnim(_renderer);
    }

    protected override bool IsPending() {
        return !destroying && !hasInput;
    }

    protected override float GetTimeDifference() {
        return Math.Abs(G.InGame.Time - time);
    }

    protected override void HandleInput(Touch touch) {
        if (touch.phase != TouchPhase.Began) return;

        var inputPosition = GetInputPosition(touch);

        if (!IsTargeted(inputPosition)) return;
        if (IsHiddenByOtherNote(inputPosition)) return;

        StartCoroutine(GiveInput());
        Judge();
    }

    protected override int TimeDifferenceToJudge(float diff) {
        for (var i = 0; i < 3; i++)
            if (diff > G.InternalSettings.JudgeOfClick[i])
                return i;

        return 3;
    }
}