using System;
using UnityEngine.UI;

public class PlayTypeRewardDialog : BaseDialog {
    
    public Text title;
    public Text reward;
    
    public Button positive;

    public void SetTitle(string text) {
        title.text = text;
    }

    public void SetReward(string text) {
        reward.text = text;
    }

    public void AddPositiveCallback(Action action) {
        positive.onClick.AddListener(() => action());
    }

    public void RemoveAllPositiveCallbacks() {
        positive.onClick.RemoveAllListeners();
    }
    
}