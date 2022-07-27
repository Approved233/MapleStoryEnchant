using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameInput;
using Terraria.UI;

namespace MSEnchant.UI.Control;

public class MSList : UIElement, IEnumerable<UIElement>
{
    public List<UIElement> _items = new();
    protected MSScrollbar _scrollbar;
    internal UIElement _innerList = new MSInnerList();
    private float _innerListHeight;
    public float ListPadding = 5f;
    public Action<List<UIElement>> ManualSortMethod;

    public int Count => _items.Count;

    public MSList()
    {
        _innerList.OverflowHidden = false;
        _innerList.Width.Set(0.0f, 1f);
        _innerList.Height.Set(0.0f, 1f);
        OverflowHidden = true;
        Append(_innerList);
    }

    public float GetTotalHeight() => _innerListHeight;

    public void Goto(ElementSearchMethod searchMethod)
    {
        for (int index = 0; index < _items.Count; ++index)
        {
            if (searchMethod(_items[index]))
            {
                _scrollbar.ViewPosition = _items[index].Top.Pixels;
                break;
            }
        }
    }

    public virtual void Add(UIElement item)
    {
        _items.Add(item);
        _innerList.Append(item);
        UpdateOrder();
        _innerList.Recalculate();
    }

    public virtual bool Remove(UIElement item)
    {
        _innerList.RemoveChild(item);
        UpdateOrder();
        return _items.Remove(item);
    }

    public virtual void Clear()
    {
        _innerList.RemoveAllChildren();
        _items.Clear();
    }

    public override void Recalculate()
    {
        base.Recalculate();
        UpdateScrollbar();
    }

    public override void ScrollWheel(UIScrollWheelEvent evt)
    {
        base.ScrollWheel(evt);
        if (_scrollbar == null)
            return;
        
        _scrollbar.Scroll(evt.ScrollWheelValue);
        // _scrollbar.ViewPosition -= evt.ScrollWheelValue;
    }

    public override void RecalculateChildren()
    {
        base.RecalculateChildren();
        float pixels = 0.0f;
        for (int index = 0; index < _items.Count; ++index)
        {
            float num = _items.Count == 1 ? 0.0f : ListPadding;
            _items[index].Top.Set(pixels, 0.0f);
            _items[index].Recalculate();
            CalculatedStyle outerDimensions = _items[index].GetOuterDimensions();
            pixels += outerDimensions.Height + num;
        }

        _innerListHeight = pixels;
    }

    private void UpdateScrollbar()
    {
        if (_scrollbar == null)
            return;
        
        _scrollbar.SetView(GetInnerDimensions().Height, _innerListHeight);
    }

    public void SetScrollbar(MSScrollbar scrollbar)
    {
        _scrollbar = scrollbar;
        UpdateScrollbar();
    }

    public void UpdateOrder()
    {
        if (ManualSortMethod != null)
            ManualSortMethod(_items);
        else
            _items.Sort(SortMethod);
        UpdateScrollbar();
    }

    public int SortMethod(UIElement item1, UIElement item2) => item1.CompareTo(item2);

    public override List<SnapPoint> GetSnapPoints()
    {
        List<SnapPoint> snapPoints = new List<SnapPoint>();
        SnapPoint point;
        if (GetSnapPoint(out point))
            snapPoints.Add(point);
        foreach (UIElement uiElement in _items)
            snapPoints.AddRange(uiElement.GetSnapPoints());
        return snapPoints;
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        if (_scrollbar != null)
            _innerList.Top.Set(0.0f - _scrollbar.GetValue(), 0.0f);
        Recalculate();
    }

    public IEnumerator<UIElement> GetEnumerator() => ((IEnumerable<UIElement>)_items).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<UIElement>)_items).GetEnumerator();

    public float ViewPosition
    {
        get => _scrollbar.ViewPosition;
        set => _scrollbar.ViewPosition = value;
    }

    public virtual void AddRange(IEnumerable<UIElement> items)
    {
        _items.AddRange(items);
        foreach (UIElement element in items)
            _innerList.Append(element);
        UpdateOrder();
        _innerList.Recalculate();
    }

    public override void MouseOver(UIMouseEvent evt)
    {
        base.MouseOver(evt);
        PlayerInput.LockVanillaMouseScroll("MSEnchant/UI/MSList");
    }

    public delegate bool ElementSearchMethod(UIElement element);

    private class MSInnerList : UIElement
    {
        public override bool ContainsPoint(Vector2 point) => true;

        protected override void DrawChildren(SpriteBatch spriteBatch)
        {
            Vector2 position1 = Parent.GetDimensions().Position();
            Vector2 dimensions1 = new Vector2(Parent.GetDimensions().Width, Parent.GetDimensions().Height);
            foreach (UIElement element in Elements)
            {
                Vector2 position2 = element.GetDimensions().Position();
                Vector2 dimensions2 = new Vector2(element.GetDimensions().Width, element.GetDimensions().Height);
                if (Collision.CheckAABBvAABBCollision(position1, dimensions1, position2, dimensions2))
                    element.Draw(spriteBatch);
            }
        }

        public override Rectangle GetViewCullingArea() => Parent.GetDimensions().ToRectangle();
    }
}