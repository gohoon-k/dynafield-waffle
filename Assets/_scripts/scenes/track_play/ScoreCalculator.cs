using System.Collections.Generic;
using UnityEngine;

public class ScoreCalculator : MonoBehaviour {

    private const float MAXHandlingScore = 920000f;
    private const float MAXComboScore = 80000f;

    private int _calculatedCombo = 0;

    private readonly List<int> _brokenCombos = new List<int>();

    void Start() { }
    
    void Update() {

        G.InGame.ScoreByJudge =
            (G.InGame.CountOfAccuracyPerfect + G.InGame.CountOfPerfect +
             0.75f * G.InGame.CountOfGreat +
             0.25f * G.InGame.CountOfBad) * (MAXHandlingScore / G.InGame.CountOfNotes);
        
        G.InGame.Accuracy = 
            (G.InGame.CountOfAccuracyPerfect + 
             0.7f * G.InGame.CountOfPerfect +
             0.3f * G.InGame.CountOfGreat +
             0.1f * G.InGame.CountOfBad) * (100f / G.InGame.CountOfNotes);

        if (G.InGame.Combo != _calculatedCombo) {
            if (G.InGame.Combo == 0) {
                _brokenCombos.Add(_calculatedCombo);
            }

            G.InGame.ScoreByCombo = 0;
            
            foreach (var brokenCombo in _brokenCombos) {
                G.InGame.ScoreByCombo += 
                    brokenCombo * (2 * MAXComboScore / (G.InGame.CountOfNotes * (G.InGame.CountOfNotes - 1))) * (brokenCombo - 1) / 2f ;
            }
            
            G.InGame.ScoreByCombo += G.InGame.Combo * (2 * MAXComboScore / (G.InGame.CountOfNotes * (G.InGame.CountOfNotes - 1))) * (G.InGame.Combo - 1) / 2f ;
            _calculatedCombo = G.InGame.Combo;
        }

    }

    public void Init() {
        _calculatedCombo = 0;
        _brokenCombos.Clear();
    }
    
}
