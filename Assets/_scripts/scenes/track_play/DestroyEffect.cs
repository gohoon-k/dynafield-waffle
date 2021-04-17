using UnityEngine;

public class DestroyEffect : MonoBehaviour
{
    private SpriteRenderer _renderer;

    private float _size;
    
    void Start()
    {
        _renderer ??= GetComponent<SpriteRenderer>();
        
        StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve, 0f, _size, 0.6f,
            step => {
                _renderer.size = new Vector2(step, gameObject.transform.localScale.y);
            }, () => { }
        ));
        StartCoroutine(Interpolators.Curve(Interpolators.LinearCurve, 0.2f, 0.0f, 0.6f,
            step => {
                var beforeColor = _renderer.color;
                _renderer.color = new Color(beforeColor.r, beforeColor.g, beforeColor.b, step);
            }, () => {
                Destroy(gameObject);
            }
        ));
    }
    
    void Update() { }

    public void Set(int type, float size) {
        var colors = new [] {
            new [] {217, 135, 4},
            new [] {59, 183, 6},
            new [] {0, 127, 255}, 
            new [] {0, 127, 255},
            new [] {255, 67, 67}
        };

        _size = size;
        
        _renderer ??= GetComponent<SpriteRenderer>();
        _renderer.color = new Color(colors[type][0]/255f, colors[type][1]/255f, colors[type][2]/255f);
    }

}
