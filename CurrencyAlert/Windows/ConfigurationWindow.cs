
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using CurrencyAlert.Models;
using CurrencyAlert.Models.Enums;
using Dalamud.Game.Text;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using KamiLib.Components;
using KamiLib.Window;
using KamiLib.Window.SelectionWindows;
using Lumina.Excel.GeneratedSheets;

namespace CurrencyAlert.Views.Windows.Config;

public class ConfigurationWindow() : TabbedSelectionWindow<TrackedCurrency>("CurrencyAlert Configuration Window", new Vector2(450.0f, 350.0f), true) {
    protected override List<TrackedCurrency> Options => System.Config.Currencies;
    protected override float SelectionListWidth { get; set; } = 150.0f;
    protected override float SelectionItemHeight => 20.0f;
    protected override bool AllowChildScroll => false;
    protected override string SelectionListTabName => "Tracked Currencies";
    protected override bool ShowListButton => true;

    protected override List<ITabItem> Tabs { get; } = [
        new GeneralSettingsTab()
    ];

    protected override void DrawListOption(TrackedCurrency option) {
         if (option is { Name: var name, Icon: { } icon}) {
             ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 3.0f * ImGuiHelpers.GlobalScale);
             ImGui.Image(icon.ImGuiHandle, ImGuiHelpers.ScaledVector2(24.0f));
             
             ImGui.SameLine();
             ImGui.AlignTextToFramePadding();
             ImGui.Text(name);
         }
         else {
             ImGui.TextColored(KnownColor.OrangeRed.Vector(), $"Error, unable to display currency. ItemId: {option.ItemId}");
         }
    }
    
    protected override void DrawSelectedOption(TrackedCurrency selectedOption) {
        DrawHeaderAndWatermark(selectedOption);
        DrawCurrentStatus(selectedOption);
        DrawSettings(selectedOption);
    }
    
    private void DrawHeaderAndWatermark(TrackedCurrency currency) {
        if (currency is { Name: var name }) {
            var region = ImGui.GetContentRegionAvail();
            var minDimension = Math.Min(region.X, region.Y);

            var textSize = ImGui.CalcTextSize(name);
            ImGui.SetCursorPosX(ImGui.GetContentRegionAvail().X / 2.0f - textSize.X / 2.0f);
            ImGui.Text(name);
            ImGui.Separator();

            var areaStart = ImGui.GetCursorPos();
            ImGui.SetCursorPosX(region.X / 2.0f - minDimension / 2.0f);
            ImGui.Image(currency.Icon.ImGuiHandle, new Vector2(minDimension), Vector2.Zero, Vector2.One, Vector4.One with { W = 0.10f });
            ImGui.SetCursorPos(areaStart);
        }
    }
    
     private void DrawCurrentStatus(TrackedCurrency currency) {
         if (currency is not { CurrentCount: var currentCount, Threshold: var threshold }) return;
         
         var color = ((float)currentCount / threshold) switch {
             < 0.75f => currency.Invert ? KnownColor.Red.Vector() : KnownColor.White.Vector(),
             < 0.85f => currency.Invert ? KnownColor.Red.Vector() : KnownColor.Orange.Vector(),
             < 0.95f => currency.Invert ? KnownColor.Red.Vector() : KnownColor.OrangeRed.Vector(),
             > 0.95f and < 1.00f => KnownColor.Red.Vector(),
             >= 1.00f and < 1.05f => currency.Invert ? KnownColor.OrangeRed.Vector() : KnownColor.Red.Vector(),
             >= 1.05f and < 1.15f => currency.Invert ? KnownColor.Orange.Vector() : KnownColor.Red.Vector(),
             >= 1.15f => currency.Invert ? KnownColor.White.Vector() : KnownColor.Red.Vector(),
             _ => KnownColor.White.Vector(),
         };

         using (var _ = ImRaii.Table("CurrentStatusTable", 3)) {
             ImGui.TableSetupColumn("##CurrentAmount", ImGuiTableColumnFlags.WidthStretch);
             ImGui.TableSetupColumn("##Slash", ImGuiTableColumnFlags.WidthFixed, 5.0f * ImGuiHelpers.GlobalScale);
             ImGui.TableSetupColumn("##ThresholdAmount", ImGuiTableColumnFlags.WidthStretch);

             ImGui.TableNextColumn();
             var text = $"{currentCount}";
             var currentCountSize = ImGui.CalcTextSize(text);
             ImGui.SetCursorPosX(ImGui.GetContentRegionAvail().X - currentCountSize.X);
             ImGui.TextColored(color, text);

             ImGui.TableNextColumn();
             ImGui.Text("/");

             ImGui.TableNextColumn();
             ImGui.Text(threshold.ToString());
         }
         
         ImGuiHelpers.ScaledDummy(10.0f);
     }

     private void DrawSettings(TrackedCurrency currency) {
         if (currency is not { ItemId: var itemId, Enabled: var enabled, ChatWarning: var chatWarning, ShowInOverlay: var overlay, Invert: var invert, Threshold: var threshold }) return;
         
         if (ImGui.Checkbox($"Enable##{itemId}", ref enabled)) {
             currency.Enabled = enabled;
             System.Config.Save();
         }

         ImGuiHelpers.ScaledDummy(5.0f);
         
         if (ImGui.Checkbox($"Chat Warning##{itemId}", ref chatWarning)) {
             currency.ChatWarning = chatWarning;
             System.Config.Save();
         }
         ImGuiComponents.HelpMarker("When amount is above threshold, print a message to chat when changing zones");

         if (ImGui.Checkbox($"Overlay##{itemId}", ref overlay)) {
             currency.ShowInOverlay = overlay;
             System.Config.Save();
         }
         ImGuiComponents.HelpMarker("Allows this currency to show in the overlay");
         
         if (ImGui.Checkbox($"Invert##{itemId}", ref invert)) {
             currency.Invert = invert;
             System.Config.Save();
         }
         ImGuiComponents.HelpMarker("Warn when below the threshold instead of above");
         
         ImGuiHelpers.ScaledDummy(5.0f);

         ImGui.PushItemWidth(50.0f * ImGuiHelpers.GlobalScale);
         if (ImGui.InputInt($"Threshold##{itemId}", ref threshold, 0, 0)) {
             currency.Threshold = threshold;
             System.Config.Save();
         }
         
         ImGui.SetCursorPosY(ImGui.GetContentRegionMax().Y - 23.0f * ImGuiHelpers.GlobalScale);
         var hotkeyHeld = ImGui.GetIO().KeyShift && ImGui.GetIO().KeyCtrl && currency.CanRemove;

         if (!hotkeyHeld) ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);

         ImGui.PushFont(UiBuilder.IconFont);
         if (ImGui.Button(FontAwesomeIcon.Trash.ToIconString(), new Vector2(ImGui.GetContentRegionAvail().X, 23.0f * ImGuiHelpers.GlobalScale)) && hotkeyHeld && currency.CanRemove) {
             System.Config.Currencies.Remove(currency);
             DeselectItem();
             System.Config.Save();
         }
         ImGui.PopFont();
         
         if (!hotkeyHeld) ImGui.PopStyleVar();
         
         if (ImGui.IsItemHovered() && !hotkeyHeld) {
             ImGui.SetTooltip(currency.CanRemove ? "Hold Shift + Control while clicking to delete this currency" : "Special currencies cannot be removed");
         }
     }
     
     protected override void DrawExtraButton() {
         using var spacingTable = ImRaii.Table("buttonTable", 3, ImGuiTableFlags.SizingStretchSame);
         if (!spacingTable) return;

         ImGui.TableNextColumn();
         if (ImGuiTweaks.IconButtonWithSize(Service.PluginInterface.UiBuilder.IconFontFixedWidthHandle, FontAwesomeIcon.Plus, "addNewCurrency", ImGui.GetContentRegionAvail(), "Add New Normal Item")) {
             System.WindowManager.AddWindow(new ItemSelectionWindow(Service.PluginInterface) {
                 MultiSelectionCallback = selected => AddSelectedItems(selected, CurrencyType.Item)
             }, WindowFlags.OpenImmediately);
         }
         
         ImGui.TableNextColumn();
         if (ImGui.Button($"{SeIconChar.HighQuality.ToIconString()}##hqSearch", ImGui.GetContentRegionAvail())) {
             System.WindowManager.AddWindow(new HighQualityItemSelectionWindow(Service.PluginInterface) {
                 MultiSelectionCallback = selected => AddSelectedItems(selected, CurrencyType.HighQualityItem),
             }, WindowFlags.OpenImmediately);
         }

         if (ImGui.IsItemHovered()) {
             ImGui.SetTooltip("Add New High Quality Item");
         }
    
         ImGui.TableNextColumn();
         if (ImGui.Button($"{SeIconChar.Collectible.ToIconString()}##collectableSearch", ImGui.GetContentRegionAvail())) {
             System.WindowManager.AddWindow(new CollectableItemSelectionWindow(Service.PluginInterface) {
                 MultiSelectionCallback = selected => AddSelectedItems(selected, CurrencyType.Collectable)
             }, WindowFlags.OpenImmediately);
         }
         
         if (ImGui.IsItemHovered()) {
             ImGui.SetTooltip("Add New Collectable Item");
         }
     }
     
     private static void AddSelectedItems(List<Item> selected, CurrencyType type) {
         foreach (var item in selected.Where(item => !System.Config.Currencies.Any(currency => currency.ItemId == item.RowId))) {
             System.Config.Currencies.Add(new TrackedCurrency {
                 Enabled = true,
                 Threshold = 1000,
                 Type = type,
                 ItemId = item.RowId,
             });
         }
     }
}

