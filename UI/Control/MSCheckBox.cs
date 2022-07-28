using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MSEnchant.Helper;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace MSEnchant.UI.Control;

public class MSCheckBox : MSElement
{
    private Asset<Texture2D> _checkedTexture;
    private Asset<Texture2D> _nonCheckedTexture;

    private Asset<Texture2D> _disabledNonCheckedTexture;
    private Asset<Texture2D> _disabledCheckedTexture;

    public bool Checked { get; set; }

    public MSCheckBox(string texture, float left = 0f, float top = 0f) : base(left, top)
    {
        _nonCheckedTexture = $"{texture}.0".LoadLocaleTexture(AssetRequestMode.ImmediateLoad);
        _checkedTexture = $"{texture}.1".LoadLocaleTexture(AssetRequestMode.ImmediateLoad);
        $"{texture}.2".LoadLocaleTextureIfExists(out _disabledNonCheckedTexture);
        $"{texture}.3".LoadLocaleTextureIfExists(out _disabledCheckedTexture);
        Width.Set(_nonCheckedTexture.Width(), 0.0f);
        Height.Set(_nonCheckedTexture.Height(), 0.0f);
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);
        
        var dimensions = GetDimensions();
        var pairs = new Asset<Texture2D>[2];

        if (Disabled)
        {
            pairs[0] = _disabledNonCheckedTexture;
            pairs[1] = _disabledCheckedTexture;
        }

        pairs[0] ??= _nonCheckedTexture;
        pairs[1] ??= _checkedTexture;

        var texture = Checked ? pairs[1] : pairs[0];
        spriteBatch.UseNonPremultiplied(() =>
        {
            spriteBatch.Draw(texture.Value, dimensions.Position(), Color.White);
        });
    }

    public override void Click(UIMouseEvent evt)
    {
        if (Disabled)
            return;

        Toggle();
        base.Click(evt);
    }

    public void Toggle()
    {
        Checked = !Checked;
    }
}