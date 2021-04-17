using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class Field {
    public Camera mainCamera;
    public GameObject main;
}

[Serializable]
public class NotePrefabs {
    public GameObject click;
    public GameObject slide;
    public GameObject hold;
}

[Serializable]
public class Scripts {
    public ScoreCalculator scoreCalculator;
    public UserInterfaceUpdater userInterfaceUpdater;
    public SuperPlayEffect superPlayEffect;
}

[Serializable]
public class Animators {
    public Animator foreground;
    public Animator ready;
    public Animator intro;
    public Animator ui;
}

[Serializable]
public class Constants {
    public float placingPrecision;
}

public class ChartPlayer : MonoBehaviour {
    public Field field;
    public Constants constants;

    public NotePrefabs notePrefabs;

    public Animators animators;
    public Scripts scripts;
    
    private GameObject _notesHolder;
    private Transform _keyBeamsHolder;
    private Transform _judgeEffectsHolder;
    private GameObject _judgeLine;

    private readonly List<Note> _notes = new List<Note>();
    private Chart _chart;

    private readonly int[] _loadableNoteCountsPerSpeed = {200, 185, 170, 155, 140, 125, 110, 100};
    private int _loadableNoteCount;

    private float[] _positions;
    private float[] _holdHeights;
    private bool[] _sHitData;

    private int _lastPlacedNoteId;
    private float _currentSpeed = 1.0f;

    private bool _trackStarted;
    private bool _syncFinished;
    private bool _gameFinished;
    private bool _outroPlayed;

    private int _currentSpeedPos;
    private int _currentMoveXPos;
    private int _currentMoveYPos;
    private int _currentMoveZPos;

    private AudioSource _audio;

    private readonly AnimationCurve[] _curves = {
        Interpolators.LinearCurve,
        Interpolators.EaseInCurve,
        Interpolators.EaseOutCurve,
        Interpolators.EaseInOutCurve
    };

    // Start is called before the first frame update
    void Start() {
        G.InitTracks();

        G.InGame.CanBePaused = false;

        _notesHolder = field.main.transform.GetChild(1).gameObject;
        _judgeLine = field.main.transform.GetChild(0).gameObject;
        _keyBeamsHolder = field.main.transform.GetChild(2);
        _judgeEffectsHolder = field.main.transform.GetChild(3);

        var chartJson = (TextAsset) Resources.Load(
            $"data/charts/{G.Tracks[G.PlaySettings.TrackId].internal_name}.{G.PlaySettings.Difficulty}", typeof(TextAsset)
        );

        _chart = JsonUtility.FromJson<Chart>(chartJson.ToString());

        G.InGame.CountOfNotes = _chart.chart.Length;
        G.InGame.CanBePaused = false;

        _loadableNoteCount = _loadableNoteCountsPerSpeed[G.PlaySettings.DisplaySpeed];

        _positions = new float[G.InGame.CountOfNotes];
        _holdHeights = new float[G.InGame.CountOfNotes];
        _sHitData = new bool[G.InGame.CountOfNotes];

        Calculate();

        for (var i = 0; i < Math.Min(_loadableNoteCount, _chart.chart.Length); i++) {
            CreateNote(_lastPlacedNoteId);
            _lastPlacedNoteId++;
        }

        _audio = GetComponent<AudioSource>();
        _audio.clip = (AudioClip) Resources.Load($"tracks/{G.Tracks[G.PlaySettings.TrackId].internal_name}", typeof(AudioClip));

        Time.timeScale = 1.0f;

        animators.foreground.Play("ingame_foreground_fade_in", -1, 0);
        StartCoroutine(Intro());
    }

