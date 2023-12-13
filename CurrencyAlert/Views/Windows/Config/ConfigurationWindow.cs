using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CurrencyAlert.Controllers;
using CurrencyAlert.Models;
using CurrencyAlert.Models.Enums;
using CurrencyAlert.Views.Windows.WindowTabs;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using ImGuiNET;
using KamiLib.Command;
using KamiLib.Interfaces;
using KamiLib.Search;
using KamiLib.System;
using KamiLib.UserInterface;

namespace CurrencyAlert.Views.Windows.Config;

public class ConfigurationWindow : TabbedSelectionWindow {
    private readonly List<ISelectionWindowTab> tabs;
    private readonly List<ITabItem> regularTabs;

    private readonly ItemSearchModal itemSearchModal;

    public ConfigurationWindow() : base("CurrencyAlert - Configuration Window", 27.0f) {
        tabs = new List<ISelectionWindowTab> {
            new CurrencyConfigurationTab(),
        };

        regularTabs = new List<ITabItem> {
            new GeneralSettingsTab(),
        };

        SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new Vector2(450, 350),
            MaximumSize = new Vector2(9999, 9999)
        };

        itemSearchModal = new ItemSearchModal {
            Name = "Add New Currency",
            AddAction = (item, type) => {
                if (CurrencyAlertSystem.Config.Currencies.Any(currency => currency.ItemId == item.RowId)) {
                    Service.ChatGui.PrintError("Unable to add a currency that is already being tracked.");
                }
                else {
                    CurrencyAlertSystem.Config.Currencies.Add(new TrackedCurrency {
                        Threshold = 1000,
                        Type = type switch {
                            SpecialSearchType.Collectable => CurrencyType.Collectable,
                            SpecialSearchType.HighQualityItem => CurrencyType.HighQualityItem,
                            _ => CurrencyType.Item
                        },
                        Enabled = true,
                        ItemId = item.RowId,

                    });
                    CurrencyAlertSystem.Config.Save(); 
                }
            }
        };

        ShowScrollBar = false;

        CommandController.RegisterCommands(this);
    }   
    
    protected override IEnumerable<ISelectionWindowTab> GetTabs() => tabs;
    protected override IEnumerable<ITabItem> GetRegularTabs() => regularTabs;

    public override bool DrawConditions() {
        if (!Service.ClientState.IsLoggedIn) return false;
    
        return true;
    }

    protected override void DrawExtras() {
        ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button(FontAwesomeIcon.Plus.ToIconString(), new Vector2(ImGui.GetContentRegionAvail().X, 23.0f * ImGuiHelpers.GlobalScale))) {
            ImGui.OpenPopup("Add New Currency");
        }
        ImGui.PopFont();
        
        itemSearchModal.DrawSearchModal();
    }
    
    [BaseCommandHandler("OpenConfigWindow")]
    // ReSharper disable once UnusedMember.Local
    private void OpenConfigWindow() {
        if (!Service.ClientState.IsLoggedIn) return;

        Toggle();
    }
}