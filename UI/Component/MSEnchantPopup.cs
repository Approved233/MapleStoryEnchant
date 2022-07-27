namespace MSEnchant.UI.Component;

public class MSEnchantPopup : MSPopup
{
    public MSEnchantPopup(string content, float left = 0, float top = 0) : base("MSEnchant/Assets/enchantUI.popUp", content, left, top)
    {
    }

    protected override void DoInit()
    {
        AddButton("buttonconfirm", 64, 130);
        AddButton("buttoncancel", 153, 130);
    }
}