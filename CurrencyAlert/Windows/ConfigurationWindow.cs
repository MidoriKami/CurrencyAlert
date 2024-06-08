using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using CurrencyAlert.Classes;
using Dalamud.Game.Text;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using KamiLib.CommandManager;
using KamiLib.Components;
using KamiLib.Window;
using KamiLib.Window.SelectionWindows;
using Lumina.Excel.GeneratedSheets;

namespace CurrencyAlert.Windows;

public class ConfigurationWindow : TabbedSelectionWindow<TrackedCurrency> {
    protected override List<TrackedCurrency> Options => System.Config.Currencies;
    protected override float SelectionListWidth { get; set; } = 150.0f;
    protected override float SelectionItemHeight => 20.0f;
    protected override bool AllowChildScroll => false;
    protected override string SelectionListTabName => "Tracked Currencies";
    protected override bool ShowListButton => true;

    protected override List<ITabItem> Tabs { get; } = [
        new GeneralSettingsTab()
    ];
    
    public ConfigurationWindow() : base("CurrencyAlert Configuration Window", new Vector2(450.0f, 400.0f), true) {
        System.CommandManager.RegisterCommand(new CommandHandler {
            Delegate = _ => Toggle(),
            ActivationPath = "/"
        });
    }

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
         
         ImGuiHelpers.ScaledDummy(5.0f);
     }

     private void DrawSettings(TrackedCurrency currency) {
         if (currency is not {
                 ItemId: var itemId, 
                 OverlayWarningText: var warningText, 
                 Name: var itemName, 
                 ShowItemName: var showName, 
                 Enabled: var enabled, 
                 ChatWarning: var chatWarning, 
                 ShowInOverlay: var overlay, 
                 Invert: var invert, 
                 Threshold: var threshold
             }) return;
         
         if (ImGui.Checkbox($"Enable##{itemId}", ref enabled)) {
             currency.Enabled = enabled;
             System.Config.Save();
         }

         ImGuiHelpers.ScaledDummy(5.0f);
         
         if (ImGuiTweaks.Checkbox($"Chat Warning##{itemId}", ref chatWarning, "When amount is above threshold, print a message to chat when changing zones")) {
             currency.ChatWarning = chatWarning;
             System.Config.Save();
         }

         if (ImGuiTweaks.Checkbox($"Invert##{itemId}", ref invert, "Warn when below the threshold instead of above")) {
             currency.Invert = invert;
             System.Config.Save();
         }
         
         if (ImGuiTweaks.Checkbox($"Overlay##{itemId}", ref overlay, "Allows this currency to show in the overlay")) {
             currency.ShowInOverlay = overlay;
             System.Config.Save();
         }

         if (ImGuiTweaks.Checkbox("Overlay Show Name", ref showName, "Show item name in the overlay")) {
             currency.ShowItemName = showName;
             
             if (currency.WarningNode is not null) {
                 currency.WarningNode.WarningText = showName ? $"{itemName} {currency.OverlayWarningText}" : $"{currency.OverlayWarningText}";
                 System.OverlayController.UpdateSettings();
             }
             
             System.Config.Save();
         }
         
         ImGuiHelpers.ScaledDummy(5.0f);

         var warningTextTempString = warningText.ToString();
         if (ImGui.InputText("Warning Text", ref warningTextTempString, 1024)) {
             currency.OverlayWarningText = warningTextTempString;
             
             if (currency.WarningNode is not null) {
                 currency.WarningNode.WarningText = showName ? $"{itemName} {currency.OverlayWarningText}" : $"{currency.OverlayWarningText}";
                 System.OverlayController.UpdateSettings();
             }
             
             System.Config.Save();
         }

         ImGuiHelpers.ScaledDummy(5.0f);
         
         ImGui.PushItemWidth(50.0f * ImGuiHelpers.GlobalScale);
         if (ImGui.InputInt($"Threshold##{itemId}", ref threshold, 0, 0)) {
             currency.Threshold = threshold;
             System.Config.Save();
         }
         
         ImGui.SetCursorPosY(ImGui.GetContentRegionMax().Y - 23.0f * ImGuiHelpers.GlobalScale);
         using (var _ = ImRaii.Disabled(!(ImGui.GetIO().KeyShift && ImGui.GetIO().KeyCtrl && currency.CanRemove))) {
             if (ImGuiTweaks.IconButtonWithSize(Service.PluginInterface.UiBuilder.IconFontFixedWidthHandle, FontAwesomeIcon.Trash, "Delete", new Vector2(ImGui.GetContentRegionAvail().X, 23.0f * ImGuiHelpers.GlobalScale))) {
                 System.Config.Currencies.Remove(currency);
                 DeselectItem();
                 System.Config.Save();
             }
         }

         if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled)) {
             ImGui.SetTooltip(currency.CanRemove ? "Hold Shift + Control while clicking to delete this currency" : "Special currencies cannot be removed");
         }
     }
     
     protected override void DrawExtraButton() {
         using var spacingTable = ImRaii.Table("buttonTable", 3, ImGuiTableFlags.SizingStretchSame);
         if (!spacingTable) return;

         ImGui.TableNextColumn();
         if (ImGuiTweaks.IconButtonWithSize(Service.PluginInterface.UiBuilder.IconFontFixedWidthHandle, FontAwesomeIcon.Plus, "addNewCurrency", ImGui.GetContentRegionAvail(), "Add New Normal Item")) {
             TryCloseAndRemoveItemListWindow();
             
             System.WindowManager.AddWindow(new ItemSelectionWindow(Service.PluginInterface) {
                 MultiSelectionCallback = selected => AddSelectedItems(selected, CurrencyType.Item)
             }, WindowFlags.OpenImmediately);
         }
         
         ImGui.TableNextColumn();
         if (ImGui.Button($"{SeIconChar.HighQuality.ToIconString()}##hqSearch", ImGui.GetContentRegionAvail())) {
             TryCloseAndRemoveItemListWindow();
             
             System.WindowManager.AddWindow(new HighQualityItemSelectionWindow(Service.PluginInterface) {
                 MultiSelectionCallback = selected => AddSelectedItems(selected, CurrencyType.HighQualityItem),
             }, WindowFlags.OpenImmediately);
         }

         if (ImGui.IsItemHovered()) {
             ImGui.SetTooltip("Add New High Quality Item");
         }
    
         ImGui.TableNextColumn();
         if (ImGui.Button($"{SeIconChar.Collectible.ToIconString()}##collectableSearch", ImGui.GetContentRegionAvail())) {
             TryCloseAndRemoveItemListWindow();
             
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

     private void TryCloseAndRemoveItemListWindow() {
         if (System.WindowManager.GetWindow<SelectionWindowBase<Item>>() is { } existingWindow) {
             existingWindow.Close();
             System.WindowManager.RemoveWindow(existingWindow);
         }
     }
}

public class GeneralSettingsTab : ITabItem {
    public string Name => "Settings";
    public bool Disabled => false;

    public void Draw() {
        ImGui.Text("General Settings");
        ImGui.Separator();
        ImGuiHelpers.ScaledDummy(5.0f);

        if (ImGui.Checkbox("Enable Chat Warnings", ref System.Config.ChatWarning)) {
            System.Config.Save();
        }
        ImGuiHelpers.ScaledDummy(5.0f);

        ImGui.Text("Overlay Settings");
        ImGui.Separator();
        ImGuiHelpers.ScaledDummy(5.0f);

        if (ImGui.Checkbox("Enabled", ref System.Config.OverlayEnabled)) {
            System.OverlayController.UpdateSettings();
            System.Config.Save();
        }
        ImGuiHelpers.ScaledDummy(5.0f);

        if (ImGui.Checkbox("Hide In Duties", ref System.Config.HideInDuties)) {
            System.Config.Save();
        }
        ImGuiHelpers.ScaledDummy(5.0f);

        if (ImGui.Checkbox("Show Icon", ref System.Config.OverlayIcon)) {
            System.OverlayController.UpdateSettings();
            System.Config.Save();
        }

        if (ImGui.Checkbox("Show Text", ref System.Config.OverlayText)) {
            System.OverlayController.UpdateSettings();
            System.Config.Save();
        }

        if (ImGui.Checkbox("Show Background", ref System.Config.ShowBackground)) {
            System.OverlayController.UpdateSettings();
            System.Config.Save();
        }
        ImGuiHelpers.ScaledDummy(5.0f);

        if (ImGui.Checkbox("Single Line Mode", ref System.Config.SingleLine)) {
            System.OverlayController.UpdateSettings();
            System.Config.Save();
        }
        ImGuiHelpers.ScaledDummy(5.0f);

        if (ImGuiTweaks.EnumCombo("Anchor Corner", ref System.Config.LayoutAnchor)) {
            System.OverlayController.UpdateSettings();
            System.Config.Save();
        }
        ImGuiHelpers.ScaledDummy(5.0f);
        
        if (ImGuiTweaks.Checkbox("List Background", ref System.Config.ShowListBackground, "Useful for seeing where the list is for size/positioning")) {
            System.OverlayController.UpdateSettings();
            System.Config.Save();
        }
        
        if (ImGui.ColorEdit4("List Background Color", ref System.Config.ListBackgroundColor, ImGuiColorEditFlags.AlphaPreviewHalf)) {
            System.OverlayController.UpdateSettings();
            System.Config.Save();
        }
        
        ImGuiHelpers.ScaledDummy(5.0f);
        
        if (ImGui.ColorEdit4("Text Color", ref System.Config.OverlayTextColor, ImGuiColorEditFlags.AlphaPreviewHalf)) {
            System.OverlayController.UpdateSettings();
            System.Config.Save();
        }

        if (ImGui.ColorEdit4("Warning Background Color", ref System.Config.BackgroundColor, ImGuiColorEditFlags.AlphaPreviewHalf)) {
            System.OverlayController.UpdateSettings();
            System.Config.Save();
        }
        
        if (ImGui.DragFloat2("Overlay Position", ref System.Config.OverlayDrawPosition, 5.0f)) {
            System.OverlayController.UpdateSettings();
            System.Config.Save();
        }
        
        if (ImGui.DragFloat2("Overlay Size", ref System.Config.OverlaySize, 5.0f)) {
            System.OverlayController.UpdateSettings();
            System.Config.Save();
        }
    }
}