using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TrackSelect : MonoBehaviour
{








    public void Back()
    {
        SceneManager.LoadScene("Intro");
    }
    
    // Start is called before the first frame update
    void Start()
    {
        G.InitTracks();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
