using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Appodeal.Editor.AppodealManager;
using Appodeal.Editor.AppodealManager.AppodealManagerData;
using UnityEngine;
using UnityEngine.Networking;
#pragma warning disable 618

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "RedundantAssignment")]
public static class AppodealSDKManagerUtils
{
#if UNITY_2018_1_OR_NEWER
    public static int CompareVersion(string interal, string latest)
    {
        var xParts = interal.Split('.');
        var yParts = latest.Split('.');
        var partsLength = Math.Max(xParts.Length, yParts.Length);
        if (partsLength <= 0) return string.Compare(interal, latest, StringComparison.Ordinal);
        for (var i = 0; i < partsLength; i++)
        {
            if (xParts.Length <= i) return -1;
            if (yParts.Length <= i) return 1;
            var xPart = xParts[i];
            var yPart = yParts[i];
            if (string.IsNullOrEmpty(xPart)) xPart = "0";
            if (string.IsNullOrEmpty(yPart)) yPart = "0";
            if (!int.TryParse(xPart, out var xInt) || !int.TryParse(yPart, out var yInt))
            {
                var abcCompare = String.Compare(xPart, yPart, StringComparison.Ordinal);
                if (abcCompare != 0)
                    return abcCompare;
                continue;
            }

            if (xInt != yInt) return xInt < yInt ? -1 : 1;
        }

        return 0;
    }

    public static string GetIntegrationDependency(AppodealNetworksData.AdapterInfo.SdkPlatform sdkPlatform, string name, string version,
        bool core)
    {
        var integration = string.Empty;
        switch (sdkPlatform)
        {
            case AppodealNetworksData.AdapterInfo.SdkPlatform.Android:
                if (core)
                {
                    integration = "<androidPackage spec='" + AppodealSDKManager.ReplaceDependencyCoreValue + name +
                                  ":" +
                                  version + "'>";
                }
                else
                {
                    integration = "<androidPackage spec='" + AppodealSDKManager.ReplaceDependencyValue + name + ":" +
                                  version + "'/>";
                }

                break;
            case AppodealNetworksData.AdapterInfo.SdkPlatform.iOS:
                integration = "<iosPod name='" + name + "'" + " version='" + version + "'" + " minTargetSdk='" + "9.0" +
                              "'/>";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(sdkPlatform), sdkPlatform, null);
        }

        return integration;
    }

    public static string GetPrettyName(string value)
    {
        var f = value.Replace("APD", string.Empty);
        return f.Replace("Adapter", string.Empty);
    }

    private static string FirstCharToUpper(string input)
    {
        switch (input)
        {
            case null: throw new ArgumentNullException(nameof(input));
            case "": throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
            default: return input.First().ToString().ToUpper() + input.Substring(1);
        }
    }

    public static string GetDependencyName(string value)
    {
        var dependencyName = value.Replace(AppodealSDKManager.ReplaceDependencyValue, string.Empty);
        return dependencyName.Substring(0,
            dependencyName.LastIndexOf(":", StringComparison.Ordinal));
    }

    public static string GetDependencyPrettyName(string value)
    {
        var dependencyName = value.Replace(AppodealSDKManager.ReplaceDependencyValue, string.Empty);
        return FirstCharToUpper(dependencyName.Substring(0,
            dependencyName.LastIndexOf(":", StringComparison.Ordinal)));
    }

    public static string GetDependencyVersion(string value, string name)
    {
        return value.Replace(AppodealSDKManager.ReplaceDependencyValue + name + ":", string.Empty);
    }

    public static string GetDependencyCoreVersion(string value, string name)
    {
        return value.Replace(AppodealSDKManager.ReplaceDependencyCoreValue + name + ":", string.Empty);
    }

    public static string checkOptional(string prettyName)
    {
        string[] optionalNetworks =
        {
            "Mintegral",
            "Mopub",
            "A4g",
            "Yandex-v280",
            "Admob-v17",
            "Flurry",
            "Fyber",
            "TwitterMoPub"
        };

        if (!optionalNetworks.Any(optionalNetwork => optionalNetwork.Equals(prettyName))) return prettyName;
        return prettyName + " (optional)";
    }

#endif
}