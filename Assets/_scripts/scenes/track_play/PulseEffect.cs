using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulseEffect : MonoBehaviour {

    public GameObject effect;
    
    void Start() {
        var upRenderer = effect.GetComponent<SpriteRenderer>();

        StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, 0, 1.25f, 0.3f,
                step => {
                    transform.localScale = new Vector3(10, step, 1);
                },
                () => { }
            )
        );
        StartCoroutine(Interpolators.Linear(0.19607843f, 0, 0.3f,
                step => {
                    var before = upRenderer.color;
                    upRenderer.color = new Color(before.r, before.g, before.b, step);
                },
                () => {
                    Destroy(gameObject);
                }
            )
        );
    }
    
}
