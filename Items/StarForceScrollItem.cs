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
        tooltips.Add(new TooltipLine(Mod, "SuccessRate", $"成功率{SuccessRate * 100:0}%")
        {
            OverrideColor = new Color(255, 153, 0)
        });
        tooltips.Add(new TooltipLine(Mod, "ToStarLevelTips", $"升级后的道具的星之力将强化为\n{ScrollStarForce}星")
        {
            OverrideColor = new Color(255, 153, 0)
        });
    }

    protected void UpdateName()
    {
        Item.SetNameOverride("星之力%star%星强化券".Replace("%star%", ScrollStarForce.ToString()));
    }

    public override void SetDefaults()
    {
        Item.maxStack = 1;
        Item.value = Item.buyPrice(5);
        Item.rare = ItemRarityID.Purple;
    }

    public override void SetStaticDefaults()
    {
        DisplayName.SetDefault("星之力强化券");
        Tooltip.SetDefault("把[c/FF9900:已经完成升级的装备道具]的星\n之力强化至指定数值，不会受到\n复原之盾的效果影响。\n　\n[c/FF9900:无法用于已经强化至指定强化数]\n[c/FF9900:值以上的装备、最高强化数值低]\n[c/FF9900:于所指定的强化数值的装备、极]\n[c/FF9900:真装备。]");
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