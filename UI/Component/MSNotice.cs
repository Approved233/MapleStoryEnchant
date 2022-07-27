using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MSEnchant.Helper;
using MSEnchant.UI.Control;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace MSEnchant.UI.Component;

public class MSNotice : MSElement
{
    protected Asset<Texture2D> TopTexture;

    protected Asset<Texture2D> ContentTexture;

    protected Asset<Texture2D> BottomTexture;

    protected Asset<Texture2D> BoxTopTexture;

    protected Asset<Texture2D> BoxContentTexture;

    protected Asset<Texture2D> BoxBottomTexture;

    private bool _showConfirmButton = true;

    public bool ShowConfirmButton
    {
        get => _showConfirmButton;
        set
        {
            _showConfirmButton = value;
            UpdateSize();
        }
    }

    private bool _showCancelButton = false;

    public bool ShowCancelButton
    {
        get => _showCancelButton;
        set
        {
            _showCancelButton = value;
            UpdateSize();
        }
    }

    protected MSMultiLineText ContentText;

    protected MSButton ConfirmButton;

    protected MSButton CancelButton;

    public string Content
    {
        get => ContentText.Content;
        set
        {
            ContentText.Content = value;
            UpdateSize();
        }
    }

    public float TextPadding = 30f;

    public MSNotice(string texture, string content, float left = 0f, float top = 0f) : base(left, top)
    {
        AlwaysTop = true;

        TopTexture = ModContent.Request<Texture2D>($"{texture}.t", AssetRequestMode.ImmediateLoad);
        ContentTexture = ModContent.Request<Texture2D>($"{texture}.c", AssetRequestMode.ImmediateLoad);
        BottomTexture = ModContent.Request<Texture2D>($"{texture}.s", AssetRequestMode.ImmediateLoad);
        BoxTopTexture = ModContent.Request<Texture2D>($"{texture}.c_box", AssetRequestMode.ImmediateLoad);
        BoxContentTexture = ModContent.Request<Texture2D>($"{texture}.box", AssetRequestMode.ImmediateLoad);
        BoxBottomTexture = ModContent.Request<Texture2D>($"{texture}.s_box", AssetRequestMode.ImmediateLoad);

        Append(ConfirmButton = new MSButton("MSEnchant/Assets/BtOK4"));
        Append(CancelButton = new MSButton("MSEnchant/Assets/BtCancel4"));
        Append(ContentText = new MSMultiLineText(content));

        ConfirmButton.OnClick += (evt, element) =>
        {
            OnButtonClick?.Invoke(evt, element);
            OnConfirmButtonClick?.Invoke(evt, element);
        };

        CancelButton.OnClick += (evt, element) =>
        {
            OnButtonClick?.Invoke(evt, element);
            OnCancelButtonClick?.Invoke(evt, element);
        };

        Width.Set(TopTexture.Width(), 0f);

        AllowDrag = true;

        UpdateSize();

        Left.Set(Left.Pixels, 0f);
        Top.Set(Top.Pixels, 0f);
    }

    protected readonly List<ComponentData> ComponentDataList = new List<ComponentData>();

    protected void UpdateSize()
    {
        ComponentDataList.Clear();

        void AppendTextureInfo(Asset<Texture2D> texture, ref Vector2 offset)
        {
            ComponentDataList.Add(new ComponentData
            {
                Texture = texture,
                Offset = new Vector2(offset.X, offset.Y)
            });
            offset.Y += texture.Height();
        }

        var offset = new Vector2();

        AppendTextureInfo(TopTexture, ref offset);

        ContentText.MaxTextWidth = ContentTexture.Width() - TextPadding * 2;

        var contentTextSize = ContentText.PixelSize();
        var countContentTexture = Math.Max(2, (int)Math.Ceiling(contentTextSize.Y / ContentTexture.Height()) + 1);

        ContentText.MarginLeft = ContentTexture.Width() / 2f;
        ContentText.MarginTop = offset.Y;

        for (var i = 0; i < countContentTexture; i++)
        {
            AppendTextureInfo(ContentTexture, ref offset);
        }

        ConfirmButton.Visible = ShowConfirmButton;
        CancelButton.Visible = ShowCancelButton;

        if (ShowConfirmButton || ShowCancelButton)
        {
            AppendTextureInfo(BoxTopTexture, ref offset);

            var buttons = new List<MSButton>();
            if (ConfirmButton.Visible)
                buttons.Add(ConfirmButton);
            if (CancelButton.Visible)
                buttons.Add(CancelButton);

            MSButton? prevButton = null;
            for (var i = buttons.Count - 1; i >= 0; i--)
            {
                var button = buttons[i];
                if (prevButton == null)
                    button.MarginLeft = BoxContentTexture.Width() - 18 - button.Width.Pixels;
                else
                    button.MarginLeft = prevButton.MarginLeft - button.Width.Pixels;

                button.MarginTop = offset.Y;

                prevButton = button;
            }

            AppendTextureInfo(BoxContentTexture, ref offset);

            AppendTextureInfo(BoxBottomTexture, ref offset);
        }
        else
            AppendTextureInfo(BottomTexture, ref offset);

        Height.Set(offset.Y, 0f);

        Recalculate();
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);

        var dimension = GetInnerDimensions();
        var pos = dimension.Position();

        spriteBatch.UseNonPremultiplied(() =>
        {
            foreach (var component in ComponentDataList)
            {
                spriteBatch.Draw(component.Texture.Value, pos + component.Offset, Color.White);
            }
        });
    }

    protected override bool PrepareDrag(Vector2 mouse)
    {
        var e = GetElementAt(mouse);
        if (e is MSButton button && button.IsPressing)
            return false;

        return base.PrepareDrag(mouse);
    }

    public event MouseEvent OnConfirmButtonClick;
    public event MouseEvent OnCancelButtonClick;
    public event MouseEvent OnButtonClick;

    public struct ComponentData
    {
        public Asset<Texture2D> Texture;
        public Vector2 Offset;
    }
}