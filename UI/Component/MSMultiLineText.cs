using System.Collections.Generic;
using System.Linq;
using log4net.Repository.Hierarchy;
using Microsoft.Xna.Framework;
using MSEnchant.UI.Control;
using ReLogic.Content;
using ReLogic.Graphics;

namespace MSEnchant.UI.Component;

public class MSMultiLineText : MSElement
{
    private string _content;

    public string Content
    {
        get => _content;
        set
        {
            _content = value;
            UpdateContent();
        }
    }

    private float _maxTextWidth;

    public float MaxTextWidth
    {
        get => _maxTextWidth;
        set
        {
            _maxTextWidth = value;
            UpdateContent();
        }
    }

    private Asset<DynamicSpriteFont> _font = Global.TextRegular;

    public Asset<DynamicSpriteFont> Font
    {
        get => _font;
        set
        {
            _font = value;
            UpdateContent();
        }
    }

    private float _fontScale = 1f;

    public float FontScale
    {
        get => _fontScale;
        set
        {
            _fontScale = value;
            UpdateContent();
        }
    }

    private bool _alignCenter = true;

    public bool AlignCenter
    {
        get => _alignCenter;
        set
        {
            _alignCenter = value;
            UpdateContent();
        }
    }

    private bool _newLineWhenReachMaxWidth = false;

    public bool NewLineWhenReachMaxWidth
    {
        get => _newLineWhenReachMaxWidth;
        set
        {
            _newLineWhenReachMaxWidth = value;
            UpdateContent();
        }
    }

    public MSMultiLineText(string content, float left = 0f, float top = 0f) : base(left, top)
    {
        Content = content;
    }

    protected void UpdateContent()
    {
        lock (Elements)
        {
            foreach (var element in Elements.ToArray())
            {
                element.Remove();
            }

            Width.Set(0f, 0f);
            Height.Set(0f, 0f);
            Recalculate();

            var lines = Content.Split("\n").ToList();
            var nextOffset = new Vector2(0);
            var size = Vector2.Zero;
            for (var i = 0; i < lines.Count; i++)
            {
                var last = Elements.LastOrDefault() as MSText;
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line))
                {
                    nextOffset = new Vector2(0, (last?.TextSize.Y ?? 0) / 2);
                    size.Y += nextOffset.Y;
                    continue;
                }

                var text = new MSText(line, FontScale)
                {
                    Font = Font
                };
                if (MaxTextWidth > 0)
                {
                    var nextLineOrigin = lines.ElementAtOrDefault(i + 1) ?? string.Empty;
                    var nextLineAppends = string.Empty;
                    while (text.TextSize.X >= MaxTextWidth)
                    {
                        if (lines.Count == i + 1)
                            lines.Add(string.Empty);

                        var lastCharacter = text.Text[^1];
                        nextLineAppends = lastCharacter + nextLineAppends;
                        text.Text = text.Text[..^1];
                    }

                    if (nextLineAppends != string.Empty)
                    {
                        if (NewLineWhenReachMaxWidth)
                            lines.Insert(i + 1, nextLineAppends);
                        else
                            lines[i + 1] = nextLineAppends + nextLineOrigin;
                    }
                }

                var dimension = GetDimensions();
                if (AlignCenter)
                    text.MarginLeft = dimension.Width / 2f - text.TextSize.X / 2f;
                else
                    text.MarginLeft = 0;

                text.MarginLeft += nextOffset.X;

                text.MarginTop = last?.MarginTop ?? 0;
                if (last != null)
                    text.MarginTop += text.TextSize.Y - 3;
                text.MarginTop += nextOffset.Y;

                nextOffset = Vector2.Zero;

                if (text.TextSize.X > size.X)
                    size.X = text.TextSize.X;

                if (text.MarginTop > size.Y)
                    size.Y = text.MarginTop + text.TextSize.Y;
                
                Append(text);
            }

            Width.Set(size.X, 0f);
            Height.Set(size.Y, 0f);
            Recalculate();
        }
    }
}