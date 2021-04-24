using System.Collections;
using UnityEngine;

public class NoteSwipe : Note {

    private SpriteRenderer _renderer;

    private float _judgeTime = -1;
    private bool _pendingSwipe = false;

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

    protected override void PlayErrorAnim() {
        AlphaAnim(_renderer);
        // AlphaAnim(_arrowRenderer);
    }

    protected override void PlayDestroyAnim() {
        ScaleAnim();
        AlphaAnim(_renderer);
    }

    protected override void HandleInput(Touch touch) {
        if (touch.phase != TouchPhase.Began && touch.phase != TouchPhase.Moved) return;
        
        var inputPosition = GetInputPosition(touch);

        if (!IsTargeted(inputPosition)) return;
        if (IsHiddenByOtherNote(inputPosition)) return;

        if (touch.phase == TouchPhase.Began && !_pendingSwipe) {
            StartCoroutine(GivePendingSwipe());
            _judgeTime = G.InGame.Time;
        }

        if (touch.phase == TouchPhase.Moved && _judgeTime > 0) {
            StartCoroutine(GiveInput());
            Judge(_judgeTime);
        }
    }

    protected override int TimeDifferenceToJudge(float diff) {
        for (var i = 0; i < 3; i++)
            if (diff > G.InternalSettings.JudgeOfSwipe[i])
                return i;

        return 3;
    }

    private IEnumerator GivePendingSwipe() {
        yield return new WaitForEndOfFrame();
        _pendingSwipe = true;
    }
    
}
