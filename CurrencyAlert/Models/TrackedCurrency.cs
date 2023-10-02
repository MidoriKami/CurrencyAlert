using System;
using System.Linq;
using CurrencyAlert.Models.Enums;
using Dalamud.Interface.Internal;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;

namespace CurrencyAlert.Models;

public unsafe class TrackedCurrency
{
    private IDalamudTextureWrap? iconTexture;
    private uint? itemId;
    private string? label;

    public required CurrencyType Type { get; init; }

    [JsonIgnore] 
    public IDalamudTextureWrap? Icon => GetIcon();

    public uint ItemId
    {
        get => GetItemId();
        init => itemId = value;
    }

    public required int Threshold { get; set; }

    public bool Enabled { get; set; } = true;

    public bool ChatWarning { get; set; }
    
    public bool ShowInOverlay { get; set; }

    [JsonIgnore] 
    public string Name => label ??= Service.DataManager.GetExcelSheet<Item>()!.GetRow(ItemId)?.Name ?? "Unable to read name";

    [JsonIgnore] 
    public bool CanRemove => Type is not (CurrencyType.LimitedTomestone or CurrencyType.NonLimitedTomestone);

    [JsonIgnore]
    public int CurrentCount => InventoryManager.Instance()->GetInventoryItemCount(ItemId, Type is CurrencyType.HighQualityItem, false, false);

    private uint GetItemId()
    {
        itemId ??= Type switch
        {
            CurrencyType.NonLimitedTomestone => Service.DataManager.GetExcelSheet<TomestonesItem>()!.First(item => item.Tomestones.Row is 2).Item.Row,
            CurrencyType.LimitedTomestone => Service.DataManager.GetExcelSheet<TomestonesItem>()!.First(item => item.Tomestones.Row is 3).Item.Row,
            _ => throw new Exception($"ItemId not initialized for type: {Type}")
        };

        return itemId.Value;
    }

    private IDalamudTextureWrap? GetIcon()
    {
        if (iconTexture is null && Service.DataManager.GetExcelSheet<Item>()!.GetRow(ItemId) is { Icon: var iconId })
        {
            var iconFlags = Type switch
            {
                CurrencyType.HighQualityItem => ITextureProvider.IconFlags.HiRes | ITextureProvider.IconFlags.ItemHighQuality,
                _ => ITextureProvider.IconFlags.HiRes,
            };
            
            return iconTexture ??= Service.TextureProvider.GetIcon(iconId, iconFlags);
        }

        return iconTexture;
    }
}