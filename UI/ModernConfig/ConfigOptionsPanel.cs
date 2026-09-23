using ImproveGame.UI.ModernConfig.FakeCategories;
using ImproveGame.UI.ModernConfig.OfficialPresets;
using ImproveGame.UI.ModernConfig.OptionElements;
using ImproveGame.UI.ModernConfig.OptionElements.PresetElements;
using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.Common;
using ImproveGame.UIFramework.SUIElements;
using System.Text.RegularExpressions;
using Terraria.GameInput;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;

namespace ImproveGame.UI.ModernConfig;

public sealed partial class ConfigOptionsPanel : SUIPanel
{
    internal static ConfigOptionsPanel Instance;
    public bool ShouldHideSearchBar;
    public bool DelayRefreshCurrentPage;
    private static Category _currentCategory;
    public static Category CategoryToSelectOnOpen;

    private HashSet<string> _addedOptions = [];
    private List<ModernConfigOption> _allOptions = [];
    public List<ModernConfigOption> AllOptions => _allOptions;
    private SUIEditableText _searchBar { get; set; }
    private SUIScrollView2 _options { get; set; }
    public SUIDropdownListContainer DropdownList { get; set; }
    public static object GlobalItem;
    public static List<string> GlobalPath;
    public static Category CurrentCategory
    {
        get => _currentCategory;
        set
        {
            if (_currentCategory != value)
            {
                _currentCategory = value;
                Instance._options.ScrollBar.TargetScrollPosition = Vector2.Zero;
                Instance._options.ScrollBar.CurrentScrollPosition = Vector2.Zero;
                Instance.RefreshCurrentPage();
            }
        }
    }

