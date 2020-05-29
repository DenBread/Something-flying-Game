using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Appodeal.Editor.AppodealManager.AppodealManagerData
{
    [SuppressMessage("ReSharper", "UnusedMemberInSuper.Global")]
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public interface IAppodealSdkManager
    {
        void Reset();
        void SdkHeaders();
        void AdapterInfoRow(AppodealNetworksData.AdapterInfo adapter, GUIStyle guiStyle, bool isCore);
        void UpdateInternalConfig(string previous, string update, AppodealNetworksData.ActionUpdate action, AppodealNetworksData.AdapterInfo.SdkPlatform sdkPlatform, string path);
        void PluginInfoRow(string package, string version, GUIStyle style, AppodealNetworksData.ActionUpdate action, bool isPlugin);
        IEnumerator GetAppodealSdkVersions(bool isCoreUpdate);

        void Compare(AppodealNetworksData.AdapterInfo.SdkPlatform sdkPlatform, Dictionary<string, AppodealNetworksData.AdapterInfo> intDict,
            Dictionary<string, AppodealNetworksData.AdapterInfo> outDict,
            Dictionary<string, AppodealNetworksData.AdapterInfo> current);

        void CompareCore(AppodealNetworksData.AdapterInfo.SdkPlatform sdkPlatform, AppodealNetworksData.AdapterInfo intCore, AppodealNetworksData.AdapterInfo outCore, AppodealNetworksData.AdapterInfo curCore);

        void SetAdaptersInfo(AppodealNetworksData.AdapterInfo[] adapterInfos, AppodealNetworksData.AdapterInfo.SdkPlatform sdkPlatform);

        AppodealNetworksData.AdapterInfo SetAdapterInformation(AppodealNetworksData.AdapterInfo.SdkPlatform sdkPlatform, string adapterName, string adapterPrettyName,
            string adapterVersion, AppodealNetworksData.ActionUpdate action, bool isCore);

        AppodealNetworksData.AdapterInfo SetAdapterInformation(AppodealNetworksData.AdapterInfo.SdkPlatform sdkPlatform, string adapterName, string adapterPrettyName,
            string adapterUpdate,
            string adapterVersion, string configAdapter, AppodealNetworksData.ActionUpdate action, bool isCore);

        void SetPluginInfo(AppodealNetworksData.PluginInfo pInfo);

        IEnumerator DownloadUnityPlugin();

        void ReadAndroidDependencies();

        void ReadIosDependencies();
        void UpdateWindow();

        void ShowUpdateDialog(string internalIntegration, string integration, AppodealNetworksData.ActionUpdate action, AppodealNetworksData.AdapterInfo.SdkPlatform sdkPlatform);

        void UpdateProgress(float updateStep);
    }
}