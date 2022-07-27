using System;
using System.Linq;
using MSEnchant.Helper;
using MSEnchant.Items;
using Terraria.ModLoader;

namespace MSEnchant.Commands;

#if DEBUG
public class SetStarScrollCommand : ModCommand
{
    public override CommandType Type => CommandType.Chat;

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        try
        {
            if (!int.TryParse(args.ElementAtOrDefault(0), out var level))
                level = 0;
            
            var item = caller.Player.inventory[caller.Player.selectedItem];
            if (item.IsNullOrAir() || item.ModItem is not StarForceScrollItem scrollItem)
                return;

            if (!float.TryParse(args.ElementAtOrDefault(1), out var successRate))
                successRate = 100;

            scrollItem.ScrollStarForce = level;
            scrollItem.SuccessRate = successRate / 100;
            caller.Player.SendMessage($"成功修改卷轴等级 {level} 星，成功率 {successRate:0}%");
        }
        catch (Exception ex)
        {
            caller.Player.SendMessage($"Set star force level cause error: {ex}");
        }
    }

    public override string Command => "sclevel";

    public override string Usage => "/sclevel <level> <successRate>";

    public override string Description => "设置星之力卷轴等级和成功率";
}
#endif