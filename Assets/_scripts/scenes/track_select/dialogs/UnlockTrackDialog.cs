using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnlockTrackDialog : BaseDialog {
    public Button positiveButton;
    public Text positiveText;
    
    public override void Open() {

        positiveText.text = G.Items.Key < 10 ? "yes\n<size=50>not enough keys</size>" : "yes";
        
        positiveButton.interactable = G.Items.Key >= 10;
        
        base.Open();
    }

}
