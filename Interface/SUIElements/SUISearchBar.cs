namespace ImproveGame.Interface.SUIElements;

public class SUISearchBar : View
{
    public event Action<string> OnSearchContentsChanged;
    /// <summary> 在搜索栏加一段 (支持拼音及拼音首字母搜索) 的文本 </summary>
    private readonly bool _allowPinyinSearch;
    public string SearchContent { get; private set; }
    public bool Visible = true;
    private UISearchBar _searchBar;
    private SUIPanel _searchBoxPanel;
    private bool _didClickSomething;
    private bool _didClickSearchBar;

    public SUISearchBar(bool allowPinyinSearch = false)
    {
        _allowPinyinSearch = allowPinyinSearch;
        Height = new StyleDimension(28f, 0f);
        this.SetPadding(0f);
        AddSearchBar();
        _searchBar.SetContents(null, forced: true);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (Visible) base.Draw(spriteBatch);
    }

    private void AddSearchBar()
    {
        UIImageButton searchBtn = new(Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Button_Search"))
        {
            VAlign = 0.5f,
            HAlign = 0f
        };
        searchBtn.OnLeftMouseDown += Click_SearchArea;
        searchBtn.SetHoverImage(Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Button_Search_Border"));
        searchBtn.SetVisibility(1f, 1f);
        Append(searchBtn);
        
        _searchBoxPanel = new SUIPanel(new Color(35, 40, 83), new Color(35, 40, 83))
        {
            Left = new StyleDimension(4f, 0f),
            Width = new StyleDimension(-searchBtn.Width.Pixels - 3f, 1f),
            Height = new StyleDimension(0f, 1f),
            VAlign = 0.5f,
            HAlign = 1f
        };
        _searchBoxPanel.OnLeftMouseDown += Click_SearchArea;
        _searchBoxPanel.SetPadding(0f);
        Append(_searchBoxPanel);

        var searchBarText = Language.GetText("UI.PlayerNameSlot");
        if (Language.ActiveCulture.Name is "zh-Hans" && _allowPinyinSearch)
        {
            // 由于在这不能直接访问 new LocalizedText，我们借助 Language.GetText 来获取
            searchBarText = Language.GetText("搜索名称(支持拼音或首字母): ");
        }
        _searchBar = new UISearchBar(searchBarText, 0.8f)
        {
            Width = new StyleDimension(0f, 1f),
            Height = new StyleDimension(0f, 1f),
            HAlign = 0f,
            VAlign = 0.5f,
            IgnoresMouseInteraction = true
        };
        _searchBar.OnContentsChanged += (s =>
        {
            SearchContent = s;
            OnSearchContentsChanged?.Invoke(s);
        });
        _searchBar.OnStartTakingInput += OnStartTakingInput;
        _searchBar.OnEndTakingInput += OnEndTakingInput;
        _searchBar.OnCanceledTakingInput += OnCanceledInput;
        _searchBoxPanel.Append(_searchBar);
        
        UIImageButton cancelBtn = new(Main.Assets.Request<Texture2D>("Images/UI/SearchCancel"))
        {
            HAlign = 1f,
            VAlign = 0.5f,
            Left = new StyleDimension(-2f, 0f)
        };
        cancelBtn.OnMouseOver += SearchCancelButton_OnMouseOver;
        cancelBtn.OnLeftClick += SearchCancelButton_OnClick;
        _searchBoxPanel.Append(cancelBtn);
    }

    private void SearchCancelButton_OnClick(UIMouseEvent evt, UIElement listeningElement)
    {
        if (_searchBar.HasContents)
        {
            _searchBar.SetContents(null, forced: true);
            SoundEngine.PlaySound(SoundID.MenuClose);
        }
        else
        {
            SoundEngine.PlaySound(SoundID.MenuTick);
        }
    }

    private void SearchCancelButton_OnMouseOver(UIMouseEvent evt, UIElement listeningElement)
    {
        SoundEngine.PlaySound(SoundID.MenuTick);
    }

    private void OnCanceledInput()
    {
        Main.LocalPlayer.ToggleInv();
    }

    private void Click_SearchArea(UIMouseEvent evt, UIElement listeningElement)
    {
        if (evt.Target.Parent != _searchBoxPanel)
        {
            _searchBar.ToggleTakingText();
            _didClickSearchBar = true;
        }
    }

    private void OnStartTakingInput()
    {
        _searchBoxPanel.BorderColor = Main.OurFavoriteColor;
    }

    private void OnEndTakingInput()
    {
        _searchBoxPanel.BorderColor = new Color(35, 40, 83);
    }

    public void AttemptStoppingUsingSearchbar()
    {
        if (!_didClickSearchBar && _searchBar.IsWritingText)
            _searchBar.ToggleTakingText();
    }

    public void ClearContents()
    {
        _searchBar.SetContents(null, forced: true);
    }

    public override void Update(GameTime gameTime)
    {
        if (!Visible) return;

        base.Update(gameTime);

        if (_didClickSomething && !_didClickSearchBar && _searchBar.IsWritingText)
            _searchBar.ToggleTakingText();

        _didClickSomething = false;
        _didClickSearchBar = false;
    }
}