// public class ConfigurationWindow : Window {
//     private readonly List<ISelectionWindowTab> tabs;
//     private readonly List<ITabItem> regularTabs;
//
//     private readonly ItemSearchModal itemSearchModal;
//
//     public ConfigurationWindow() : base("CurrencyAlert - Configuration Window", 27.0f) {
//         tabs = new List<ISelectionWindowTab> {
//             new CurrencyConfigurationTab(),
//         };
//
//         regularTabs = new List<ITabItem> {
//             new GeneralSettingsTab(),
//         };
//
//         SizeConstraints = new Window.WindowSizeConstraints {
//             MinimumSize = new Vector2(450, 350),
//             MaximumSize = new Vector2(9999, 9999)
//         };
//
//         itemSearchModal = new ItemSearchModal {
//             Name = "Add New Currency",
//             AddAction = (item, type) => {
//                 if (CurrencyAlertSystem.Config.Currencies.Any(currency => currency.ItemId == item.RowId)) {
//                     Service.ChatGui.PrintError("Unable to add a currency that is already being tracked.");
//                 }
//                 else {
//                     CurrencyAlertSystem.Config.Currencies.Add(new TrackedCurrency {
//                         Threshold = 1000,
//                         Type = type switch {
//                             SpecialSearchType.Collectable => CurrencyType.Collectable,
//                             SpecialSearchType.HighQualityItem => CurrencyType.HighQualityItem,
//                             _ => CurrencyType.Item
//                         },
//                         Enabled = true,
//                         ItemId = item.RowId,
//
//                     });
//                     CurrencyAlertSystem.Config.Save(); 
//                 }
//             }
//         };
//
//         ShowScrollBar = false;
//
//         CommandController.RegisterCommands(this);
//     }   
//     
//     protected override IEnumerable<ISelectionWindowTab> GetTabs() => tabs;
//     protected override IEnumerable<ITabItem> GetRegularTabs() => regularTabs;
//
//     public override bool DrawConditions() {
//         if (!Service.ClientState.IsLoggedIn) return false;
//     
//         return true;
//     }
//
//     protected override void DrawExtras() {
//         ImGui.PushFont(UiBuilder.IconFont);
//         if (ImGui.Button(FontAwesomeIcon.Plus.ToIconString(), new Vector2(ImGui.GetContentRegionAvail().X, 23.0f * ImGuiHelpers.GlobalScale))) {
//             ImGui.OpenPopup("Add New Currency");
//         }
//         ImGui.PopFont();
//         
//         itemSearchModal.DrawSearchModal();
//     }
//     
//     [BaseCommandHandler("OpenConfigWindow")]
//     // ReSharper disable once UnusedMember.Local
//     private void OpenConfigWindow() {
//         if (!Service.ClientState.IsLoggedIn) return;
//
//         Toggle();
//     }
// }
//
// public class CurrencyConfigurationTab : ISelectionWindowTab {
//     public string TabName => "Currencies";
//     public ISelectable? LastSelection { get; set; }
//
//     public IEnumerable<ISelectable> GetTabSelectables() => CurrencyAlertSystem.Config.Currencies
//         .Select(currency => new TrackedCurrencySelectable(currency));
// }
//
// public class TrackedCurrencySelectable : ISelectable, IDrawable {
//     public IDrawable Contents => this;
//
//     public string ID => currency.ItemId.ToString();
//
//     private readonly TrackedCurrency currency;
//     private readonly CurrencySelectableView label;
//     private readonly CurrencyConfigView config;
//     
//     public TrackedCurrencySelectable(TrackedCurrency currency) {
//         this.currency = currency;
//         label = new CurrencySelectableView(this.currency);
//         config = new CurrencyConfigView(this.currency);
//     }
//
//     public void DrawLabel() => label.Draw();
//
//     public void Draw() => config.Draw();
// }

