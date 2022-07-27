using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MSEnchant.UI.Control;

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
                    continue;
                }

                var text = new MSText(line);
                if (MaxTextWidth > 0)
                {
                    var nextLine = lines.ElementAtOrDefault(i + 1) ?? string.Empty;
                    while (text.TextSize.X >= MaxTextWidth)
                    {
                        if (lines.Count == i + 1)
                            lines.Add(string.Empty);

                        var lastCharacter = text.Text[^1];
                        nextLine = lastCharacter + nextLine;
                        text.Text = text.Text[..^1];
                        lines[i + 1] = nextLine;
                    }
                }

                var dimension = GetDimensions();
                text.MarginLeft = dimension.Width / 2f - text.TextSize.X / 2f;
                text.MarginLeft += nextOffset.X;

                text.MarginTop = last?.MarginTop ?? 0;
                if (last != null)
                    text.MarginTop += text.TextSize.Y - 3;
                text.MarginTop += nextOffset.Y;

                nextOffset = Vector2.Zero;

                if (text.TextSize.X > size.X)
                    size.X = text.TextSize.X;

                if (text.MarginTop > size.Y)
                    size.Y = text.MarginTop;

                Append(text);
            }


            Width.Set(size.X, 0f);
            Height.Set(size.Y, 0f);
            Recalculate();
        }
    }
}