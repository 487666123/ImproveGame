using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.Common;
using ImproveGame.UIFramework.Graphics2D;
using ImproveGame.UIFramework.SUIElements;
using System.ComponentModel;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;

namespace ImproveGame.UI.ModernConfig.OptionElements;

public class OptionSlider : ModernConfigOption //去掉了sealed
{
    private class SlideBox : View
    {
        private Vector2 MousePositionRelative => Main.MouseScreen - GetDimensions().Position();
        public event Action ValueChangeCallback;
        public event Action EndDraggingCallback;
        private float _value;
        private bool _dragging;

        public Utils.ColorLerpMethod ColorMethod;
        public bool colorMethodPendingModified = true;
        Texture2D colorBar;


        public float Value
        {
            get => Math.Clamp(_value, 0, 1);
            set
            {
                float wantedValue = Math.Clamp(value, 0, 1);
                if (Math.Clamp(_value, 0, 1) != wantedValue)
                {
                    _value = Math.Clamp(value, 0, 1);
                    ValueChangeCallback?.Invoke();
                }
            }
        }

        public SlideBox()
        {
            BgColor = Color.Black * 0.3f;
            Rounded = new Vector4(6f);
            SetSizePixels(80f, 28f);
            VAlign = 0.5f;
            Rounded = new Vector4(14f);
        }

        ~SlideBox()
        {
            Main.RunOnMainThread(() => colorBar?.Dispose());
            //colorBar?.Dispose();
        }

        public override void LeftMouseDown(UIMouseEvent evt)
        {
            base.LeftMouseDown(evt);
            UpdateDragging();
            _dragging = true;
        }

        public override void LeftMouseUp(UIMouseEvent evt)
        {
            base.LeftMouseUp(evt);
            if (_dragging)
            {
                _dragging = false;
                EndDraggingCallback?.Invoke();
            }
        }

        private void UpdateDragging()
        {
            var dimensions = GetDimensions();
            var size = dimensions.Size();
            float roundRadius = size.Y / 2f - 2f; // 半径
            float roundDiameter = roundRadius * 2; // 直径
            float moveWidth = size.X - roundDiameter; // 移动宽度
            float mousePositionX = MousePositionRelative.X;
            float adjustedMousePosX = mousePositionX - roundRadius; // 让圆心保持在鼠标位置
            Value = adjustedMousePosX / moveWidth;
        }

        public override void DrawSelf(SpriteBatch spriteBatch)
        {
            // 基本字段
            var dimensions = GetDimensions();
            var position = dimensions.Position();
            var center = dimensions.Center();
            var size = dimensions.Size();

            if (ColorMethod != null && !IgnoresMouseInteraction)
            {
                if (colorBar == null)
                {
                    try
                    {
                        colorBar = new Texture2D(Main.graphics.GraphicsDevice, 300, 1);
                    }
                    catch
                    {
                        Texture2D texdummy = null;
                        Main.RunOnMainThread(() => { texdummy = new Texture2D(Main.graphics.GraphicsDevice, 300, 1); });
                        colorBar = texdummy;
                    }
                }
                if (colorMethodPendingModified)
                {
                    Color[] colors = new Color[300];
                    for (int n = 0; n < 300; n++)
                        colors[n] = ColorMethod.Invoke(n / 299f);
                    colorBar.SetData(colors);
                    colorMethodPendingModified = false;
                }
                // 114514114514
                SDFRectangle.BarColor(position, size, Rounded, colorBar, Vector2.UnitX / dimensions.Width, 0, Main.UIScaleMatrix);//-Main.GlobalTimeWrappedHourly

                //SDFRectangle.HasBorder(position, size, Rounded, Color.Red, 4f, Color.White, Main.UIScaleMatrix);
            }
            else
                base.DrawSelf(spriteBatch);

            if (_dragging)
                UpdateDragging();



            // 圆的属性
            float roundRadius = size.Y / 2f - 2f; // 半径
            float roundDiameter = roundRadius * 2; // 直径
            float moveWidth = size.X - roundDiameter; // 移动宽度
            var roundCenter = position + new Vector2(Value * moveWidth, 0f);
            roundCenter.Y = center.Y;
            roundCenter.X += roundRadius;
            var roundLeftTop = roundCenter - new Vector2(roundRadius);

            // 颜色选择
            var innerColor = ColorMethod != null ? ColorMethod.Invoke(_value) : UIStyle.SliderRound;
            //var borderColor = innerColor;
            var borderColor = UIStyle.SliderRound;
            if (MyUtils.MouseInRound(roundCenter, (int)roundRadius))
                borderColor = UIStyle.SliderRoundHover;


            if (IgnoresMouseInteraction)
            {
                borderColor = Color.Gray * 0.6f;
                innerColor = Color.Gray * 0.6f;
            }

            // 绘制
            SDFGraphics.HasBorderRound(roundLeftTop, default, roundDiameter, innerColor, 2f, borderColor, MyUtils.GetMatrix(true));
        }

