using System;
using System.Linq;
using MSEnchant.Helper;
using Terraria.ModLoader;

namespace MSEnchant.Commands;

#if DEBUG
public class SetStarForceLevelCommand : ModCommand
{
    public override CommandType Type => CommandType.Chat;

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        try
        {
            if (!int.TryParse(args.ElementAtOrDefault(0), out var level))
                level = 0;
            
            var item = caller.Player.inventory[caller.Player.selectedItem];
            
            var msItem = item.GetEnchantItem();
            if (msItem == null)
                return;
            
            if (!int.TryParse(args.ElementAtOrDefault(1), out var destroyValue))
                destroyValue = 0;
            
            msItem.StarForce = level;
            msItem.Destroyed = destroyValue == 1;
            
            msItem.UpdateData();
            caller.Player.SendMessage($"成功修改星之力等級為 {level} 星");
        }
        catch (Exception ex)
        {
            caller.Player.SendMessage($"Set star force level cause error: {ex}");
        }
    }

    public override string Command => "sflevel";

    public override string Usage => "/sflevel <level> [destroy]";

    public override string Description => "設置星之力等級";
}
#endif