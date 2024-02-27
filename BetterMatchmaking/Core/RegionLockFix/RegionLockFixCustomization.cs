﻿using ImGuiNET;
using SharpPluginLoader.Core.Steam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BetterMatchmaking;

internal class RegionLockFixCustomization : SingletonAccessor
{
	private bool enabled = true;
	public bool Enabled { get => enabled; set => enabled = value; }

	public string DistanceFilter { get; set; }

	[JsonIgnore]
	public LobbyDistanceFilter DistanceFilterEnum { get; set; } = LobbyDistanceFilter.WorldWide;

	public RegionLockFixCustomization()
	{
		DistanceFilter = localizationManager.Default.ImGui.Worldwide;
	}

	public RegionLockFixCustomization Init()
	{
		DistanceFilterEnum = (LobbyDistanceFilter)Array.FindIndex(
			LocalizationManager.Instance.Default.ImGui.DistanceFilters, arrayString => arrayString.Equals(DistanceFilter)
		);

		return this;
	}

	public bool RenderImGui()
	{
		var changed = false;
		var tempChanged = false;
		var selectedIndex = 0;

		if (ImGui.TreeNode(localizationManager.ImGui.RegionLockFix))
		{
			changed = ImGui.Checkbox(localizationManager.ImGui.Enabled, ref enabled) || changed;

			selectedIndex = (int)DistanceFilterEnum;
			tempChanged = ImGui.Combo(localizationManager.ImGui.DistanceFilter, ref selectedIndex, localizationManager.ImGui.DistanceFilters, 4);
			if (tempChanged)
			{
				DistanceFilterEnum = (LobbyDistanceFilter)selectedIndex;
				DistanceFilter = localizationManager.Default.ImGui.DistanceFilters[selectedIndex];
			}
			changed = changed || tempChanged;

			ImGui.TreePop();

		}

		return changed;
	}
}