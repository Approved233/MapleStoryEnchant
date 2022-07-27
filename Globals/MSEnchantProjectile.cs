using System.Linq;
using MSEnchant.Helper;
using MSEnchant.Models;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace MSEnchant.Globals;

public class MSEnchantProjectile : GlobalProjectile
{
    public override void OnSpawn(Projectile projectile, IEntitySource source)
    {
        if (!projectile.sentry && !projectile.minion)
            return;

        if (source is not EntitySource_ItemUse entitySourceItemUse || entitySourceItemUse.Entity is not Player player)
            return;
        
        var msItem = entitySourceItemUse.Item.GetEnchantItem();
        if (msItem == null)
            return;

        projectile.originalDamage += (int)(msItem.BonusAttributes.FirstOrDefault(a => a.Type == StarForceAttributeType.Damage)?.Value ?? 0);
    }

    public override void ModifyHitNPC(Projectile projectile, NPC target, ref int damage, ref float knockback,
        ref bool crit,
        ref int hitDirection)
    {
        if (projectile.damage <= 0 || projectile.owner == 255)
            return;

        if (!projectile.TryFindOwnerPlayer(out var ownerPlayer, out var masterProjectile))
            return;

        if (!masterProjectile.sentry && !masterProjectile.minion)
            return;

        damage += (int)ownerPlayer.GetEquipmentBonus(StarForceAttributeType.Damage);
    }
}