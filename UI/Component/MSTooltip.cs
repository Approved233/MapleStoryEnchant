using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MSEnchant.Helper;
using MSEnchant.UI.Control;
using Terraria;

namespace MSEnchant.UI.Component;

public class MSTooltip : MSElement
{
    protected MSText TextComponent;

    public string Content
    {
        get => TextComponent.Text;
        set => TextComponent.Text = value;
    }

    public MSTooltip(string text, float left = 0f, float top = 0f) : base(left, top)
    {
        Append(TextComponent = new MSText(text));
        
        Content = text;
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);

        var dimension = GetDimensions();
        
        var res = Global.TooltipTextures;

        if (TextComponent.Left.Pixels == 0 || TextComponent.Top.Pixels == 0)
        {
            TextComponent.Left.Set(res["w"].Width() / 2f, 0f);
            TextComponent.Top.Set(res["n"].Height() / 2f, 0f);
            TextComponent.Recalculate();
        }
        
        var textSize = TextComponent.TextSize + res["se"].Size() - new Vector2(3f);
        
        var width = (int)textSize.X;
        var height = (int)textSize.Y;
        
        var guideX = new[] { 0, res["w"].Width(), width - res["e"].Width(), width };
        var guideY = new[] { 0, res["n"].Height(), height - res["s"].Height(), height };

        for (var i = 0; i < guideX.Length; i++) guideX[i] += (int)dimension.X;
        for (var i = 0; i < guideY.Length; i++) guideY[i] += (int)dimension.Y;

        spriteBatch.UseNonPremultiplied(() =>
        {
            DrawRect(spriteBatch, "nw", guideX, guideY, new Vector2(0), new Vector2(1));
            DrawRect(spriteBatch, "ne", guideX, guideY, new Vector2(2, 0), new Vector2(3, 1));
            DrawRect(spriteBatch, "sw", guideX, guideY, new Vector2(0, 2), new Vector2(1, 3));
            DrawRect(spriteBatch, "se", guideX, guideY, new Vector2(2), new Vector2(3));

            if (guideX[2] > guideX[1])
            {
                DrawRect(spriteBatch, "n", guideX, guideY, new Vector2(1, 0), new Vector2(2, 1));
                DrawRect(spriteBatch, "s", guideX, guideY, new Vector2(1, 2), new Vector2(2, 3));
            }

            if (guideY[2] > guideY[1])
            {
                DrawRect(spriteBatch, "w", guideX, guideY, new Vector2(0, 1), new Vector2(1, 2));
                DrawRect(spriteBatch, "e", guideX, guideY, new Vector2(2, 1), new Vector2(3, 2));
            }

            if (guideX[2] > guideX[1] && guideY[2] > guideY[1])
            {
                DrawRect(spriteBatch, "c", guideX, guideY, new Vector2(1), new Vector2(2));
            }
            
            DrawRect(spriteBatch, "cover", guideX, guideY, new Vector2(0), new Vector2(3), posOffset: new Vector2(3));
        });
    }

    protected void DrawRect(SpriteBatch spriteBatch, string loc, int[] guideX, int[] guideY, Vector2 point1,
        Vector2 point2, Vector2? posOffset = null)
    {
        posOffset ??= Vector2.Zero;
        var texture = Global.TooltipTextures[loc].Value;

        spriteBatch.Draw(texture, new Vector2(guideX[(int)point1.X], guideY[(int)point1.Y]) + posOffset.Value, new Rectangle(
                0, 0,
                guideX[(int)point2.X] - guideX[(int)point1.X] - (int)posOffset.Value.X, guideY[(int)point2.Y] - guideY[(int)point1.Y] - (int)posOffset.Value.Y),
            Color.White);
    }
}