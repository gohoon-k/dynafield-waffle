using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeAdController : MonoBehaviour {

    public int length = 5;
    
    private BaseDialog _current;
    
    void Start() {
        _current = GetComponent<BaseDialog>();
        _current.message.text = $"가상의 광고 진행 중...\n남은 시간: {length}초";
    }

    public IEnumerator StartCountdown(Action action) {
        for (int i = 1; i <= length; i++) {
            yield return new WaitForSeconds(1f);
            _current.message.text = $"가상의 광고 진행 중...\n남은 시간: {length - i}초";
        }

        yield return new WaitForSeconds(1f);
        _current.Close(true);
        action();
    }
}
