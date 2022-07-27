using Microsoft.Xna.Framework;
using MSEnchant.Helper;
using MSEnchant.UI.Control;
using Terraria;
using Terraria.UI;

namespace MSEnchant.UI.Component;

public class MSPopup : MSElement
{
    protected MSImage BackgroundComponent;

    protected MSMultiLineText MultiLineText;

    public string Content
    {
        get => MultiLineText.Content;
        set => MultiLineText.Content = value;
    }

    protected string BaseTexturePath;

    protected virtual float ContentHeight => 114f;

    protected virtual float ContentTop => 0f;

    public MSPopup(string texture, string content, float left = 0f, float top = 0f) : base(left, top)
    {
        BaseTexturePath = texture;
        AllowDrag = true;
        AlwaysTop = true;

        Append(BackgroundComponent = new MSImage($"{texture}.backgrnd"));

        Width.Set(BackgroundComponent.Texture.Width(), 0f);
        Height.Set(BackgroundComponent.Texture.Height(), 0f);

        DoInit();

        Append(MultiLineText = new MSMultiLineText(string.Empty, Width.Pixels / 2f, ContentTop));

        Content = content;

        if (ContentTop == 0f)
            MultiLineText.Top.Set(ContentHeight / 2 - MultiLineText.Height.Pixels / 2, 0f);
    }

    protected virtual void DoInit()
    {
    }

    protected void AddButton(string name, float left = 0f, float top = 0f)
    {
        var button = new MSButton($"{BaseTexturePath}.{name}", left, top);
        button.OnClick += (evt, element) =>
        {
            OnButtonClick?.Invoke(button, name);
            this.RemoveNextFrame();
        };
        AddChild(button);
    }

    protected void AddChild(UIElement element)
    {
        BackgroundComponent.Append(element);
    }

    protected override bool PrepareDrag(Vector2 mouse)
    {
        var e = GetElementAt(mouse);
        if (e is MSButton button && button.IsPressing)
            return false;
        
        return base.PrepareDrag(mouse);
    }

    public event Global.PopupButtonClickDelegate OnButtonClick;
}