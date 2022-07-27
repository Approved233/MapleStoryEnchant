using MSEnchant.UI.State;
using MSEnchant.UI.Window;
using Terraria.ModLoader;

namespace MSEnchant.Commands;

#if DEBUG
public class EnableMainUICommand : ModCommand
{
    public override void Action(CommandCaller caller, string input, string[] args)
    {
        MSEnchantUI.Instance.EnableWindow<MainWindow>();
    }

    public override string Command => "enchantUI";

    public override string Usage => "/enchantUI";

    public override CommandType Type => CommandType.Chat;
}
#endif