    // Update is called once per frame
    void Update() {
        if (G.InGame.ReadyAnimated && !G.InGame.Paused) {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                Pause();
            }

            if (!_trackStarted) {
                StartCoroutine(StartTrack());
                _trackStarted = true;
            }

            if (!_syncFinished) {
                StartCoroutine(AdjustSync());
            } else {
                G.InGame.CanBePaused = true;

                if (_chart.speed != null &&
                    _currentSpeedPos < _chart.speed.Length &&
                    _chart.speed[_currentSpeedPos].t <= G.InGame.Time) {
                    _currentSpeed = _chart.speed[_currentSpeedPos].s;
                    _currentSpeedPos++;
                }

                // Notify line move code here

                MoveY();
                MoveX();
                MoveZ();

                MoveNotesToZero();

                MoveNotes();

                if (_lastPlacedNoteId < _chart.chart.Length && GetNotes().Count <= _loadableNoteCount) {
                    CreateNote(_lastPlacedNoteId);
                    _lastPlacedNoteId++;
                }

                G.InGame.Time += Time.deltaTime;
            }
        }

        if (G.InGame.CanBePaused && GetNotes().FindAll(note => !note.judged).Count == 0) {
            G.InGame.CanBePaused = false;
            _gameFinished = true;
        }

        if (_gameFinished && !_outroPlayed) {
            _outroPlayed = true;

            if (!G.PlaySettings.AutoPlay) {
                if ((int) Math.Ceiling(G.InGame.TotalScore) == 1000000) {
                    scripts.superPlayEffect.Play(0);
                } else if (G.InGame.Combo == G.InGame.CountOfNotes) {
                    scripts.superPlayEffect.Play(1);
                }
            }

            StartCoroutine(Outro());
        }
    }

    private void MoveNotes() {
        var positive = _notesHolder.transform.GetChild(0);
        var positiveBefore = positive.localPosition;
        positive.localPosition =
            new Vector3(positiveBefore.x,
                positiveBefore.y - _currentSpeed * G.PlaySettings.Speed * constants.placingPrecision * Time.deltaTime,
                positiveBefore.z);

        var negative = _notesHolder.transform.GetChild(2);
        var negativeBefore = negative.localPosition;
        negative.localPosition =
            new Vector3(negativeBefore.x,
                negativeBefore.y + _currentSpeed * G.PlaySettings.Speed * constants.placingPrecision * Time.deltaTime,
                negativeBefore.z);
    }

    private void CreateNote(int id) {
        var targetNotePrefab = _chart.chart[id].ty switch {
            0 => notePrefabs.click,
            1 => notePrefabs.slide,
            2 => notePrefabs.hold,
            _ => notePrefabs.click
        };

        var note = Instantiate(
            targetNotePrefab,
            _notesHolder.transform.GetChild(_chart.chart[id].d == 1 ? 0 : 2),
            true
        );
        var script = note.GetComponent<Note>();

        script.parent = this;

        script.id = id;
        script.time = _chart.chart[id].t;
        script.direction = _chart.chart[id].d;
        script.size = _chart.chart[id].s;
        script.sizeInWorld = _chart.chart[id].s / 20f;
        script.xPos = _chart.chart[id].x;
        script.hasAnotherNote = _sHitData[id];

        if (script is NoteHold holdScript) {
            holdScript.duration = _chart.chart[id].dur;

            var start = note.transform.GetChild(0).GetComponent<SpriteRenderer>();
            var hold = note.transform.GetChild(1).GetComponent<SpriteRenderer>();
            var progress = note.transform.GetChild(2).GetComponent<SpriteRenderer>();
            var end = note.transform.GetChild(3).GetComponent<SpriteRenderer>();

            note.transform.GetChild(3).localPosition = new Vector3(0, _holdHeights[id], 0);

            start.size = new Vector2(holdScript.size / 10f, start.size.y);
            end.size = new Vector2(holdScript.size / 10f, end.size.y);
            progress.size = new Vector2(0, _holdHeights[id] * 2);
            hold.size = new Vector2(holdScript.size / 10f, _holdHeights[id] * 2);

            holdScript.startRenderer = start;
            holdScript.holdRenderer = hold;
            holdScript.progressRenderer = progress;
            holdScript.endRenderer = end;
        } else {
            var spriteRenderer = note.GetComponent<SpriteRenderer>();
            spriteRenderer.size = new Vector2(script.size / 10f, spriteRenderer.size.y);
        }

        note.transform.localPosition = new Vector2(script.xPos / 100f, _positions[id]);

        _notes.Add(script);
    }

    private void Calculate() {
        var currentSpeed = 1.0f;
        var pos = 0f;
        var currentNoteId = 0;
        var currentSpeedId = 0;

        var lastFrame = Math.Ceiling(_chart.chart[G.InGame.CountOfNotes - 1].t) * constants.placingPrecision;

        var diffBetweenSpeeds = new List<List<float>>();
        var lastSpeedFrame = 0;

        for (var frame = 0; frame <= lastFrame; frame++) {
            var time = frame / constants.placingPrecision;

            if (currentSpeedId < _chart.speed.Length && _chart.speed[currentSpeedId].t <= time) {
                diffBetweenSpeeds.Add(new List<float> {frame - lastSpeedFrame, currentSpeed});

                currentSpeed = _chart.speed[currentSpeedId].s;
                lastSpeedFrame = frame;

                currentSpeedId++;
            }

            var loop = 0;
            while (currentNoteId < G.InGame.CountOfNotes && _chart.chart[currentNoteId].t <= time) {
                _positions[currentNoteId] = pos;

                if (_chart.chart[currentNoteId].dur > 0) {
                    var subPos = pos;
                    var subSpeed = currentSpeed;
                    var subSpeedId = currentSpeedId;
                    var holdLastFrame = frame + _chart.chart[currentNoteId].dur * constants.placingPrecision;

                    for (var subFrame = frame; subFrame <= holdLastFrame; subFrame++) {
                        if (subSpeedId < _chart.speed.Length &&
                            _chart.speed[subSpeedId].t <= subFrame / constants.placingPrecision) {
                            subSpeed = _chart.speed[subSpeedId].s;
                            subSpeedId++;
                        }

                        subPos += subSpeed * G.PlaySettings.Speed;
                    }

                    _holdHeights[currentNoteId] = subPos - pos;
                }

                currentNoteId++;
                loop++;
            }

            while (loop > 0) {
                loop--;
                _sHitData[currentNoteId - loop - 1] = true;
            }

            pos = diffBetweenSpeeds.Sum(diff => (int) diff[0] * diff[1] * G.PlaySettings.Speed);
            pos += (frame - lastSpeedFrame) * currentSpeed * G.PlaySettings.Speed;
        }
    }

    private void MoveY() {
        if (_chart.move == null || _currentMoveYPos >= _chart.move.Length ||
            !(_chart.move[_currentMoveYPos].t <= G.InGame.Time)) return;

        StartCoroutine(
            Interpolators.Curve(
                _curves[_chart.move[_currentMoveYPos].i],
                field.main.transform.position.y,
                -_chart.move[_currentMoveYPos].d / 100f,
                _chart.move[_currentMoveYPos].dur,
                step => {
                    var before = field.main.transform.position;
                    field.main.transform.position = new Vector3(before.x, step, before.z);
                },
                () => { }
            )
        );
        _currentMoveYPos++;
    }

    private void MoveX() {
        if (_chart.move_x == null || _currentMoveXPos >= _chart.move_x.Length ||
            !(_chart.move_x[_currentMoveXPos].t <= G.InGame.Time)) return;

        StartCoroutine(
            Interpolators.Curve(
                _curves[_chart.move_x[_currentMoveXPos].i],
                field.main.transform.position.x,
                _chart.move_x[_currentMoveXPos].d / 500f,
                _chart.move_x[_currentMoveXPos].dur,
                step => {
                    var before = field.main.transform.position;
                    field.main.transform.position = new Vector3(step, before.y, before.z);
                },
                () => { }
            )
        );
        _currentMoveXPos++;
    }

    private void MoveZ() {
        if (_chart.zoom == null || _currentMoveZPos >= _chart.zoom.Length ||
            !(_chart.zoom[_currentMoveZPos].t <= G.InGame.Time)) return;

        StartCoroutine(
            Interpolators.Curve(
                _curves[_chart.zoom[_currentMoveZPos].i],
                field.main.transform.position.z,
                _chart.zoom[_currentMoveZPos].d / 100f - 10,
                _chart.zoom[_currentMoveZPos].dur,
                step => {
                    var before = field.main.transform.position;
                    field.main.transform.position = new Vector3(before.x, before.y, step);
                },
                () => { }
            )
        );
        _currentMoveZPos++;
    }

    private void MoveNotesToZero() {
        var zeroNotes = GetZeroNotes();
        var validZeroNotes =
            zeroNotes.FindAll(note => note.transform.parent != _notesHolder.transform.GetChild(1).transform);

        foreach (var note in validZeroNotes) {
            Transform noteTransform;
            (noteTransform = note.transform).parent = _notesHolder.transform.GetChild(1).transform;
            var before = noteTransform.localPosition;
            noteTransform.localPosition = new Vector3(before.x, 0, before.z);
        }
    }

    public void Pause() {
        if (!G.InGame.CanBePaused) return;

        if (!G.InGame.PreparePause && !G.InGame.Paused) StartCoroutine(nameof(PreparePause));
        else {
            StopCoroutine(nameof(PreparePause));
            if (!G.InGame.Paused) {
                G.InGame.PreparePause = false;

                G.InGame.Paused = true;
                Interpolators.paused = true;
                _audio.Pause();

                G.InGame.ReadyAnimated = false;

                scripts.userInterfaceUpdater.ShowPauseMenu(true);

                StartCoroutine(DelayResume());
            } else {
                if (!G.InGame.CanBeResumed) return;
                
                StartCoroutine(Ready());
                scripts.userInterfaceUpdater.ShowPauseMenu(false);
            }
        }
    }

    public void Retry() {
        scripts.userInterfaceUpdater.Retry();
        if (G.Items.Energy <= 0) return;

        PlayerPrefs.SetInt(G.Keys.FormatKey(G.Keys.PlayTimes),
            PlayerPrefs.GetInt(G.Keys.FormatKey(G.Keys.PlayTimes), 0) + 1);
        G.Items.Energy--;
        
        StopAllCoroutines();

        G.InGame.CanBePaused = false;

        G.InGame.Init();
        scripts.scoreCalculator.Init();

        foreach (var note in _notes) {
            Destroy(note.gameObject);
        }

        for (var i = 0; i < _keyBeamsHolder.childCount; i++) {
            Destroy(_keyBeamsHolder.GetChild(i).gameObject);
        }

        for (var i = 0; i < _judgeEffectsHolder.childCount; i++) {
            Destroy(_judgeEffectsHolder.GetChild(i).gameObject);
        }

        field.main.transform.position = new Vector3(0, 0, 0);
        _notesHolder.transform.GetChild(0).localPosition = new Vector3(0, 0, 0);
        _notesHolder.transform.GetChild(2).localPosition = new Vector3(0, 0, 0);

        _notes.Clear();

        _lastPlacedNoteId = 0;
        _currentSpeed = 1.0f;
        _currentSpeedPos = 0;
        _currentMoveXPos = 0;
        _currentMoveYPos = 0;
        _currentMoveZPos = 0;

        for (var i = 0; i < Math.Min(_loadableNoteCount, _chart.chart.Length); i++) {
            CreateNote(_lastPlacedNoteId);
            _lastPlacedNoteId++;
        }

        _audio.Stop();

        _trackStarted = false;
        _syncFinished = false;

        G.InGame.ReadyAnimated = false;
        G.InGame.Paused = false;
        Interpolators.paused = false;

        scripts.userInterfaceUpdater.ShowPauseMenu(false);
        StartCoroutine(RetryReady());
    }

    private IEnumerator RetryReady() {
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(Ready());
    }

    public void Stop() {
        G.InGame.CanBePaused = false;
        G.InGame.Init();
        G.InGame.ReadyAnimated = false;
        G.InGame.Paused = false;
        Interpolators.paused = false;

        foreach (var note in _notes) {
            Destroy(note.gameObject);
        }

        scripts.userInterfaceUpdater.ShowPauseMenu(false);

        animators.foreground.Play("ingame_foreground_fade_out", -1, 0);
        animators.ui.Play("ingame_ui_fade_out", -1, 0);

        StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, 375f, 0f, 0.5f,
            step => { _judgeLine.transform.localScale = new Vector3(step, 1f, 1f); },
            () => { }
        ));

        StartCoroutine(Exit());
    }

    private IEnumerator Exit() {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("_scenes/TrackSelect");
    }

    private IEnumerator Intro() {
        yield return new WaitForSeconds(0.5f);

        StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, 0f, 375f, 0.35f,
            step => { _judgeLine.transform.localScale = new Vector3(step, 1f, 1f); },
            () => { }
        ));
        animators.intro.Play("ingame_intro", -1, 0);
        scripts.userInterfaceUpdater.Intro();

        yield return new WaitForSeconds(2f);

        animators.ui.Play("ingame_ui_fade_in", -1, 0);

        yield return new WaitForSeconds(1f);

        StartCoroutine(Ready());
    }

    private IEnumerator Outro() {
        yield return new WaitForSeconds(_chart.end_margin);

        StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, 375f, 0f, 0.35f,
            step => { _judgeLine.transform.localScale = new Vector3(step, 1f, 1f); },
            () => { }
        ));

        yield return new WaitForSeconds(0.25f);

        animators.ui.Play("ingame_ui_fade_out", -1, 0);

        yield return new WaitForSeconds(0.4f);

        animators.foreground.Play("ingame_foreground_fade_out", -1, 0);

        yield return new WaitForSeconds(2f);
    }

    private IEnumerator Ready() {
        animators.ready.Play("ingame_ready", -1, 0);

        yield return new WaitForSeconds(2.416f);

        G.InGame.ReadyAnimated = true;
        G.InGame.Paused = false;
        Interpolators.paused = false;

        if (_trackStarted)
            _audio.Play();
    }

    private IEnumerator PreparePause() {
        G.InGame.PreparePause = true;
        yield return new WaitForSeconds(1f);
        G.InGame.PreparePause = false;
    }

    private IEnumerator DelayResume() {
        G.InGame.CanBeResumed = false;
        yield return new WaitForSeconds(2f);
        G.InGame.CanBeResumed = true;
    }

    private IEnumerator StartTrack() {
        yield return new WaitForSeconds(1.525f);
        if (!_audio.isPlaying)
            _audio.Play();
    }

    private IEnumerator AdjustSync() {
        yield return new WaitForSeconds(G.PlaySettings.Sync / 100f);
        _syncFinished = true;
    }

    public float GetCurrentSpeed() {
        return _currentSpeed;
    }

    public GameObject GetNotesHolder() {
        return _notesHolder;
    }

    public Transform GetKeyBeamsHolder() {
        return _keyBeamsHolder;
    }

    public Transform GetJudgeEffectsHolder() {
        return _judgeEffectsHolder;
    }

    private List<Note> GetNotes() {
        return _notes;
    }

    public List<Note> GetActiveNotes() {
        return _notes.FindAll(note => G.InGame.Time - note.time >= -0.5f);
    }

    private List<Note> GetZeroNotes() {
        return _notes.FindAll(note => G.InGame.Time + Time.deltaTime - note.time >= 0f);
    }

    public void RemoveNote(Note target) {
        _notes.Remove(target);
    }
}