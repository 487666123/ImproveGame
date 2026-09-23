using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.Common;
using ImproveGame.UIFramework.SUIElements;

namespace ImproveGame.UI.PlayerStats;

/// <summary>
/// 属性类别
/// </summary>
public class BaseStatsCategory(Texture2D texture, string nameKey, bool isAddedFromCall = false, Texture2D modSmallIcon = null)
{
    /// <summary>
    /// Whether this stats category is added via Mod.Call
    /// </summary>
    public bool IsAddedFromCall { get; set; } = isAddedFromCall;

    public Vector2? UIPosition;

    /// <summary>
    /// 收藏
    /// </summary>
    public bool Favorite { get; set; }

    public List<BaseStat> BaseProperties { get; private set; } = new();

    public Texture2D ModSmallIcon { get; set; } = modSmallIcon;

    public Texture2D Texture { get; set; } = texture;

    public string NameKey { get; set; } = nameKey;

    public string Name => IsAddedFromCall ? Language.GetTextValue(NameKey) : MyUtils.GetText(NameKey);

    /// <summary>
    /// 创建卡片
    /// </summary>
    public StatsCard CreateCard(out SUIImage image, out SUITitle title)
    {
        // 卡片主体
        StatsCard card = new(this, UIStyle.PanelBorder * 0f, UIStyle.PanelBg * 0f, 10f, 2);
        card.OverflowHidden = true;

        // 标题图标
        image = new SUIImage(Texture)
        {
            ImagePercent = new Vector2(0.5f),
            ImageOrigin = new Vector2(0.5f),
            ImageScale = 0.8f,
            DragIgnore = true,
            TickSound = false
        };
        image.Width.Pixels = 20f;
        image.Height.Percent = 1f;
        image.JoinParent(card.TitleView);

        // 标题文字
        title = new(Name, 0.36f)
        {
            HAlign = 0.5f
        };
        title.Height.Percent = 1f;
        title.Width.Pixels = title.TextSize.X;
        title.JoinParent(card.TitleView);

        // 模组图标
        if (ModSmallIcon is null) return card;

        var modIcon = new SUIImage(ModSmallIcon)
        {
            ImagePercent = new Vector2(0.5f),
            ImageOrigin = new Vector2(0.5f),
            ImageScale = 0.8f,
            DragIgnore = true,
            TickSound = false,
            Width = { Pixels = 20f },
            Height = { Percent = 1f },
            Left = { Pixels = -20f, Percent = 1f }
        };
        modIcon.JoinParent(card.TitleView);

        return card;
    }

    public void AppendProperties(StatsCard card)
    {
        for (int i = 0; i < BaseProperties.Count; i++)
        {
            BaseStat stat = BaseProperties[i];
            StatBar statBar = new(stat.Name, stat.Value, stat);

            if (stat.Favorite)
            {
                card.Append(statBar);
            }
        }
    }

    public void AppendPropertiesForControl(View view, StatsCard card)
    {
        for (int i = 0; i < BaseProperties.Count; i++)
        {
            BaseStat stat = BaseProperties[i];
            StatBar bar = new(stat.Name, stat.Value, stat);

            bar.OnUpdate += (_) =>
            {
                Color red = new Color(1f, 0f, 0f);
                bar.BorderColor = (stat.Parent.Favorite && stat.Favorite) ? UIStyle.ItemSlotBorderFav : Color.Transparent;
                bar.Border = (stat.Parent.Favorite && stat.Favorite) ? 2 : 0;
            };

            bar.OnLeftMouseDown += (_, _) =>
            {
                stat.Favorite = !stat.Favorite;

                foreach (var item in view.Children)
                {
                    if (item is StatsCard innerCard && innerCard.StatsCategory == card.StatsCategory)
                    {
                        innerCard.RemoveAllChildren();
                        innerCard.Append(innerCard.TitleView);
                        innerCard.StatsCategory.AppendProperties(innerCard);
                    }
                }
            };

            card.Console = true;
            card.Append(bar);
        }
    }
}
