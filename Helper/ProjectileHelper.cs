using System.Linq;
using Terraria;

namespace MSEnchant.Helper;

public static class ProjectileHelper
{

    public static bool TryFindOwnerPlayer(this Projectile projectile, out Player ownerPlayer, out Projectile masterProjectile)
    {
        ownerPlayer = null;
        masterProjectile = projectile;
        var index = projectile.owner;
        while (index != 255)
        {
            var player = Main.player.ElementAtOrDefault(index);
            if (player != null)
            {
                ownerPlayer = player;
                break;
            }

            masterProjectile = Main.projectile.ElementAtOrDefault(index);
            index = masterProjectile?.owner ?? 255;
        }

        return ownerPlayer != null && masterProjectile != null;
    }
    
}