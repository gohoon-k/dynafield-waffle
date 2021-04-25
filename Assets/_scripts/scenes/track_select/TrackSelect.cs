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
    
    
    
    void CreateAccount()
    {
        PlayerPrefs.SetInt("DisplaySyne", 5);
        PlayerPrefs.SetInt("DisplaySpeed", 3);
        PlayerPrefs.SetInt("energy",15);
        PlayerPrefs.SetInt("cooldown",-1);
        PlayerPrefs.SetInt("account", 1);
        PlayerPrefs.Save();
    }
    void UpdateEnergyUI()
    {
        
    }
    void CheckCooldownFinished()
    {
        
    }
    
    #region PlaybuttonList  //버튼용 함수 모음

    public void ToggleDifficulty()              //난이도 변경
    {
        if (G.PlaySettings.Difficulty == 0)     //난이도가 easy였을때 hard로 변경
        {
            G.PlaySettings.Difficulty = 1;
            difficulty_easy.color = new Color(difficulty_easy.color.r, difficulty_easy.color.g,
                difficulty_easy.color.b, 0.4f);
            difficulty_hard.color = new Color(difficulty_hard.color.r, difficulty_hard.color.g,
                difficulty_hard.color.b, 1.0f);
            difficulty.GetComponent<Text>().text = G.Tracks[0].difficulty[1].ToString();
        }
        else
        {
            G.PlaySettings.Difficulty = 0;      //난이도가 hard였을때 easy로 변경
            difficulty_easy.color = new Color(difficulty_easy.color.r, difficulty_easy.color.g,
                difficulty_easy.color.b, 1.0f);
            difficulty_hard.color = new Color(difficulty_hard.color.r, difficulty_hard.color.g,
                difficulty_hard.color.b, 0.4f);
            difficulty.GetComponent<Text>().text = G.Tracks[0].difficulty[0].ToString();
        }
    }
    
    public void SetDisplayspeed(int delta)      //DisplaySpeed 변경   
    {
        G.PlaySettings.DisplaySpeed += delta;
        speed.GetComponent<Text>().text = G.PlaySettings.DisplaySpeed.ToString();
    }
    
    public void Back()                          //뒤로가기 버튼
    {
        SceneManager.LoadScene("Intro");
    }
    
    #endregion
    
    // Start is called before the first frame update
    void Start()
    {
        #region Init

        if (PlayerPrefs.HasKey("account") == false)
        {
            CreateAccount();
        }
        
        speed.GetComponent<Text>().text = G.PlaySettings.DisplaySpeed.ToString();
        G.InitTracks();
        CheckCooldownFinished();
        UpdateEnergyUI();
        
        #endregion
        

        
        if (G.Items.Energy == 0 && G.Items.CoolDown == -1)
        {
            G.Items.CoolDown = DateTime.Now.AddMinutes(5).Ticks;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        CheckCooldownFinished();
        UpdateEnergyUI();
    }
}
