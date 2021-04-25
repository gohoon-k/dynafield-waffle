using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TrackSelect : MonoBehaviour
{

    #region UIObj

    public Text max_score;        //최고 스코어
    public Text max_accuracy;     //최고 정확도
    public GameObject energy_left;      //현재 에너지
    public GameObject energy_max;       //최대 에너지
    public GameObject energy_timer;     //에너지 충전 대기시간
    public Text difficulty;       //난이도 숫자
    public Text difficulty_easy;  //난이도 easy
    public Text difficulty_hard;  //난이도 hard
    public Text speed;            //채보속도 숫자
    

    #endregion
    





    void UpdateEnergyUI()
    {
        
    }







    #region PlaybuttonList

    

    
    public void ToggleDifficulty()
    {
        if (G.PlaySettings.Difficulty == 0)
        {
            G.PlaySettings.Difficulty = 1;
            difficulty_easy.color = new Color(difficulty_easy.color.r, difficulty_easy.color.g,
                difficulty_easy.color.b, 0.4f);
            difficulty_hard.color = new Color(difficulty_hard.color.r, difficulty_hard.color.g,
                difficulty_hard.color.b, 1.0f);

        }
        else
        {
            G.PlaySettings.Difficulty = 0;
            difficulty_easy.color = new Color(difficulty_easy.color.r, difficulty_easy.color.g,
                difficulty_easy.color.b, 1.0f);
            difficulty_hard.color = new Color(difficulty_hard.color.r, difficulty_hard.color.g,
                difficulty_hard.color.b, 0.4f);
        }
    }
    
    public void SetDifficulty(int delta)
    {
        G.PlaySettings.DisplaySpeed += delta;
        speed.GetComponent<Text>().text = G.PlaySettings.DisplaySpeed.ToString();
    }
    
    public void Back()
    {
        SceneManager.LoadScene("Intro");
    }
    
    #endregion
    
    // Start is called before the first frame update
    void Start()
    {
        speed.GetComponent<Text>().text = G.PlaySettings.DisplaySpeed.ToString();
        
        G.InitTracks();

        if (G.Items.Energy == 0 && G.Items.CoolDown == -1)
        {
            G.Items.CoolDown = DateTime.Now.AddMinutes(5).Ticks;
        }
        
        //CheckCooldownFinished();

        UpdateEnergyUI();

    }

    // Update is called once per frame
    void Update()
    {
        //CheckCooldownFinished();
        UpdateEnergyUI();
    }
}
