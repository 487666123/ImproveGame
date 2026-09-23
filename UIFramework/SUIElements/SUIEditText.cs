using ImproveGame.Common.Configs;
using ImproveGame.UIFramework.BaseViews;
using Terraria.GameInput;

namespace ImproveGame.UIFramework.SUIElements;

public class SUIEditText : TimerView
{
    /// <summary>
    /// 正在编辑文本
    /// </summary>
    public bool IsWritingText { get; private set; }

    public Vector2 TextAlign;
    public Vector2 TextOffset;

    public Color TextColor = Color.White;
    public Color TextBorderColor = Color.Black;

    /// <summary>
    /// 显示文本
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// 文字比例
    /// </summary>
    public float TextScale { get; set; } = 1f;

    /// <summary>
    /// 使文本作为 HJson 的 Key
    /// </summary>
    public bool KeyMode { get; set; }

    /// <summary>
    /// 提示文本
    /// </summary>
    public string HintText { get; set; }

    /// <summary>
    /// 使文本作为 HJson 的 Key
    /// </summary>
    public bool HintKeyMode { get; set; }

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        IsWritingText = true;
        base.LeftMouseDown(evt);
    }

    public override void LeftMouseUp(UIMouseEvent evt)
    {
        base.LeftMouseUp(evt);
    }

    public override void MouseOver(UIMouseEvent evt)
    {
        SoundEngine.PlaySound(SoundID.MenuTick);

        base.MouseOver(evt);
    }

    public override void Update(GameTime gameTime)
    {
        // 正在编辑文本
        if (IsWritingText)
        {
            // 可以占用键盘
            PlayerInput.WritingText = true;
            // 当前输入文本接管者覆盖
            Main.CurrentInputTextTakerOverride = this;
        }

        base.Update(gameTime);
    }

    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);

        Vector2 innerPos = GetInnerDimensions().Position();
        Vector2 innerSize = GetInnerDimensions().Size();

        if (string.IsNullOrEmpty(Text))
        {
            Vector2 textSize = MyUtils.GetChatFontSize(HintText, TextScale);
            Vector2 textOffset = (innerSize - textSize) * TextAlign + TextOffset;
            textOffset.Y += UIConfigs.Instance.BigFontOffsetY * TextScale;
            Vector2 textPos = innerPos + textOffset;

            MyUtils.DrawString(textPos, HintText, TextColor, TextBorderColor, TextScale);
        }
        else
        {
            Vector2 textSize = MyUtils.GetChatFontSize(Text, TextScale);
            Vector2 textOffset = (innerSize - textSize) * TextAlign + TextOffset;
            textOffset.Y += UIConfigs.Instance.BigFontOffsetY * TextScale;
            Vector2 textPos = innerPos + textOffset;

            MyUtils.DrawString(textPos, Text, TextColor, TextBorderColor, TextScale);
        }
    }
}
