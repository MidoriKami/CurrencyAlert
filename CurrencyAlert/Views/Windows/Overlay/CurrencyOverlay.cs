using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using CurrencyAlert.Controllers;
using CurrencyAlert.Models;
using CurrencyAlert.Models.Enums;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace CurrencyAlert.Views.Windows.Overlay;

public class CurrencyOverlay : Window {
    private readonly List<TrackedCurrency> previewCurrencies = new() {
        new TrackedCurrency { Type = CurrencyType.Item, ItemId = 28, Threshold = 1400 }, // Poetics
        new TrackedCurrency { Type = CurrencyType.NonLimitedTomestone, Threshold = 1400 }, // NonLimitedTomestone
        new TrackedCurrency { Type = CurrencyType.LimitedTomestone, Threshold = 1400 }, // LimitedTomestone
        new TrackedCurrency { Type = CurrencyType.Item, ItemId = 27, Threshold = 3500 }, // AlliedSeals
        new TrackedCurrency { Type = CurrencyType.Item, ItemId = 10307, Threshold = 3500 }, // CenturioSeals
        new TrackedCurrency { Type = CurrencyType.Item, ItemId = 26533, Threshold = 3500 }, // SackOfNuts
        new TrackedCurrency { Type = CurrencyType.Item, ItemId = 26807, Threshold = 800 }, // BicolorGemstones
    };

    private static float IconSize => 24.0f * ImGuiHelpers.GlobalScale;
    private List<TrackedCurrency> Currencies => CurrencyAlertSystem.Config is { RepositionMode: true } ? previewCurrencies : CurrencyAlertSystem.Config.Currencies;
    
    public CurrencyOverlay() : base("CurrencyAlert - Overlay Window") {
        Flags |= ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar;

        ForceMainWindow = true;
    }

    public override void PreOpenCheck() 
        => IsOpen = CurrencyAlertSystem.Config.OverlayEnabled && 
                    (HasActiveWarnings(Currencies) || CurrencyAlertSystem.Config.RepositionMode) && 
                    Service.ClientState.IsLoggedIn;

    public override void PreDraw() {
        Flags |= ImGuiWindowFlags.NoMove;
        if (CurrencyAlertSystem.Config.RepositionMode) Flags &= ~ImGuiWindowFlags.NoMove;

        Flags |= ImGuiWindowFlags.NoBackground;
        if (CurrencyAlertSystem.Config.ShowBackground) Flags &= ~ImGuiWindowFlags.NoBackground;
        
        ImGui.PushStyleColor(ImGuiCol.WindowBg, CurrencyAlertSystem.Config.BackgroundColor);
    }

    public override void Draw() {
        if (!ImGui.IsWindowFocused() && CurrencyAlertSystem.Config.WindowPosChanged) {
            ImGui.SetWindowPos(CurrencyAlertSystem.Config.OverlayDrawPosition);
        }
        else {
            CurrencyAlertSystem.Config.OverlayDrawPosition = ImGui.GetWindowPos();
        }
        
        foreach (var currency in Currencies) {
            if (currency is { ShowInOverlay: true, Enabled: true, HasWarning: true } || CurrencyAlertSystem.Config.RepositionMode) {
                DrawCurrency(currency);
            }
        }

        if (CurrencyAlertSystem.Config.RepositionMode) {
            const string text = "Reposition/Sample Mode Active";
            
            var textSize = ImGui.CalcTextSize(text);
            var startY = ImGui.GetWindowPos().Y - textSize.Y - ImGui.GetStyle().ItemSpacing.Y;
            var startX = ImGui.GetWindowPos().X + ImGui.GetWindowSize().X / 2.0f - textSize.X / 2.0f;
            ImGui.GetBackgroundDrawList().AddText(new Vector2(startX, startY), ImGui.GetColorU32(KnownColor.OrangeRed.Vector()), text);
        }
    }

    public override void PostDraw() {
        ImGui.PopStyleColor();
    }

    private void DrawCurrency(TrackedCurrency currency) {
        if (currency is { Icon: null } or { Name: "" }) return;
        
        var icon = currency.Icon;
        var iconEnabled = CurrencyAlertSystem.Config is { OverlayIcon: true };
        var textEnabled = CurrencyAlertSystem.Config is { OverlayText: true };
        var longTextLabel = CurrencyAlertSystem.Config is { OverlayLongText: true };
        var text = GetLabelForCurrency(currency, longTextLabel);
        var textColor = CurrencyAlertSystem.Config.OverlayTextColor;
        
        if (iconEnabled) {
            ImGui.Image(icon.ImGuiHandle, new Vector2(IconSize));
        }
        
        if (textEnabled && iconEnabled) {
            ImGui.SameLine();
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 3.0f * ImGuiHelpers.GlobalScale);
        }
        
        if (textEnabled && !iconEnabled) {
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 3.0f * ImGuiHelpers.GlobalScale);
        }
        
        if (textEnabled) {
            ImGui.TextColored(textColor, text);
        }

        if (CurrencyAlertSystem.Config.SingleLine) {
            ImGui.SameLine();
        }
    }

    private static string GetLabelForCurrency(TrackedCurrency currency, bool longLabel)
        => longLabel ? $"{currency.Name} is {(currency.Invert ? "below" : "above")} threshold" : $"{currency.Name}";

    private static bool HasActiveWarnings(IEnumerable<TrackedCurrency> currencies)
        => currencies.Any(currency => currency is { HasWarning: true, Enabled: true, ShowInOverlay: true});
}