﻿using System;
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
using KamiLib.Classes;
using KamiLib.CommandManager;
using KamiLib.Window;
using KamiLib.Window.SelectionWindows;
using Lumina.Excel.Sheets;

namespace CurrencyAlert.Windows;

public class ConfigurationWindow : TabbedSelectionWindow<TrackedCurrency> {

    public ConfigurationWindow() : base("CurrencyAlert Configuration Window", new Vector2(450.0f, 400.0f)) {
        System.CommandManager.RegisterCommand(new CommandHandler {
            Delegate = _ => Toggle(), ActivationPath = "/",
        });
    }

    protected override List<TrackedCurrency> Options => System.Config.Currencies;
    
    protected override float SelectionListWidth { get; set; } = 150.0f;
    
    protected override float SelectionItemHeight => 20.0f;
    
    protected override bool AllowChildScroll => false;
    
    protected override string SelectionListTabName => "Tracked Currencies";
    
    protected override bool ShowListButton => true;

    protected override List<ITabItem> Tabs { get; } = [
        new GeneralSettingsTab(),
        new ListNodeSettingsTab(),
        new CurrencyNodeSettingsTab(),
    ];

    protected override void DrawListOption(TrackedCurrency option) {
        // If ID is zero, and type is LimitedTomestone, then the limited tomestone doesn't exist.
        // This only happens between expansion release and the release of savage, so this won't be relevant again for 2-3 years.
        if (option is { ItemId: 0, Type: CurrencyType.LimitedTomestone }) {
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 3.0f * ImGuiHelpers.GlobalScale);
            ImGui.Image(Service.TextureProvider.GetFromGameIcon(60071).GetWrapOrEmpty().ImGuiHandle, ImGuiHelpers.ScaledVector2(24.0f));

            ImGui.SameLine();
            ImGui.AlignTextToFramePadding();
            ImGui.Text("Limited Tomestone (Currently Unavailable)");

            return;
        }

        if (option is { Name: var name, Icon: { } icon }) {
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
        var region = ImGui.GetContentRegionAvail();
        var minDimension = Math.Min(region.X, region.Y);

        // If ID is zero, and type is LimitedTomestone, then the limited tomestone doesn't exist.
        // This only happens between expansion release and the release of savage, so this won't be relevant again for 2-3 years.
        if (currency is { ItemId: 0, Type: CurrencyType.LimitedTomestone }) {
            const string text = "Limited Tomestone (Currently Unavailable)";
            var textSize = ImGui.CalcTextSize(text);
            ImGui.SetCursorPosX(ImGui.GetContentRegionAvail().X / 2.0f - textSize.X / 2.0f);
            ImGui.Text(text);
            ImGui.Separator();

            var areaStart = ImGui.GetCursorPos();
            ImGui.SetCursorPosX(region.X / 2.0f - minDimension / 2.0f);
            ImGui.Image(Service.TextureProvider.GetFromGameIcon(60071).GetWrapOrEmpty().ImGuiHandle, new Vector2(minDimension), Vector2.Zero, Vector2.One, Vector4.One with {
                W = 0.10f,
            });
            ImGui.SetCursorPos(areaStart);

            return;
        }

        if (currency is { Name: var name }) {
            var textSize = ImGui.CalcTextSize(name);
            ImGui.SetCursorPosX(ImGui.GetContentRegionAvail().X / 2.0f - textSize.X / 2.0f);
            ImGui.Text(name);
            ImGui.Separator();

            var areaStart = ImGui.GetCursorPos();
            ImGui.SetCursorPosX(region.X / 2.0f - minDimension / 2.0f);
            ImGui.Image(currency.Icon.ImGuiHandle, new Vector2(minDimension), Vector2.Zero, Vector2.One, Vector4.One with {
                W = 0.10f,
            });
            ImGui.SetCursorPos(areaStart);
        }
    }

    private void DrawCurrentStatus(TrackedCurrency currency) {
        if (currency is not { CurrentCount: var currentCount, Threshold: var threshold }) return;

        var color = ((float) currentCount / threshold) switch {
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
        using var id = ImRaii.PushId(currency.ItemId.ToString());
        
        var configChanged = false;

        configChanged |= ImGui.Checkbox($"Enable", ref currency.Enabled);

        ImGuiHelpers.ScaledDummy(5.0f);

        configChanged |= ImGuiTweaks.Checkbox("Chat Warning", ref currency.ChatWarning, "When amount is above threshold, print a message to chat when changing zones");
        configChanged |= ImGuiTweaks.Checkbox("Invert", ref currency.Invert, "Warn when below the threshold instead of above");
        configChanged |= ImGuiTweaks.Checkbox("Overlay", ref currency.ShowInOverlay, "Allows this currency to show in the overlay");
        configChanged |= ImGuiTweaks.Checkbox("Overlay Show Name", ref currency.ShowItemName, "Show item name in the overlay");

        ImGuiHelpers.ScaledDummy(5.0f);

        if (ImGui.InputTextWithHint("##WarningText","Warning Text", ref currency.WarningText, 1024)) {
            configChanged = true;
        }

        ImGuiHelpers.ScaledDummy(5.0f);

        ImGui.PushItemWidth(50.0f * ImGuiHelpers.GlobalScale);
        configChanged |= ImGui.InputInt($"Threshold", ref currency.Threshold, 0, 0);

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

        if (configChanged) {
            System.Config.Save();
        }
    }

    protected override void DrawExtraButton() {
        using var spacingTable = ImRaii.Table("buttonTable", 3, ImGuiTableFlags.SizingStretchSame);
        if (!spacingTable) return;

        ImGui.TableNextColumn();
        if (ImGuiTweaks.IconButtonWithSize(Service.PluginInterface.UiBuilder.IconFontFixedWidthHandle, FontAwesomeIcon.Plus, "addNewCurrency", ImGui.GetContentRegionAvail(), "Add New Normal Item")) {
            TryCloseAndRemoveItemListWindow();

            System.WindowManager.AddWindow(new ItemSelectionWindow(Service.PluginInterface) {
                MultiSelectionCallback = selected => AddSelectedItems(selected, CurrencyType.Item),
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
                MultiSelectionCallback = selected => AddSelectedItems(selected, CurrencyType.Collectable),
            }, WindowFlags.OpenImmediately);
        }

        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("Add New Collectable Item");
        }
    }

