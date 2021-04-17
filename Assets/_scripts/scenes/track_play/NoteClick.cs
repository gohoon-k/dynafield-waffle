using System.Collections;
using UnityEngine;

public class NoteClick : Note {

    private SpriteRenderer _renderer;
    
    new void Start() {
        base.Start();

        _renderer = GetComponent<SpriteRenderer>();
    }

    new void Update() {
        if (G.PlaySettings.AutoPlay && G.InGame.Time >= time) {
            Judge();
        }
        
        base.Update();
    }

    protected override void PlayErrorAnim() {
        // animator.Play("click_error_anim", -1, 0);
        // StartCoroutine(DestroySelf());
        AlphaAnim(_renderer);
    }

    protected override void PlayDestroyAnim() {
        // animator.Play("click_destroy_anim", -1, 0);
        // StartCoroutine(DestroySelf());
        ScaleAnim();
        AlphaAnim(_renderer);
    }

    protected override void HandleInput(Touch touch) {
        if (touch.phase != TouchPhase.Began) return;

        var inputPosition = GetInputPosition(touch);

        if (!IsTargeted(inputPosition.x)) return;
        if (IsHiddenByOtherNote(inputPosition.x)) return;

        StartCoroutine(GiveInput());
        Judge();
    }

    protected override int DifferenceToJudge(float diff) {
        for (var i = 0; i < 3; i++)
            if (diff > G.InternalSettings.JudgeOfClick[i])
                return i;

        return 3;
    }
}