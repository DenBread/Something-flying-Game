using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AppodealAds.Unity.Api;
using AppodealAds.Unity.Common;

public class TestAds : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Appodeal.initialize("f414c9e9cc772d4115e692b587f6c256b8a147025a8dc025", Appodeal.INTERSTITIAL | Appodeal.BANNER_VIEW, true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartAds()
    {
        Appodeal.show(Appodeal.BANNER_VIEW);
    }
}
