using UnityEngine;

public class NoteHold : Note {
    public SpriteRenderer startRenderer;
    public SpriteRenderer holdRenderer;
    public SpriteRenderer progressRenderer;
    public SpriteRenderer endRenderer;

    public float duration;

    private float _handledTime;

    private int _fingerId = -1;
    private bool _handling;

    private float _startTime = -1f;
    private float _endTime = -1f;

    private int _animationType;

    private bool _isAutoHandled;

    new void Start() {
        base.Start();
    }

    new void Update() {
        if (G.InGame.Paused) return;
        
        if (G.PlaySettings.AutoPlay && G.InGame.Time >= time && !_isAutoHandled) {
            StartCoroutine(GiveInput());
            _handling = true;
            _startTime = G.InGame.Time;
            _isAutoHandled = true;
        }

        if (G.PlaySettings.AutoPlay && G.InGame.Time >= time) {
            Progress();
        }

        if (G.PlaySettings.AutoPlay && G.InGame.Time >= time + duration) {
            _endTime = G.InGame.Time;
            Judge();
        }
        
        base.Update();

        ResizeHold();
        
        if (G.InGame.Time >= time + duration) {
            if (_animationType == 1) {
                PlayDestroyAnim();
            } else if (_animationType == -1 || _animationType == 0) {
                PlayRemoveAnim();
            }
            _animationType = 100;
        }
    }

    private void DeactivateAnim(SpriteRenderer target) {
        StartCoroutine(
            Interpolators.Linear(
                target.color.a, 0.5f, 0.25f,
                step => { target.color = new Color(1f, 1f, 1f, step); },
                () => { }
            )
        );
    }

    protected override void PlayErrorAnim() {
        DeactivateAnim(startRenderer);
        DeactivateAnim(holdRenderer);
        DeactivateAnim(endRenderer);
    }

    protected override void PlayDestroyAnim() {
        ScaleAnim();
        AlphaAnim(startRenderer);
        AlphaAnim(holdRenderer);
        AlphaAnim(endRenderer);
    }

    private void PlayRemoveAnim() {
        AlphaAnim(startRenderer);
        AlphaAnim(holdRenderer);
        AlphaAnim(endRenderer);
    }

    private void ResizeHold() {
        if (G.InGame.Time - time < 0) return;

        var endTransform = transform.GetChild(3);
        var beforeSize = holdRenderer.size;
        var beforeProgressSize = progressRenderer.size;
        if (beforeSize.y <= 0) {
            endTransform.localPosition = new Vector3(0, 0, 0);
            holdRenderer.size = new Vector2(beforeSize.x, 0);
            progressRenderer.size = new Vector2(beforeProgressSize.x, 0);
            return;
        }

        endTransform.localPosition = new Vector3(0,
            endTransform.localPosition.y - parent.GetCurrentSpeed() * G.PlaySettings.Speed * parent.constants.placingPrecision * Time.deltaTime, 0);
        
        holdRenderer.size = new Vector2(beforeSize.x,
            beforeSize.y - 2 * parent.GetCurrentSpeed() * G.PlaySettings.Speed * parent.constants.placingPrecision * Time.deltaTime);
        
        progressRenderer.size = new Vector2(
            beforeProgressSize.x,
            beforeProgressSize.y - 2 * parent.GetCurrentSpeed() * G.PlaySettings.Speed * parent.constants.placingPrecision * Time.deltaTime
        );
    }

    private void Progress() {
        var before = progressRenderer.size;
        progressRenderer.size = new Vector2(size * 0.7f / 10f * (_handledTime / duration), before.y);
        _handledTime += Time.deltaTime;
    }

    protected override void HandleInput(Touch touch) {
        if (judged) return;

        var inputPosition = GetInputPosition(touch);

        if (_handling && touch.fingerId == _fingerId && !IsTargeted(inputPosition.x)) {
            _handling = false;
            _fingerId = -1;

            Judge();
        }

        if (!IsTargeted(inputPosition.x)) return;
        if (IsHiddenByOtherNote(inputPosition.x)) return;

        if (touch.phase == TouchPhase.Began) {
            if (G.InGame.Time - time < -0.35f) {
                _startTime = 0;
                _endTime = 0;
                Judge();
                return;
            }

            _handling = true;
            _fingerId = touch.fingerId;
            _startTime = G.InGame.Time;

            StartCoroutine(GiveInput());
        } else if (touch.fingerId == _fingerId && touch.phase == TouchPhase.Ended) {
            _handling = false;
            _fingerId = -1;
            _endTime = G.InGame.Time;

            Judge();
        } else if (touch.fingerId == _fingerId &&
                   (touch.phase == TouchPhase.Stationary || touch.phase == TouchPhase.Moved)) {
            
            Progress();
        }

        if (_handling && G.InGame.Time >= time + duration) {
            _endTime = G.InGame.Time;
            Judge();
        }
    }

    protected override void Judge(float judgeTime = -1) {
        if (judged) return;

        StartCoroutine(GiveJudged());

        // It's not error, but animation name is same with error animation.
        PlayErrorAnim();

        var result = TimeDifferenceToJudge((_endTime - _startTime) / duration);

        if (result >= 2) _animationType = 1;
        else _animationType = -1;

        CreateJudgeEffect(result);

        CreateDestroyEffect(result);

        ApplyStatistics(result);
    }

    protected override void CheckError() {
        if (destroying || _handling || _startTime + 1 > 0.001) return;
        
        if (G.InGame.Time - time < 0.35f) return;

        StartCoroutine(GiveDestroyed());
        
        PlayErrorAnim();
        
        CreateJudgeEffect(4);
        
        CreateDestroyEffect(4);
        
        G.InGame.CountOfError++;
        G.InGame.Combo = 0;
    }

    protected override int TimeDifferenceToJudge(float diff) {
        for (var i = 0; i < 3; i++)
            if (diff < G.InternalSettings.JudgeOfHold[i])
                return i;

        return 3;
    }
}