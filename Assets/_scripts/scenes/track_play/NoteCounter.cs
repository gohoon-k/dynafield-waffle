using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteCounter : Note {
    public GameObject effectGroup;

    public GameObject handlingEffect;
    public GameObject clickEffect;
    public GameObject clickEffectBeam;
    public GameObject countEffect;
    public SpriteRenderer timeoutEffect;

    public int count;
    public float timeout;

    private GameObject _handlingEffectInstantiated;

    public SpriteRenderer mainRenderer;
    public SpriteRenderer effectRenderer;

    private TextMesh _countText;
    private TextMesh[] _countEffects;
    public TextMesh countTextInNote;
    public MeshRenderer countTextInNoteRenderer;

    private bool _handling;
    private int _handledCount;
    private float _handledTime;

    private float _interval;

    new void Start() {
        base.Start();

        _countEffects = new TextMesh[count];

        _handlingEffectInstantiated = Instantiate(handlingEffect, effectGroup.transform, true);
        _countText = _handlingEffectInstantiated.GetComponent<TextMesh>();
        
        countTextInNote.text = $"> {count} <";
        countTextInNoteRenderer.sortingOrder = 200;
        
        countTextInNote.transform.parent.localRotation = Quaternion.Euler(0, 0, direction == 1 ? 0 : 180);

        for (var i = 0; i < count; i++) {
            _countEffects[i] = Instantiate(countEffect, _handlingEffectInstantiated.transform, true)
                .GetComponent<TextMesh>();
            _countEffects[i].color = new Color(1, 1, 1, 0);
        }
    }

    new void Update() {
        base.Update();

        if (G.InGame.Paused) return;

        if (G.PlaySettings.AutoPlay && G.InGame.Time >= time && !_handling) {
            StartCoroutine(GiveInput());
            StartCoroutine(GiveHandling());
            StartHandle();

            _interval = timeout / (count + 1);
        }

        if (G.PlaySettings.AutoPlay && _handling && _handledCount == count) EndHandle();

        if (G.PlaySettings.AutoPlay && _handling && G.InGame.Time >= time + _interval * (_handledCount + 1) && !judged)
            Handle(new Vector3(Random.Range(-9, 9), Random.Range(-3.5f, 3.5f), 0));

        if (_handling)
            Progress();

        if (G.InGame.Time >= time + timeout && !judged)
            EndHandle();
    }

    protected override void HandleInput(Touch touch) {
        if (judged) return;
        
        if (touch.phase != TouchPhase.Began) return;

        var inputPosition = GetInputPosition(touch);

        if (_handledCount == count) EndHandle();

        if (_handling) Handle(inputPosition);

        if (_handling) return;

        if (!IsTargeted(inputPosition)) return;
        if (IsHiddenByOtherNote(inputPosition)) return;
        if (G.InGame.Time - time > 0.1f) return;

        StartCoroutine(GiveInput());

        StartCoroutine(GiveHandling());

        StartHandle();
    }

    private void StartHandle() {
        StartCoroutine(Interpolators.Linear(1, 0, 0.25f,
            step => { countTextInNote.color = new Color(1, 1, 1, step); }, () => { }
        ));

        var before = effectRenderer.size;
        StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, before, before * 1.75f, 0.35f,
                step => { effectRenderer.size = step; },
                () => { }
            )
        );
        StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, 1f, 0, 0.35f,
                step => { effectRenderer.color = new Color(1, 1, 1, step); },
                () => { }
            )
        );

        _countText.text = $"{count}";

        StartCoroutine(Interpolators.Linear(0, 0.45f, 0.1f,
                step => { _countText.color = new Color(1, 1, 1, step); },
                () => { }
            )
        );
        StartCoroutine(Interpolators.Linear(0, 1, 0.25f,
                step => { _handlingEffectInstantiated.transform.localScale = new Vector3(step, step, 1); },
                () => { }
            )
        );
        StartCoroutine(Interpolators.Linear(1, 1.15f, timeout - 0.25f,
                step => {
                    if (_handledCount == count) return;
                    _handlingEffectInstantiated.transform.localScale = new Vector3(step, step, 1);
                },
                () => { }
            )
        );
    }

    private void Handle(Vector3 pos) {
        var clickEffectInstantiated = Instantiate(clickEffect, effectGroup.transform, true);
        clickEffectInstantiated.transform.position = pos;

        var clickEffectBeamInstantiated = Instantiate(clickEffectBeam, effectGroup.transform, true);
        clickEffectBeamInstantiated.transform.position = pos;

        var currentHandledCount = _handledCount;
        _handledCount++;
        
        if (count - _handledCount >= 0)
            _countText.text = $"{count - _handledCount}";

        if (currentHandledCount >= _countEffects.Length) return;
        
        _countEffects[currentHandledCount].text = $"{count - currentHandledCount - 1}";
        StartCoroutine(Interpolators.Linear(1, 1.75f, 0.15f,
                step => {
                    _countEffects[currentHandledCount].gameObject.transform.localScale = new Vector3(step, step, 1);
                },
                () => { }
            )
        );
        StartCoroutine(Interpolators.Linear(0.2f, 0, 0.15f,
                step => { _countEffects[currentHandledCount].color = new Color(1, 1, 1, step); },
                () => { }
            )
        );
    }

    private void EndHandle() {
        Judge();

        StartCoroutine(Interpolators.Linear(_handlingEffectInstantiated.transform.localScale.x, 1.75f, 0.25f,
                step => { _handlingEffectInstantiated.transform.localScale = new Vector3(step, step, 1); },
                () => { }
            )
        );
        StartCoroutine(Interpolators.Linear(0.45f, 0, 0.25f,
                step => { _countText.color = new Color(1, 1, 1, step); },
                () => { Destroy(_handlingEffectInstantiated); }
            )
        );
    }

    private void Progress() {
        _handledTime += Time.deltaTime;
        var ratio = _handledTime / timeout;

        timeoutEffect.size = new Vector2(size / 10f * ratio, 1);
    }

    private IEnumerator GiveHandling() {
        yield return new WaitForEndOfFrame();
        _handling = true;
    }

    protected override float GetTimeDifference() {
        return (float) _handledCount / count;
    }

    protected override bool IsPending() {
        return !destroying && !hasInput;
    }

    protected override void PlayErrorAnim() {
        AlphaAnim(mainRenderer);
    }

    protected override void PlayDestroyAnim() {
        ScaleAnim();
        AlphaAnim(mainRenderer, false);
        AlphaAnim(timeoutEffect);
    }
    
    public override List<SpriteRenderer> GetRenderers() {
        return new List<SpriteRenderer> { mainRenderer };
    }

    protected override int TimeDifferenceToJudge(float diff) {
        for (var i = 0; i < 2; i++)
            if (diff < G.InternalSettings.JudgeOfCounter[i])
                return i;

        return 3;
    }
}