    private static SUIText CreateRightArrow()
    {
        SUIText arrow = new()
        {
            TextOrKey = ">",
            RelativeMode = RelativeMode.Horizontal,
            TextScale = 0.9f,
            Spacing = new Vector2(6f, 0f),
            // 碰撞箱测试用代码，显示轮廓
            // BorderColor = Color.Red,
            // BgColor = Color.Black * 0.4f,
            // Border = 2f,
            // Rounded = new Vector4(2f),
        };
        arrow.RecalculateText();
        arrow.SetInnerPixels(arrow.TextSize * arrow.TextScale);
        return arrow;
    }
    //public static List<Category> PreviousPageList = [];
    private static SUIText GeneratePathTextElement(Category current, object item, List<string> path)
    {
        var list = ModernConfigUI.Instance.PathPanel;
        SUIText prevPage = new()
        {
            TextOrKey = current.Label.Trim(),
            Spacing = new Vector2(6f, 0f),
            RelativeMode = RelativeMode.Horizontal,
            TextScale = 0.9f,
            // 碰撞箱测试用代码，显示轮廓
            // BorderColor = Color.Red,
            // BgColor = Color.Black * 0.4f,
            // Border = 2f,
            // Rounded = new Vector4(2f),
        };
        prevPage.OnLeftClick += (evt, elem) =>
        {
            if (CurrentCategory == current) return;
            int index = list.ListView.Elements.IndexOf(elem);

            if (index == 0)
            {
                ModernConfigUI.Instance.PathPanelTimer.Close();
            }
            else
            {
                list.ListView.Elements.RemoveRange(index + 1, list.ListView.Elements.Count - index - 1);
                list.Recalculate();
            }
            GlobalItem = item;
            GlobalPath = path;
            CurrentCategory = current;
            GlobalItem = null;
            GlobalPath = null;
            SoundEngine.PlaySound(SoundID.MenuClose);
        };
        prevPage.OnUpdate += (elem) =>
        {
            var s = elem as SUIText;
            var dimension = s.GetDimensions();
            Color targetColor;
            if (CurrentCategory == current)
                targetColor = Color.White;
            else if (s.IsMouseHovering)
                targetColor = Color.Yellow;
            else
                targetColor = Color.Lerp(Color.LightGray, Color.Gray, 0.5f + 0.5f * MathF.Cos(Main.GlobalTimeWrappedHourly * 2.5f + dimension.Position().X * .02f));
            s.TextColor = Color.Lerp(s.TextColor, targetColor, 0.15f);
            s.RecalculateText();
        };
        prevPage.RecalculateText();
        prevPage.SetInnerPixels(prevPage.TextSize * prevPage.TextScale);
        return prevPage;
    }
    public static void SwitchToSubPage(Category destination, object item, List<string> path)
    {
        var timer = ModernConfigUI.Instance.PathPanelTimer;
        var list = ModernConfigUI.Instance.PathPanel;
        SoundEngine.PlaySound(SoundID.MenuOpen);

        // PreviousPageList.Add(CurrentCategory);
        if (timer.AnyClose)
        {
            GeneratePathTextElement(CurrentCategory, null, null).JoinParent(list.ListView);
            timer.Open();
        }

        GlobalItem = item;
        GlobalPath = path;
        CurrentCategory = destination;
        GlobalItem = null;
        GlobalPath = null;
        CreateRightArrow().JoinParent(list.ListView);
        GeneratePathTextElement(CurrentCategory, item, path).JoinParent(list.ListView);
        list.Recalculate();
    }
    public ConfigOptionsPanel(Color color) : base(color, color)
    {
        EnableBlur = false;
        const int searchBarHeight = 30;
        const int gap = 6;
        Instance = this;

        _searchBar = new SUIEditableText
        {
            RelativeMode = RelativeMode.Vertical,
            BgColor = UIStyle.SearchBarBg,
            BorderColor = UIStyle.SearchBarBorder,
            Rounded = new Vector4(10f),
            MaxLength = 30,
            VAlign = 0.5f,
            OverflowHidden = true
        };
        _searchBar.OnUpdate += element =>
        {
            var view = (SUIEditableText)element;
            view.BorderColor = view.IsMouseHovering ? UIStyle.SearchBarBorderSelected : UIStyle.SearchBarBorder;
            view.BgColor = UIStyle.SearchBarBg;

            switch (ShouldHideSearchBar)
            {
                case true when _searchBar.Height.Pixels != 0:
                    _searchBar.SetSize(0f, 0f, 1f);
                    Recalculate();
                    break;
                case false when _searchBar.Height.Pixels == 0:
                    _searchBar.SetSize(0f, searchBarHeight, 1f);
                    Recalculate();
                    break;
            }
        };
        _searchBar.ContentsChanged += SearchBarTextChanged;
        _searchBar.InnerText.TextOffset.X = 6f;
        _searchBar.InnerText.Placeholder = MyUtils.GetText("Search");
        _searchBar.SetSize(0f, searchBarHeight, 1f);
        _searchBar.JoinParent(this);

        _options = new SUIScrollView2(Orientation.Vertical)
        {
            RelativeMode = RelativeMode.Vertical,
            Spacing = new Vector2(gap)
        };
        _options.OnUpdate += element =>
        {
            // 如果隐藏了搜索栏，这里的size要补上对应的高度，不然最底部会有一片空域
            switch (ShouldHideSearchBar)
            {
                case true when element.Height.Pixels != -gap:
                    element.SetSize(0f, -gap, 1f, 1f);
                    Recalculate();
                    break;
                case false when element.Height.Pixels != -searchBarHeight - gap:
                    element.SetSize(0f, -searchBarHeight - gap, 1f, 1f);
                    Recalculate();
                    break;
            }
        };
        _options.SetPadding(0f, 0f);
        _options.SetSize(0f, -searchBarHeight - gap, 1f, 1f);
        _options.JoinParent(this);

        DropdownList = new SUIDropdownListContainer();
        DropdownList.JoinParent(this);
    }

