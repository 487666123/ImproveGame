using ImproveGame.Common.Configs;
using ImproveGame.UI.ExtremeStorage;
using ImproveGame.UIFramework;
using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.Common;
using ImproveGame.UIFramework.SUIElements;
using Terraria.ModLoader.UI;

namespace ImproveGame.UI.RecipeSearch;

[AutoCreateGUI(LayerName.Vanilla.Inventory, "Recipe Search UI")]
public class RecipeSearchUI : BaseBody
{
    public override bool Enabled
    {
        get => Player.Settings.CraftingGridControl == Player.Settings.CraftingGridMode.Classic && Main.PipsUseGrid && Main.screenWidth >= 1180 && UIConfigs.Instance.RecipeSearch &&
               !RecipeSearchSystem.UsingGuide;
        set { }
    }

    private bool Hovering => _mainPanel.IsMouseHovering || _rightSidePanel.IsMouseHovering;

    public override bool CanSetFocusTarget(UIElement target) => Hovering;

    // 透明的爹
    private View _mainPanel;

    // 右边的透明的爹
    private View _rightSidePanel;

    // 搜索栏
    public string SearchContent => _searchBar.SearchContent;
    private SUISearchBar _searchBar;

    public override void OnInitialize()
    {
        _mainPanel = new View()
            .SetPosPixels(310, 310)
            .SetSizePixels(280, 34);
        _mainPanel.SetPadding(0f);
        _mainPanel.JoinParent(this);

        _searchBar = new SUISearchBar(true, false)
        {
            Height = new StyleDimension(28f, 0f),
            Width = new StyleDimension(0f, 1f),
            HAlign = 0.5f,
            DragIgnore = true,
            RelativeMode = RelativeMode.Vertical
        };
        // _searchBar.OnDraw += SearchBarOnDraw;
        // _searchBar.OnSearchContentsChanged += _ => Recipe.FindRecipes();
        _searchBar.JoinParent(_mainPanel);

        _rightSidePanel = new View()
            .SetSizePixels(260, 88);
        _rightSidePanel.SetPos(-572, 262, 1f, 0f);
        _rightSidePanel.SetPadding(0f);
        _rightSidePanel.JoinParent(this);
        SetupSettingButtons(_rightSidePanel);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (Hovering)
        {
            Main.LocalPlayer.mouseInterface = true;
        }
    }

    public override void DrawChildren(SpriteBatch spriteBatch)
    {
        base.DrawChildren(spriteBatch);

        if (_searchBar.IsWritingText)
        {
            Vector2 position = _searchBar.GetDimensions().Position();
            position.Y -= 30f;
            Main.instance.SetIMEPanelAnchor(position, 0f);
        }

        if (_searchBar.IsSearchButtonMouseHovering)
            UICommon.TooltipMouseText(MyUtils.GetText("UI.RecipeSearchUI.Tips"));
    }

    private void SetupSettingButtons(View parent)
    {
        // 开关
        UIPlayerSetting setting = Main.LocalPlayer.GetModPlayer<UIPlayerSetting>();
        Vector2 switchSpacing = new(0, 6);
        var fuzzySwitch = new LongSwitch(
            () => setting.FuzzySearch,
            state =>
            {
                setting.FuzzySearch = state;
                // Recipe.FindRecipes();
            },
            "UI.ItemSearcher.FuzzySearch")
        {
            ResetAnotherPosition = true,
            RelativeMode = RelativeMode.Vertical,
            Spacing = switchSpacing,
            Height = { Pixels = 34f }
        };
        fuzzySwitch.JoinParent(parent);

        var tooltipSwitch = new LongSwitch(
            () => setting.SearchTooltip,
            state =>
            {
                setting.SearchTooltip = state;
                // Recipe.FindRecipes();
            },
            "UI.ItemSearcher.SearchTooltip")
        {
            RelativeMode = RelativeMode.Vertical,
            Spacing = switchSpacing,
            Height = { Pixels = 34f }
        };
        tooltipSwitch.JoinParent(parent);
    }
}