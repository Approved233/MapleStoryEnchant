using System;
using MSEnchant.Helper;
using MSEnchant.UI.Control;
using MSEnchant.UI.State;
using Terraria;

namespace MSEnchant.UI.Window;

public class MainWindow : MSWindow
{
    public override string BaseTexturePath => "MSEnchant/Assets/enchantUI.main";

    public override Type[] LinkWindowTypes => new[]
    {
        typeof(MiniGameWindow),
        typeof(StarForceWindow),
        typeof(TransmissionWindow)
    };

    protected override void DoInit()
    {
        AddBackGroundTexture("backgrnd2", 11, 22);

        AddCloseButton(322, tooltip: "结束强化。");

        var guideLayer = new MSImage("MSEnchant/Assets/enchantUI.main.layerguide", 133, 122);
        Append(guideLayer);

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