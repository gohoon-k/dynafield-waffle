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
    public GameObject swipe;
    public GameObject counter;
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
public class Effects {
    public GameObject counter;
    public GameObject judgeLinePulse;
    public GameObject notifyFieldMove;
}

[Serializable]
public class Constants {
    public float placingPrecision;
    public bool viewMode;
    public bool autoPlay;
    public bool debugMode;
}

public class ChartPlayer : MonoBehaviour {
    public Field field;
    public Constants constants;

    public NotePrefabs notePrefabs;

    public Effects effects;
    
    public Animators animators;
    public Scripts scripts;
    
    private Transform _notesParent;
    private Transform _keyBeamsParent;
    private Transform _judgeEffectsParent;
    
    private Transform _judgeLine;
    private Animator _judgeLineEffect1;
    private Transform _judgeLineEffect2Parent;
    
    private Animator _notifyFieldUp;
    private Animator _notifyFieldDown;

    private readonly List<Note> _notes = new List<Note>();
    private Chart _chart;

    private readonly int[] _loadableNoteCountsPerSpeed = {200, 185, 170, 155, 140, 125, 110, 100};
    private int _loadableNoteCount;

    private float[] _notePositions;
    private float[] _holdNoteHeights;
    private bool[] _sHitData;

    private int _lastPlacedNoteId;
    private float _currentSpeed = 1.0f;

    private bool _trackStarted;
    private bool _synchronizing;
    private bool _syncFinished;
    private bool _gameFinished;
    private bool _outroPlayed;

    private int _currentSpeedIndex;
    private int _currentMoveXIndex;
    private int _currentMoveYIndex;
    private int _currentMoveZIndex;
    
    private int _notifiedMoveYIndex;

    private readonly List<List<float>> _differenceBetweenSpeeds = new List<List<float>>();
    private float _lastSpeedTime;

    private AudioSource _audio;

    private readonly AnimationCurve[] _curves = {
        Interpolators.LinearCurve,
        Interpolators.EaseInCurve,
        Interpolators.EaseOutCurve,
        Interpolators.EaseInOutCurve
    };
    
    void Start() {
        G.InitTracks();
        
        _chart = JsonUtility.FromJson<Chart>(((TextAsset) Resources.Load(
            $"data/charts/{G.Tracks[G.PlaySettings.TrackId].internal_name}.{G.PlaySettings.Difficulty}", typeof(TextAsset)
        )).ToString());

        G.InGame.CountOfNotes = _chart.chart.Length;
        G.InGame.CanBePaused = false;

        if (constants.debugMode) {
            G.PlaySettings.AutoPlay = constants.autoPlay;
        }
        
        if (!constants.debugMode) {
            G.Items.Energy = PlayerPrefs.GetInt(G.Keys.Energy, 15);

            G.Items.Energy--;
            PlayerPrefs.SetInt(G.Keys.Energy, G.Items.Energy);
        }
        
        PlayerPrefs.SetInt(G.Keys.FormatKey(G.Keys.PlayTimes),
            PlayerPrefs.GetInt(G.Keys.FormatKey(G.Keys.PlayTimes), 0) + 1);
        
        _notePositions = new float[G.InGame.CountOfNotes];
        _holdNoteHeights = new float[G.InGame.CountOfNotes];
        _sHitData = new bool[G.InGame.CountOfNotes];

        _notesParent = field.main.transform.Find("notes");
        _judgeLine = field.main.transform.Find("judge_line");

        _judgeLineEffect1 = _judgeLine.Find("line_effect_1").GetComponent<Animator>();
        _judgeLineEffect2Parent = _judgeLine.Find("line_effect_2");
        
        _keyBeamsParent = field.main.transform.Find("key_beam_effects");
        _judgeEffectsParent = field.main.transform.Find("judge_effects");

        _notifyFieldUp = effects.notifyFieldMove.transform.Find("move_to_up").GetComponent<Animator>();
        _notifyFieldDown = effects.notifyFieldMove.transform.Find("move_to_down").GetComponent<Animator>();

        _loadableNoteCount = Application.isEditor && constants.viewMode ? 2000 : _loadableNoteCountsPerSpeed[G.PlaySettings.DisplaySpeed];

        Calculate();

        for (var i = 0; i < Math.Min(_loadableNoteCount, _chart.chart.Length); i++) {
            CreateNote(_lastPlacedNoteId);
            _lastPlacedNoteId++;
        }

        _audio = GetComponent<AudioSource>();
        _audio.clip = (AudioClip) Resources.Load($"tracks/{G.Tracks[G.PlaySettings.TrackId].internal_name}", typeof(AudioClip));

        animators.foreground.Play("ingame_foreground_fade_in", -1, 0);
        StartCoroutine(Intro());
    }
    
