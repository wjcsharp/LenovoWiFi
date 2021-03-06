﻿using System;
using System.Configuration;

using NLog;

namespace Lenovo.WiFi
{
    internal class Settings
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const string KeyHostedNetworkName = "hostednetworkname";
        private const string KeyHostedNetworkKey = "hostednetworkkey";

        private static Settings _instance;

        private readonly Configuration _configuration;
        private readonly KeyValueConfigurationElement _nameConfiguration;
        private readonly KeyValueConfigurationElement _keyConfiguration;

        private Settings()
        {
            Logger.Trace("Load: Configuration loading...");

            _configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            _nameConfiguration = _configuration.AppSettings.Settings[KeyHostedNetworkName];
            _keyConfiguration = _configuration.AppSettings.Settings[KeyHostedNetworkKey];

            Logger.Trace("Load: Configuration loaded");
            Logger.Info("Load: Name: {0}, Key: {1}", _nameConfiguration.Value, _keyConfiguration.Value);
        }

        public static Settings Instance
        {
            get { return _instance ?? (_instance = new Settings()); }
        }

        public string HostedNetworkName
        {
            get
            {
                Logger.Trace("HostedNetworkName: Getting value");
                if (String.IsNullOrEmpty(_nameConfiguration.Value))
                {
                    Logger.Trace("HostedNetworkName: Value empty, generating new value...");

                    _nameConfiguration.Value = GenerateHostedNetworkName();
                    _configuration.Save();
                }
                Logger.Info("HostedNetworkName: Value: {0}", _nameConfiguration.Value);

                return _nameConfiguration.Value;
            }
            set
            {
                Logger.Trace("HostedNetworkName: New value: {0}", value);
                if (String.IsNullOrEmpty(value))
                {
                    Logger.Error("HostedNetworkName: New value is null or empty");
                    throw new ArgumentNullException("value");
                }

                if (_nameConfiguration.Value != value)
                {
                    _nameConfiguration.Value = value;
                    _configuration.Save();

                    Logger.Trace("HostedNetworkName: New value set");
                }
                else
                {
                    Logger.Trace("HostedNetworkName: New value is identical as current one.");
                }
            }
        }

        public string HostedNetworkKey
        {
            get
            {
                Logger.Trace("HostedNetworkKey: Getting value");
                if (String.IsNullOrEmpty(_keyConfiguration.Value))
                {
                    Logger.Trace("HostedNetworkKey: Value empty, generating new value...");

                    _keyConfiguration.Value = GenerateHostedNetworkKey();
                    _configuration.Save();
                }
                Logger.Info("HostedNetworkKey: Value: {0}", _nameConfiguration.Value);

                return _keyConfiguration.Value;
            }
            set
            {
                Logger.Trace("HostedNetworkKey: New value: {0}", value);
                if (String.IsNullOrEmpty(value))
                {
                    Logger.Error("HostedNetworkKey: New value is null or empty");
                    throw new ArgumentNullException("value");
                }

                if (_keyConfiguration.Value != value)
                {
                    _keyConfiguration.Value = value;
                    _configuration.Save();

                    Logger.Trace("HostedNetworkKey: New value set");
                }
                else
                {
                    Logger.Trace("HostedNetworkKey: New value is identical as current one.");
                }
            }
        }

        private static string GenerateHostedNetworkName()
        {
            return String.Format("LenovoWiFi{0:000}", new Random().Next(1, 100));
        }

        private static string GenerateHostedNetworkKey()
        {
            return "1234567890";
        }
    }
}
