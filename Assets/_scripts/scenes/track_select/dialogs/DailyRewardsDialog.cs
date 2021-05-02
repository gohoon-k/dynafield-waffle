using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class RewardObject {
    public int index;
    public CanvasGroup rewardGroup;
}

public class DailyRewardsDialog : MonoBehaviour {
    public Text receiveText;
    public Button receiveButton;

    public string forceToday;

    public List<GameObject> rewards = new List<GameObject>();

    [HideInInspector] public bool isActive;
    
    private GameObject _dialogs;

    private RectTransform _content;
    private CanvasGroup _group;
    
    private readonly List<RewardObject> _rewards = new List<RewardObject>();

    private bool _init;

    private const float PresentReceivedAlpha = 0.7f;
    private const float PastAlpha = 0.45f;
    private const float FutureAlpha = 0.2f;

    private void Init() {
        _dialogs ??= transform.parent.gameObject;
        _content ??= transform.GetChild(0).GetComponent<RectTransform>();
        _group ??= GetComponent<CanvasGroup>();
        
        var i = 0;
        foreach (var created in rewards.Select(reward => new RewardObject {
            index = i,
            rewardGroup = reward.GetComponent<CanvasGroup>()
        })) {
            _rewards.Add(created);
            i++;
        }

        _init = true;
    }

    public void Open() {
        if (!_init) Init();

        isActive = true;
        
        _group.alpha = 0;
        
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
        
        _content.localScale = new Vector3(1.5f, 1.5f, 1);

        receiveButton.interactable = !received;
        receiveText.text = received ? "수령 완료" : "받기";
        
        _rewards.ForEach(reward => {
            reward.rewardGroup.alpha =
                reward.index == currentReward ? (received ? PresentReceivedAlpha : 1) :
                reward.index > currentReward ? FutureAlpha : PastAlpha;
        });

        StartCoroutine(Interpolators.Linear(0, 1, 0.4f, step => {
            _group.alpha = step;
            
            var localScale = 1.5f - step / 2f;
            _content.localScale = new Vector3(localScale, localScale, 1);
        }, () => { }));
    }

    public void Close() {
        isActive = false;
        
        StartCoroutine(Interpolators.Linear(1, 0, 0.25f, step => {
            _group.alpha = step;
            
            var localScale = 1.5f - step / 2f;
            _content.localScale = new Vector3(localScale, localScale, 1);
        }, () => {
            gameObject.SetActive(false);
            _dialogs.SetActive(false);
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

        _rewards[currentReward].rewardGroup.alpha = PresentReceivedAlpha;
    }
}