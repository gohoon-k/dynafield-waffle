using System;
using System.Collections;
using UnityEngine;

public abstract class Note : MonoBehaviour {
    public ChartPlayer parent;

    public int id;

    public float time;
    public short xPos;
    public sbyte direction;
    public short size;
    public float sizeInWorld;

    public bool hasAnotherNote;

    public bool destroying;
    public bool hasInput;
    public bool judged;

    public GameObject destroyEffect;
    public GameObject judgeEffect;

    public virtual void Start() {
        transform.localRotation = Quaternion.Euler(0, 0, 0);
    }

    public virtual void Update() {
        if (G.InGame.Paused) return;

        NoteUpdate();
    }

    private bool IsActive() {
        return parent.GetActiveNotes().Contains(this);
    }

    protected bool IsHiddenByOtherNote(Vector3 input) {
        var activeNotes = parent.GetActiveNotes();
        var targetNotes = activeNotes.FindAll(note => note.IsTargeted(input) && !note.destroying && !note.hasInput);

        targetNotes.Sort((a, b) => a.time < b.time ? -1 : Math.Abs(a.time - b.time) < 0.000000001f ? 0 : 1);

        if (targetNotes.Count == 0) return false;

        return !targetNotes[0].Compare(this);
    }

    protected bool IsTargeted(Vector3 input) {
        var fieldPosition = parent.field.main.transform.position;
        return xPos / 100f + sizeInWorld > input.x && xPos / 100f - sizeInWorld < input.x &&
               input.y - fieldPosition.y >= -3 && input.y - fieldPosition.y <= 3;
    }

    protected virtual void Judge() {
        if (judged) return;

        var tf = transform;

        tf.parent = parent.GetNotesHolder().GetChild(1);

        StartCoroutine(GiveJudged());

        PlayDestroyAnim();

        StartCoroutine(GiveDestroyed());

        var result = TimeDifferenceToJudge(GetTimeDifference());

        if (result == 3)
            StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, tf.localPosition.y, 0, 0.05f,
                step => { tf.localPosition = new Vector3(tf.localPosition.x, step); },
                () => { }
            ));

        CreateJudgeEffect(result);

        CreateDestroyEffect(result);

        ApplyStatistics(result);

        parent.Pulse();
    }

    private void NoteUpdate() {
        CheckError();

        if (destroying) return;

        if (!IsActive()) return;

        if (G.PlaySettings.AutoPlay) return;

        foreach (var touch in Input.touches) {
            HandleInput(touch);
        }
    }

    protected virtual void CheckError() {
        if (!IsPending()) return;

        if (G.InGame.Time - time < 0.35f) return;

        StartCoroutine(GiveDestroyed());

        PlayErrorAnim();

        CreateJudgeEffect(4);

        CreateDestroyEffect(4);

        G.InGame.CountOfError++;
        G.InGame.Combo = 0;
    }

    protected void CreateDestroyEffect(int judgeTypeInt) {
        var destroyEffectObject = Instantiate(destroyEffect, parent.GetKeyBeamsHolder().transform, true);
        var destroyEffectScript = destroyEffectObject.GetComponent<DestroyEffect>();
        destroyEffectScript.Set(judgeTypeInt, sizeInWorld);
        destroyEffectObject.transform.localPosition = new Vector2(xPos / 100f, 0);
    }

    protected void CreateJudgeEffect(int judgeTypeInt) {
        var judgeEffectObject = Instantiate(judgeEffect, parent.GetJudgeEffectsHolder().transform, false);
        var judgeEffectScript = judgeEffectObject.GetComponent<JudgeEffect>();
        judgeEffectScript.Set(judgeTypeInt, direction, G.InGame.Time - time < 0f);
        judgeEffectObject.transform.localPosition = new Vector3(xPos / 100f, direction * 1.1f, 0);
    }

    protected Vector3 GetInputPosition(Touch touch) {
        return parent.field.mainCamera.ScreenToWorldPoint(
            new Vector3(touch.position.x, touch.position.y,
                -parent.field.mainCamera.transform.position.z - parent.field.main.transform.position.z)
        );
    }

    protected void ApplyStatistics(int result) {
        switch (result) {
            case 0:
                G.InGame.CountOfBad++;
                G.InGame.Combo = 0;
                break;
            case 1:
                G.InGame.CountOfGreat++;
                G.InGame.Combo++;
                break;
            case 2:
                G.InGame.CountOfPerfect++;
                G.InGame.Combo++;
                break;
            case 3:
                G.InGame.CountOfAccuracyPerfect++;
                G.InGame.Combo++;
                break;
        }
    }

    protected void ScaleAnim() {
        var beforeScale = transform.localScale;
        StartCoroutine(
            Interpolators.Curve(
                Interpolators.EaseOutCurve,
                beforeScale.x,
                beforeScale.x * 1.5f,
                0.25f,
                step => { transform.localScale = new Vector2(step, step); },
                () => { }
            )
        );
    }

    protected void AlphaAnim(SpriteRenderer target) {
        StartCoroutine(
            Interpolators.Linear(
                target.color.a, 0, 0.25f,
                step => { target.color = new Color(1f, 1f, 1f, step); },
                () => {
                    parent.RemoveNote(this);
                    Destroy(gameObject);
                }
            )
        );
    }

    protected IEnumerator GiveInput() {
        yield return new WaitForEndOfFrame();
        hasInput = true;
    }

    protected IEnumerator GiveJudged() {
        yield return new WaitForEndOfFrame();
        judged = true;
    }

    protected IEnumerator GiveDestroyed() {
        yield return new WaitForEndOfFrame();
        destroying = true;
    }

    public virtual void SetRenderer(SpriteRenderer noteRenderer) { }

    protected abstract void PlayErrorAnim();

    protected abstract void PlayDestroyAnim();

    protected abstract void HandleInput(Touch touch);

    protected abstract int TimeDifferenceToJudge(float diff);

    protected abstract float GetTimeDifference();

    protected abstract bool IsPending();

    private bool Compare(Note other) {
        return other.id == id || other.time - time < 0.00001f;
    }
}