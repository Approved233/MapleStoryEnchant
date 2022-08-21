using System;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using MSEnchant.Globals;
using MSEnchant.UI.State;
using MSEnchant.UI.Window;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace MSEnchant.Helper;

public static class ItemHelper
{
    public static bool HandleItemPick(Item item)
    {
        if (item.IsNullOrAir())
            return false;

        var success = false;

        var msItem = item.GetEnchantItem();
        if (msItem == null)
            goto END;

        if (!msItem.HasStarForce)
        {
            MSEnchantUI.Instance.ShowNoticeCenter(Language.GetTextValue("Mods.MSEnchant.UIText.InvalidItem"));
            goto END;
        }

        success = true;

        END:
        var player = Main.player[Main.myPlayer];
        var emptySlot = player.FindEmptySlot();
        if (emptySlot > -1)
        {
            player.inventory[emptySlot] = item;
            Main.mouseItem = new Item();
        }

        SoundEngine.PlaySound(Global.DragEndSound);
        return success;
    }

    public static MSEnchantItem? GetEnchantItem(this Item item)
    {
        if (item.IsNullOrAir())
            return null;

        if (!item.TryGetGlobalItem<MSEnchantItem>(out var msItem))
            return null;

        msItem.TryInitData(item);
        return msItem;
    }

    private static MethodInfo drawTooltipMethod = null;

    public static void DrawTooltipHacked(this Item item)
    {
        drawTooltipMethod ??=
            typeof(Main).GetMethod("MouseText_DrawItemTooltip", BindingFlags.NonPublic | BindingFlags.Instance);
        if (drawTooltipMethod == null)
            return;

        var mouse = new Vector2(Main.mouseX, Main.mouseY) + new Vector2(14f);
        if (Main.ThickMouse)
            mouse += new Vector2(6f);

        if (!Main.mouseItem.IsAir)
            mouse.X += 34;

        var info = Activator.CreateInstance(drawTooltipMethod.GetParameters().First().ParameterType);

        Main.HoverItem = item;
        drawTooltipMethod.Invoke(Main.instance, new[] { info, 0, (byte)0, (int)mouse.X, (int)mouse.Y });
    }

    public static void DrawTooltipHackedQueue(this Item item)
    {
        Global.DrawTooltipQueue.Enqueue(item);
    }

    public static bool IsItemValidInUIAction(this Item item)
    {
        if (item.IsNullOrAir())
            return false;

        return Main.LocalPlayer.FindItemInInventory(item);
    }

    public static bool IsNullOrAir(this Item item)
    {
        return item == null || item.IsAir;
    }

    private static MethodInfo dropItemMethod;

    public static void DropItem(this Item item, IEntitySource source, Rectangle rectangle)
    {
        dropItemMethod ??= typeof(Item).GetMethod("DropItem", BindingFlags.Static);
        if (dropItemMethod == null)
            return;

        dropItemMethod.Invoke(null, new object[] { source, item, rectangle });
    }

    public static ItemType GetItemType(this Item item)
    {
        if (item.OriginalDamage > 0 && item.ammo == AmmoID.None && !item.consumable)
            return ItemType.Weapon;

        if (item.OriginalDefense > 0 && (item.legSlot != 0 || item.bodySlot != 0 || item.headSlot != 0))
            return ItemType.Armor;

        if (item.accessory && !item.vanity && ItemID.Sets.CanGetPrefixes[item.type])
            return ItemType.Accessory;

        return ItemType.Unknown;
    }

    private static PropertyInfo? bossBagNPCProperty = null;
    
    public static bool IsBossBag(this ModItem item)
    {
        if (bossBagNPCProperty == null)
            bossBagNPCProperty = item.GetType().GetProperty("BossBagNPC", BindingFlags.Instance | BindingFlags.Public);

        if (bossBagNPCProperty == null)
            return false;
        
        var value = bossBagNPCProperty.GetValue(item);
        if (value == null)
            return false;

        return (int)value > 0;
    }
}