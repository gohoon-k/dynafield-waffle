using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogManager : MonoBehaviour {
    public TrackSelect selector;

    public GameObject dialogs;

    public BaseDialog energyRefillDialog;
    public BaseDialog startCooldownNowDialog;

    public BaseDialog adDialog;

    public StoreDialog storeDialog;
    public CalibrationDialog calibrationDialog;
    public HowToDialog howToDialog;

    public DailyRewardsDialog dailyRewardsDialog;

    void Start() {
        if (PlayerPrefs.GetInt(G.Keys.FirstExecution, 0) == 0) {
            StartCoroutine(ShowHowToWithDelay());
        } else if (PlayerPrefs.GetString(G.Keys.CheckedDate, "1990-01-01") != DateTime.Now.ToString("yyyy-MM-dd")) {
            StartCoroutine(ShowDailyRewardWithDelay());
        }
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
}