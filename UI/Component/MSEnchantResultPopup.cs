using Microsoft.Xna.Framework;
using MSEnchant.UI.Control;
using Terraria;

namespace MSEnchant.UI.Component;

public class MSEnchantResultPopup : MSPopup
{
    public Item DisplayItemBefore
    {
        set => ItemBefore.DisplayItem = value;
    }

    public Item DisplayItemAfter
    {
        set => ItemAfter.DisplayItem = value;
    }

    public Color ItemBeforeDrawColor
    {
        get => ItemBefore.DrawColor;
        set => ItemBefore.DrawColor = value;
    }
    
    public Color ItemAfterDrawColor
    {
        get => ItemAfter.DrawColor;
        set => ItemAfter.DrawColor = value;
    }
    
    protected MSItem ItemBefore;

    protected MSItem ItemAfter;

    protected override float ContentTop => 97f;

    public MSEnchantResultPopup(string content, float left = 0, float top = 0) : base(
        "enchantUI.popUp2", content, left, top)
    {
    }

    protected override void DoInit()
    {
        AddButton("buttonconfirm", 108, 130);

        AddChild(new MSImage("enchantUI.popUp2.cover", 83, 30));

        AddChild(ItemBefore = new MSItem(32, 32, 90, 36)
        {
            DisplayShadow = false
        });

        AddChild(ItemAfter = new MSItem(32, 32, 178, 36)
        {
            DisplayShadow = false
        });
    }
}