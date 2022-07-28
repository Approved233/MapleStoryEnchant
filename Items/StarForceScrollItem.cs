using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MSEnchant.Helper;
using MSEnchant.Models;
using MSEnchant.UI.State;
using MSEnchant.UI.Window;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace MSEnchant.Items;

public class StarForceScrollItem : ModItem
{
    public double SuccessRate = 1;

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

    public override bool AllowPrefix(int pre)
    {
        return false;
    }

    public override bool CanUseItem(Player player)
    {
        return false;
    }

    public override bool CanStack(Item item2)
    {
        return false;
    }

    public override bool CanStackInWorld(Item item2)
    {
        return false;
    }

    public override bool CanResearch()
    {
        return false;
    }

    private BlendState oldInventoryState;

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor,
        Color itemColor,
        Vector2 origin, float scale)
    {
        spriteBatch.SetBlendState(BlendState.NonPremultiplied, out oldInventoryState);
        return base.PreDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);
    }

    public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame,
        Color drawColor, Color itemColor,
        Vector2 origin, float scale)
    {
        if (oldInventoryState != null)
            spriteBatch.SetBlendState(oldInventoryState, out _);
        base.PostDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);
    }

    private BlendState oldWorldState;

    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation,
        ref float scale,
        int whoAmI)
    {
        spriteBatch.SetBlendState(BlendState.NonPremultiplied, out oldWorldState);
        return base.PreDrawInWorld(spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
    }

    public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation,
        float scale,
        int whoAmI)
    {
        if (oldWorldState != null)
            spriteBatch.SetBlendState(oldWorldState, out _);

        base.PostDrawInWorld(spriteBatch, lightColor, alphaColor, rotation, scale, whoAmI);
    }

    public override void LoadData(TagCompound tag)
    {
        ScrollStarForce = tag.GetInt("ScrollStarForce");
        SuccessRate = tag.GetDouble("SuccessRate");
    }

    public override void SaveData(TagCompound tag)
    {
        tag["ScrollStarForce"] = ScrollStarForce;
        tag["SuccessRate"] = SuccessRate;
    }

    public override void NetReceive(BinaryReader reader)
    {
        ScrollStarForce = reader.ReadInt32();
        SuccessRate = reader.ReadDouble();
    }

    public override void NetSend(BinaryWriter writer)
    {
        writer.Write(ScrollStarForce);
        writer.Write(SuccessRate);
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

    public bool CanApplyTo(Item targetItem)
    {
        var msItem = targetItem.GetEnchantItem();
        if (msItem == null)
            return false;

        if (msItem.Destroyed || msItem.IsReachedMaxStarForce || ScrollStarForce > msItem.MaxStarForceLevel ||
            msItem.StarForce >= ScrollStarForce)
            return false;

        return true;
    }

    public StarForceScrollResult ApplyResult(Item targetItem)
    {
        var msItem = targetItem.GetEnchantItem();
        if (msItem == null)
            return StarForceScrollResult.NoResult;

        if (!CanApplyTo(targetItem))
            return StarForceScrollResult.NoResult;

        if (Main.rand.NextDouble() >= SuccessRate)
            return StarForceScrollResult.Failed;

        return StarForceScrollResult.Success;
    }

    public StarForceScrollResult ApplyTo(Item targetItem)
    {
        var state = MSEnchantUI.Instance;
        var r = ApplyResult(targetItem);
        var msItem = targetItem.GetEnchantItem();
        if (msItem == null)
            return StarForceScrollResult.NoResult;

        if (!CanApplyTo(targetItem))
            return StarForceScrollResult.NoResult;

        Item.TurnToAir();

        if (Main.rand.NextDouble() >= SuccessRate)
            return StarForceScrollResult.Failed;

        msItem.StarForce = ScrollStarForce;
        msItem.UpdateData();
        return StarForceScrollResult.Success;
    }
}