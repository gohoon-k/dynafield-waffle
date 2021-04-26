using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TrackSelect : MonoBehaviour
{

    #region UIObj
    
    public Text track_info;       //곡 정보
    public Text best_score;        //최고 스코어
    public Text best_accuracy;     //최고 정확도
    public Text energy_left;      //현재 에너지
    public Text energy_max;       //최대 에너지
    public Text energy_timer;     //에너지 충전 대기시간
    public Text difficulty;       //난이도 숫자
    public Text difficulty_easy;  //난이도 easy
    public Text difficulty_hard;  //난이도 hard
    public Text speed;            //채보속도 숫자
    public Image start_toggle;
    public Text start_message;
    public Image prepareplay;
    
    
    
    #endregion
    
    void UpdateEnergyUI()
    {
        if (G.Items.CoolDown == -1)
        {
            energy_left.color = new Color(difficulty_easy.color.r, difficulty_easy.color.g,
                difficulty_easy.color.b, 1.0f);
            energy_max.color = new Color(difficulty_easy.color.r, difficulty_easy.color.g,
                difficulty_easy.color.b, 1.0f);
            energy_timer.color = new Color(difficulty_easy.color.r, difficulty_easy.color.g,
                difficulty_easy.color.b, 0.0f);
        }
        else
        {
            
            DateTime difference = new DateTime(G.Items.CoolDown - DateTime.Now.Ticks);
            energy_timer.text = difference.ToString("mm:ss");
            energy_left.color = new Color(difficulty_easy.color.r, difficulty_easy.color.g,
                difficulty_easy.color.b, 0.15f);
            energy_max.color = new Color(difficulty_easy.color.r, difficulty_easy.color.g,
                difficulty_easy.color.b, 0.15f);
            energy_timer.color = new Color(difficulty_easy.color.r, difficulty_easy.color.g,
                difficulty_easy.color.b, 1.0f);
        }
        
    }
    void CheckCooldownFinished()
    {
        if (G.Items.CoolDown != -1 && G.Items.CoolDown - DateTime.Now.Ticks < 0)
        {
            G.Items.Energy = 15;
            G.Items.CoolDown = -1;
        }
    }

    void PreparePlay()
    {
        prepareplay.gameObject.SetActive(true);
        start_toggle.color = new Color(difficulty_easy.color.r, difficulty_easy.color.g,
            difficulty_easy.color.b, 0.3f);
    }

    public void  Account()
    {
        PlayerPrefs.SetInt(G.Keys.Speed, 5);
        PlayerPrefs.SetInt(G.Keys.Sync, 3);
        PlayerPrefs.SetInt(G.Keys.Energy,15);
        PlayerPrefs.SetInt(G.Keys.CoolDown,-1);
        PlayerPrefs.SetInt("initialized", 1);
        PlayerPrefs.Save();
    }
    
    #region PlaybuttonList  //버튼용 함수 모음
    public void SelectTrack(int dir)        //트랙변경
    {
        G.PlaySettings.TrackId += dir;      //TrackId 더하는부분
        if (G.PlaySettings.TrackId == -1) G.PlaySettings.TrackId = G.Tracks.Length - 1;
        if (G.PlaySettings.TrackId == G.Tracks.Length) G.PlaySettings.TrackId = 0;
        
        //곡에 따라 UI 수정
        track_info.GetComponent<Text>().text = String.Format("{0} <size=40>{1}</size>",
               G.Tracks[G.PlaySettings.TrackId].title, G.Tracks[G.PlaySettings.TrackId].artist);
        difficulty.text = G.Tracks[G.PlaySettings.TrackId].difficulty[G.PlaySettings.Difficulty].ToString();
        
        //값이 없으면 0 반환
        best_score.text = PlayerPrefs.HasKey(G.Keys.FormatKey(G.Keys.BestScore))
            ? PlayerPrefs.GetInt(G.Keys.FormatKey(G.Keys.BestScore)).ToString()
            : "0";
        best_accuracy.text = PlayerPrefs.HasKey(G.Keys.FormatKey(G.Keys.BestAccuracy))
            ? PlayerPrefs.GetFloat(G.Keys.FormatKey(G.Keys.BestAccuracy)).ToString()
            : "0%";
    }
    public void ToggleDifficulty()              //난이도 변경
    {
        if (G.PlaySettings.Difficulty == 0)     //난이도가 easy였을때 hard로 변경
        {
            G.PlaySettings.Difficulty = 1;
            difficulty_easy.color = new Color(difficulty_easy.color.r, difficulty_easy.color.g,
                difficulty_easy.color.b, 0.4f);
            difficulty_hard.color = new Color(difficulty_hard.color.r, difficulty_hard.color.g,
                difficulty_hard.color.b, 1.0f);
        }
        else
        {
            G.PlaySettings.Difficulty = 0;      //난이도가 hard였을때 easy로 변경
            difficulty_easy.color = new Color(difficulty_easy.color.r, difficulty_easy.color.g,
                difficulty_easy.color.b, 1.0f);
            difficulty_hard.color = new Color(difficulty_hard.color.r, difficulty_hard.color.g,
                difficulty_hard.color.b, 0.4f);
        }
        difficulty.GetComponent<Text>().text = G.Tracks[G.PlaySettings.TrackId].difficulty[G.PlaySettings.Difficulty].ToString();
    }
    public void SetDisplayspeed(int delta)      //DisplaySpeed 변경   
    {
        G.PlaySettings.DisplaySpeed += delta;
        speed.text = G.PlaySettings.DisplaySpeed.ToString();
    }

    public void PusyBack()
    {
        if (start_toggle.gameObject.activeSelf != true)
        {
            start_toggle.gameObject.SetActive(true);
            start_message.gameObject.SetActive(true);
            if (G.Items.Energy == 0)
            {
                PreparePlay();
            }
        }
        else
        {
            start_toggle.gameObject.SetActive(false);
            start_message.gameObject.SetActive(false);
            start_toggle.color = new Color(difficulty_easy.color.r, difficulty_easy.color.g,
                difficulty_easy.color.b, 1.0f);
            prepareplay.gameObject.SetActive(false);
        }
    }

    public void StartPlay()
    {
        G.Items.Energy--;
        PlayerPrefs.SetInt(G.Keys.SelectedTrack,G.PlaySettings.TrackId);
        PlayerPrefs.SetInt(G.Keys.Speed, G.PlaySettings.DisplaySpeed);
        PlayerPrefs.SetInt(G.Keys.Sync, G.PlaySettings.DisplaySync);
        PlayerPrefs.SetInt(G.Keys.Energy, G.Items.Energy);
        PlayerPrefs.Save();
        SceneManager.LoadScene("TrackPlay");
    }
    
    public void Back()                          //뒤로가기 버튼
    {
        PlayerPrefs.SetInt(G.Keys.SelectedTrack,G.PlaySettings.TrackId);
        PlayerPrefs.SetInt(G.Keys.Speed, G.PlaySettings.DisplaySpeed);
        PlayerPrefs.SetInt(G.Keys.Sync, G.PlaySettings.DisplaySync);
        PlayerPrefs.SetInt(G.Keys.Energy, G.Items.Energy);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Intro");
    }
    
    #endregion
    
    // Start is called before the first frame update
    void Start()
    {
        
        #region Initialization  //초기화

        if (PlayerPrefs.HasKey("initialized") == false) {Account();}
        G.InitTracks();
        G.PlaySettings.TrackId = PlayerPrefs.GetInt(G.Keys.SelectedTrack);
        G.PlaySettings.DisplaySpeed = PlayerPrefs.GetInt(G.Keys.Speed);
        G.PlaySettings.DisplaySync = PlayerPrefs.GetInt(G.Keys.Sync);
        G.Items.Energy = PlayerPrefs.GetInt(G.Keys.Energy);
        G.Items.CoolDown = PlayerPrefs.GetInt(G.Keys.CoolDown);
        PlayerPrefs.Save();
        SelectTrack(G.PlaySettings.TrackId);
        CheckCooldownFinished();
        UpdateEnergyUI();
        speed.text = G.PlaySettings.DisplaySpeed.ToString();
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
