using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MSEnchant.Helper;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace MSEnchant.Items;

public class StarItem : ModItem
{
    public override string Texture => "MSEnchant/Assets/StarItem";

    private BlendState oldInventoryState;
    
    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor,
        Color itemColor,
        Vector2 origin, float scale)
    {
        spriteBatch.SetBlendState(BlendState.NonPremultiplied, out oldInventoryState);
        return base.PreDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);
    }

    public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor,
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

    public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale,
        int whoAmI)
    {
        if (oldWorldState != null)
            spriteBatch.SetBlendState(oldWorldState, out _);
        
        base.PostDrawInWorld(spriteBatch, lightColor, alphaColor, rotation, scale, whoAmI);
    }

    public override bool CanResearch()
    {
        return true;
    }

    public override void SetDefaults()
    {
        Item.maxStack = 30000;
        Item.value = Item.buyPrice(0, 0, 0, 1);
        Item.rare = ItemRarityID.White;
    }

    public override void SetStaticDefaults()
    {
        DisplayName.SetDefault("星星");
        Tooltip.SetDefault("可以为已消耗所有可升级次数的\n装备注入星之力，提高星星等级。");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 100;
    }
}