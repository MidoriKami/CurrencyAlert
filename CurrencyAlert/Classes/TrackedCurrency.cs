using System;
using System.Linq;
using System.Text.Json.Serialization;
using CurrencyAlert.Models.Enums;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Textures;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.GeneratedSheets;

namespace CurrencyAlert.Models;

public unsafe class TrackedCurrency {
    private uint? itemId;
    private string? label;
    private uint? iconId;

    public required CurrencyType Type { get; init; }

    [JsonIgnore]
    public IDalamudTextureWrap Icon => Service.TextureProvider.GetFromGameIcon(new GameIconLookup {
        HiRes = true,
        ItemHq = Type is CurrencyType.HighQualityItem,
        IconId = iconId ??= Service.DataManager.GetExcelSheet<Item>()!.GetRow(ItemId)?.Icon ?? 0,
    }).GetWrapOrEmpty();

    public uint ItemId {
        get => GetItemId();
        init => itemId = value;
    }

    public required int Threshold { get; set; }

    public bool Enabled { get; set; } = true;

    public bool ChatWarning { get; set; }
    
    public bool ShowInOverlay { get; set; }

    public bool Invert { get; set; }
    
    [JsonIgnore] 
    public string Name => label ??= Service.DataManager.GetExcelSheet<Item>()!.GetRow(ItemId)?.Name ?? "Unable to read name";

    [JsonIgnore] 
    public bool CanRemove => Type is not (CurrencyType.LimitedTomestone or CurrencyType.NonLimitedTomestone);

    [JsonIgnore] 
    public int CurrentCount => InventoryManager.Instance()->GetInventoryItemCount(ItemId, Type is CurrencyType.HighQualityItem, false, false);

    [JsonIgnore] 
    public bool HasWarning => Invert ? CurrentCount < Threshold : CurrentCount > Threshold;

    private uint GetItemId() {
        itemId ??= Type switch {
            CurrencyType.NonLimitedTomestone => Service.DataManager.GetExcelSheet<TomestonesItem>()!.First(item => item.Tomestones.Row is 2).Item.Row,
            CurrencyType.LimitedTomestone => Service.DataManager.GetExcelSheet<TomestonesItem>()!.First(item => item.Tomestones.Row is 3).Item.Row,
            _ => throw new Exception($"ItemId not initialized for type: {Type}")
        };

        return itemId.Value;
    }
}