    void Update() {
        if (G.InGame.ReadyAnimated && !G.InGame.Paused) {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                Pause();
            }

            if (!_trackStarted) {
                StartCoroutine(StartTrack());
                _trackStarted = true;
            }

            if (!_syncFinished && !_synchronizing) {
                _synchronizing = true;
                StartCoroutine(AdjustSync());
            } else if (_syncFinished) {
                G.InGame.CanBePaused = true;
                
                ChangeSpeed();
                
                NotifyFieldMove();

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

    private void OnApplicationQuit() {
        if (G.Items.Energy == 0)
            G.Items.CoolDown = DateTime.Now.AddMinutes(10).ToBinary();
    }

    private void MoveNotes() {
        var pos = _differenceBetweenSpeeds.Sum(diff => diff[0] * diff[1] * constants.placingPrecision * G.PlaySettings.Speed);
        pos += (G.InGame.Time - _lastSpeedTime) * constants.placingPrecision * _currentSpeed * G.PlaySettings.Speed;

        var positive = _notesParent.GetChild(0);
        var positiveBefore = positive.localPosition;
        positive.localPosition = new Vector3(positiveBefore.x, -pos, positiveBefore.z);

        var negative = _notesParent.GetChild(2);
        var negativeBefore = negative.localPosition;
        negative.localPosition = new Vector3(negativeBefore.x, pos, negativeBefore.z);
    }

    private void CreateNote(int id) {
        var targetNotePrefab = _chart.chart[id].ty switch {
            0 => notePrefabs.click,
            1 => notePrefabs.slide,
            2 => notePrefabs.hold,
            3 => notePrefabs.swipe,
            4 => notePrefabs.counter,
            _ => notePrefabs.click
        };

        var note = Instantiate(
            targetNotePrefab,
            _notesParent.GetChild(_chart.chart[id].d == 1 ? 0 : 2),
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

        switch (script) {
            case NoteHold holdScript: {
                holdScript.duration = _chart.chart[id].dur;

                var start = note.transform.GetChild(0).GetComponent<SpriteRenderer>();
                var hold = note.transform.GetChild(1).GetComponent<SpriteRenderer>();
                var progress = note.transform.GetChild(2).GetComponent<SpriteRenderer>();
                var end = note.transform.GetChild(3).GetComponent<SpriteRenderer>();

                note.transform.GetChild(3).localPosition = new Vector3(0, _holdNoteHeights[id], 0);

                start.size = new Vector2(holdScript.size / 10f, start.size.y);
                end.size = new Vector2(holdScript.size / 10f, end.size.y);
                progress.size = new Vector2(0, _holdNoteHeights[id] * 2);
                hold.size = new Vector2(1, _holdNoteHeights[id] * 2 / (holdScript.size / 10f));

                hold.gameObject.transform.localScale = new Vector3(holdScript.size / 20f, holdScript.size / 20f, 1);

                holdScript.startRenderer = start;
                holdScript.holdRenderer = hold;
                holdScript.progressRenderer = progress;
                holdScript.endRenderer = end;
                break;
            }
            case NoteCounter counterScript: {
                var mainRenderer = note.GetComponent<SpriteRenderer>();
                var effectRenderer = note.transform.GetChild(0).GetComponent<SpriteRenderer>();

                mainRenderer.size = new Vector2(script.size / 10f, mainRenderer.size.y);
                effectRenderer.size = new Vector2(script.size / 10f, effectRenderer.size.y);
                
                counterScript.mainRenderer = mainRenderer;
                counterScript.effectRenderer = effectRenderer;
            
                counterScript.timeout = _chart.chart[id].timeout;
                counterScript.count = _chart.chart[id].counts;

                counterScript.effectGroup = effects.counter;
                break;
            }
            default: {
                var spriteRenderer = note.GetComponent<SpriteRenderer>();
                spriteRenderer.size = new Vector2(script.size / 10f, spriteRenderer.size.y);
                
                script.SetRenderer(spriteRenderer);
                break;
            }
        }

        note.transform.localPosition = new Vector2(script.xPos / 100f, _notePositions[id]);

        _notes.Add(script);
    }

    private void Calculate() {
        var placingCursor = 0f;
        
        var currentSpeed = 1.0f;
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
                _notePositions[currentNoteId] = placingCursor;
                
                currentNoteId++;
                loop++;

                if (_chart.chart[currentNoteId - 1].ty != 2) continue;
                
                var subPlacingCursor = 0f;
                var subSpeed = currentSpeed;
                var subSpeedId = currentSpeedId;
                var holdLastFrame = frame + _chart.chart[currentNoteId - 1].dur * constants.placingPrecision;

                var subDiffBetweenSpeeds = new List<List<float>>();
                var subLastSpeedFrame = 0;

                var subFrameFromZero = 0;

                for (var subFrame = frame; subFrame <= holdLastFrame; subFrame++) {
                    if (subSpeedId < _chart.speed.Length &&
                        _chart.speed[subSpeedId].t <= subFrame / constants.placingPrecision) {
                        subDiffBetweenSpeeds.Add(new List<float> {subFrameFromZero - subLastSpeedFrame, subSpeed});
                            
                        subSpeed = _chart.speed[subSpeedId].s;
                        subLastSpeedFrame = subFrameFromZero;
                            
                        subSpeedId++;
                    }

                    subFrameFromZero++;
                        
                    subPlacingCursor = subDiffBetweenSpeeds.Sum(diff => (int) diff[0] * diff[1] * G.PlaySettings.Speed);
                    subPlacingCursor += (subFrameFromZero - subLastSpeedFrame) * subSpeed * G.PlaySettings.Speed;
                }

                _holdNoteHeights[currentNoteId - 1] = subPlacingCursor;
            }

            while (loop > 0) {
                loop--;
                _sHitData[currentNoteId - loop - 1] = true;
            }

            placingCursor = diffBetweenSpeeds.Sum(diff => (int) diff[0] * diff[1] * G.PlaySettings.Speed);
            placingCursor += (frame - lastSpeedFrame) * currentSpeed * G.PlaySettings.Speed;
        }
    }

    private void ChangeSpeed() {
        if (_chart.speed == null || _currentSpeedIndex >= _chart.speed.Length ||
            !(_chart.speed[_currentSpeedIndex].t <= G.InGame.Time)) return;
        
        _differenceBetweenSpeeds.Add(new List<float> {G.InGame.Time - _lastSpeedTime, _currentSpeed});
        _lastSpeedTime = G.InGame.Time;
                    
        _currentSpeed = _chart.speed[_currentSpeedIndex].s;
        _currentSpeedIndex++;
    }

    private void NotifyFieldMove() {
        if (_chart.move == null || _notifiedMoveYIndex >= _chart.move.Length ||
            !(_chart.move[_notifiedMoveYIndex].t - 0.65f <= G.InGame.Time)) return;

        var destination = -_chart.move[_currentMoveYIndex].d / 100f;
        var current = field.main.transform.position.y;
        
        (destination < current ? _notifyFieldDown : _notifyFieldUp)
            .Play("notify_field_move_blink", -1, 0);

        _notifiedMoveYIndex++;
    }
    
    private void MoveY() {
        if (_chart.move == null || _currentMoveYIndex >= _chart.move.Length ||
            !(_chart.move[_currentMoveYIndex].t <= G.InGame.Time)) return;

        StartCoroutine(
            Interpolators.Curve(
                _curves[_chart.move[_currentMoveYIndex].i],
                field.main.transform.position.y,
                -_chart.move[_currentMoveYIndex].d / 100f,
                _chart.move[_currentMoveYIndex].dur,
                step => {
                    var before = field.main.transform.position;
                    field.main.transform.position = new Vector3(before.x, step, before.z);
                },
                () => { }
            )
        );
        _currentMoveYIndex++;
    }

    private void MoveX() {
        if (_chart.move_x == null || _currentMoveXIndex >= _chart.move_x.Length ||
            !(_chart.move_x[_currentMoveXIndex].t <= G.InGame.Time)) return;

        StartCoroutine(
            Interpolators.Curve(
                _curves[_chart.move_x[_currentMoveXIndex].i],
                field.main.transform.position.x,
                _chart.move_x[_currentMoveXIndex].d / 500f,
                _chart.move_x[_currentMoveXIndex].dur,
                step => {
                    var before = field.main.transform.position;
                    field.main.transform.position = new Vector3(step, before.y, before.z);
                },
                () => { }
            )
        );
        _currentMoveXIndex++;
    }

    private void MoveZ() {
        if (_chart.zoom == null || _currentMoveZIndex >= _chart.zoom.Length ||
            !(_chart.zoom[_currentMoveZIndex].t <= G.InGame.Time)) return;

        StartCoroutine(
            Interpolators.Curve(
                _curves[_chart.zoom[_currentMoveZIndex].i],
                field.main.transform.position.z,
                _chart.zoom[_currentMoveZIndex].d / 100f - 10,
                _chart.zoom[_currentMoveZIndex].dur,
                step => {
                    var before = field.main.transform.position;
                    field.main.transform.position = new Vector3(before.x, before.y, step);
                },
                () => { }
            )
        );
        _currentMoveZIndex++;
    }

    private void MoveNotesToZero() {
        var zeroNotes = GetZeroNotes();
        var validZeroNotes =
            zeroNotes.FindAll(note => note.transform.parent != _notesParent.GetChild(1).transform);

        foreach (var note in validZeroNotes) {
            Transform noteTransform;
            (noteTransform = note.transform).parent = _notesParent.GetChild(1).transform;
            var before = noteTransform.localPosition;
            noteTransform.localPosition = new Vector3(before.x, 0, before.z);
        }
    }

    public void Pulse() {
        _judgeLineEffect1.Play("judge_line_pulse", -1, 0);

        Instantiate(effects.judgeLinePulse, _judgeLineEffect2Parent, true).transform.localPosition = new Vector3(0, 0, 0);
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
        if (G.Items.Energy <= 0) return;

        PlayerPrefs.SetInt(G.Keys.FormatKey(G.Keys.PlayTimes),
            PlayerPrefs.GetInt(G.Keys.FormatKey(G.Keys.PlayTimes), 0) + 1);

        if (!constants.debugMode) {
            G.Items.Energy--;
            PlayerPrefs.SetInt(G.Keys.Energy, G.Items.Energy);
        }
        
        scripts.userInterfaceUpdater.Retry();

        StopAllCoroutines();

        G.InGame.CanBePaused = false;

        G.InGame.Init();
        scripts.scoreCalculator.Init();

        foreach (var note in _notes) {
            Destroy(note.gameObject);
        }

        for (var i = 0; i < _keyBeamsParent.childCount; i++) {
            Destroy(_keyBeamsParent.GetChild(i).gameObject);
        }

        for (var i = 0; i < _judgeEffectsParent.childCount; i++) {
            Destroy(_judgeEffectsParent.GetChild(i).gameObject);
        }

        for (var i = 0; i < effects.counter.transform.childCount; i++) {
            Destroy(effects.counter.transform.GetChild(i).gameObject);
        }

        field.main.transform.position = new Vector3(0, 0, 0);
        _notesParent.GetChild(0).localPosition = new Vector3(0, 0, 0);
        _notesParent.GetChild(2).localPosition = new Vector3(0, 0, 0);

        _notes.Clear();
        _differenceBetweenSpeeds.Clear();

        _lastSpeedTime = 0;

        _lastPlacedNoteId = 0;
        _currentSpeed = 1.0f;
        _currentSpeedIndex = 0;
        _currentMoveXIndex = 0;
        _currentMoveYIndex = 0;
        _currentMoveZIndex = 0;
        _notifiedMoveYIndex = 0;

        for (var i = 0; i < Math.Min(_loadableNoteCount, _chart.chart.Length); i++) {
            CreateNote(_lastPlacedNoteId);
            _lastPlacedNoteId++;
        }

        _audio.Stop();

        _trackStarted = false;
        _syncFinished = false;
        _synchronizing = false;

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

        yield return new WaitForSeconds(3f);

        _notifyFieldUp.Play("notify_field_move_intro");
        _notifyFieldDown.Play("notify_field_move_intro");
    }

    private IEnumerator Outro() {
        yield return new WaitForSeconds(_chart.end_margin);

        _notifyFieldUp.Play("notify_field_move_outro", -1, 0);
        _notifyFieldDown.Play("notify_field_move_outro", -1, 0);
        
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

    public Transform GetNotesHolder() {
        return _notesParent;
    }

    public Transform GetKeyBeamsHolder() {
        return _keyBeamsParent;
    }

    public Transform GetJudgeEffectsHolder() {
        return _judgeEffectsParent;
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