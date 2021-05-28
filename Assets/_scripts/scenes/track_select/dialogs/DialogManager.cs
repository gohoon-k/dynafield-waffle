using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour {
    public TrackSelect selector;

    public GameObject dialogs;

    public BaseDialog energyRefillDialog;
    public BaseDialog startCooldownNowDialog;
    public BaseDialog trackUnlockDialog;

    public BaseDialog fakePurchasingDialog;
    public BaseDialog fakePurchaseSuccessDialog;
    public BaseDialog adDialog;

    public PlayTypeRewardDialog playTypeRewardDialog;

    public StoreDialog storeDialog;
    public CalibrationDialog calibrationDialog;
    public HowToDialog howToDialog;

    public DailyRewardsDialog dailyRewardsDialog;

    public bool DialogShowing =>
        energyRefillDialog.isActive || startCooldownNowDialog.isActive || trackUnlockDialog.isActive ||
        adDialog.isActive || playTypeRewardDialog.isActive ||
        fakePurchasingDialog.isActive || fakePurchaseSuccessDialog.isActive ||
        storeDialog.isActive || calibrationDialog.isActive || howToDialog.isActive || dailyRewardsDialog.isActive;

    private bool _beforeDialogState = false;

    private bool _initialAction = true;

    void Start() {
        if (PlayerPrefs.GetInt(G.Keys.FirstExecution, 0) == 0) {
            StartCoroutine(ShowHowToWithDelay());
            _initialAction = false;
        }
    }

    private void Update() {
        if (_beforeDialogState != DialogShowing) {
            _beforeDialogState = DialogShowing;

            StartCoroutine(Interpolators.Curve(Interpolators.EaseOutCurve,
                DialogShowing ? 0 : 1,
                DialogShowing ? 1 : 0,
                DialogShowing ? 0.4f : 0.25f, step => {
                    selector.uiElements.backgrounds.main.blur.color = new Color(1, 1, 1, step);
                    selector.uiElements.scalable.main.localScale = new Vector3(
                        1 + step / 7.5f, 1 + step / 7.5f
                    );
                    selector.uiElements.scalable.mainGroup.alpha = 1 - step;
                }, () => { }));
        }

        if (_initialAction) {
            _initialAction = false;
            if (PlayerPrefs.GetString(G.Keys.CheckedDate, "1990-01-01") != DateTime.Now.ToString("yyyy-MM-dd")) {
                var rewards = CheckPlayTypeReward(PlayerPrefs.GetInt(G.Keys.FormatKey(G.Keys.PlayType), 0));
                if (rewards.valid) {
                    StartCoroutine(ShowDailyRewardWithDelay(() => {
                        StartCoroutine(OpenPlayTypeRewardWithDelay(rewards)); 
                    }));
                } else {
                    StartCoroutine(ShowDailyRewardWithDelay());
                }
            } else {
                var rewards = CheckPlayTypeReward(PlayerPrefs.GetInt(G.Keys.FormatKey(G.Keys.PlayType), 0));
                StartCoroutine(OpenPlayTypeRewardWithDelay(rewards));
            }
        }
    }

    public void CloseAllDialogs() {
        if (energyRefillDialog.isActive) energyRefillDialog.Close(true);
        if (startCooldownNowDialog.isActive) startCooldownNowDialog.Close(true);
        if (trackUnlockDialog.isActive) trackUnlockDialog.Close(true);
        if (storeDialog.isActive) storeDialog.Close(true);
        if (calibrationDialog.isActive) calibrationDialog.Close(true);
        if (howToDialog.isActive) howToDialog.Close(true);
        if (dailyRewardsDialog.isActive) dailyRewardsDialog.Close();
    }
    
    private PlayTypeReward CheckPlayTypeReward(int playType) {
        if (playType < 1 || playType > 3) 
            return new PlayTypeReward {
                valid = false,
                types = null,
                rewards = null
            };

        var types = new List<int>();
        var rewards = new List<int>();
        for (var i = 1; i <= playType; i++) {
            if (PlayerPrefs.GetInt(G.Keys.FormatPlayTypeRewards(i), 0) == 0) {
                types.Add(i);
                rewards.Add(G.InternalSettings.PlayTypeRewards[G.PlaySettings.Difficulty][i]);
            }
        }
        
        return new PlayTypeReward {
            valid = types.Count > 0 && rewards.Count > 0,
            types = types,
            rewards = rewards
        };
    }

    private IEnumerator ShowHowToWithDelay() {
        yield return new WaitForSeconds(1.25f);

        OpenHowToDialogWithDailyReward();
        PlayerPrefs.SetInt(G.Keys.FirstExecution, 1);
    }

    private IEnumerator ShowDailyRewardWithDelay(Action closeAction = null) {
        yield return new WaitForSeconds(1.25f);

        OpenDailyRewardsDialog(closeAction);
    }

    private IEnumerator OpenPlayTypeRewardWithDelay(PlayTypeReward reward) {
        yield return new WaitForSeconds(0.35f);
        
        OpenPlayTypeRewardDialog(reward.rewards, reward.types);
    }

    public void EnergyButtonAction() {
        if (G.Items.Energy == 0) {
            OpenEnergyRefillDialog();
        } else {
            OpenStartCooldownNowDialog();
        }
    }

    public void OpenEnergyRefillDialog() {
        if (storeDialog.isOpen) storeDialog.Close(false);
        dialogs.SetActive(true);
        energyRefillDialog.Open();
    }

    private void OpenStartCooldownNowDialog() {
        dialogs.SetActive(true);
        startCooldownNowDialog.Open();
    }

    public void OpenAdDialog() {
        if (energyRefillDialog.isOpen) energyRefillDialog.Close(false);
        adDialog.message.text = "광고가 진행 중입니다...";
        adDialog.Open();
        selector.MuteAudio(true);
        StartCoroutine(selector.StartEnergyRefillAd());
    }

    public void OpenStoreDialog() {
        dialogs.SetActive(true);
        storeDialog.Open();
    }

    public void OpenCalibrationDialog() {
        dialogs.SetActive(true);
        calibrationDialog.Open();
    }

    public void OpenHowToDialog() {
        dialogs.SetActive(true);
        howToDialog.Open();
    }

    private void OpenHowToDialogWithDailyReward() {
        dialogs.SetActive(true);
        howToDialog.Open(() => {
            dialogs.SetActive(true);
            dailyRewardsDialog.Open();
        });
    }

    private void OpenDailyRewardsDialog(Action closeAction = null) {
        dialogs.SetActive(true);
        dailyRewardsDialog.Open(closeAction);
    }

    public void OpenDailyRewardsDialogB() {
        OpenDailyRewardsDialog();
    }

    public void OpenUnlockTrackDialog() {
        dialogs.SetActive(true);
        trackUnlockDialog.Open();
    }

    private void OpenPlayTypeRewardDialog(List<int> rewards, List<int> types) {
        dialogs.SetActive(true);

        var typeString = string.Join(" / ", types.Select(type => G.InternalSettings.PlayTypeNames[type]));
        var amountString = string.Join(" + ", rewards);
        var amount = rewards.Sum();
        var difficultyName = G.PlaySettings.Difficulty == 0 ? "EASY" : "HARD";
        playTypeRewardDialog.SetTitle(
            $"<size=150>{G.Tracks[G.PlaySettings.TrackId].title}</size>\n" +
            $"<size=90>{difficultyName} {G.Tracks[G.PlaySettings.TrackId].difficulty[G.PlaySettings.Difficulty]}</size>\n"
        );
        playTypeRewardDialog.message.text =
            $"처음으로 {typeString}를 달성하여 다음 보상을 지급합니다!";
        playTypeRewardDialog.SetReward(
            $"<size=130>{amountString}</size>  <size=90>key(s)</size>"
        );
        playTypeRewardDialog.AddPositiveCallback(() => {
            G.Items.Key += amount;
            selector.UpdateKeyUI(amount);

            types.ForEach(type => PlayerPrefs.SetInt(G.Keys.FormatPlayTypeRewards(type), 1));
            PlayerPrefs.Save();

            playTypeRewardDialog.Close(true);
            playTypeRewardDialog.RemoveAllPositiveCallbacks();
        });
        playTypeRewardDialog.Open();
    }

    [Serializable]
    class PlayTypeReward {
        public bool valid;
        public List<int> rewards;
        public List<int> types;
    }
}