    public void RefreshCurrentPage()
    {
        _options.ListView.RemoveAllChildren();
        _allOptions.Clear();
        _addedOptions.Clear();
        ShouldHideSearchBar = false;
        _currentCategory.AddOptions(this);
        string text = _searchBar.Text;
        SearchBarTextChanged(ref text);
        Recalculate();
    }
    public void SetSearchBarText(string text) => _searchBar.Text = text;
    private void SearchBarTextChanged(ref string text)
    {
        DropdownList.Enabled = false;
        if (ShouldHideSearchBar)
            return;

        if (string.IsNullOrEmpty(text))
        {
            // 刷一下就走
            _options.ListView.RemoveAllChildren();
            _allOptions.ForEach(o =>
            {
                o.Highlighted = false;
                o.ResetDebugText();
                o.JoinParent(_options.ListView);
            });
            Recalculate();
            return;
        }

        // 找出所有匹配的选项，并着色（设置Highlighted）
        var sortedOptions = new List<ModernConfigOption>();

        // 转换成标准字符串搜索输入
        var optionNames = _allOptions
            .Select(o => ItemTagRegex().Replace(o.Label, "")).ToList();
        // 调用DeepSeek写的搜索方法
        var results = MyUtils.TextSearch(text, optionNames);
        // 对结果进行处理
        foreach (MyUtils.SearchResult result in results)
        {
            var option = _allOptions[result.OriginalIndex];
            // 将allOptions里的对应元素按照次序生成排序后的列表
            sortedOptions.Add(option);
            // 匹配高亮
            option.Highlighted = result.HasAnyMatch;
            // 调试信息
            option.DebugText =
                $"oi: {result.OriginalIndex}, ld: {result.LevenshteinDistance}, ldp: {result.LevenshteinDistancePinyin}, ms: {result.MatchScore}";
        }

        // 最后重新加入到_options
        _options.ListView.RemoveAllChildren();
        sortedOptions.ForEach(o => o.JoinParent(_options.ListView));
        Recalculate();
    }

    public void AddToggle(ModConfig config, object nameOrMemberInfo) => AddToAllOptions<OptionToggle>(config, nameOrMemberInfo);

    public void AddValueSlider(ModConfig config, object nameOrMemberInfo) => AddToAllOptions<OptionSlider>(config, nameOrMemberInfo);

    public void AddEditableText(ModConfig config, object nameOrMemberInfo) => AddToAllOptions<OptionEditableText>(config, nameOrMemberInfo);
    //public void AddValueText(ModConfig config, string name) => AddToAllOptions<OptionNumber>(config, name);

    public void AddEnum(ModConfig config, object nameOrMemberInfo) => AddToAllOptions<OptionDropdownList>(config, nameOrMemberInfo);

    public void AddObject(ModConfig config, object nameOrMemberInfo) => AddToAllOptions<OptionObject>(config, nameOrMemberInfo);
    public void AddCustomUIConfig(ModConfig config, object nameOrMemberInfo) => AddToAllOptions<OptionCustomUIConfig>(config, nameOrMemberInfo);
    public void AddArray(ModConfig config, object nameOrMemberInfo) => AddToAllOptions<OptionArray>(config, nameOrMemberInfo);
    public void AddList(ModConfig config, object nameOrMemberInfo) => AddToAllOptions<OptionList>(config, nameOrMemberInfo);
    public void AddHashSet(ModConfig config, object nameOrMemberInfo) => AddToAllOptions<OptionHashSet>(config, nameOrMemberInfo);
    public void AddDictionary(ModConfig config, object nameOrMemberInfo) => AddToAllOptions<OptionDictionary>(config, nameOrMemberInfo);
    public void AddVector2(ModConfig config, object nameOrMemberInfo) => AddToAllOptions<OptionVector2>(config, nameOrMemberInfo);
    public void AddColor(ModConfig config, object nameOrMemberInfo) => AddToAllOptions<OptionColor>(config, nameOrMemberInfo);
    public void AddDefinition(ModConfig config, object nameOrMemberInfo) => AddToAllOptions<OptionDefinition>(config, nameOrMemberInfo);
    public void AddNotSupportText(ModConfig config, PropertyFieldWrapper variableInfo) => AddToAllOptions<OptionNotSupportText>(config, variableInfo);

