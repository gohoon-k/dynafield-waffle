using UnityEngine;

public class NoteSwipe : Note {

    private SpriteRenderer _noteRenderer;

    private float _judgeTime = -1;

    new void Start() {
        base.Start();

        _noteRenderer = GetComponent<SpriteRenderer>();
    }
    
    new void Update() {
        if (G.PlaySettings.AutoPlay && G.InGame.Time >= time) {
            Judge();
        }
        
        base.Update();
    }

    protected override void PlayErrorAnim() {
        AlphaAnim(_noteRenderer);
        // AlphaAnim(_arrowRenderer);
    }

    protected override void PlayDestroyAnim() {
        ScaleAnim();
        AlphaAnim(_noteRenderer);
    }

    protected override void HandleInput(Touch touch) {
        if (touch.phase != TouchPhase.Began && touch.phase != TouchPhase.Moved) return;
        
        var inputPosition = GetInputPosition(touch);

        if (!IsTargeted(inputPosition.x)) return;
        if (IsHiddenByOtherNote(inputPosition.x)) return;

        if (touch.phase == TouchPhase.Began && !hasInput) {
            StartCoroutine(GiveInput());
            _judgeTime = G.InGame.Time;
        }

        if (touch.phase == TouchPhase.Moved && _judgeTime > 0) {
            Judge(_judgeTime);
        }
    }

    protected override int DifferenceToJudge(float diff) {
        for (var i = 0; i < 3; i++)
            if (diff > G.InternalSettings.JudgeOfSwipe[i])
                return i;

        return 3;
    }
    
}
