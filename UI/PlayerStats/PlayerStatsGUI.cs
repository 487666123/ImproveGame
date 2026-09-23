using ImproveGame.Common.Configs;
using ImproveGame.UIFramework;
using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.Common;
using ImproveGame.UIFramework.SUIElements;
using Terraria.GameInput;
using Terraria.ModLoader.IO;
using Terraria.ModLoader.UI;

namespace ImproveGame.UI.PlayerStats;

[AutoCreateGUI(LayerName.Vanilla.RadialHotbars, "Player Stats")]
public class PlayerStatsGUI : BaseBody
{
    public static PlayerStatsGUI Instance { get; private set; }
    public PlayerStatsGUI() => Instance = this;

    #region ViewBody
    public override bool Enabled { get => Visible; set => Visible = value; }

    public override bool CanSetFocusTarget(UIElement target)
    {
        if (target != this)
        {
            foreach (var item in Children)
            {
                if (item != Body && (item.IsMouseHovering || (item is View view && view.IsLeftMousePressed)))
                {
                    return true;
                }
            }

            foreach (var item in Body.Children)
            {
                if (item.IsMouseHovering || (item is View view && view.IsLeftMousePressed))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool _visible;

    public static bool Visible
    {
        get => UIConfigs.Instance.PlyInfo switch
        {
            UIConfigs.PAPDisplayMode.AlwaysDisplayed => _visible,
            UIConfigs.PAPDisplayMode.WhenOpeningBackpack => Main.playerInventory && _visible,
            UIConfigs.PAPDisplayMode.NotDisplayed => false,
            _ => false
        };
        set => _visible = value;
    }
    #endregion

    public View Body { get; set; }
    public SUIPanel Window { get; set; }
    public StatsGrid StatsGrid { get; set; }

    public SUIImage ControllerSwitch { get; set; }

    public override void OnInitialize()
    {
        #region 所有的属性面板
        Body = new()
        {
            Width = new StyleDimension(0, 1f),
            Height = new StyleDimension(0, 1f),
        };
        Body.OnUpdate += Body_OnUpdate;
        Body.JoinParent(this);

        foreach (var item in PlayerStatsSystem.Instance.StatsCategories)
        {
            if (item.Value.Favorite)
            {
                StatsCard card = CreateStatsCardForDisplay(Body, item.Value);
                card.JoinParent(Body);
            }
        }
        #endregion

        #region 控制窗口
        Window = new SUIPanel(UIStyle.PanelBorder, UIStyle.PanelBg, 10, 2, true)
        {
            FinallyDrawBorder = true
        };

        #region 标题栏
        // 标题
        var TitleView = new View
        {
            DragIgnore = true,
            BgColor = UIStyle.TitleBg2,
            Border = 0f,
            BorderColor = Color.Transparent,
            Rounded = new Vector4(10f, 10f, 0f, 0f),
        };
        TitleView.SetPadding(0);
        TitleView.Width.Precent = 1f;
        TitleView.Height.Pixels = 42f;
        TitleView.JoinParent(Window);

        var Title = new SUITitle(MyUtils.GetText("UI.PlayerStats.Control"), 0.42f)
        {
            VAlign = 0.5f
        };
        Title.SetPadding(15f, 0f, 10f, 0f);
        Title.SetInnerPixels(Title.TextSize);
        Title.JoinParent(TitleView);

        var Cross = new SUICross
        {
            HAlign = 1f,
            VAlign = 0.5f,
            Rounded = new Vector4(0f, 10f, 0f, 0f),
            CrossSize = 20f,
            CrossRounded = 4.5f * 0.85f
        };
        Cross.Border = 0f;
        Cross.BorderColor = Color.Transparent;
        Cross.Width.Pixels = 42f;
        Cross.Height.Set(0f, 1f);
        Cross.JoinParent(TitleView);

        Cross.OnLeftMouseDown += (_, _) =>
        {
            RemoveChild(Window);
            SoundEngine.PlaySound(SoundID.MenuClose);
        };
        #endregion

        // 一整个列表
        StatsGrid = new StatsGrid();

        foreach (var item in PlayerStatsSystem.Instance.StatsCategories)
        {
            var card = CreateCardForControl(Body, item.Value);
            card.EnableBlur = false;

            var list1 = StatsGrid.ListView.Children;
            var list2 = StatsGrid.ListView2.Children;

            var b1 = list1.LastOrDefault()?.Bottom() ?? 0f;
            var b2 = list2.LastOrDefault()?.Bottom() ?? 0f;

            if (b1 <= b2)
            {
                card.JoinParent(StatsGrid.ListView);
            }
            else
            {
                card.JoinParent(StatsGrid.ListView2);
            }
        }

        StatsGrid.Top.Pixels = TitleView.BottomPixels;
        StatsGrid.SetPadding(7f);
        StatsGrid.PaddingTop -= 3;
        StatsGrid.PaddingRight -= 1;
        StatsGrid.SetInnerPixels(160f * 2f + 4 + 21f, 250f);
        StatsGrid.JoinParent(Window);

        Window.SetPadding(0);
        Window.SetInnerPixels(StatsGrid.Width.Pixels, StatsGrid.BottomPixels);

        Window.HAlign = Window.VAlign = 0.5f;
        #endregion

        #region 控制器的开关按钮
        ControllerSwitch = new StatsToggle();
        ControllerSwitch.OnLeftMouseDown +=
            (_, _) =>
            {
                if (HasChild(Window))
                {
                    RemoveChild(Window);
                }
                else
                {
                    Window.JoinParent(this);
                }
            };
        ControllerSwitch.JoinParent(this);
        #endregion
    }

    /// <summary>
    /// 进入游戏时调用，加载收藏列表
    /// </summary>
    public void LoadAndSetupFavorites()
    {
        if (!Main.LocalPlayer.TryGetModPlayer(out UIPlayerSetting playerSetting))
            return;

        var proCats = PlayerStatsSystem.Instance.StatsCategories;

        foreach (var item in proCats)
        {
            if (playerSetting.ProCatsPos.TryGet(item.Key, out Vector2 pos))
            {
                item.Value.UIPosition = pos;
            }

            if (playerSetting.ProCatsFav.TryGet(item.Key, out bool fav))
            {
                item.Value.Favorite = fav;
            }

            if (playerSetting.ProFavs.TryGet(item.Key, out TagCompound tags))
            {
                foreach (var pro in item.Value.BaseProperties)
                {
                    if (tags.TryGet(pro.Name, out bool proFav))
                    {
                        pro.Favorite = proFav;
                    }
                }
            }
        }

        // 添加到展示列表
        foreach (var item in PlayerStatsSystem.Instance.StatsCategories)
        {
            if (item.Value.Favorite)
            {
                StatsCard card = CreateStatsCardForDisplay(Body, item.Value);
                card.JoinParent(Body);
            }
        }
    }

    /// <summary>
    /// 去除重复的和未收藏的
    /// </summary>
    private void Body_OnUpdate(UIElement body)
    {
        List<UIElement> list = [.. body.Children];
        HashSet<BaseStatsCategory> appeared = [];

        for (int i = 0; i < list.Count; i++)
        {
            var uie = list[i];

            if (uie is StatsCard card)
            {
                // 去重 proCat
                if (appeared.Contains(card.StatsCategory))
                {
                    uie.Remove();
                    break;
                }

                appeared.Add(card.StatsCategory);

                // 去掉未收藏
                if (!card.StatsCategory.Favorite)
                {
                    uie.Remove();
                }
            }
        }
    }

    public override void Update(GameTime gameTime)
    {
        bool b = false;

        foreach (var item in Children)
        {
            if (item != Body && (item.IsMouseHovering || (item is View view && view.IsLeftMousePressed)))
            {
                b = true;
                break;
            }
        }

        foreach (var item in Body.Children)
        {
            if (item.IsMouseHovering || (item is View view && view.IsLeftMousePressed))
            {
                b = true;
                break;
            }
        }

        if (b)
        {
            PlayerInput.LockVanillaMouseScroll("ImproveGame: PlayerStats GUI");
            Main.LocalPlayer.mouseInterface = true;
        }

        base.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);

        if (ControllerSwitch.IsMouseHovering)
        {
            UICommon.TooltipMouseText(MyUtils.GetText("UI.PlayerStats.Introduction"));
        }
    }

    /// <summary>
    /// 创建展示用的属性卡片
    /// </summary>
    public static StatsCard CreateStatsCardForDisplay(View view, BaseStatsCategory proCat)
    {
        StatsCard card = proCat.CreateCard(out _, out _);
        proCat.AppendProperties(card);

        if (proCat.UIPosition is Vector2 pos)
        {
            card.SetPosPixels(pos);
        }
        else
        {
            if (view.Children.Any())
            {
                var uie = view.Children.Last();
                card.SetPosPixels(uie.Right() + 4f, uie.Top.Pixels);
            }
            else
            {
                card.SetPosPixels(620f, 20f);
            }
        }

        card.Draggable = true;
        card.SetInnerPixels(160, (30 + 2) * card.Children.Count() - 2);

        return card;
    }

    /// <summary>
    /// 创建控制用的卡片
    /// </summary>
    public static StatsCard CreateCardForControl(View view, BaseStatsCategory proCat)
    {
        StatsCard card = proCat.CreateCard(out _, out _);
        proCat.AppendPropertiesForControl(view, card);

        card.OnUpdate += (_) =>
        {
            if (card.StatsCategory.Favorite)
            {
                card.BorderColor = UIStyle.ItemSlotBorderFav;
            }
            else
            {
                card.BorderColor = UIStyle.PanelBorder;
            }
        };

        card.TitleView.OnUpdate += (_) =>
        {
            card.TitleView.Border = card.TitleView.HoverTimer.Lerp(0, 2);
            card.TitleView.BorderColor = card.TitleView.HoverTimer.Lerp(UIStyle.PanelBorder, UIStyle.ItemSlotBorderFav);
        };

        card.TitleView.OnLeftMouseDown += (_, _) =>
        {
            proCat.Favorite = !proCat.Favorite;

            if (proCat.Favorite)
            {
                // 正常版的创建
                StatsCard innerCard = CreateStatsCardForDisplay(view, proCat);
                innerCard.JoinParent(view);
            }
        };

        card.RelativeMode = RelativeMode.Vertical;
        card.Spacing = new Vector2(4f);
        card.SetInnerPixels(160, (30 + 2) * card.Children.Count() - 2);

        return card;
    }
}