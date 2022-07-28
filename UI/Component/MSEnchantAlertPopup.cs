namespace MSEnchant.UI.Component;

public class MSEnchantAlertPopup : MSPopup
{
    public MSEnchantAlertPopup(string content, float left = 0, float top = 0) : base("enchantUI.popUp3", content, left, top)
    {
    }

    protected override void DoInit()
    {
        AddButton("buttonconfirm", 108, 130);
    }
}