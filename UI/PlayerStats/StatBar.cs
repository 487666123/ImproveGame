using ImproveGame.Common.Configs;
using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.Common;

namespace ImproveGame.UI.PlayerStats;

public class StatBar : TimerView
{
    public BaseStat BaseStat { get; set; }

    public float Scale = 0.85f;
    public string StatName;
    public Vector2 TextSize;
    private Func<string> StatValue { get; set; }

    public StatBar(string statName, Func<string> statValue, BaseStat baseStat)
    {
        PreventOverflow = false;
        RelativeMode = RelativeMode.Vertical;
        Spacing = new Vector2(2f);

        Width.Pixels = 0f;
        Width.Percent = 1f;
        Height.Pixels = 30f;

        PaddingLeft = 10f;
        PaddingRight = 14f;
        DragIgnore = true;

        StatName = statName;
        TextSize = FontAssets.MouseText.Value.MeasureString(statName) * Scale;
        StatValue = statValue;
        StatValue ??= () => string.Empty;

        BgColor = UIStyle.StatCardBg;
        Rounded = new Vector4(6f);

        BaseStat = baseStat;
    }

    public override void DrawSelf(SpriteBatch sb)
    {
        base.DrawSelf(sb);
        Vector2 pos = GetDimensions().Position();
        Vector2 size = GetDimensions().Size();
        Vector2 innerPos = GetInnerDimensions().Position();
        Vector2 innerSize = GetInnerDimensions().Size();

        Vector2 iconPos = innerPos + new Vector2(30 * Scale, innerSize.Y) / 2f;

        Vector2 textPos = innerPos +
            new Vector2(0, UIConfigs.Instance.GeneralFontOffsetY * Scale + (innerSize.Y - TextSize.Y) / 2);
        MyUtils.DrawString(textPos, StatName, Color.White, Color.Black, Scale, spread: 1f);

        string infoText = StatValue();
        Vector2 infoSize = MyUtils.GetFontSize(infoText) * Scale;
        Vector2 infoPos = innerPos + new Vector2(innerSize.X - infoSize.X,
            UIConfigs.Instance.GeneralFontOffsetY * Scale + (innerSize.Y - infoSize.Y) / 2);
        MyUtils.DrawString(infoPos, infoText, Color.White, Color.Black, Scale, spread: 1f);
    }
}
