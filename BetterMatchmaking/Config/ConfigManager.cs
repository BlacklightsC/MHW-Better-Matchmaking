﻿using ImGuiNET;
using SharpPluginLoader.Core.Configuration;
using SharpPluginLoader.Core.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BetterMatchmaking;

internal sealed class ConfigManager : SingletonAccessor, IDisposable
{
	// Singleton Pattern
	private static readonly ConfigManager _singleton = new();

	public static ConfigManager Instance => _singleton;

	// Explicit static constructor to tell C# compiler
	// not to mark type as beforefieldinit
	static ConfigManager() { }

	// Singleton Pattern End
	public ConfigWatcher ConfigWatcherInstance { get; set; }

	public ConfigCustomization Customization { get; set; }

	public Config Default { get; set; }
	public Config Current { get; set; }

	private ConfigManager() { }

	public ConfigManager Init()
	{
		TeaLog.Info("ConfigManager: Initializing...");

		InstantiateSingletons();

		ConfigWatcherInstance = new();
		Customization = new();
		Default = new();

		Default.InitDefault();

		// Create folder hierarchy if it doesn't exist
		Directory.CreateDirectory(Constants.PLUGIN_DATA_PATH);

		// If config file doesn't exist - use default one
		if(!File.Exists(Constants.DEFAULT_CONFIG_FILE_PATH_NAME))
		{
			TeaLog.Info("ConfigManager: Config Doesn't Exist, Using Default One.");

			SetCurrentConfig(Default);
			Current.Save();

			TeaLog.Info("ConfigManager: Initialization Done!");
			return this;
		}

		// If config file exists..

		// Load from file
		var config = LoadConfig();

		// If config file is incorrect - use default one
		if (config == null)
		{
			TeaLog.Info("Config: Loading Failed!");
			SetCurrentConfig(Default);

			TeaLog.Info("ConfigManager: Initialization Done!");
			return this;
		}

		// If config file is good - use it and save
		SetCurrentConfig(config);
		Current.Save();

		ConfigWatcherInstance.Init();

		TeaLog.Info("ConfigManager: Initialization Done!");
		return this;
	}

	public ConfigManager SetCurrentConfig(Config config)
	{
		Current = config;

		DebugManager_I.Customization = config.Debug;



		RegionLockFix_I.SessionCustomization = config.Sessions.RegionLockFix;
		MaxSearchResultLimit_I.SessionCustomization = config.Sessions.MaxSearchResultLimit;
		SessionPlayerCountFilter_I.Customization = config.Sessions.PlayerCountFilter;

		PlayerTypeFilter_I.Customization = config.Sessions.InGameFilterOverride.PlayerType;
		QuestPreferenceFilter_I.Customization = config.Sessions.InGameFilterOverride.QuestPreference;
		LanguageFilter_I.SessionCustomization = config.Sessions.InGameFilterOverride.Language;



		RegionLockFix_I.QuestCustomization = config.Quests.RegionLockFix;
		MaxSearchResultLimit_I.QuestCustomization = config.Quests.MaxSearchResultLimit;

		QuestTypeFilter_I.Customization = config.Quests.InGameFilterOverride.QuestType;
		DifficultyFilter_I.Customization = config.Quests.InGameFilterOverride.Difficulty;
		RewardFilter_I.Customization = config.Quests.InGameFilterOverride.Rewards;
		LanguageFilter_I.QuestCustomization = config.Quests.InGameFilterOverride.Language;
		TargetFilter_I.Customization = config.Quests.InGameFilterOverride.Target;

		return this;
	}

	public static Config LoadConfig()
	{
		try
		{
			TeaLog.Info("Config: Loading...");

			var json = JsonManager.ReadFromFile(Constants.DEFAULT_CONFIG_FILE_PATH_NAME);

			var config = JsonSerializer.Deserialize<Config>(json, JsonManager.JSON_SERIALIZER_OPTIONS_INSTANCE).Init();

			TeaLog.Info("Config: Loading Done!");
			return config;
		}
		catch (Exception exception)
		{
			TeaLog.Error(exception.ToString());
			return null;
		}
	}

	public ConfigManager ResetConfig()
	{
		var newConfig = Default.DeepCopy();
		SetCurrentConfig(newConfig);
		newConfig.Save();

		return this;
	}

	public override string ToString()
	{
		return JsonManager.Serialize(this);
	}

	public void Dispose()
	{
		ConfigWatcherInstance.Dispose();
	}
}
