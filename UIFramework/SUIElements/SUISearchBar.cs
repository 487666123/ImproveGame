using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.Common;

namespace ImproveGame.UIFramework.SUIElements;

public class SUISearchBar : View
{
    private const float DefaultHeight = 28f;

    public event Action<string> OnSearchContentsChanged;
    public event Action OnDraw;

    private readonly bool _pinyinSearchTip;
    public readonly SUIEditableText SearchBarInner;

    public bool IsSearchButtonMouseHovering => SearchBarInner.IsMouseHovering;
    public bool IsWritingText => SearchBarInner.IsWritingText;
    public string SearchContent { get; private set; }
    public bool Visible = true;

    public SUISearchBar(bool pinyinSearchTip = false, bool showIme = false)
    {
        _pinyinSearchTip = pinyinSearchTip;
        Height = new StyleDimension(DefaultHeight, 0f);
        this.SetPadding(0f);

        SearchBarInner = InitSearchBar(showIme);
        InitCancelButton();
    }

    private SUIEditableText InitSearchBar(bool showIme)
    {
        var searchBar = new SUIEditableText
        {
            BgColor = UIStyle.SearchBarBg,
            BorderColor = UIStyle.SearchBarBorder,
            Rounded = new Vector4(10f),
            MaxLength = 30,
            VAlign = 0.5f,
            OverflowHidden = true,
            ShowImePanel = showIme
        };

        searchBar.OnUpdate += element =>
        {
            var view = (SUIEditableText)element;
            view.BorderColor = view.IsMouseHovering ? UIStyle.SearchBarBorderSelected : UIStyle.SearchBarBorder;
            view.BgColor = UIStyle.SearchBarBg;
        };

        searchBar.ContentsChanged += (ref string s) =>
        {
            SearchContent = s;
            OnSearchContentsChanged?.Invoke(s);
        };

        searchBar.InnerText.TextScale = 0.9f;
        searchBar.InnerText.TextOffset.X = 6f;
        searchBar.InnerText.Placeholder = MyUtils.GetText("Search");

        if (string.Equals(Language.ActiveCulture.Name, "zh-Hans") && _pinyinSearchTip)
            searchBar.InnerText.Placeholder = "搜索名称(支持拼音或首字母): ";

        searchBar.SetSize(0f, 0f, 1f, 1f);
        searchBar.JoinParent(this);

        return searchBar;
    }

    private void InitCancelButton()
    {
        var cancelBtn = new UIImageButton(Main.Assets.Request<Texture2D>("Images/UI/SearchCancel"))
        {
            HAlign = 1f,
            VAlign = 0.5f,
            Left = new StyleDimension(-2f, 0f)
        };

        cancelBtn.OnMouseOver += SearchCancelButton_OnMouseOver;
        cancelBtn.OnLeftClick += SearchCancelButton_OnClick;
        Append(cancelBtn);
    }

    private void SearchCancelButton_OnClick(UIMouseEvent evt, UIElement listeningElement)
    {
        if (!string.IsNullOrEmpty(SearchBarInner.Text))
        {
            SearchBarInner.Text = "";
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

    public override void Draw(SpriteBatch spriteBatch)
    {
        if (Visible)
        {
            base.Draw(spriteBatch);
            OnDraw?.Invoke();
        }
    }

    public override void Update(GameTime gameTime)
    {
        if (!Visible) return;

        base.Update(gameTime);
    }

    public void SetText(string text)
    {
        SearchBarInner.Text = text;
    }
}