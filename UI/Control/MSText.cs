using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria.UI.Chat;

namespace MSEnchant.UI.Control;

public class MSText : MSElement
{
    private string _visibleText;

    private string _text;

    private string _lastTextReference;

    public Vector2 TextSize { get; protected set; } = Vector2.Zero;

    public Vector2 CharacterSize { get; protected set; } = Vector2.Zero;

    public float TextOriginX { get; set; }

    public float TextOriginY { get; set; }

    public Color Color { get; set; } = Color.White;

    public bool DynamicallyScaleDownToWidth { get; set; }

    private Asset<DynamicSpriteFont> _font = Global.TextRegular;

    public Asset<DynamicSpriteFont> Font
    {
        get => _font;
        set
        {
            _font = value;
            InternalSetText(Text, TextScale);
        }
    }

    public string Text
    {
        get => _text;
        set => InternalSetText(value, TextScale);
    }

    private float _textScale;

    public float TextScale
    {
        get => _textScale;
        set
        {
            _textScale = value;
            InternalSetText(Text, TextScale);
        }
    }

    public MSText(string text, float textScale = 1f, float left = 0f, float top = 0f) : base(left, top)
    {
        InternalSetText(text, textScale);
    }
    
    private void VerifyTextState()
    {
        if (_lastTextReference == Text)
            return;
        InternalSetText(_text, _textScale);
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);

        VerifyTextState();
        var innerDimensions = GetInnerDimensions();
        var pos = innerDimensions.Position();

        var textScale = TextScale;
        if (DynamicallyScaleDownToWidth && TextSize.X > (double)innerDimensions.Width)
            textScale *= innerDimensions.Width / TextSize.X;

        var font = Font.Value;
        var textSize = font.MeasureString(_visibleText);
        if (Math.Abs(textScale - 1f) > 0.1f)
            pos.Y += CharacterSize.Y * textScale;
        ChatManager.DrawColorCodedString(spriteBatch, font, _visibleText, pos, Color, 0.0f,
            textSize * new Vector2(TextOriginX, TextOriginY), new Vector2(textScale));
    }

    private void InternalSetText(string text, float textScale)
    {
        var font = Font.Value;
        _text = text;
        _textScale = textScale;
        _lastTextReference = _text;
        _visibleText = _text;
        var stringSize = ChatManager.GetStringSize(font, _visibleText, new Vector2(1f));
        var characterSize = ChatManager.GetStringSize(font, "　", new Vector2(1f));
        TextSize = stringSize * textScale;
        CharacterSize = characterSize * textScale;
        MinWidth.Set(TextSize.X + PaddingLeft + PaddingRight, 0.0f);
        MinHeight.Set(TextSize.Y + PaddingTop + PaddingBottom, 0.0f);
    }
}