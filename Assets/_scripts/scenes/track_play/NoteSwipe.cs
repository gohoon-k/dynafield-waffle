using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteSwipe : Note {

    private SpriteRenderer _noteRenderer;

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
        // AlphaAnim(_arrowRenderer);
        // ArrowAnim();
    }

    // private void ArrowAnim() {
    //     var swipeDestination = swipeDirection switch {
    //         0 => new Vector3(0, 0.5f, 0),
    //         1 => new Vector3(0.5f, 0, 0),
    //         2 => new Vector3(0, -0.5f, 0),
    //         3 => new Vector3(-0.5f, 0, 0),
    //         _ => new Vector3(0, 0, 0)
    //     };
    //     StartCoroutine(Interpolators.Linear(_arrow.transform.position, swipeDestination, 0.2f,
    //             step => { _arrow.transform.position = step; }, 
    //             () => { }
    //         )
    //     );
    // }

    protected override void HandleInput(Touch touch) {
        if (touch.phase != TouchPhase.Moved) return;
        
        var inputPosition = GetInputPosition(touch);

        if (!IsTargeted(inputPosition.x)) return;
        if (IsHiddenByOtherNote(inputPosition.x)) return;

        if (!hasInput)
            StartCoroutine(GiveInput());
        
        Judge();
    }

    protected override int DifferenceToJudge(float diff) {
        for (var i = 0; i < 3; i++)
            if (diff > G.InternalSettings.JudgeOfSwipe[i])
                return i;

        return 3;
    }
    
}
