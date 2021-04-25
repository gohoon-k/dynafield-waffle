using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteCounterClickEffect : MonoBehaviour {

    public sbyte type;
    
    void Start() {
        var clickEffectRenderer = GetComponent<SpriteRenderer>();
        
        StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, 0, type == 1 ? 400 : 200, 0.3f,
                step => { transform.localScale = new Vector3(step, type == 1 ? step : 5000, 1); },
                () => { }
            )
        );
        StartCoroutine(Interpolators.Linear(type == 1 ? 0.4f : 0.2f, 0, 0.3f,
                step => {
                    var beforeColor = clickEffectRenderer.color;
                    clickEffectRenderer.color = new Color(beforeColor.r, beforeColor.g, beforeColor.b, step);
                },
                () => {
                    Destroy(gameObject);
                }
            )
        );
    }

}
