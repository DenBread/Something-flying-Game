using System;
using System.Diagnostics.CodeAnalysis;

namespace Appodeal.Editor.AppodealManager.AppodealManagerData
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class AppodealNetworksData
    {
        [Serializable]
        public class PluginInfo
        {
            public string source;
            public string name;
            public string version;
            public string updateUrl;
            public ActionUpdate action;
        }

        [Serializable]
        public class AppodealSDKVersion
        {
            public string version;
            public string platform;
            public int id;
            public string build_type;
        }

        public enum ActionUpdate
        {
            Update,
            Import,
            NoAction
        }
        
        [Serializable]
        public class AdapterInfo
        {
            public SdkPlatform sdkPlatform;
            public string name;

            // ReSharper disable once InconsistentNaming
            public string pretty_name;
            public Version version;
            public Integration integration;
            public Integration internalIntegration;
            public ActionUpdate action;

            public AdapterInfo(SdkPlatform sdkPlatform, string name, string prettyName, Version version,
                Integration integration,
                ActionUpdate action)
            {
                this.sdkPlatform = sdkPlatform;
                this.name = name;
                pretty_name = prettyName;
                this.version = version;
                this.integration = integration;
                this.action = action;
            }

            public AdapterInfo(SdkPlatform sdkPlatform, string name, string prettyName, Version version,
                Integration integration,
                Integration internalIntegration, ActionUpdate action)
            {
                this.sdkPlatform = sdkPlatform;
                this.name = name;
                pretty_name = prettyName;
                this.version = version;
                this.integration = integration;
                this.internalIntegration = internalIntegration;
                this.action = action;
            }

            [Serializable]
            public class Version
            {
                public Version(string adapter)
                {
                    this.adapter = adapter;
                }

                public string adapter;
            }

            [Serializable]
            public class Integration
            {
                public Integration(string code)
                {
                    this.code = code;
                }

                public string code;
            }
            
            [SuppressMessage("ReSharper", "InconsistentNaming")]
            public enum SdkPlatform
            {
                Android,
                iOS
            }
        }
    }
}