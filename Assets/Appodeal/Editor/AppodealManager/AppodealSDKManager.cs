#if UNITY_2018_1_OR_NEWER
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Appodeal.Editor.AppodealManager.AppodealManagerData;
using marijnz.EditorCoroutines;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
#pragma warning disable 612
using static marijnz.EditorCoroutines.EditorCoroutines;

#pragma warning disable 618
#pragma warning restore 612


namespace Appodeal.Editor.AppodealManager
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "NotAccessedField.Local")]
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    [SuppressMessage("ReSharper", "CollectionNeverQueried.Local")]
    public class AppodealSDKManager : EditorWindow, IAppodealSdkManager
    {
        #region Constants

        private const string iOS = "ios";
        private const string Android = "android";
        private const string AppodealSdkManager = "Appodeal SDK Manager";
        private const string AppodealUnityPlugin = "Appodeal Unity Plugin";
        private const string AppodealIosAdapters = "Appodeal iOS Adapters";
        private const string AppodealIosCore = "Appodeal Core iOS ";
        private const string AppodealAndroidCore = "Appodeal Core Android ";
        private const string AppodealAndroidAdapters = "Appodeal Android Adapters";
        private const string AppodealDependencies = "Assets/Appodeal/Editor/AppodealAdapterDependencies.xml";
        private const string AppodealCoreDependencies = "Assets/Appodeal/Editor/AppodealCoreDependencies.xml";
        private const string PackageHeader = "Package";
        private const string VersionHeader = "Version";
        private const string ActionHeader = "Action";
        private const string DownloadDir = "Assets/Appodeal";
        private const string BoxStyle = "box";
        private const string UpdateField = "Update";
        private const string ImportField = "Import";
        private const string PluginStageUrl = "https://mw-backend.appodeal.com/v1/unity/last";
        private const string AdaptersIosUrl = "https://mw-backend.appodeal.com/v1/adapters/ios/";
        private const string AdaptersAndroidUrl = "https://mw-backend.appodeal.com/v1/adapters/android/";
        private const string AdapterCoreIosUrl = "https://mw-backend.appodeal.com/v1/adapters/ios/";
        private const string AdapterCoreAndroidUrl = "https://mw-backend.appodeal.com/v1/adapters/android/";
        private const string LogDownloadAdapters = "Downloading adapters versions ...";
        public const string ReplaceDependencyValue = "com.appodeal.ads.sdk.networks:";
        public const string ReplaceDependencyCoreValue = "com.appodeal.ads.sdk:";
        public const string FilterStableVersion = "stable";
        public const string FilterBetaVersion = "beta";

        #endregion

        #region GUIStyles

        private GUIStyle headerStyle;
        private GUIStyle labelStyle;
        private GUIStyle labelStyleArea;
        private GUIStyle labelStyleLink;
        private GUIStyle packageInfoStyle;
        private readonly GUILayoutOption btnFieldWidth = GUILayout.Width(60);

        #endregion

        #region Variables

        private EditorCoroutine coroutine;
        private AppodealNetworksData.PluginInfo pluginInfo;
        private AppodealNetworksData.AdapterInfo appodealCoreIos;
        private AppodealNetworksData.AdapterInfo appodealCoreAndroid;
        private AppodealNetworksData.AdapterInfo internalCoreIos;
        private AppodealNetworksData.AdapterInfo internalCoreAndroid;
        private AppodealNetworksData.AdapterInfo currentCoreIos;
        private AppodealNetworksData.AdapterInfo currentCoreAndroid;
        private float progress;
        private WebClient downloader;
        private Vector2 scrollPosition;
        private float loading;
        private string stableAndroidVersion;
        private string stableiOSVersion;

        #endregion

        #region Dictionaries

        private Dictionary<string, AppodealNetworksData.AdapterInfo> internaliOSAdapters =
            new Dictionary<string, AppodealNetworksData.AdapterInfo>();

        private Dictionary<string, AppodealNetworksData.AdapterInfo> appodealiOSAdapters =
            new Dictionary<string, AppodealNetworksData.AdapterInfo>();

        private Dictionary<string, AppodealNetworksData.AdapterInfo> currentiOSAdapters =
            new Dictionary<string, AppodealNetworksData.AdapterInfo>();

        private Dictionary<string, AppodealNetworksData.AdapterInfo> internalAndroidAdapters =
            new Dictionary<string, AppodealNetworksData.AdapterInfo>();

        private Dictionary<string, AppodealNetworksData.AdapterInfo> appodealAndroidAdapters =
            new Dictionary<string, AppodealNetworksData.AdapterInfo>();

        private Dictionary<string, AppodealNetworksData.AdapterInfo>
            currentAndroidAdapters = new Dictionary<string, AppodealNetworksData.AdapterInfo>();

        #endregion

        public static void ShowSdkManager()
        {
            GetWindow(typeof(AppodealSDKManager), true, AppodealSdkManager);
        }

        private void Awake()
        {
            labelStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };
            labelStyleArea = new GUIStyle(EditorStyles.label)
            {
                wordWrap = true
            };
            labelStyleLink = new GUIStyle(EditorStyles.label)
            {
                normal = {textColor = Color.blue},
                active = {textColor = Color.white},
            };
            headerStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                fixedHeight = 18
            };
            packageInfoStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Normal,
                fixedHeight = 18
            };
            ((IAppodealSdkManager) this).Reset();
        }

        private void OnEnable()
        {
            loading = 0f;
            coroutine = this.StartCoroutine(((IAppodealSdkManager) this).GetAppodealSdkVersions(false));
        }

        void IAppodealSdkManager.Reset()
        {
            if (downloader != null)
            {
                downloader.CancelAsync();
                return;
            }

            if (coroutine != null)
                this.StopCoroutine(coroutine.routine);
            if (progress > 0)
                EditorUtility.ClearProgressBar();
            coroutine = null;
            downloader = null;

            internaliOSAdapters = new Dictionary<string, AppodealNetworksData.AdapterInfo>();
            appodealiOSAdapters = new Dictionary<string, AppodealNetworksData.AdapterInfo>();
            currentiOSAdapters = new Dictionary<string, AppodealNetworksData.AdapterInfo>();

            internalAndroidAdapters = new Dictionary<string, AppodealNetworksData.AdapterInfo>();
            appodealAndroidAdapters = new Dictionary<string, AppodealNetworksData.AdapterInfo>();
            currentAndroidAdapters = new Dictionary<string, AppodealNetworksData.AdapterInfo>();
            loading = 0f;
            progress = 0f;
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition,
                false,
                false);

            GUILayout.BeginVertical();
            EditorGUI.ProgressBar(new Rect(5, 5, 
                position.width - 25, 20), loading / 100, $"Loading {loading}%");
            GUILayout.Space(15);

            if (loading > 10f)
            {
                GUILayout.Space(15);
                EditorGUILayout.LabelField(AppodealUnityPlugin, labelStyle, GUILayout.Height(20));
                if (pluginInfo != null)
                {
                    using (new EditorGUILayout.VerticalScope(BoxStyle))
                    {
                        ((IAppodealSdkManager) this).SdkHeaders();
                        ((IAppodealSdkManager) this).PluginInfoRow(pluginInfo.name, pluginInfo.version,
                            packageInfoStyle, pluginInfo.action, true);
                    }
                }

                GUILayout.Space(5);
                EditorGUILayout.LabelField(AppodealIosCore, labelStyle, GUILayout.Height(20));


                if (currentCoreIos != null)
                {
                    using (new EditorGUILayout.VerticalScope(BoxStyle))
                    {
                        ((IAppodealSdkManager) this).SdkHeaders();
                        ((IAppodealSdkManager) this).AdapterInfoRow(currentCoreIos, packageInfoStyle, true);
                    }
                }

                GUILayout.Space(5);
                EditorGUILayout.LabelField(AppodealAndroidCore, labelStyle, GUILayout.Height(20));
                if (currentCoreAndroid != null)
                {
                    using (new EditorGUILayout.VerticalScope(BoxStyle))
                    {
                        ((IAppodealSdkManager) this).SdkHeaders();
                        ((IAppodealSdkManager) this).AdapterInfoRow(currentCoreAndroid, packageInfoStyle, true);
                    }
                }
            }

            if (loading >= 100f)
            {
                GUILayout.Space(5);
                EditorGUILayout.LabelField(AppodealIosAdapters, labelStyle, GUILayout.Height(20));
                if (currentiOSAdapters.Count > 0)
                {
                    using (new EditorGUILayout.VerticalScope(BoxStyle))
                    {
                        ((IAppodealSdkManager) this).SdkHeaders();
                        foreach (var appodealSdkAdapterInfo in currentiOSAdapters.Values)
                        {
                            ((IAppodealSdkManager) this).AdapterInfoRow(appodealSdkAdapterInfo, packageInfoStyle,
                                false);
                        }
                    }
                }

                GUILayout.Space(5);
                EditorGUILayout.LabelField(AppodealAndroidAdapters, labelStyle, GUILayout.Height(20));
                if (currentAndroidAdapters.Count > 0)
                {
                    using (new EditorGUILayout.VerticalScope(BoxStyle))
                    {
                        ((IAppodealSdkManager) this).SdkHeaders();
                        foreach (var appodealSdkAdapterInfo in currentAndroidAdapters.Values)
                        {
                            ((IAppodealSdkManager) this).AdapterInfoRow(appodealSdkAdapterInfo, packageInfoStyle,
                                false);
                        }
                    }
                }
            }

            GUILayout.Space(5);
            GUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        void IAppodealSdkManager.SdkHeaders()
        {
            GUILayout.Space(5);
            using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandWidth(false)))
            {
                GUILayout.Space(10);
                EditorGUILayout.LabelField(PackageHeader, headerStyle);
                GUILayout.Button(VersionHeader, headerStyle);
                GUILayout.Space(14);
                GUILayout.Button(ActionHeader, headerStyle, btnFieldWidth);
                GUILayout.Button(string.Empty, headerStyle, GUILayout.Width(1));
                GUILayout.Space(1);
            }

            GUILayout.Space(5);
        }

        void IAppodealSdkManager.AdapterInfoRow(AppodealNetworksData.AdapterInfo adapter, GUIStyle style, bool isCore)
        {
            using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandWidth(false)))
            {
                GUILayout.Space(10);
                EditorGUILayout.LabelField(adapter.pretty_name, style);
                GUILayout.Button(adapter.version.adapter, style);
                GUILayout.Space(6);
                GUILayout.Button(string.Empty, style, GUILayout.Width(1));
                switch (adapter.action)
                {
                    case AppodealNetworksData.ActionUpdate.Update:
                        if (GUILayout.Button(new GUIContent {text = UpdateField}, btnFieldWidth))
                        {
                            if (isCore)
                            {
                                switch (adapter.sdkPlatform)
                                {
                                    case AppodealNetworksData.AdapterInfo.SdkPlatform.Android:
                                        ((IAppodealSdkManager) this).ShowUpdateDialog(adapter.internalIntegration.code,
                                            adapter.integration.code,
                                            adapter.action, adapter.sdkPlatform);
                                        break;
                                    case AppodealNetworksData.AdapterInfo.SdkPlatform.iOS:
                                        ((IAppodealSdkManager) this).ShowUpdateDialog(adapter.internalIntegration.code,
                                            adapter.integration.code,
                                            adapter.action, adapter.sdkPlatform);
                                        break;
                                }
                            }
                            else
                            {
                                ((IAppodealSdkManager) this).UpdateInternalConfig(adapter.internalIntegration.code,
                                    adapter.integration.code,
                                    adapter.action, adapter.sdkPlatform, AppodealDependencies);
                                ((IAppodealSdkManager) this).UpdateWindow();
                            }
                        }

                        break;
                    case AppodealNetworksData.ActionUpdate.NoAction:
                        GUI.enabled = false;
                        GUILayout.Button(new GUIContent {text = UpdateField}, btnFieldWidth);
                        GUI.enabled = true;
                        break;
                    case AppodealNetworksData.ActionUpdate.Import:
                        if (GUILayout.Button(new GUIContent {text = ImportField}, btnFieldWidth))
                        {
                            ((IAppodealSdkManager) this).UpdateInternalConfig(string.Empty,
                                adapter.integration.code,
                                AppodealNetworksData.ActionUpdate.Import,
                                adapter.sdkPlatform, AppodealDependencies);
                            ((IAppodealSdkManager) this).UpdateWindow();
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                GUILayout.Space(1);
                GUILayout.Button(string.Empty, headerStyle, GUILayout.Width(8));
            }

            GUILayout.Space(4);
        }

        void IAppodealSdkManager.UpdateInternalConfig(string previous, string update,
            AppodealNetworksData.ActionUpdate action,
            AppodealNetworksData.AdapterInfo.SdkPlatform sdkPlatform, string path)
        {
            switch (action)
            {
                case AppodealNetworksData.ActionUpdate.Update:
                    File.WriteAllText(path,
                        Regex.Replace(File.ReadAllText(path),
                            previous,
                            update));
                    break;
                case AppodealNetworksData.ActionUpdate.Import:
                    switch (sdkPlatform)
                    {
                        case AppodealNetworksData.AdapterInfo.SdkPlatform.Android:
                            File.WriteAllText(path,
                                Regex.Replace(File.ReadAllText(path),
                                    "<androidPackages>",
                                    "        <androidPackages>" + "\n" + update));

                            break;
                        case AppodealNetworksData.AdapterInfo.SdkPlatform.iOS:
                            File.WriteAllText(path,
                                Regex.Replace(File.ReadAllText(path),
                                    "<iosPods>",
                                    "        <iosPods>" + "\n" + update));

                            break;
                    }

                    break;
                case AppodealNetworksData.ActionUpdate.NoAction:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }

        void IAppodealSdkManager.PluginInfoRow(string package, string version, GUIStyle style,
            AppodealNetworksData.ActionUpdate action,
            bool isPlugin)
        {
            using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandWidth(false)))
            {
                GUILayout.Space(10);
                EditorGUILayout.LabelField(package, style);
                GUILayout.Button(version, style);
                GUILayout.Space(6);
                GUILayout.Button(String.Empty, style, GUILayout.Width(1));
                switch (action)
                {
                    case AppodealNetworksData.ActionUpdate.Update:
                        if (GUILayout.Button(new GUIContent {text = UpdateField}, btnFieldWidth))
                        {
                            if (isPlugin)
                            {
                                if (!string.IsNullOrEmpty(pluginInfo.source))
                                {
                                    this.StartCoroutine(((IAppodealSdkManager) this).DownloadUnityPlugin());
                                }
                            }
                        }

                        break;
                    case AppodealNetworksData.ActionUpdate.NoAction:
                        GUI.enabled = false;
                        GUILayout.Button(new GUIContent {text = UpdateField}, btnFieldWidth);
                        GUI.enabled = true;
                        break;
                }

                GUILayout.Space(1);
                GUILayout.Button(string.Empty, headerStyle, GUILayout.Width(8));
            }

            GUILayout.Space(4);
        }

        IEnumerator IAppodealSdkManager.GetAppodealSdkVersions(bool isCoreUpdate)
        {
            yield return null;

            progress = 0.01f;

            if (isCoreUpdate) yield break;
            Debug.Log("Downloading plugin versions ...");

            ((IAppodealSdkManager) this).ReadIosDependencies();
            ((IAppodealSdkManager) this).ReadAndroidDependencies();
            
            var sdkVersionsiOS = new List<AppodealNetworksData.AppodealSDKVersion>();
            var sdkVersionsAndroid = new List<AppodealNetworksData.AppodealSDKVersion>();
            var AppodealSDKVersionsRequest = UnityWebRequest.Get("https://mw-backend.appodeal.com/v1/sdk");
            yield return AppodealSDKVersionsRequest.Send();

            if (AppodealSDKVersionsRequest.isError)
            {
                Debug.LogError(AppodealSDKVersionsRequest.error);
            }
            else
            {
                var appodealSdkVersions =
                    JsonHelper.FromJson<AppodealNetworksData.AppodealSDKVersion>(
                        JsonHelper.fixJson(AppodealSDKVersionsRequest.downloadHandler.text));
                
                 var filter = AppodealAds.Unity.Api.Appodeal.APPODEAL_PLUGIN_VERSION.Contains("-Beta")
                     ? FilterBetaVersion : FilterStableVersion;

                foreach (var appodealSdkVersion in appodealSdkVersions)
                {
                    if (string.IsNullOrEmpty(appodealSdkVersion.build_type)) continue;
                    if (!appodealSdkVersion.build_type.Equals(filter)) continue;
                    if (appodealSdkVersion.platform.Equals(iOS))
                    {
                        sdkVersionsiOS.Add(appodealSdkVersion);
                    }
                    else
                    {
                        sdkVersionsAndroid.Add(appodealSdkVersion);
                    }
                }

                stableAndroidVersion = sdkVersionsAndroid.LastOrDefault()?.version;
                stableiOSVersion = sdkVersionsiOS.LastOrDefault()?.version;
                
                Debug.Log($"Latest Android SDK version - {stableAndroidVersion}");
                Debug.Log($"Latest iOS SDK version - {stableiOSVersion}");
            }

            var pluginInformation = UnityWebRequest.Get(PluginStageUrl);
            yield return pluginInformation.Send();

            if (pluginInformation.isError)
            {
                Debug.LogError(pluginInformation.error);
            }
            else
            {
                if (!string.IsNullOrEmpty(pluginInformation.downloadHandler.text))
                {
                    pluginInfo = (AppodealNetworksData.PluginInfo) JsonUtility.FromJson(
                        pluginInformation.downloadHandler.text,
                        typeof(AppodealNetworksData.PluginInfo));

                    if (pluginInfo != null)
                    {
                        ((IAppodealSdkManager) this).UpdateProgress(25f);
                        ((IAppodealSdkManager) this).SetPluginInfo(pluginInfo);
                    }
                }
                else
                {
                    Debug.LogError("Unable to retrieve SDK version");
                }
            }

            Debug.Log(LogDownloadAdapters);
            Debug.Log(AdapterCoreIosUrl + stableiOSVersion);
            var requestiOSCore = UnityWebRequest.Get(AdapterCoreIosUrl + stableiOSVersion);
            yield return requestiOSCore.Send();
            if (requestiOSCore.isError)
            {
                Debug.LogError(pluginInformation.error);
            }
            else
            {
                if (!string.IsNullOrEmpty(requestiOSCore.downloadHandler.text))
                {
                    appodealCoreIos = ((IAppodealSdkManager) this).SetAdapterInformation(
                        AppodealNetworksData.AdapterInfo.SdkPlatform.iOS,
                        "Appodeal",
                        "Appodeal",
                        stableiOSVersion, AppodealNetworksData.ActionUpdate.Update,
                        true);
                }
            }

            Debug.Log(AdapterCoreAndroidUrl + stableAndroidVersion);
            var requestAndroidCore = UnityWebRequest.Get(AdapterCoreAndroidUrl + stableAndroidVersion);
            yield return requestAndroidCore.Send();
            if (requestAndroidCore.isError)
            {
                Debug.LogError(requestAndroidCore.error);
            }
            else
            {
                if (!string.IsNullOrEmpty(requestAndroidCore.downloadHandler.text))
                {
                    appodealCoreAndroid = ((IAppodealSdkManager) this).SetAdapterInformation(
                        AppodealNetworksData.AdapterInfo.SdkPlatform.Android,
                        "core",
                        "Appodeal",
                        stableAndroidVersion,
                        AppodealNetworksData.ActionUpdate.Update,
                        true);
                }
            }

            if (appodealCoreIos != null && !string.IsNullOrEmpty(appodealCoreIos.version.adapter))
            {
                var requestiOSAdapters = UnityWebRequest.Get(AdaptersIosUrl + stableiOSVersion);
                yield return requestiOSAdapters.Send();
                if (requestiOSAdapters.isError)
                {
                    Debug.LogError(requestAndroidCore.error);
                }
                else
                {
                    if (!string.IsNullOrEmpty(requestiOSAdapters.downloadHandler.text))
                    {
                        if (JsonHelper.FromJson<AppodealNetworksData.AdapterInfo>(
                            JsonHelper.fixJson(requestiOSAdapters.downloadHandler.text)) != null)
                        {
                            appodealiOSAdapters.Clear();
                            ((IAppodealSdkManager) this).SetAdaptersInfo
                            (JsonHelper.FromJson<AppodealNetworksData.AdapterInfo>(
                                    JsonHelper.fixJson(requestiOSAdapters.downloadHandler.text)),
                                AppodealNetworksData.AdapterInfo.SdkPlatform.iOS);
                        }
                    }
                }
            }

            if (appodealCoreAndroid != null && !string.IsNullOrEmpty(appodealCoreAndroid.version.adapter))
            {
                var requestAndroidAdapters =
                    UnityWebRequest.Get(AdaptersAndroidUrl + stableAndroidVersion);
                yield return requestAndroidAdapters.Send();
                if (requestAndroidAdapters.isError)
                {
                    Debug.LogError(requestAndroidCore.error);
                }
                else
                {
                    if (!string.IsNullOrEmpty(requestAndroidAdapters.downloadHandler.text))
                    {
                        if (
                            JsonHelper.FromJson<AppodealNetworksData.AdapterInfo>(
                                JsonHelper.fixJson(requestAndroidAdapters.downloadHandler.text)) != null)
                        {
                            appodealAndroidAdapters.Clear();
                            ((IAppodealSdkManager) this).SetAdaptersInfo(
                                JsonHelper.FromJson<AppodealNetworksData.AdapterInfo>(
                                    JsonHelper.fixJson(requestAndroidAdapters.downloadHandler.text)),
                                AppodealNetworksData.AdapterInfo.SdkPlatform.Android);
                        }
                    }
                }
            }

            ((IAppodealSdkManager) this).UpdateProgress(25f);

            coroutine = null;
        }

        void IAppodealSdkManager.ShowUpdateDialog(string internalIntegration,
            string integration,
            AppodealNetworksData.ActionUpdate action,
            AppodealNetworksData.AdapterInfo.SdkPlatform sdkPlatform)
        {
            var option = EditorUtility.DisplayDialog("Unsaved Changes",
                "If you will update core, all adapters this platform will be updated automatically. " +
                "Do you want to update core?",
                "Ok",
                "Cancel");

            if (!option) return;
            switch (sdkPlatform)
            {
                case AppodealNetworksData.AdapterInfo.SdkPlatform.iOS:
                    ((IAppodealSdkManager) this).UpdateInternalConfig(internalIntegration,
                        integration,
                        action, sdkPlatform, AppodealCoreDependencies);

                    foreach (var key in appodealiOSAdapters.Keys.Where(key => internaliOSAdapters.ContainsKey(key)))
                    {
                        if (appodealiOSAdapters.TryGetValue(key, out var outAdapterInfo) &&
                            internaliOSAdapters.TryGetValue(key, out var intAdapterInfo))
                        {
                            ((IAppodealSdkManager) this).UpdateInternalConfig(intAdapterInfo.integration.code,
                                outAdapterInfo.integration.code,
                                AppodealNetworksData.ActionUpdate.Update, outAdapterInfo.sdkPlatform,
                                AppodealDependencies);
                        }
                    }

                    ((IAppodealSdkManager) this).UpdateWindow();
                    break;
                case AppodealNetworksData.AdapterInfo.SdkPlatform.Android:
                    ((IAppodealSdkManager) this).UpdateInternalConfig(internalIntegration,
                        integration,
                        action, sdkPlatform, AppodealCoreDependencies);

                    foreach (var key in appodealAndroidAdapters.Keys.Where(key =>
                        internalAndroidAdapters.ContainsKey(key)))
                    {
                        if (appodealAndroidAdapters.TryGetValue(key, out var outAdapterInfo) &&
                            internalAndroidAdapters.TryGetValue(key, out var intAdapterInfo))
                        {
                            ((IAppodealSdkManager) this).UpdateInternalConfig(intAdapterInfo.integration.code,
                                outAdapterInfo.integration.code,
                                AppodealNetworksData.ActionUpdate.Update, sdkPlatform, AppodealDependencies);
                        }
                    }

                    ((IAppodealSdkManager) this).UpdateWindow();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sdkPlatform), sdkPlatform, null);
            }
        }

        void IAppodealSdkManager.CompareCore(AppodealNetworksData.AdapterInfo.SdkPlatform sdkPlatform,
            AppodealNetworksData.AdapterInfo intCore,
            AppodealNetworksData.AdapterInfo outCore,
            AppodealNetworksData.AdapterInfo curCore)
        {
            switch (sdkPlatform)
            {
                case AppodealNetworksData.AdapterInfo.SdkPlatform.Android:
                    switch (AppodealSDKManagerUtils.CompareVersion(intCore.version.adapter, outCore.version.adapter))
                    {
                        case 0:
                            currentCoreAndroid = intCore;
                            break;
                        case 1:
                            currentCoreAndroid = intCore;
                            break;
                        case -1:
                            currentCoreAndroid = ((IAppodealSdkManager) this).SetAdapterInformation(sdkPlatform,
                                outCore.name,
                                outCore.pretty_name,
                                intCore.version.adapter, outCore.version.adapter,
                                intCore.version.adapter,
                                AppodealNetworksData.ActionUpdate.Update, true);
                            break;
                    }

                    break;
                case AppodealNetworksData.AdapterInfo.SdkPlatform.iOS:
                    switch (AppodealSDKManagerUtils.CompareVersion(intCore.version.adapter, outCore.version.adapter))
                    {
                        case 0:
                            currentCoreIos = intCore;
                            break;
                        case 1:
                            currentCoreIos = intCore;
                            break;
                        case -1:
                            currentCoreIos = ((IAppodealSdkManager) this).SetAdapterInformation(sdkPlatform,
                                outCore.name,
                                outCore.pretty_name,
                                intCore.version.adapter, outCore.version.adapter,
                                intCore.version.adapter,
                                AppodealNetworksData.ActionUpdate.Update, true);
                            break;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sdkPlatform), sdkPlatform, null);
            }
        }

        void IAppodealSdkManager.Compare(AppodealNetworksData.AdapterInfo.SdkPlatform sdkPlatform,
            Dictionary<string, AppodealNetworksData.AdapterInfo> intDict,
            Dictionary<string, AppodealNetworksData.AdapterInfo> outDict,
            Dictionary<string, AppodealNetworksData.AdapterInfo> current)
        {
            foreach (var key in outDict.Keys)
            {
                if (intDict.ContainsKey(key))
                {
                    if (outDict.TryGetValue(key, out AppodealNetworksData.AdapterInfo outAdapterInfo) &&
                        intDict.TryGetValue(key, out AppodealNetworksData.AdapterInfo intAdapterInfo))
                    {
                        switch (AppodealSDKManagerUtils.CompareVersion(intAdapterInfo.version.adapter,
                            outAdapterInfo.version.adapter))
                        {
                            case 1:
                                current.Add(intAdapterInfo.name, intAdapterInfo);
                                break;
                            case 0:
                                current.Add(intAdapterInfo.name, intAdapterInfo);
                                break;
                            case -1:
                                current.Add(outAdapterInfo.name,
                                    ((IAppodealSdkManager) this).SetAdapterInformation(sdkPlatform, outAdapterInfo.name,
                                        outAdapterInfo.pretty_name,
                                        intAdapterInfo.version.adapter, outAdapterInfo.version.adapter,
                                        intAdapterInfo.version.adapter,
                                        AppodealNetworksData.ActionUpdate.Update, false));
                                break;
                        }
                    }
                }
                else
                {
                    if (outDict.TryGetValue(key, out AppodealNetworksData.AdapterInfo outAdapterInfo))
                    {
                        current.Add(outAdapterInfo.name, outAdapterInfo);
                    }
                }
            }
        }

        void IAppodealSdkManager.SetAdaptersInfo(AppodealNetworksData.AdapterInfo[] adapterInfos,
            AppodealNetworksData.AdapterInfo.SdkPlatform sdkPlatform)
        {
            if (appodealCoreAndroid != null && internalCoreAndroid != null && appodealCoreIos != null &&
                internalCoreIos != null)
            {
                ((IAppodealSdkManager) this).CompareCore(AppodealNetworksData.AdapterInfo.SdkPlatform.Android,
                    internalCoreAndroid,
                    appodealCoreAndroid,
                    currentCoreAndroid);
                ((IAppodealSdkManager) this).CompareCore(AppodealNetworksData.AdapterInfo.SdkPlatform.iOS,
                    internalCoreIos,
                    appodealCoreIos,
                    currentCoreIos);
            }

            foreach (var adapterInfo in adapterInfos)
            {
                switch (sdkPlatform)
                {
                    case AppodealNetworksData.AdapterInfo.SdkPlatform.iOS:
                        appodealiOSAdapters.Add(
                            !string.IsNullOrEmpty(adapterInfo.name) ? adapterInfo.name : string.Empty,
                            ((IAppodealSdkManager) this).SetAdapterInformation(sdkPlatform, adapterInfo.name,
                                adapterInfo.pretty_name,
                                adapterInfo.version.adapter, AppodealNetworksData.ActionUpdate.Import, false));
                        break;
                    case AppodealNetworksData.AdapterInfo.SdkPlatform.Android:
                        appodealAndroidAdapters.Add(
                            !string.IsNullOrEmpty(adapterInfo.name) ? adapterInfo.name : string.Empty,
                            ((IAppodealSdkManager) this).SetAdapterInformation(
                                AppodealNetworksData.AdapterInfo.SdkPlatform.Android,
                                adapterInfo.name,
                                adapterInfo.pretty_name,
                                adapterInfo.version.adapter, AppodealNetworksData.ActionUpdate.Import, false));
                        break;
                }
            }

            currentAndroidAdapters.Clear();
            currentiOSAdapters.Clear();

            if (internaliOSAdapters.Count <= 0 || appodealiOSAdapters.Count <= 0 ||
                internalAndroidAdapters.Count <= 0 || appodealAndroidAdapters.Count <= 0) return;
            ((IAppodealSdkManager) this).Compare(AppodealNetworksData.AdapterInfo.SdkPlatform.iOS, internaliOSAdapters,
                appodealiOSAdapters,
                currentiOSAdapters);
            ((IAppodealSdkManager) this).Compare(AppodealNetworksData.AdapterInfo.SdkPlatform.Android,
                internalAndroidAdapters,
                appodealAndroidAdapters,
                currentAndroidAdapters);
        }

        AppodealNetworksData.AdapterInfo IAppodealSdkManager.SetAdapterInformation(
            AppodealNetworksData.AdapterInfo.SdkPlatform sdkPlatform,
            string adapterName,
            string adapterPrettyName,
            string adapterVersion,
            AppodealNetworksData.ActionUpdate action,
            bool isCore)
        {
            return new AppodealNetworksData.AdapterInfo(
                sdkPlatform,
                !string.IsNullOrEmpty(adapterName) ? adapterName : string.Empty,
                !string.IsNullOrEmpty(adapterPrettyName)
                    ? AppodealSDKManagerUtils.checkOptional(adapterPrettyName)
                    : string.Empty,
                !string.IsNullOrEmpty(adapterVersion)
                    ? new AppodealNetworksData.AdapterInfo.Version(adapterVersion)
                    : new AppodealNetworksData.AdapterInfo.Version(string.Empty),
                !string.IsNullOrEmpty(adapterName) &&
                !string.IsNullOrEmpty(adapterVersion)
                    ? new AppodealNetworksData.AdapterInfo.Integration(AppodealSDKManagerUtils.GetIntegrationDependency(
                        sdkPlatform,
                        adapterName,
                        adapterVersion, isCore))
                    : new AppodealNetworksData.AdapterInfo.Integration(string.Empty),
                action);
        }

        AppodealNetworksData.AdapterInfo IAppodealSdkManager.SetAdapterInformation(
            AppodealNetworksData.AdapterInfo.SdkPlatform sdkPlatform,
            string adapterName,
            string adapterPrettyName,
            string adapterUpdate,
            string adapterVersion,
            string configAdapter,
            AppodealNetworksData.ActionUpdate action,
            bool isCore)
        {
            return new AppodealNetworksData.AdapterInfo(
                sdkPlatform,
                !string.IsNullOrEmpty(adapterName) ? adapterName : string.Empty,
                !string.IsNullOrEmpty(adapterPrettyName)
                    ? AppodealSDKManagerUtils.checkOptional(adapterPrettyName)
                    : string.Empty,
                !string.IsNullOrEmpty(adapterUpdate)
                    ? new AppodealNetworksData.AdapterInfo.Version(adapterUpdate)
                    : new AppodealNetworksData.AdapterInfo.Version(string.Empty),
                !string.IsNullOrEmpty(adapterName) &&
                !string.IsNullOrEmpty(adapterVersion)
                    ? new AppodealNetworksData.AdapterInfo.Integration(AppodealSDKManagerUtils.GetIntegrationDependency(
                        sdkPlatform,
                        adapterName,
                        adapterVersion, isCore))
                    : new AppodealNetworksData.AdapterInfo.Integration(string.Empty),
                !string.IsNullOrEmpty(configAdapter)
                    ? new AppodealNetworksData.AdapterInfo.Integration(AppodealSDKManagerUtils.GetIntegrationDependency(
                        sdkPlatform,
                        adapterName,
                        configAdapter, isCore))
                    : new AppodealNetworksData.AdapterInfo.Integration(string.Empty),
                action);
        }

        void IAppodealSdkManager.SetPluginInfo(AppodealNetworksData.PluginInfo pInfo)
        {
            var compareResult = !string.IsNullOrEmpty(pInfo.version) &&
                                !string.IsNullOrEmpty(AppodealAds.Unity.Api.Appodeal.APPODEAL_PLUGIN_VERSION)
                ? AppodealSDKManagerUtils.CompareVersion(pInfo.version,
                    AppodealAds.Unity.Api.Appodeal.APPODEAL_PLUGIN_VERSION)
                : 3;
            pInfo.name = !string.IsNullOrEmpty(pInfo.name)
                ? AppodealUnityPlugin
                : pInfo.name;
            pInfo.version = compareResult == 1 && !string.IsNullOrEmpty(pInfo.version)
                ? pInfo.version
                : AppodealAds.Unity.Api.Appodeal.APPODEAL_PLUGIN_VERSION;
            pInfo.action = compareResult != 0 && compareResult != -1 && compareResult != 3
                ? AppodealNetworksData.ActionUpdate.Update
                : AppodealNetworksData.ActionUpdate.NoAction;
            pInfo.source = !string.IsNullOrEmpty(pInfo.source)
                ? pInfo.source
                : string.Empty;
        }

        IEnumerator IAppodealSdkManager.DownloadUnityPlugin()
        {
            yield return null;
            var ended = false;
            var cancelled = false;
            Exception error = null;
            int oldPercentage = 0, newPercentage = 0;
            var path = Path.Combine(DownloadDir, AppodealUnityPlugin);
            progress = 0.01f;
            downloader = new WebClient {Encoding = Encoding.UTF8};
            downloader.DownloadProgressChanged += (sender, args) => { newPercentage = args.ProgressPercentage; };
            downloader.DownloadFileCompleted += (sender, args) =>
            {
                ended = true;
                cancelled = args.Cancelled;
                error = args.Error;
            };

            if (!string.IsNullOrEmpty(pluginInfo.source))
            {
                Debug.LogFormat("Downloading {0} to {1}", pluginInfo.source, path);
                Debug.Log(pluginInfo.source);
                downloader.DownloadFileAsync(new Uri(pluginInfo.source), path);
            }

            while (!ended)
            {
                Repaint();
                var percentage = oldPercentage;
                yield return new WaitUntil(() => ended || newPercentage > percentage);
                oldPercentage = newPercentage;
                progress = oldPercentage / 100.0f;
            }

            if (error != null)
            {
                Debug.LogError(error);
                cancelled = true;
            }

            downloader = null;
            coroutine = null;
            progress = 0;
            EditorUtility.ClearProgressBar();
            if (!cancelled)
            {
                AssetDatabase.ImportPackage(path, true);
            }
            else
            {
                Debug.Log("Download terminated.");
            }
        }

        void IAppodealSdkManager.ReadAndroidDependencies()
        {
            Debug.Log($"Reading iOS dependency XML file {AppodealDependencies}");

            var sources = new List<string>();
            string specName = null;

            XmlUtilities.ParseXmlTextFileElements(AppodealDependencies,
                (reader, elementName, isStart, parentElementName, elementNameStack) =>
                {
                    if (elementName == "dependencies" &&
                        parentElementName == "" || elementName == "androidPackages" &&
                        (parentElementName == "dependencies" || parentElementName == ""))
                        return true;

                    if (elementName == "androidPackage" && parentElementName == "androidPackages")
                    {
                        if (isStart)
                        {
                            specName = reader.GetAttribute("spec");
                            sources = new List<string>();
                            if (specName == null)
                            {
                                Debug.Log(
                                    $"Pod name not specified while reading {AppodealDependencies}:{reader.LineNumber}\n");
                                return false;
                            }
                        }
                        else
                        {
                            if (specName != null)
                            {
                                internalAndroidAdapters.Add(AppodealSDKManagerUtils.GetDependencyName(specName),
                                    new AppodealNetworksData.AdapterInfo(
                                        AppodealNetworksData.AdapterInfo.SdkPlatform.Android,
                                        AppodealSDKManagerUtils.GetDependencyName(specName),
                                        AppodealSDKManagerUtils.GetDependencyPrettyName(specName),
                                        new AppodealNetworksData.AdapterInfo.Version(
                                            AppodealSDKManagerUtils.GetDependencyVersion(
                                                specName,
                                                AppodealSDKManagerUtils.GetDependencyName(specName))),
                                        new AppodealNetworksData.AdapterInfo.Integration(
                                            AppodealSDKManagerUtils.GetIntegrationDependency(
                                                AppodealNetworksData.AdapterInfo.SdkPlatform.Android,
                                                AppodealSDKManagerUtils.GetDependencyName(specName),
                                                AppodealSDKManagerUtils.GetDependencyVersion(specName,
                                                    AppodealSDKManagerUtils.GetDependencyName(specName)), false)),
                                        AppodealNetworksData.ActionUpdate.NoAction));
                            }
                        }

                        return true;
                    }

                    if (elementName == "sources" && parentElementName == "androidPackage")
                        return true;
                    if (elementName == "sources" && parentElementName == "androidPackages")
                    {
                        if (isStart)
                        {
                            sources = new List<string>();
                        }
                        else
                        {
                            using (var enumerator = sources.GetEnumerator())
                            {
                                while (enumerator.MoveNext())
                                {
                                    string current = enumerator.Current;
                                    Debug.Log(current);
                                }
                            }
                        }

                        return true;
                    }

                    if (elementName != "source" || parentElementName != "sources")
                        return false;
                    if (isStart && reader.Read() && reader.NodeType == XmlNodeType.Text)
                        sources.Add(reader.ReadContentAsString());
                    return true;
                });

            Debug.Log($"Reading iOS dependency XML file {AppodealCoreDependencies}");

            XmlUtilities.ParseXmlTextFileElements(AppodealCoreDependencies,
                (reader, elementName, isStart, parentElementName, elementNameStack) =>
                {
                    if (elementName == "dependencies" &&
                        parentElementName == "" || elementName == "androidPackages" &&
                        (parentElementName == "dependencies" || parentElementName == ""))
                    {
                        return true;
                    }


                    if (elementName == "androidPackage" && parentElementName == "androidPackages")
                    {
                        if (isStart)
                        {
                            specName = reader.GetAttribute("spec");
                            sources = new List<string>();
                            if (specName == null)
                            {
                                Debug.Log(
                                    $"Pod name not specified while reading {AppodealDependencies}:{reader.LineNumber}\n");
                                return false;
                            }

                            if (specName.Contains("com.appodeal.ads.sdk:core:"))
                            {
                                internalCoreAndroid = new AppodealNetworksData.AdapterInfo(
                                    AppodealNetworksData.AdapterInfo.SdkPlatform.Android,
                                    "core",
                                    "Appodeal",
                                    new AppodealNetworksData.AdapterInfo.Version(
                                        AppodealSDKManagerUtils.GetDependencyCoreVersion(
                                            specName,
                                            "core")),
                                    new AppodealNetworksData.AdapterInfo.Integration(
                                        AppodealSDKManagerUtils.GetIntegrationDependency(
                                            AppodealNetworksData.AdapterInfo.SdkPlatform.Android,
                                            AppodealSDKManagerUtils.GetDependencyName(specName),
                                            AppodealSDKManagerUtils.GetDependencyVersion(specName,
                                                AppodealSDKManagerUtils.GetPrettyName(specName)), true)),
                                    AppodealNetworksData.ActionUpdate.NoAction);
                            }
                        }

                        return true;
                    }

                    if (elementName == "sources" && parentElementName == "androidPackage")
                        return true;
                    if (elementName == "sources" && parentElementName == "androidPackages")
                    {
                        if (isStart)
                        {
                            sources = new List<string>();
                        }
                        else
                        {
                            using (var enumerator = sources.GetEnumerator())
                            {
                                while (enumerator.MoveNext())
                                {
                                    string current = enumerator.Current;
                                    Debug.Log(current);
                                }
                            }
                        }

                        return true;
                    }

                    if (elementName != "source" || parentElementName != "sources")
                        return false;
                    if (isStart && reader.Read() && reader.NodeType == XmlNodeType.Text)
                        sources.Add(reader.ReadContentAsString());
                    return true;
                });
            ((IAppodealSdkManager) this).UpdateProgress(25f);
        }

        void IAppodealSdkManager.ReadIosDependencies()
        {
            Debug.Log($"Reading iOS dependency XML file {AppodealDependencies}");
            var sources = new List<string>();
            string podName = null;
            string version = null;

            XmlUtilities.ParseXmlTextFileElements(AppodealDependencies,
                (reader, elementName, isStart, parentElementName, elementNameStack) =>
                {
                    if (elementName == "dependencies" &&
                        parentElementName == "" || elementName == "iosPods" &&
                        (parentElementName == "dependencies" || parentElementName == ""))
                        return true;

                    if (elementName == "iosPod" && parentElementName == "iosPods")
                    {
                        if (isStart)
                        {
                            podName = reader.GetAttribute("name");
                            version = reader.GetAttribute("version");
                            sources = new List<string>();
                            if (podName == null)
                            {
                                Debug.Log(
                                    $"Pod name not specified while reading {AppodealDependencies}:{reader.LineNumber}\n");
                                return false;
                            }
                        }
                        else
                        {
                            if (podName != null)
                            {
                                internaliOSAdapters.Add(podName,
                                    new AppodealNetworksData.AdapterInfo(
                                        AppodealNetworksData.AdapterInfo.SdkPlatform.iOS, podName,
                                        AppodealSDKManagerUtils.GetPrettyName(podName),
                                        new AppodealNetworksData.AdapterInfo.Version(version),
                                        new AppodealNetworksData.AdapterInfo.Integration(
                                            AppodealSDKManagerUtils.GetIntegrationDependency(
                                                AppodealNetworksData.AdapterInfo.SdkPlatform.iOS, podName,
                                                version, false)),
                                        AppodealNetworksData.ActionUpdate.NoAction));
                            }
                        }

                        return true;
                    }

                    if (elementName == "sources" && parentElementName == "iosPod")
                        return true;
                    if (elementName == "sources" && parentElementName == "iosPods")
                    {
                        if (isStart)
                        {
                            sources = new List<string>();
                        }
                        else
                        {
                            using (List<string>.Enumerator enumerator = sources.GetEnumerator())
                            {
                                while (enumerator.MoveNext())
                                {
                                    string current = enumerator.Current;
                                    Debug.Log(current);
                                }
                            }
                        }

                        return true;
                    }

                    if (!(elementName == "source") || !(parentElementName == "sources"))
                        return false;
                    if (isStart && reader.Read() && reader.NodeType == XmlNodeType.Text)
                        sources.Add(reader.ReadContentAsString());
                    return true;
                });

            Debug.Log($"Reading iOS dependency XML file {AppodealCoreDependencies}");

            XmlUtilities.ParseXmlTextFileElements(AppodealCoreDependencies,
                (reader, elementName, isStart, parentElementName, elementNameStack) =>
                {
                    if (elementName == "dependencies" &&
                        parentElementName == "" || elementName == "iosPods" &&
                        (parentElementName == "dependencies" || parentElementName == ""))
                        return true;

                    if (elementName == "iosPod" && parentElementName == "iosPods")
                    {
                        if (isStart)
                        {
                            podName = reader.GetAttribute("name");
                            version = reader.GetAttribute("version");
                            sources = new List<string>();
                            if (podName == null)
                            {
                                Debug.Log(
                                    $"Pod name not specified while reading {AppodealDependencies}:{reader.LineNumber}\n");
                                return false;
                            }
                        }
                        else
                        {
                            if (podName != null)
                            {
                                internalCoreIos = new AppodealNetworksData.AdapterInfo(
                                    AppodealNetworksData.AdapterInfo.SdkPlatform.iOS, podName,
                                    AppodealSDKManagerUtils.GetPrettyName(podName),
                                    new AppodealNetworksData.AdapterInfo.Version(version),
                                    new AppodealNetworksData.AdapterInfo.Integration(
                                        AppodealSDKManagerUtils.GetIntegrationDependency(
                                            AppodealNetworksData.AdapterInfo.SdkPlatform.iOS, podName,
                                            version, true)),
                                    AppodealNetworksData.ActionUpdate.NoAction);
                            }
                        }

                        return true;
                    }

                    if (elementName == "sources" && parentElementName == "iosPod")
                        return true;
                    if (elementName == "sources" && parentElementName == "iosPods")
                    {
                        if (isStart)
                        {
                            sources = new List<string>();
                        }
                        else
                        {
                            using (List<string>.Enumerator enumerator = sources.GetEnumerator())
                            {
                                while (enumerator.MoveNext())
                                {
                                    string current = enumerator.Current;
                                    Debug.Log(current);
                                }
                            }
                        }

                        return true;
                    }

                    if (!(elementName == "source") || !(parentElementName == "sources"))
                        return false;
                    if (isStart && reader.Read() && reader.NodeType == XmlNodeType.Text)
                        sources.Add(reader.ReadContentAsString());
                    return true;
                });
            ((IAppodealSdkManager) this).UpdateProgress(25f);
        }

        void IAppodealSdkManager.UpdateWindow()
        {
            ((IAppodealSdkManager) this).Reset();
            coroutine = this.StartCoroutine(((IAppodealSdkManager) this).GetAppodealSdkVersions(false));
            GUI.enabled = true;
        }

        void IAppodealSdkManager.UpdateProgress(float updateStep)
        {
            if (loading > 100f)
            {
                loading = 100f;
            }
            else
            {
                loading += updateStep;
            }
        }
    }
}
#endif