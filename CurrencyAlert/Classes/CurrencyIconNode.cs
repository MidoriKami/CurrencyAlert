using System;
using System.Drawing;
using System.Numerics;
using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit.Nodes;
using Newtonsoft.Json;

namespace CurrencyAlert.Classes;

[JsonObject(MemberSerialization.OptIn)]
public class CurrencyIconNode : SimpleComponentNode {
    [JsonProperty] private readonly IconImageNode currencyIcon;
    [JsonProperty] private readonly TextNode itemCountText;

    public CurrencyIconNode() {
        currencyIcon = new IconImageNode {
            NodeId = 2,
            NodeFlags = NodeFlags.Visible,
            Size = new Vector2(32.0f, 32.0f),
            WrapMode = 1,
            IsVisible = true,
        };
        System.NativeController.AttachNode(currencyIcon, this);
        
        itemCountText = new TextNode {
            NodeId = 3,
            FontSize = 12,
            FontType = FontType.Axis,
            Size = new Vector2(32.0f, 16.0f),
            Position = new Vector2(0.0f, 16.0f),
            TextColor = KnownColor.White.Vector(),
            TextOutlineColor = KnownColor.Black.Vector(),
            TextFlags = TextFlags.Edge,
            IsVisible = true,
        };
        System.NativeController.AttachNode(itemCountText, this);
    }

    public int ItemCount {
        get => int.Parse(itemCountText.Text.ToString());
        set {
            if (itemCountText.IsVisible) {
                itemCountText.SetNumber(value);
            }
        }
    }

    public uint IconId {
        get => currencyIcon.IconId;
        set => currencyIcon.IconId = value;
    }

    public bool ShowItemCount {
        get => itemCountText.IsVisible;
        set => itemCountText.IsVisible = value;
    }

    public override float Width 
        => Margin.Left + Math.Max(currencyIcon.Width, itemCountText.Width) + Margin.Right;

    public override float Height 
        => Margin.Top + Math.Max(currencyIcon.Height, itemCountText.Height) + Margin.Bottom;
}