using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnlockTrackDialog : BaseDialog {
    public Button positiveButton;
    
    public override void Open() {

        backgroundAlpha = 0.6f;

        if (G.Items.Key < 10) {
            positive.text = "yes\n<size=50>not enough keys</size>";
        } else {
            positive.text = "yes";
        }
        
        positiveButton.interactable = G.Items.Key >= 10;
        
        base.Open();
    }

}
