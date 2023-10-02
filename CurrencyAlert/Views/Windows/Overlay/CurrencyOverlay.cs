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

public class CurrencyOverlay : Window
{
    private readonly List<TrackedCurrency> previewCurrencies = new() {
        new TrackedCurrency { Type = CurrencyType.Item, ItemId = 28, Threshold = 1400 }, // Poetics
        new TrackedCurrency { Type = CurrencyType.NonLimitedTomestone, Threshold = 1400 }, // NonLimitedTomestone
        new TrackedCurrency { Type = CurrencyType.LimitedTomestone, Threshold = 1400 }, // LimitedTomestone
        new TrackedCurrency { Type = CurrencyType.Item, ItemId = 27, Threshold = 3500 }, // AlliedSeals
        new TrackedCurrency { Type = CurrencyType.Item, ItemId = 10307, Threshold = 3500 }, // CenturioSeals
        new TrackedCurrency { Type = CurrencyType.Item, ItemId = 26533, Threshold = 3500 }, // SackOfNuts
        new TrackedCurrency { Type = CurrencyType.Item, ItemId = 26807, Threshold = 800 }, // BicolorGemstones
    };

    private static float LineHeight => 24.0f * ImGuiHelpers.GlobalScale + ImGui.GetStyle().ItemSpacing.Y;
    private static float IconSize => 24.0f * ImGuiHelpers.GlobalScale;
    private IEnumerable<TrackedCurrency> Currencies => CurrencyAlertSystem.Config is { RepositionMode: true } ? previewCurrencies : CurrencyAlertSystem.Config.Currencies;
    
    private float windowWidth;
    private float windowHeight;

    private Vector2 workingPosition;

    public CurrencyOverlay() : base("CurrencyAlert - Overlay Window2")
    {
        Flags |= ImGuiWindowFlags.NoDecoration |
                 ImGuiWindowFlags.NoDocking |
                 ImGuiWindowFlags.NoFocusOnAppearing |
                 ImGuiWindowFlags.NoNavFocus |
                 ImGuiWindowFlags.NoNavInputs |
                 ImGuiWindowFlags.NoBringToFrontOnFocus |
                 ImGuiWindowFlags.NoBackground |
                 ImGuiWindowFlags.NoInputs;

        ForceMainWindow = true;

        Position = Vector2.Zero;
        PositionCondition = ImGuiCond.Always;
        
        Size = Vector2.Zero;
        SizeCondition = ImGuiCond.Always;
    }

    public override void PreOpenCheck() => IsOpen = CurrencyAlertSystem.Config.OverlayEnabled;

