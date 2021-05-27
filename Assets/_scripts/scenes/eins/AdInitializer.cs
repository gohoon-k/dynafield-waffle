using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using UnityEngine;

public class AdInitializer : MonoBehaviour
{

    void Start()
    {
        MobileAds.Initialize(initStatus => { G.AdInitializeStatus = true; });
    }

}
