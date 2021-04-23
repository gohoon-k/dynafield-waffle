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
            (G.InternalSettings.ScoreRatioByJudges[0] * G.InGame.CountOfAccuracyPerfect + 
             G.InternalSettings.ScoreRatioByJudges[1] * G.InGame.CountOfPerfect +
             G.InternalSettings.ScoreRatioByJudges[2] * G.InGame.CountOfGreat +
             G.InternalSettings.ScoreRatioByJudges[3] *  G.InGame.CountOfBad) * 
            (MAXHandlingScore / G.InGame.CountOfNotes);
        
        G.InGame.Accuracy = 
            (G.InternalSettings.AccuracyRatioByJudges[0] * G.InGame.CountOfAccuracyPerfect + 
             G.InternalSettings.AccuracyRatioByJudges[1] *  G.InGame.CountOfPerfect +
             G.InternalSettings.AccuracyRatioByJudges[2] *  G.InGame.CountOfGreat +
             G.InternalSettings.AccuracyRatioByJudges[3] *  G.InGame.CountOfBad) * 
            (100f / G.InGame.CountOfNotes);

        if (G.InGame.Combo != _calculatedCombo) {
            if (G.InGame.MaxCombo < G.InGame.Combo)
                G.InGame.MaxCombo = G.InGame.Combo;
            
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