    public override void Draw()
    {
        if (CurrencyAlertSystem.Config is { OverlayIcon: false, OverlayText: false }) return;
        
        windowWidth = CalculateWidth(Currencies);
        windowHeight = CalculateHeight(Currencies);
        
        if (CurrencyAlertSystem.Config.GrowUp)
        {
            workingPosition = CurrencyAlertSystem.Config.OverlayDrawPosition + new Vector2(0.0f, windowHeight);
        }
        else
        {
            workingPosition = CurrencyAlertSystem.Config.OverlayDrawPosition;
        }
        
        if (CurrencyAlertSystem.Config.ShowBackground && GetActiveCurrencies(Currencies).Any())
        {
            ImGui.GetBackgroundDrawList().AddRectFilled(
                CurrencyAlertSystem.Config.OverlayDrawPosition - ImGuiHelpers.ScaledVector2(5.0f), 
                CurrencyAlertSystem.Config.OverlayDrawPosition + new Vector2(windowWidth, windowHeight) + ImGuiHelpers.ScaledVector2(5.0f), 
                ImGui.GetColorU32(CurrencyAlertSystem.Config.BackgroundColor), 
                5.0f);  
        }
        
        if (CurrencyAlertSystem.Config.RepositionMode)
        {
            if (CurrencyAlertSystem.Config.WindowPosChanged)
            {
                ImGui.SetNextWindowPos(CurrencyAlertSystem.Config.OverlayDrawPosition, ImGuiCond.Always);
                CurrencyAlertSystem.Config.WindowPosChanged = false;
            }
            else
            {
                ImGui.SetNextWindowPos(CurrencyAlertSystem.Config.OverlayDrawPosition, ImGuiCond.Appearing);
            }
            
            ImGui.SetNextWindowSize(new Vector2(windowWidth, windowHeight));
            if (ImGui.Begin("##Draggable", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoBackground))
            {
                if (ImGui.IsWindowFocused())
                {
                    if (CurrencyAlertSystem.Config.OverlayDrawPosition != ImGui.GetWindowPos())
                    {
                        CurrencyAlertSystem.Config.OverlayDrawPosition = ImGui.GetWindowPos();
                        CurrencyAlertSystem.Config.Save();
                    }
                }
                
                ImGui.GetBackgroundDrawList().AddRect(
                    CurrencyAlertSystem.Config.OverlayDrawPosition - ImGuiHelpers.ScaledVector2(5.0f), 
                    CurrencyAlertSystem.Config.OverlayDrawPosition + new Vector2(windowWidth, windowHeight) + ImGuiHelpers.ScaledVector2(5.0f), 
                    ImGui.GetColorU32(KnownColor.OrangeRed.Vector() with { W = 0.50f }), 
                    5.0f,
                    ImDrawFlags.None,
                    3.0f);

                const string warningText = "Preview/Reposition Mode Enabled";
                var textSize = ImGui.CalcTextSize(warningText);
                ImGui.GetBackgroundDrawList().AddText(
                    CurrencyAlertSystem.Config.OverlayDrawPosition + new Vector2(windowWidth / 2.0f - textSize.X / 2.0f, -textSize.Y - 5.0f * ImGuiHelpers.GlobalScale),
                    ImGui.GetColorU32(KnownColor.OrangeRed.Vector()),
                    warningText);
            }
            ImGui.End();
        }
        
        DrawCurrencyList(Currencies);
        
        // Debug Location Info, draws dots around what I consider to be the "window"
        // ImGui.GetBackgroundDrawList().AddCircleFilled(CurrencyAlertSystem.Config.OverlayDrawPosition, 5.0f, ImGui.GetColorU32(KnownColor.Orange.Vector()));
        // ImGui.GetBackgroundDrawList().AddCircleFilled(CurrencyAlertSystem.Config.OverlayDrawPosition + new Vector2(windowWidth, 0.0f), 5.0f, ImGui.GetColorU32(KnownColor.Red.Vector()));
        // ImGui.GetBackgroundDrawList().AddCircleFilled(CurrencyAlertSystem.Config.OverlayDrawPosition + new Vector2(0.0f, windowHeight), 5.0f, ImGui.GetColorU32(KnownColor.Green.Vector()));
        // ImGui.GetBackgroundDrawList().AddCircleFilled(CurrencyAlertSystem.Config.OverlayDrawPosition + new Vector2(windowWidth, windowHeight), 5.0f, ImGui.GetColorU32(KnownColor.Purple.Vector()));
    }

    private void DrawCurrencyList(IEnumerable<TrackedCurrency> currencies)
    {
        foreach (var currency in currencies)
        {
            if (currency is { ShowInOverlay: true, Enabled: true } && currency.CurrentCount > currency.Threshold || CurrencyAlertSystem.Config.RepositionMode)
            {
                if (CurrencyAlertSystem.Config.GrowUp) workingPosition.Y -= LineHeight;

                ImGui.SetCursorScreenPos(workingPosition);
                DrawCurrency(currency);

                if (!CurrencyAlertSystem.Config.GrowUp) workingPosition.Y += LineHeight;
            }
        }
    }
    
