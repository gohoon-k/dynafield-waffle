using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboEffect : MonoBehaviour {

    public int period;
    
    private Animator _animator;
    private TextMesh _textMesh;
    private TextMesh _effectMesh;

    private int _currentStep = 1;

    // Start is called before the first frame update
    void Start() {
        _animator = GetComponent<Animator>();
        _textMesh = transform.GetChild(0).GetComponent<TextMesh>();
        _effectMesh = transform.GetChild(1).GetComponent<TextMesh>();
    }

    // Update is called once per frame
    // void Update()
    // {
    //     if (G.InGame.Combo > 0 && G.InGame.Combo > _currentStep * period && G.InGame.Combo / period != _currentStep - 1) {
    //         _textMesh.text = $"{_currentStep * period}";
    //         _effectMesh.text = $"{_currentStep * period}";
    //         _currentStep++;
    //         _animator.Play("ingame_combo_effect", -1, 0);
    //     }
    // }
}
