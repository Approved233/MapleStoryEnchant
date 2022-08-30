using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using MSEnchant.Globals;
using MSEnchant.Helper;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace MSEnchant.Items;

public class StarForceScrollItem : BaseScrollItem
{
    private int _scrollStarForce;

    public int ScrollStarForce
    {
        get => _scrollStarForce;
        set
        {
            _scrollStarForce = value;
            UpdateName();
        }
    }

    public override string Texture => "MSEnchant/Assets/StarForceScroll";

    public override bool CanStack(Item item2)
    {
        return false;
    }

    public override void LoadData(TagCompound tag)
    {
        ScrollStarForce = tag.GetInt("ScrollStarForce");
        base.LoadData(tag);
    }

    public override void SaveData(TagCompound tag)
    {
        tag["ScrollStarForce"] = ScrollStarForce;
        base.SaveData(tag);
    }

    public override void NetReceive(BinaryReader reader)
    {
        ScrollStarForce = reader.ReadInt32();
        base.NetReceive(reader);
    }

    public override void NetSend(BinaryWriter writer)
    {
        writer.Write(ScrollStarForce);
        base.NetSend(writer);
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        // 255 153 0
        tooltips.Add(new TooltipLine(Mod, "SuccessRate",
            Language.GetTextValue("Mods.MSEnchant.ItemTooltip.StarForceScrollItem_SuccessRate",
                (SuccessRate * 100).ToString("0")))
        {
            OverrideColor = new Color(255, 153, 0)
        });
        tooltips.Add(new TooltipLine(Mod, "ToStarLevelTips",
            Language.GetTextValue("Mods.MSEnchant.ItemTooltip.StarForceScrollItem_TargetStarLevelTips",
                ScrollStarForce))
        {
            OverrideColor = new Color(255, 153, 0)
        });
    }

    protected void UpdateName()
    {
        Item.SetNameOverride(Language.GetTextValue("Mods.MSEnchant.ItemName.StarForceScrollItem_Format", ScrollStarForce));
    }

    public override void SetDefaults()
    {
        Item.maxStack = 1;
        Item.value = Item.buyPrice(5);
        Item.rare = ItemRarityID.Purple;
    }

    public override bool CanApplyTo(Item targetItem)
    {
        var msItem = targetItem.GetEnchantItem();
        if (msItem == null)
            return false;

        if (msItem.Destroyed || msItem.IsReachedMaxStarForce || ScrollStarForce > msItem.MaxStarForceLevel ||
            msItem.StarForce >= ScrollStarForce)
            return false;

        return true;
    }

    protected override void OnScrollSuccess(Item targetItem, MSEnchantItem msItem)
    {
        msItem.StarForce = ScrollStarForce;
        msItem.UpdateData();
    }

}