    private void DrawCurrency(TrackedCurrency currency)
    {
        if (currency is { Icon: null } or { Name: "" }) return;
        
        var icon = currency.Icon;
        var iconEnabled = CurrencyAlertSystem.Config is { OverlayIcon: true };
        var textEnabled = CurrencyAlertSystem.Config is { OverlayText: true };
        var longTextLabel = CurrencyAlertSystem.Config is { OverlayLongText: true };
        var text = GetLabelForCurrency(currency, longTextLabel);
        var textColor = CurrencyAlertSystem.Config.OverlayTextColor;
        var textSize = ImGui.CalcTextSize(text);
        
        if (CurrencyAlertSystem.Config.RightAlign)
        {
            switch (textEnabled, iconEnabled)
            {
                case { textEnabled: true, iconEnabled: true }:
                    ImGui.SetCursorPosX(workingPosition.X + windowWidth - textSize.X - ImGui.GetStyle().ItemSpacing.X - IconSize);
                    ImGui.TextColored(textColor, text);
                    ImGui.SameLine();
                    ImGui.Image(icon.ImGuiHandle, new Vector2(IconSize));
                    break;

                case { textEnabled: true, iconEnabled: false }:
                    ImGui.SetCursorPosX(workingPosition.X + windowWidth - textSize.X);
                    ImGui.TextColored(textColor, text);
                    break;

                case { textEnabled: false, iconEnabled: true }:
                    ImGui.SetCursorPosX(workingPosition.X + windowWidth - IconSize);
                    ImGui.Image(icon.ImGuiHandle, new Vector2(IconSize));
                    break;
            }
        }
        else
        {
            if (iconEnabled) ImGui.Image(icon.ImGuiHandle, new Vector2(IconSize));
            if (textEnabled && iconEnabled) ImGui.SameLine();
            if (textEnabled) ImGui.TextColored(textColor, text);
        }
    }

    private static float CalculateHeight(IEnumerable<TrackedCurrency> currencies)
        => GetActiveCurrencies(currencies).Count() * (IconSize * ImGuiHelpers.GlobalScale + ImGui.GetStyle().ItemSpacing.Y);

    private float CalculateWidth(IEnumerable<TrackedCurrency> currencies) 
        => GetCurrencyWidths(currencies).Prepend(0.0f).Max();

    private IEnumerable<float> GetCurrencyWidths(IEnumerable<TrackedCurrency> currencies)
        => GetActiveCurrencies(currencies).Select(GetLineWidth);
    
    private static IEnumerable<TrackedCurrency> GetActiveCurrencies(IEnumerable<TrackedCurrency> currencies)
        => currencies.Where(currency => currency.CurrentCount > currency.Threshold || CurrencyAlertSystem.Config.RepositionMode);

    private float GetLineWidth(TrackedCurrency currency) => CurrencyAlertSystem.Config switch
    {
        { OverlayIcon: true, OverlayText: true, OverlayLongText: true } 
            => IconSize + ImGui.GetStyle().ItemSpacing.X + ImGui.CalcTextSize(GetLabelForCurrency(currency, true)).X,
        
        { OverlayIcon: true, OverlayText: true, OverlayLongText: false } 
            => IconSize + ImGui.GetStyle().ItemSpacing.X + ImGui.CalcTextSize(GetLabelForCurrency(currency, false)).X,

        { OverlayIcon: true, OverlayText: false } 
            => IconSize,
        
        { OverlayIcon: false, OverlayText: true, OverlayLongText: false } 
            => ImGui.CalcTextSize(GetLabelForCurrency(currency, false)).X,
        
        { OverlayIcon: false, OverlayText: true, OverlayLongText: true } 
            => ImGui.CalcTextSize(GetLabelForCurrency(currency, true)).X,
        
        _ => 0.0f
    };

    private static string GetLabelForCurrency(TrackedCurrency currency, bool longLabel)
        => longLabel ? $"{currency.Name} is above threshold" : $"{currency.Name}";
}