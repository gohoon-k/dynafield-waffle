using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreManager : MonoBehaviour {

    public TrackSelect selector;
    
    public StoreDialog storeDialog;
    
    public BaseDialog fakePurchasingDialog;
    public BaseDialog fakeSuccessDialog;

    public void PurchaseMaxEnergy() {
        StartCoroutine(ExtendMaxEnergy());
    }

    public void PurchaseKeys(int count) {
        StartCoroutine(ChargeKey(count));
    }
    
    private IEnumerator ExtendMaxEnergy() {
        storeDialog.Close(false);
        fakePurchasingDialog.Open();
        yield return new WaitForSeconds(2f);
        fakePurchasingDialog.Close(false);
        fakeSuccessDialog.Open();
        yield return new WaitForSeconds(1f);
        fakeSuccessDialog.Close(true);
        
        G.Items.MaxEnergyStep++;
        G.Items.CoolDown = -1;
        G.Items.Energy = G.Items.MaxEnergy[G.Items.MaxEnergyStep];

        selector.UpdateEnergyUI(G.Items.MaxEnergy[G.Items.MaxEnergyStep]);
    }

    private IEnumerator ChargeKey(int counts) {
        storeDialog.Close(false);
        fakePurchasingDialog.Open();
        yield return new WaitForSeconds(2f);
        fakePurchasingDialog.Close(false);
        fakeSuccessDialog.Open();
        yield return new WaitForSeconds(1f);
        fakeSuccessDialog.Close(true);

        var before = G.Items.Key;
        G.Items.Key += counts;
        
        selector.UpdateKeyUI(before);
    }
    
}
