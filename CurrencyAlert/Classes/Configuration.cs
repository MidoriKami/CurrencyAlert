﻿using System.Collections.Generic;
using Dalamud.Configuration;
using KamiLib.Configuration;

namespace CurrencyAlert.Classes;

public class Configuration : IPluginConfiguration {
    public bool ChatWarning = false;

    public List<TrackedCurrency> Currencies = [];
    public bool HideInDuties = false;

    public int Version { get; set; } = 7;

    public static Configuration Load()
        => Service.PluginInterface.LoadConfigFile("CurrencyAlert.config.json", () => new Configuration());

    public void Save()
        => Service.PluginInterface.SaveConfigFile("CurrencyAlert.config.json", this);
}