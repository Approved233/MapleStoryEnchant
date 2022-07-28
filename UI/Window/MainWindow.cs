using System;
using MSEnchant.Helper;
using MSEnchant.UI.Control;
using MSEnchant.UI.State;
using Terraria;
using Terraria.Localization;

namespace MSEnchant.UI.Window;

public class MainWindow : MSWindow
{
    public override string BaseTexturePath => "enchantUI.main";

    public override Type[] LinkWindowTypes => new[]
    {
        typeof(MiniGameWindow),
        typeof(StarForceWindow),
        typeof(TransmissionWindow)
    };

    protected override void DoInit()
    {
        AddBackGroundTexture("backgrnd2", 11, 22);

        AddCloseButton(322, tooltip: Language.GetTextValue("Mods.MSEnchant.UIText.EndEnchant"));

        Append(new MSImage("enchantUI.main.layerguide"));

        OnMouseUp += (evt, element) =>
        {
            var item = Main.mouseItem;
            if (!ItemHelper.HandleItemPick(item))
                return;

            var msItem = item.GetEnchantItem();
            if (msItem == null)
                return;

            if (msItem.Destroyed || msItem.IsReachedMaxStarForce)
            {
                MSEnchantUI.Instance.ReplaceWindow<TransmissionWindow>(this, window => { window.SetItem(item); });
            }
            else
            {
                MSEnchantUI.Instance.ReplaceWindow<StarForceWindow>(this, window => { window.EnchantItem = item; });
            }
        };
    }
}