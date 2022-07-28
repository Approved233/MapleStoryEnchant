using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MSEnchant.Helper;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace MSEnchant.UI.Control;

public class MSImage : MSElement
{
    private Asset<Texture2D> _texture;

    public Asset<Texture2D> Texture
    {
        get => _texture;
        set
        {
            _texture = value;
            if (_texture != null)
                UpdateTexture();
        }
    }

    private Vector2 _imageScale = Vector2.One;

    public Vector2 ImageScale
    {
        get => _imageScale;
        set
        {
            _imageScale = value;
            UpdateTexture();
        }
    }

    public bool ScaleToFit { get; set; } = false;

    public Vector2 NormalizedOrigin { get; set; } = Vector2.Zero;

    public bool RemoveFloatingPointsFromDrawPosition { get; set; } = false;

    public float Rotation { get; set; } = 0;

    public SpriteEffects Effect { get; set; } = SpriteEffects.None;

    public bool DrawAlpha { get; set; } = true;

    public MSImage(Asset<Texture2D> texture, float left = 0f, float top = 0f) : base(left, top)
    {
        Texture = texture;
    }

    public MSImage(string texture, float left = 0f, float top = 0f) : base(texture.GetTextureOrigin(new Vector2(left, top)))
    {
        if (!texture.LoadLocaleTextureIfExists(out var result, AssetRequestMode.ImmediateLoad))
            result = null;

        Texture = result;
    }

    public void SetTexture(string texture)
    {
        if (string.IsNullOrEmpty(texture))
            Texture = null;
        else
            Texture = texture.LoadLocaleTexture(AssetRequestMode.ImmediateLoad);
    }

    protected void UpdateTexture()
    {
        var texture = Texture.Value;
        var size = texture.Size() * ImageScale;

        Width.Set(size.X, 0f);
        Height.Set(size.Y, 0f);
        Recalculate();
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);

        if (!Visible)
            return;

        var texture = Texture?.Value;
        if (texture == null)
            return;

        var dimensions = GetDimensions();

        var drawAction = () =>
        {
            if (ScaleToFit)
            {
                spriteBatch.Draw(texture, dimensions.ToRectangle(), Color.White);
            }
            else
            {
                var size = texture.Size();
                var position = dimensions.Position() + size * (new Vector2(1f) - ImageScale) / 2f +
                               size * NormalizedOrigin;
                if (RemoveFloatingPointsFromDrawPosition)
                    position = position.Floor();

                spriteBatch.Draw(texture, position, new Rectangle?(), Color.White, Rotation, size * NormalizedOrigin,
                    ImageScale, Effect, 0.0f);
            }
        };

        if (DrawAlpha)
            spriteBatch.UseNonPremultiplied(() => { drawAction(); });
        else
            drawAction();
    }
}