    private static void AddSelectedItems(List<Item> selected, CurrencyType type) {
        foreach (var item in selected.Where(item => !System.Config.Currencies.Any(currency => currency.ItemId == item.RowId))) {
            System.Config.Currencies.Add(new TrackedCurrency {
                Enabled = true, Threshold = 1000, Type = type, ItemId = item.RowId,
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
        var configChanged = false;

        ImGuiTweaks.Header("General Settings");
        using (ImRaii.PushIndent()) {
            configChanged |= ImGui.Checkbox("Enable Chat Warnings", ref System.Config.ChatWarning);
        }

        ImGuiTweaks.Header("Overlay Settings");
        using (ImRaii.PushIndent()) {
            configChanged |= ImGui.Checkbox("Hide in Duties", ref System.Config.HideInDuties);
        }
        
        if (configChanged) {
            System.Config.Save();
        }
    }
}

public class ListNodeSettingsTab : ITabItem {
    public string Name => "Warning List Style";
    
    public bool Disabled => false;
    
    public void Draw() {
        var listNode = System.OverlayController.OverlayListNode;
        if (listNode is null) return;
        
        ImGuiTweaks.Header("Warning List Overlay Style");
        using (var child = ImRaii.Child("list_config", ImGui.GetContentRegionAvail() - new Vector2(0.0f, 33.0f))) {
            if (child) {
                listNode.DrawConfig();
            }
        }
        
        ImGui.Separator();
        
        if (ImGui.Button("Save", ImGuiHelpers.ScaledVector2(100.0f, 23.0f))) {
            listNode.Save(OverlayController.ListNodeConfigPath);
            listNode.RecalculateLayout();
        }
        
        ImGui.SameLine(ImGui.GetContentRegionMax().X / 2.0f - 75.0f * ImGuiHelpers.GlobalScale);
        if (ImGui.Button("Refresh Layout", ImGuiHelpers.ScaledVector2(150.0f, 23.0f))) {
            listNode.RecalculateLayout();
        }

        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("Triggers a refresh of the UI element to recalculate dynamic element size/positions");
        }
        
        ImGui.SameLine(ImGui.GetContentRegionMax().X - 100.0f * ImGuiHelpers.GlobalScale);
        ImGuiTweaks.DisabledButton("Reset", () => {
            listNode.Load(OverlayController.ListNodeConfigPath);
            listNode.RecalculateLayout();
        });
    }
}

public class CurrencyNodeSettingsTab : ITabItem {
    public string Name => "Currency Node Style";
    
    public bool Disabled => false;
    
    public void Draw() {
        var listNode = System.OverlayController.OverlayListNode;
        if (listNode is null) return;

        var firstNode = listNode.FirstOrDefault();
        if (firstNode is null) return;
        
        ImGuiTweaks.Header("Currency Node Overlay Style");

        ImGui.Spacing();
        ImGui.TextColored(KnownColor.GreenYellow.Vector(), "Modifications will only appear to effect the first warning, but once saved will apply to all warnings");
        ImGui.Spacing();
        ImGui.Spacing();
        
        using (var child = ImRaii.Child("currency_style", ImGui.GetContentRegionAvail() - new Vector2(0.0f, 33.0f))) {
            if (child) {
                firstNode.DrawConfig();
            }
        }
        
        ImGui.Separator();
        
        if (ImGui.Button("Save", ImGuiHelpers.ScaledVector2(100.0f, 23.0f))) {
            firstNode.Save(OverlayController.CurrencyNodeConfigPath);

            foreach (var node in listNode) {
                node.Load(OverlayController.CurrencyNodeConfigPath);
            }
                
            listNode.RecalculateLayout();
        }
        
        ImGui.SameLine(ImGui.GetContentRegionMax().X / 2.0f - 75.0f * ImGuiHelpers.GlobalScale);
        if (ImGui.Button("Refresh Layout", ImGuiHelpers.ScaledVector2(150.0f, 23.0f))) {
            listNode.RecalculateLayout();
        }

        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("Triggers a refresh of the UI element to recalculate dynamic element size/positions");
        }
        
        ImGui.SameLine(ImGui.GetContentRegionMax().X - 100.0f * ImGuiHelpers.GlobalScale);
        ImGuiTweaks.DisabledButton("Reset", () => {
            foreach (var node in listNode) {
                node.Load(OverlayController.CurrencyNodeConfigPath);
            }
            
            listNode.RecalculateLayout();
        });
    }
}