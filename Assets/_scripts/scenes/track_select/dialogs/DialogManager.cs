using System;
using System.Collections;
using System.Collections.Generic;
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

    public StoreDialog storeDialog;
    public CalibrationDialog calibrationDialog;
    public HowToDialog howToDialog;

    public DailyRewardsDialog dailyRewardsDialog;

    public bool DialogShowing =>
        energyRefillDialog.isActive || startCooldownNowDialog.isActive || trackUnlockDialog.isActive ||
        adDialog.isActive ||
        fakePurchasingDialog.isActive || fakePurchaseSuccessDialog.isActive ||
        storeDialog.isActive || calibrationDialog.isActive || howToDialog.isActive || dailyRewardsDialog.isActive;

    private bool _beforeDialogState = false;

    void Start() {
        if (PlayerPrefs.GetInt(G.Keys.FirstExecution, 0) == 0) {
            StartCoroutine(ShowHowToWithDelay());
        } else if (PlayerPrefs.GetString(G.Keys.CheckedDate, "1990-01-01") != DateTime.Now.ToString("yyyy-MM-dd")) {
            StartCoroutine(ShowDailyRewardWithDelay());
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

    private IEnumerator ShowHowToWithDelay() {
        yield return new WaitForSeconds(1.25f);

        OpenHowToDialogWithDailyReward();
        PlayerPrefs.SetInt(G.Keys.FirstExecution, 1);
    }

    private IEnumerator ShowDailyRewardWithDelay() {
        yield return new WaitForSeconds(1.25f);

        OpenDailyRewardsDialog();
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
        adDialog.Open();
        StartCoroutine(adDialog.gameObject.GetComponent<FakeAdController>().StartCountdown(
            () => { selector.RefillEnergy(); }
        ));
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

    public void OpenDailyRewardsDialog() {
        dialogs.SetActive(true);
        dailyRewardsDialog.Open();
    }

    public void OpenUnlockTrackDialog() {
        dialogs.SetActive(true);
        trackUnlockDialog.Open();
    }
}