    private void AddToAllOptions<T>(ModConfig config, object nameOrMemberInfo) where T : ModernConfigOption
    {
        if (nameOrMemberInfo is not PropertyFieldWrapper MemberInfo)
            if (nameOrMemberInfo is string memberName)
                MemberInfo = ModernConfigOption.GetWrapper(config.GetType(), memberName);
            else return;
        var name = MemberInfo.Name;
        // 如果已经添加过这个选项，直接返回
        if (!_addedOptions.Add(name))
            return;
        // 如果当前分类不允许添加这个选项，直接返回
        if (!CurrentCategory.CanOptionBeAdded(config, name))
            return;
        // 创建实例并加入到_allOptions列表
        var instance = GenerateOptionElement<T>(config, MemberInfo);
        _allOptions.Add(instance);
    }

    public void RemoveFromAllOptions(ModConfig config, object nameOrMemberInfo)
    {
        if (nameOrMemberInfo is not PropertyFieldWrapper MemberInfo)
            if (nameOrMemberInfo is string memberName)
                MemberInfo = ModernConfigOption.GetWrapper(config.GetType(), memberName);
            else return;
        var name = MemberInfo.Name;

        _addedOptions.Remove(name);
        _allOptions.RemoveAll(o => o.OptionName == name);
    }

    private static ModernConfigOption GenerateOptionElement<T>(ModConfig config, PropertyFieldWrapper memberInfo) where T : ModernConfigOption
    {
        var instance = (ModernConfigOption)Activator.CreateInstance(typeof(T));//, config, nameOrMemberInfo
        instance.Bind(config, memberInfo, null);
        return instance;
    }

    public void AddToOptionsDirect<T>(ModConfig config, object nameOrMemberInfo) where T : ModernConfigOption
    {
        if (nameOrMemberInfo is not PropertyFieldWrapper MemberInfo)
            if (nameOrMemberInfo is string memberName)
                MemberInfo = ModernConfigOption.GetWrapper(config.GetType(), memberName);
            else return;
        // 创建实例并加入到_allOptions列表
        var instance = GenerateOptionElement<T>(config, MemberInfo);
        instance.JoinParent(_options.ListView);
    }

    public void AddToOptionsDirect(View view)
    {
        view.JoinParent(_options.ListView);
    }

    public void AddOfficialPreset<T>() where T : OfficialPreset
    {
        var preset = Activator.CreateInstance<T>();
        AddToOptionsDirect(new OfficialPresetElement(preset.Label, preset.Tooltip, preset.Link, preset.OnApply));
    }

    public override void Update(GameTime gameTime)
    {
        if (DelayRefreshCurrentPage)
        {
            DelayRefreshCurrentPage = false;
            RefreshCurrentPage();
        }
        base.Update(gameTime);

        if (IsMouseHovering)
            PlayerInput.LockVanillaMouseScroll("ImproveGame: Modern Config UI");
        if (CategoryToSelectOnOpen is not null)
        {
            CurrentCategory = CategoryToSelectOnOpen;
            CategoryToSelectOnOpen = null;
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);

        if (!_searchBar.IsWritingText)
            return;

        Vector2 position = _searchBar.GetDimensions().ToRectangle().BottomLeft();
        position.Y += 32f;
        Main.instance.SetIMEPanelAnchor(position, 0f);
    }

    // 用于捕获分辨率变化，以便 Recalculate 并且重新计算位置
    private sealed class CaptureResolutionChange : ILoadable
    {
        public void Load(Mod mod)
        {
            Main.OnResolutionChanged += OnResolutionChangedHandler;
        }

        public void Unload()
        {
            Main.OnResolutionChanged -= OnResolutionChangedHandler;
        }

        private void OnResolutionChangedHandler(Vector2 vector)
        {
            if (CurrentCategory is AboutPage)
                Instance.DelayRefreshCurrentPage = true;
        }
    }

    [GeneratedRegex(@"\[centeritem:[^\]]*\]", RegexOptions.IgnoreCase, "zh-CN")]
    private static partial Regex ItemTagRegex();
}