using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAnimationPlayer : MonoBehaviour {

    public bool animate;
    
    // Start is called before the first frame update
    void Start()
    {
        if (animate) {
            GetComponent<Animator>().Play("intro_camera");
        }    
    }
    
}
