using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MSEnchant.Effects;
using MSEnchant.Helper;
using MSEnchant.Items;
using MSEnchant.Models;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace MSEnchant.Globals;

public class MSEnchantPlayer : ModPlayer
{
    public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
    {
        var msItem = item?.GetEnchantItem();
        if (msItem == null)
            return;

        damage.Flat +=
            (float)(msItem.BonusAttributes.FirstOrDefault(a => a.Type == StarForceAttributeType.Damage)?.Value ?? 0f);
        damage.Flat += Player.GetEquipmentBonus(StarForceAttributeType.Damage);
    }

    public override void PostUpdateEquips()
    {
        Player.statDefense += (int)Player.GetEquipmentBonus(StarForceAttributeType.Defense);
    }

    public override void AnglerQuestReward(float rareMultiplier, List<Item> rewardItems)
    {
        var starItem = ModContent.GetInstance<StarItem>().Item;
        starItem.stack = Main.rand.Next((int)(50 * rareMultiplier), (int)(300 * rareMultiplier));
        rewardItems.Add(starItem);
    }

    private List<WorldEffectAnimation> playingEffectAnimations = new List<WorldEffectAnimation>();

    public void PlayEffect(WorldEffectAnimation animation)
    {
        lock (playingEffectAnimations)
        {
            playingEffectAnimations.Add(animation);
        }
    }

    public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a,
        ref bool fullBright)
    {
        base.DrawEffects(drawInfo, ref r, ref g, ref b, ref a, ref fullBright);

        lock (playingEffectAnimations)
        {
            foreach (var effect in playingEffectAnimations)
            {
                effect.UpdateFrame();
                effect.DrawFrame(Main.spriteBatch, Player.Center);
            }
            
            playingEffectAnimations.RemoveAll(r => r.PlayEnded);
        }
    }
}