public class GeneralSettingsTab : ITabItem {
    public string Name => "Settings";
    public bool Disabled => false;

    public void Draw() {
        var settingsChange = false;
        
        ImGui.Text("General Settings");
        ImGui.Separator();
        ImGuiHelpers.ScaledDummy(5.0f);

        settingsChange |= ImGui.Checkbox("Enable Chat Warnings", ref System.Config.ChatWarning);
        ImGuiHelpers.ScaledDummy(5.0f);
        
        ImGui.Text("Overlay Settings");
        ImGui.Separator();
        ImGuiHelpers.ScaledDummy(5.0f);

        settingsChange |= ImGui.Checkbox("Enabled", ref System.Config.OverlayEnabled);
        ImGuiHelpers.ScaledDummy(5.0f);
        
        settingsChange |= ImGui.Checkbox("Hide In Duties", ref System.Config.HideInDuties);
        ImGuiHelpers.ScaledDummy(5.0f);

        settingsChange |= ImGui.Checkbox("Show Icon", ref System.Config.OverlayIcon);
        settingsChange |= ImGui.Checkbox("Show Text", ref System.Config.OverlayText);
        settingsChange |= ImGui.Checkbox("Show Long Text", ref System.Config.OverlayLongText);
        settingsChange |= ImGui.Checkbox("Show Background", ref System.Config.ShowBackground);
        ImGuiHelpers.ScaledDummy(5.0f);

        settingsChange |= ImGui.Checkbox("Single Line Mode", ref System.Config.SingleLine);
        ImGuiHelpers.ScaledDummy(5.0f);
        
        settingsChange |= ImGui.ColorEdit4("Text Color", ref System.Config.OverlayTextColor, ImGuiColorEditFlags.AlphaPreviewHalf);
        settingsChange |= ImGui.ColorEdit4("Background Color", ref System.Config.BackgroundColor, ImGuiColorEditFlags.AlphaPreviewHalf);

        if (ImGui.DragFloat2("Overlay Position", ref System.Config.OverlayDrawPosition, 5.0f)) {
            settingsChange = true;
        }
        
        if (settingsChange) System.Config.Save();
    }
}