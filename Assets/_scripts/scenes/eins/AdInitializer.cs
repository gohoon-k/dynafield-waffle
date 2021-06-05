using System;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using UnityEngine;

public class AdInitializer : MonoBehaviour {
    void Start() {
        MobileAds.SetRequestConfiguration(new RequestConfiguration.Builder().build());
        MobileAds.Initialize(initStatus => { G.AdInitializeStatus = true; });
    }
}