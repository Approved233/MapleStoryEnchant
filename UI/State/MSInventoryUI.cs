using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MSEnchant.UI.Control;
using MSEnchant.UI.Window;
using Terraria;
using Terraria.UI;

namespace MSEnchant.UI.State;

public class MSInventoryUI : UIState
{
    public UserInterface UserInterface { get; private set; }

    public MSInventoryUI()
    {
        UserInterface = new UserInterface();
        UserInterface.SetState(this);
    }

    protected MSButton EnchantButton;

    public override void OnInitialize()
    {
        Append(EnchantButton = new MSButton("MSEnchant/Assets/Item.AutoBuild.buttonUpgrade", 570, 244)
        {
            Tooltip = "强化道具。"
        });
        EnchantButton.OnClick += (evt, element) =>
        {
            MSEnchantUI.Instance.EnableWindow<MainWindow>();
        };
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        
        EnchantButton.Visible = Main.playerInventory && Main.LocalPlayer.chest == -1 && Main.npcShop == 0 || Main.recBigList;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);

        var pointElement = GetElementAt(new Vector2(Main.mouseX, Main.mouseY));
        if (pointElement is MSElement e)
            e.DrawTooltip(spriteBatch);
    }
}