using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MSEnchant.Globals;
using MSEnchant.Helper;
using MSEnchant.Models;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace MSEnchant.Items;

public abstract class BaseScrollItem : ModItem
{

    public virtual double SuccessRate { get; set; } = 1;
    
    public override bool AllowPrefix(int pre)
    {
        return false;
    }

    public override bool CanUseItem(Player player)
    {
        return false;
    }

    public override bool CanStackInWorld(Item item2)
    {
        return false;
    }
    
    public override void LoadData(TagCompound tag)
    {
        SuccessRate = tag.GetDouble("SuccessRate");
    }

    public override void SaveData(TagCompound tag)
    {
        tag["SuccessRate"] = SuccessRate;
    }
    
    public override void NetReceive(BinaryReader reader)
    {
        SuccessRate = reader.ReadDouble();
    }

    public override void NetSend(BinaryWriter writer)
    {
        writer.Write(SuccessRate);
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

    public virtual bool CanApplyTo(Item item)
    {
        return true;
    }
    
    public virtual ScrollResult ApplyScroll(Item targetItem)
    {
        var msItem = targetItem.GetEnchantItem();
        if (msItem == null)
            return ScrollResult.NoResult;

        if (!CanApplyTo(targetItem))
            return ScrollResult.NoResult;

        if (!Item.CostItem(1))
            return ScrollResult.NoResult;
        
        if (Main.rand.NextDouble() >= SuccessRate)
            return ScrollResult.Failed;

        OnScrollSuccess(targetItem, msItem);
        
        return ScrollResult.Success;
    }
    
    protected virtual void OnScrollSuccess(Item targetItem, MSEnchantItem msItem)
    {
        
    }

}