        public void OutSideEditEnd() => EndDraggingCallback?.Invoke();
    }
    private Utils.ColorLerpMethod _colorLerpMethod;
    public void SetColorMethod(Utils.ColorLerpMethod colorMethod)
    {
        _colorLerpMethod = colorMethod;
        _slideBox.ColorMethod = colorMethod;
    }
    public void SetColorPendingModified() => _slideBox.colorMethodPendingModified = true;
    private SUISplitButton _splitButton;
    private SlideBox _slideBox;
    private SUINumericText _numericTextBox;
    public readonly static Type[] SupportedTypes = [typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal)];
    private readonly static Type[] FractionTypes = [typeof(float), typeof(double), typeof(decimal)];
    protected override void OnBind()
    {
        CheckValid();
        var v = GetValue();
        var box = new View
        {
            IsAdaptiveWidth = true,
            HAlign = 1f,
            VAlign = 0.5f,
            Height = StyleDimension.Fill
        };
        box.JoinParent(this);
        AddUpDown(box);
        AddTextBox(box);
        AddSlideBox(box);
    }
    internal double Min = 0;
    internal double Max = 1;
    internal double Default = 1;
    internal double? Increment = null;
    protected override void CheckAttributes()
    {
        base.CheckAttributes();
        var type = VarType;
        var pair = (Min, Max);
        if (IsInt)
            pair = (0.0, 100.0);


        if (type == typeof(byte))
            pair = (0.0, 255.0);
        else if (type == typeof(sbyte))
            pair = (-128.0, 127.0);
        if (VariableInfo.IsProperty && false) //如果是属性就可以玩得花一点(x
        {
            if (type == typeof(short))
                pair = (-32768.0, 32767.0);
            else if (type == typeof(ushort))
                pair = (0.0, 65536.0);
            else if (type == typeof(int))
                pair = (-2147483648.0, 2147483647.0);
            else if (type == typeof(uint))
                pair = (0.0, 4294967295);
        }
        (Min, Max) = pair;


        var rangeAttribute = GetAttribute<RangeAttribute>();
        if (rangeAttribute != null)
        {
            Max = Convert.ToDouble(rangeAttribute.Max);
            Min = Convert.ToDouble(rangeAttribute.Min);
        }


        var defaultValueAttribute = GetAttribute<DefaultValueAttribute>();
        if (defaultValueAttribute != null)
            Default = Convert.ToDouble(defaultValueAttribute.Value);

        var incrementAttribute = GetAttribute<IncrementAttribute>();
        if (incrementAttribute != null)
            Increment = Convert.ToDouble(incrementAttribute.Increment);
        if (Increment == null && IsInt)
            Increment = 1.0;

        var customConfigAttribute = GetAttribute<CustomModConfigItemAttribute>();
        if (customConfigAttribute != null)
        {
            var elem = Activator.CreateInstance(customConfigAttribute.Type);
            if (elem is RangeElement range)
            {
                if (range.ColorMethod.Invoke(0.0f) != Color.Black && range.ColorMethod.Invoke(1.0f) != Color.White)
                    _colorLerpMethod = range.ColorMethod;
            }
        }
        var sliderColor = GetAttribute<SliderColorAttribute>()?.Color;
        if (_colorLerpMethod == null && sliderColor != null)
            _colorLerpMethod = t => Color.Lerp(Color.Black * 0.3f, sliderColor.Value, t * t);// 
    }
    protected virtual void CheckValid()
    {
        if (!SupportedTypes.Contains(VarType))
            throw new Exception($"Field \"{OptionName}\" is not a supported type. OptionSlider supports all build-in-types except for Boolean, IntPtr and UIntPtr");
    }
    private void AddUpDown(View box)
    {
        //if (Increment == null) return;
        _splitButton = new SUISplitButton()
        {
            RelativeMode = RelativeMode.Horizontal,
            BgColor = Color.Black * 0.4f,
            Rounded = new Vector4(14f),
            VAlign = 0.5f,
            buttonBorderColor = Color.Black,
            buttonColor = Color.Black * 0.4f,
            Width = new(25, .0f),
            Height = new(25, .0f)
        };
        _splitButton.OnLeftMouseDown += (evt, elem) =>
        {
            SoundEngine.PlaySound(SoundID.MenuTick);
            var btn = elem as SUISplitButton;
            double value = Min + (Max - Min) * _slideBox.Value + (btn.IsUP ? 1 : -1) * Increment.Value;
            value = Math.Clamp(value, Min, Max);
            SetConfigValue(value, broadcast: true);
        };
        _splitButton.JoinParent(box);
    }

    private void AddTextBox(View box)
    {
        bool isInt = IsInt;
        _numericTextBox = new SUINumericText
        {
            RelativeMode = RelativeMode.Horizontal,
            BgColor = Color.Black * 0.4f,
            Rounded = new Vector4(14f),
            MinValue = Min,
            MaxValue = Max,
            InnerText =
            {
                TextAlign = new Vector2(0.5f, 0.5f),
                TextOffset = new Vector2(0f, -2f),
                MaxCharacterCount = isInt ? 12 : 4,
                MaxLines = 1,
                IsWrapped = false
            },
            MaxLength = isInt ? 12 : 4,
            DefaultValue = Default,
            Format = isInt ? "0" : "0.00",
            VAlign = 0.5f
        };
        _numericTextBox.ContentsChanged += (ref string text) =>
        {
            if (!double.TryParse(text, out var value))
                return;
            if (!_numericTextBox.IsWritingText) return; // 有可能是属性导致跟着另一个变了
            // if (Increment is double d)
            //     value = Math.Round((value - Min) / d) * d + Min;
            value = Math.Clamp(value, Min, Max);
            SetConfigValue(value, broadcast: false);
        };
        _numericTextBox.EndTakingInput += () =>
        {
            if (!_numericTextBox.IsValueSafe)
                return;
            var value = _numericTextBox.Value;
            // if (Increment is double d)
            //     value = Math.Round((value - Min) / d) * d + Min;
            value = Math.Clamp(value, Min, Max);
            SetConfigValue(value, broadcast: true);
        };
        _numericTextBox.SetPadding(2, 2, 2, 2); // Padding影响里面的文字绘制
        _numericTextBox.SetSizePixels(50, 28);
        _numericTextBox.JoinParent(box);
    }

    private void AddSlideBox(View box)
    {
        _slideBox = new SlideBox
        {
            RelativeMode = RelativeMode.Horizontal,
            Spacing = new Vector2(4),
        };
        _slideBox.ColorMethod = _colorLerpMethod;
        _slideBox.ValueChangeCallback += () =>
        {
            double value = Min + (Max - Min) * _slideBox.Value;
            // 更好的体验不要Increment检测
            if (Config.Mod.Name is not "ImproveGame" && Increment is double d && d != 0)
            {
                value = Math.Round(value / d) * d;
            }
            value = Math.Clamp(value, Min, Max);
            SetConfigValue(value, broadcast: false);
        };
        _slideBox.EndDraggingCallback += () =>
        {
            double value = Min + (Max - Min) * _slideBox.Value;
            if (Config.Mod.Name is not "ImproveGame" && Increment is double d && d != 0)
            {
                value = Math.Round(value / d) * d;
            }
            value = Math.Clamp(value, Min, Max);
            SetConfigValue(value, broadcast: true);
        };
        _slideBox.JoinParent(box);
    }

    private void SetConfigValue(double value, bool broadcast)
    {
        //if (!Interactable) return;
        object realValue = Convert.ChangeType(IsFractional ? value : Math.Round(value), VarType);
        SetValueDirect(realValue, broadcast);
        //ConfigHelper.SetConfigValue(Config, VariableInfo, realValue, Item, broadcast, path: path);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (Increment == null)
            _splitButton?.Remove();
        _splitButton.IgnoresMouseInteraction = !Interactable;
        _slideBox.IgnoresMouseInteraction = !Interactable;
        _numericTextBox.IgnoresMouseInteraction = !Interactable;

        // 简直是天才的转换
        var value = float.Parse(GetValue()!.ToString()!);
        if (!_numericTextBox.IsWritingText)
            _numericTextBox.Value = value;
        _slideBox.Value = InverseLerp((float)Min, (float)Max, value);
    }

    private static float InverseLerp(float a, float b, float value) => (value - a) / (b - a);
    // 为什么不用Utils.GetLerpValue() //((

    private bool IsFractional => FractionTypes.Contains(VarType);
    private bool IsInt => !IsFractional;
    public float LerpValue
    {
        get => _slideBox.Value;
        set
        {
            if (!_numericTextBox.IsWritingText)
                _numericTextBox.Value = MathHelper.Lerp((float)Min, (float)Max, value);
            _slideBox.Value = value;
        }
    }
    public void OutSideEditEnd() => _slideBox?.OutSideEditEnd();
    protected override void OnSetValueExternal(object value)
    {
        base.OnSetValueExternal(value);
    }
}