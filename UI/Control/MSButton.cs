using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MSEnchant.Helper;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using Terraria.UI;

namespace MSEnchant.UI.Control;

public class MSButton : MSElement
{
    protected Asset<Texture2D> Texture;

    protected Asset<Texture2D> HoverTexture;

    protected Asset<Texture2D> PressTexture;

    protected Asset<Texture2D> DisabledTexture;

    public bool IsPressing { get; private set; }

    public string HoverSound { get; set; } = "MSEnchant/Assets/BtMouseOver";

    public string ClickSound { get; set; } = "MSEnchant/Assets/BtMouseClick";

    public MSButton(string texture, float left = 0f, float top = 0f) : base(left, top)
    {
        Texture = ModContent.Request<Texture2D>($"{texture}.normal.0", AssetRequestMode.ImmediateLoad);
        ModContent.RequestIfExists($"{texture}.mouseOver.0", out HoverTexture);
        ModContent.RequestIfExists($"{texture}.pressed.0", out PressTexture);
        ModContent.RequestIfExists($"{texture}.disabled.0", out DisabledTexture);
        Width.Set(Texture.Width(), 0.0f);
        Height.Set(Texture.Height(), 0.0f);

        OnMouseOver += (evt, element) =>
        {
            if (string.IsNullOrWhiteSpace(ClickSound))
                return;
            
            var sound = new SoundStyle(HoverSound);
            SoundEngine.PlaySound(sound);
        };
        
        OnMouseUp += (evt, element) =>
        {
            IsPressing = false;
        };
        
        OnMouseDown += (evt, element) =>
        {
            IsPressing = true;
        };

        OnClick += (evt, element) =>
        {
            IsPressing = false;
            
            if (!string.IsNullOrWhiteSpace(ClickSound))
            {
                var sound = new SoundStyle(ClickSound);
                SoundEngine.PlaySound(sound);
            }
        };
    }

    public void SetHoverImage(string texture) => ModContent.RequestIfExists($"{texture}.0", out HoverTexture);

    public void SetPressTexture(string texture) => ModContent.RequestIfExists($"{texture}.0", out PressTexture);

    public void SetDisabledTexture(string texture) => ModContent.RequestIfExists($"{texture}.0", out DisabledTexture);

    public void SetImage(string texture)
    {
        ModContent.RequestIfExists($"{texture}.0", out Texture, AssetRequestMode.ImmediateLoad);
        Width.Set(Texture.Width(), 0.0f);
        Height.Set(Texture.Height(), 0.0f);
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);
        
        var dimensions = GetDimensions();

        Texture2D texture;

        if (DisabledTexture != null && Disabled)
            texture = DisabledTexture.Value;
        else if (PressTexture != null && IsPressing)
            texture = PressTexture.Value;
        else if (HoverTexture != null && IsMouseContaining)
            texture = HoverTexture.Value;
        else
            texture = Texture.Value;

        spriteBatch.UseNonPremultiplied(() =>
        {
            spriteBatch.Draw(texture, dimensions.Position(), Color.White);
        });
    }

}