using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

internal class RewardObject {
    public int Index;
    public Text DayDescription;
    public Text Rewards;
}

public class DailyRewardsDialog : MonoBehaviour {
    public GameObject dialogs;

    public RectTransform content;

    public Image background;
    public Text title;
    public Text closeButton;
    public Text receiveText;
    public Button receiveButton;
    public Text description;

    public GameObject rewardsParent;

    public string forceToday;

    public bool isActive;

    private readonly List<RewardObject> _rewards = new List<RewardObject>();

    private bool _init;

    private const float PresentReceivedAlpha = 0.7f;
    private const float PastAlpha = 0.45f;
    private const float FutureAlpha = 0.2f;

    private void Init() {
        for (var i = 0; i < rewardsParent.transform.childCount; i++) {
            var reward = new RewardObject {
                Index = i,
                DayDescription = rewardsParent.transform.GetChild(i).GetChild(0).GetComponent<Text>(),
                Rewards = rewardsParent.transform.GetChild(i).GetChild(1).GetComponent<Text>()
            };

            reward.DayDescription.color = reward.Rewards.color = new Color(1, 1, 1, 0);
            _rewards.Add(reward);
        }

        _init = true;
    }

    public void Open() {
        if (!_init) Init();

        isActive = true;
        
        gameObject.SetActive(true);

        var checkedDate = PlayerPrefs.GetString(G.Keys.CheckedDate, "1990-01-01");
        var received = PlayerPrefs.GetInt(G.Keys.ReceivedPreviousReward, 1) == 1;
        var currentReward = PlayerPrefs.GetInt(G.Keys.RewardIndex, -1);

        var today = forceToday != "-1" ? forceToday : DateTime.Now.ToString("yyyy-MM-dd");

        if (!checkedDate.Equals(today)) {
            PlayerPrefs.SetString(G.Keys.CheckedDate, today);

            if (received) {
                if (currentReward >= 6) currentReward = 0;
                else currentReward++;
                PlayerPrefs.SetInt(G.Keys.RewardIndex, currentReward);
                PlayerPrefs.SetInt(G.Keys.ReceivedPreviousReward, 0);
                received = false;
            }
        }

        title.color = closeButton.color = receiveText.color = description.color = new Color(1, 1, 1, 0);

        background.color = new Color(0, 0, 0, 0);

        receiveText.color = new Color(0, 0, 0, 0);
        content.localScale = new Vector3(1.5f, 1.5f, 1);

        receiveButton.interactable = !received;
        receiveText.text = received ? "수령 완료" : "받기";

        StartCoroutine(Interpolators.Linear(0, 1, 0.4f, step => {
            _rewards.ForEach(reward => {
                reward.DayDescription.color = reward.Rewards.color = new Color(
                    1, 1, 1,
                    reward.Index == currentReward ? 
                        (received ? step * PresentReceivedAlpha : step) : 
                        reward.Index > currentReward ? 
                            FutureAlpha * step : 
                            PastAlpha * step
                );
            });

            title.color = closeButton.color = receiveText.color = description.color =  new Color(1, 1, 1, step);

            background.color = new Color(0, 0, 0, step * 0.85f);
            content.localScale = new Vector3(1.5f - step / 2f, 1.5f - step / 2f, 1);
        }, () => { }));
    }

    public void Close() {
        isActive = false;
        
        var currentReward = PlayerPrefs.GetInt(G.Keys.RewardIndex, -1);
        var received = PlayerPrefs.GetInt(G.Keys.ReceivedPreviousReward, 1) == 1;

        StartCoroutine(Interpolators.Linear(1, 0, 0.25f, step => {
            _rewards.ForEach(reward => {
                reward.DayDescription.color = reward.Rewards.color = new Color(
                    1, 1, 1,
                    reward.Index == currentReward ? 
                        (received ? step * PresentReceivedAlpha : step) : 
                        reward.Index > currentReward ? 
                            FutureAlpha * step : 
                            PastAlpha * step
                );
            });

            title.color = closeButton.color = receiveText.color = description.color = new Color(1, 1, 1, step);

            background.color = new Color(0, 0, 0, step * 0.85f);
            content.localScale = new Vector3(1.5f - step / 2f, 1.5f - step / 2f, 1);
        }, () => {
            gameObject.SetActive(false);
            dialogs.SetActive(false);
        }));
    }

    public void GetReward(TrackSelect selector) {
        var currentReward = PlayerPrefs.GetInt(G.Keys.RewardIndex, -1);

        if (currentReward != 6) {
            if (G.InternalSettings.rewardType[currentReward] == 0) {
                G.Items.Energy += G.InternalSettings.rewardAmount[currentReward];
                selector.UpdateEnergyUI(G.InternalSettings.rewardAmount[currentReward]);
            } else if (G.InternalSettings.rewardType[currentReward] == 1) {
                var beforeKey = G.Items.Key;
                G.Items.Key += G.InternalSettings.rewardAmount[currentReward];
                selector.UpdateKeyUI(beforeKey);
            }
        } else {
            var beforeKey = G.Items.Key;
            G.Items.Energy += 5;
            G.Items.Key += 2;
            selector.UpdateEnergyUI(5);
            selector.UpdateKeyUI(beforeKey);
        }

        PlayerPrefs.SetInt(G.Keys.ReceivedPreviousReward, 1);

        receiveText.text = "수령 완료";
        receiveButton.interactable = false;

        _rewards[currentReward].DayDescription.color =
            _rewards[currentReward].Rewards.color =
                new Color(1, 1, 1, PresentReceivedAlpha);
    }
}