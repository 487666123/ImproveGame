using ImproveGame.UI.ModernConfig.OptionElements;
using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.Common;

namespace ImproveGame.UIFramework.SUIElements;

public class SUIDropdownListContainer : View
{
    private class DropdownOption : TimerView
    {
        internal const int ElementHeight = 32;
        private readonly SUIDropdownListContainer _container;
        private readonly SlideText _labelElement;

        public DropdownOption(string name, SUIDropdownListContainer father)
        {
            RelativeMode = RelativeMode.Vertical;
            Spacing = new Vector2(0);
            SetSizePixels(0f, ElementHeight);
            SetSizePercent(1f, 0f);
            Rounded = new Vector4(8);
            Border = 3;
            _container = father; // 叠！

            OnLeftMouseDown += LeftMouseDownEvent;

            _labelElement = new SlideText(name, 2)
            {
                VAlign = 0.5f,
                Left = { Pixels = 8 },
                RelativeMode = RelativeMode.None
            };
            _labelElement.JoinParent(this);
        }

        private void LeftMouseDownEvent(UIMouseEvent evt, UIElement listeningElement)
        {
            _container.OnOptionSelected(_labelElement._text);
        }

        private static Color BeginBgColor => UIStyle.ButtonBg;
        private static Color EndBgColor => UIStyle.ButtonBgHover;

        public override void DrawSelf(SpriteBatch spriteBatch)
        {
            BgColor = HoverTimer.Lerp(Color.Transparent, EndBgColor);
            if (_container._currentSelectedLabel == _labelElement._text)
                BgColor = HoverTimer.Lerp(BeginBgColor, EndBgColor);

            if (IsMouseHovering)
                _container.HoverOnOptionCallback?.Invoke(_labelElement._text);

            base.DrawSelf(spriteBatch);
        }
    }

    /// <summary>
    /// 调用下拉框的UI元素
    /// </summary>
    public View DropdownCaller;

    private readonly SUIScrollView2 _dropdownList;
    private string _currentSelectedLabel;

    /// <summary>
    /// 某个选项被选中时调用
    /// </summary>
    public Action<string> OptionSelectedCallback;

    /// <summary>
    /// 绘制时调用，用于显示Tooltip
    /// </summary>
    public Action DrawCallback;

    /// <summary>
    /// 鼠标悬停在某个选项上时调用，用于显示Tooltip，执行后于DrawCallback
    /// </summary>
    public Action<string> HoverOnOptionCallback;

    public bool Enabled
    {
        get => !IgnoresMouseInteraction;
        set
        {
            IgnoresMouseInteraction = !value;

            if (value == false)
            {
                DropdownCaller = null;
                _animationTimer.Close();
            }
        }
    }

    public SUIDropdownListContainer()
    {
        SetSizePercent(1f);
        SetPadding(0, 0, 0, 0);
        Enabled = false;

        _dropdownList = new SUIScrollView2(Orientation.Vertical)
        {
            HAlign = 0f,
            VAlign = 0f,
            OverflowHidden = true,
            Rounded = new Vector4(10f),
            BgColor = UIStyle.DropdownListBg,
            IgnoresMouseInteraction = false
        };
        _dropdownList.SetPadding(4);
        _dropdownList.JoinParent(this);
    }

    public void SetCurrentPosition(float x, float y, float width, int count)
    {
        var dimensions = GetDimensions();
        // 决定高度
        float containerBottom = dimensions.ToRectangle().Bottom - 10;
        float optionsHeight = DropdownOption.ElementHeight * count + 10;
        float maximumHeight = 340; // 不要太长，限制高度
        float height = MyUtils.Min(optionsHeight, maximumHeight);
        _dropdownList.SetSizePixels(width, height);

        // 决定位置，x和y给的是屏幕坐标
        _mouseYClicked = y * Main.UIScale;
        // 限制在UI界面内
        x -= dimensions.X;
        float bottom = y + height;
        if (bottom > containerBottom)
        {
            // 计算屏幕坐标
            var scrnY = containerBottom - height;
            // 计算对父元素的相对坐标
            var relativeY = scrnY - dimensions.Y;
            _dropdownList.SetPosPixels(x, relativeY);
        }
        else
        {
            y -= dimensions.Y;
            _dropdownList.SetPosPixels(x, y);
        }
    }

    public void BuildDropdownList(float x, float y, float width, string[] options, string currentLabel, View caller)
    {
        int count = options.Length;
        _dropdownList.ListView.RemoveAllChildren();
        DropdownCaller = caller;
        SetCurrentPosition(x, y, width, count);


        // 添加元素
        foreach (string label in options)
        {
            var option = new DropdownOption(label, this);
            option.OnUpdate += delegate
            {
                if (option.IsMouseHovering)
                    Main.LocalPlayer.mouseInterface = true;
            };

            option.JoinParent(_dropdownList.ListView);
        }

        // 使其有效 & 最后处理
        _animationTimer.Open();
        _currentSelectedLabel = currentLabel;
        Enabled = true;
        Recalculate();
    }

    public void OnOptionSelected(string option)
    {
        Enabled = false;
        OptionSelectedCallback?.Invoke(option);
    }

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        Enabled = false;
        SoundEngine.PlaySound(SoundID.MenuTick);
        base.LeftMouseDown(evt);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (!IsMouseHovering)
            return;

        Main.LocalPlayer.mouseInterface = true;
    }

    #region Animation - 出现动画

    private float _mouseYClicked; // 鼠标点击时的Y坐标
    private readonly AnimationTimer _animationTimer = new AnimationTimer(3f);

    public override void Draw(SpriteBatch sb)
    {
        if (!Enabled && _animationTimer.Closed)
            return;

        _animationTimer.UpdateHighFps();
        _dropdownList.BgColor = UIStyle.DropdownListBg;

        // 创建矩形
        var dimensionsRect = _dropdownList.GetDimensions().ToRectangle();
        float top = dimensionsRect.Top * Main.UIScale;
        float bottom = dimensionsRect.Bottom * Main.UIScale;
        float maxOffset = Math.Max(Math.Abs(top - _mouseYClicked), Math.Abs(bottom - _mouseYClicked));
        float offset = _animationTimer.Lerp(0, maxOffset);
        int screenWidth = (int)(Main.screenWidth * Main.UIScale);
        var clippingRectangle = new Rectangle(0, (int)(_mouseYClicked - offset), screenWidth, (int)offset * 2);

        Rectangle scissorRectangle = sb.GraphicsDevice.ScissorRectangle;
        SamplerState anisotropicClamp = SamplerState.AnisotropicClamp;

        sb.End();
        Rectangle adjustedClippingRectangle =
            Rectangle.Intersect(clippingRectangle, sb.GraphicsDevice.ScissorRectangle);
        sb.GraphicsDevice.ScissorRectangle = adjustedClippingRectangle;
        sb.GraphicsDevice.RasterizerState = OverflowHiddenRasterizerState;
        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, anisotropicClamp, DepthStencilState.None,
            OverflowHiddenRasterizerState, null, Main.UIScaleMatrix);

        DrawCallback?.Invoke();
        base.Draw(sb);

        var rasterizerState = sb.GraphicsDevice.RasterizerState;
        sb.End();
        sb.GraphicsDevice.ScissorRectangle = scissorRectangle;
        sb.GraphicsDevice.RasterizerState = rasterizerState;
        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, anisotropicClamp, DepthStencilState.None,
            rasterizerState, null, Main.UIScaleMatrix);
    }

    #endregion
}