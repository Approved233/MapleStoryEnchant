using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using MSEnchant.Globals;
using MSEnchant.UI.State;
using MSEnchant.UI.Window;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
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

        var beforeHover = Main.HoverItem;
        Main.HoverItem = item.Clone();
        drawTooltipMethod.Invoke(Main.instance, new[] { info, 0, (byte)0, (int)mouse.X, (int)mouse.Y });
        Main.HoverItem = beforeHover;
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
    
    public static bool IsBossBag(this ModItem? item)
    {
        if (item == null)
            return false;

        if (bossBagNPCProperty == null)
            bossBagNPCProperty = typeof(ModItem).GetProperty("BossBagNPC", BindingFlags.Instance | BindingFlags.Public);

        if (bossBagNPCProperty != null)
        {
            var value = bossBagNPCProperty.GetValue(item);

            if (value != null && (int)value > 0)
                return true;
        }

        if (item.Item.consumable && item.Item.expert && item.GetType().GetMethod("ModifyItemLoot") != null && item.CanRightClick() && item.Item.GetItemGroup() == ContentSamples.CreativeHelper.ItemGroup.BossBags)
            return true;

        return false;
    }

    public static bool IsFishingCrate(this ModItem? item)
    {
        if (item == null)
            return false;

        if (!item.Item.consumable)
            return false;

        return item.GetType().GetMethod("ModifyItemLoot") != null && item.CanRightClick() &&
               item.Item.GetItemGroup() == ContentSamples.CreativeHelper.ItemGroup.Crates;
    }

    public static ContentSamples.CreativeHelper.ItemGroup GetItemGroup(this Item item)
    {
        if (Global.ItemGroupCache.TryGetValue(item.type, out var group))
            return group;
        
        group = ContentSamples.CreativeHelper.GetItemGroup(item, out _);
        ItemLoader.ModifyResearchSorting(item, ref group);
        Global.ItemGroupCache.Add(item.type, group);
        return group;
    }

    public static void UpdateStarForceAttributes(this IEnumerable<Item> items)
    {
        foreach (var item in items)
        {
            item.GetEnchantItem()?.UpdateData();
        }
    }

    public static bool CostItem(this Item item, int num)
    {
        var stack = item.stack;
        if (stack < num)
            return false;
        
        if (stack > num)
        {
            item.stack -= num;
            return true;
        }

        item.TurnToAir();
        return true;
    }

    public static bool IsBossBag(this Item item)
    {
        return ItemID.Sets.BossBag[item.type] || item.ModItem.IsBossBag();
    }

    public static bool IsFishingCrate(this Item item)
    {
        return ItemID.Sets.IsFishingCrate[item.type] || item.ModItem.IsFishingCrate();
    }
    
    public static void DropItemLocal(this Player player, IEntitySource source, Rectangle rectangle, int itemId, int stack = 1, Action<int>? onItemSpawn = null)
    {
        var index = Item.NewItem(source, rectangle, itemId, stack, noBroadcast: Main.netMode == NetmodeID.Server);
        onItemSpawn?.Invoke(index);
        if (Main.netMode == NetmodeID.Server)
        {
            Main.timeItemSlotCannotBeReusedFor[index] = 5000;
            NetMessage.SendData(MessageID.InstancedItem, player.whoAmI, number: index);
            Main.item[index].active = false;
        } 
    }

}