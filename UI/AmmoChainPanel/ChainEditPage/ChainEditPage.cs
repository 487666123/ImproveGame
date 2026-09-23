using ImproveGame.Content.Functions.ChainedAmmo;
using ImproveGame.Content.Items;
using ImproveGame.Content.Items.IconDummies;
using ImproveGame.Core;
using ImproveGame.UIFramework;
using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.Common;
using ImproveGame.UIFramework.SUIElements;
using Terraria.GameInput;

namespace ImproveGame.UI.AmmoChainPanel.ChainEditPage;

public class ChainEditPage : View
{
    public bool ShouldResetCurrentChain;
    public int SelectedAmmoType;

    public string OriginalChainName => _iconElement is null ? "" : _iconElement.OriginalName;
    public string ChainName;
    public bool IsCreatingAChain;
    public AmmoChain EditingChain;

    // 上半部分面板
    private View _upperPanel;

    // 下半部分面板
    private View _lowerPanel;

    private SUIEditableText _editableText;

    private IconElement _iconElement { get; set; }

    private EditChainScrollView _currentChain { get; set; }

    private SUIScrollView2 _availableAmmos { get; set; }

    public ChainEditPage()
    {
        int midPixel = 90;
        SetSizePercent(1f, 1f);

        _upperPanel = new View
        {
            DragIgnore = true,
            RelativeMode = RelativeMode.Vertical
        };
        _upperPanel.SetPadding(24, 14f, 24, 6f);
        _upperPanel.SetSize(0f, midPixel, 1f, 0f);
        _upperPanel.JoinParent(this);
        SetupUpperPanel();

        this.MakeHorizontalSeparator();

        _lowerPanel = new View
        {
            DragIgnore = true,
            RelativeMode = RelativeMode.Vertical
        };
        _lowerPanel.SetPadding(14f, 10, 14f, 20f);
        _lowerPanel.SetSize(0f, -midPixel, 1f, 1f);
        _lowerPanel.JoinParent(this);
        SetupLowerPanel();
    }

    private void SetupUpperPanel()
    {
        _iconElement = new IconElement(this, ChainName)
        {
            RelativeMode = RelativeMode.Vertical
        };
        _iconElement.JoinParent(_upperPanel);

        _editableText = new SUIEditableText
        {
            RelativeMode = RelativeMode.Horizontal,
            BgColor = Color.Black * 0.4f,
            Spacing = new Vector2(16f),
            Rounded = new Vector4(4f),
            InnerText =
            {
                TextAlign = new Vector2(0.5f),
                MaxLines = 2
            },
            MaxLength = 26
        };
        _editableText.ContentsChanged += OnTextContentChanged;
        _editableText.SetPadding(10, 4, 10, 4); // Padding影响里面的文字绘制
        _editableText.SetSizePixels(336, 70);
        _editableText.JoinParent(_upperPanel);

        var colorGrids = new SUIScrollView2(Orientation.Vertical)
        {
            RelativeMode = RelativeMode.Horizontal,
            Spacing = new Vector2(12f)
        };
        colorGrids.SetPadding(0f, 4f);
        colorGrids.SetSize(140f, 70f, 0f, 0f);
        colorGrids.JoinParent(_upperPanel);
        SetupColorGrids(colorGrids);
    }

    private void OnTextContentChanged(ref string content)
    {
        ChainName = content;
    }

    private void SetupLowerPanel()
    {
        _currentChain = new EditChainScrollView(Orientation.Horizontal)
        {
            RelativeMode = RelativeMode.Vertical,
            Spacing = new Vector2(4f)
        };
        _currentChain.SetPadding(2f, 0f);
        _currentChain.SetSize(0f, 76f, 1f, 0f);
        _currentChain.JoinParent(_lowerPanel);
        _currentChain.Append(new TipClickToAdd(this));
        EditingChain = new AmmoChain();
        SetupCurrentChain(EditingChain);

        _availableAmmos = new SUIScrollView2(Orientation.Horizontal)
        {
            RelativeMode = RelativeMode.Vertical,
            Spacing = new Vector2(4f)
        };
        _availableAmmos.SetPadding(2f, 0f);
        _availableAmmos.SetSize(0f, 150f, 1f, 0f);
        _availableAmmos.JoinParent(_lowerPanel);
        SetupAvailableAmmos();

        var saveButton = new SUIButton(MyUtils.GetText("UI.AmmoChain.Confirm"))
        {
            RelativeMode = RelativeMode.Vertical,
            Spacing = new Vector2(10f, 10f),
            TextAlign = new Vector2(0.5f),
            TextColor = new Color(153, 229, 80)
        };
        saveButton.OnLeftMouseDown += ClickOnSaveButton;
        saveButton.SetSize(280f, 40f, 0f, 0f);
        saveButton.JoinParent(_lowerPanel);

        var cancelButton = new SUIButton(MyUtils.GetText("UI.AmmoChain.Cancel"))
        {
            RelativeMode = RelativeMode.Horizontal,
            Spacing = new Vector2(10f, 0f),
            TextAlign = new Vector2(0.5f),
            TextColor = new Color(217, 87, 99)
        };
        cancelButton.OnLeftMouseDown += ClickOnCancelButton;
        cancelButton.SetSize(280f, 40f, 0f, 0f);
        cancelButton.JoinParent(_lowerPanel);
    }

    private void ClickOnSaveButton(UIMouseEvent evt, UIElement listeningelement)
    {
        if (EditingChain.Chain.Count is 0)
        {
            MyUtils.AddNotification(MyUtils.GetText("UI.AmmoChain.Empty"), itemIconType: ModContent.ItemType<AmmoChainItem>());
            return;
        }

        if (ChainName.IsPathIllegal())
        {
            MyUtils.AddNotification(MyUtils.GetText("PathIllegal"));
            return;
        }

        if (!IsCreatingAChain)
        {
            // 改名了，删了原来的再创建新的
            if (OriginalChainName != ChainName)
            {
                ChainSaver.TryDeleteFile(OriginalChainName, true);
                ChainSaver.SaveAsFile(EditingChain, ChainName);
            }
            else
            {
                ChainSaver.ModifyExistingFile(EditingChain, ChainName);
            }
        }
        else
        {
            ChainSaver.SaveAsFile(EditingChain, ChainName);
        }

        AmmoChainUI.Instance.GoToWeaponPage();
        DisableTextInput();
    }

    private void ClickOnCancelButton(UIMouseEvent evt, UIElement listeningelement)
    {
        DisableTextInput();
        AmmoChainUI.Instance.GoToWeaponPage();
    }

    public void DisableTextInput()
    {
        if (_editableText.IsWritingText)
            _editableText.ToggleTakingText();
    }

    /// <summary>
    /// 重新加载 可用的 预设列表视图
    /// </summary>
    public void SetupAvailableAmmos()
    {
        _availableAmmos.ListView.RemoveAllChildren();

        // 特殊弹药，代表选择当前武器的常规弹药（就是不限制，根据原版逻辑选择）
        new SelectableAmmoSlot(new Item(ModContent.ItemType<UniversalAmmoIcon>()), this)
        {
            BgColor = UIStyle.ItemSlotBgFav * 0.8f
        }.JoinParent(_availableAmmos.ListView);

        var items = ContentSamples.ItemsByType.Values.Where(i => i.CanBeUsedAsAmmo()).ToList();
        items.Sort((a, b) => a.ammo.CompareTo(b.ammo));
        foreach (var itemSlot in items.Select(item => new SelectableAmmoSlot(item, this)))
        {
            itemSlot.BgColor = UIStyle.TrashSlotBg;
            itemSlot.JoinParent(_availableAmmos.ListView);
        }

        Recalculate();
    }

    public void StartEditing(AmmoChain chain, bool isCreatingAChain, string chainName)
    {
        IsCreatingAChain = isCreatingAChain;
        EditingChain = chain;
        ChainName = chainName;
        _editableText.Text = ChainName;
        DisableTextInput();
        _iconElement.OriginalName = ChainName;
        SetupCurrentChain(EditingChain);
    }

    public void SetupCurrentChain(AmmoChain chain)
    {
        _currentChain.ListView.RemoveAllChildren();

        if (chain.Chain.Count == 0)
            return;

        new AmmoGapElement(0, this).JoinParent(_currentChain.ListView);

        for (var i = 0; i < chain.Chain.Count; i++)
        {
            var element = chain.Chain[i];
            var itemTypeData = element.ItemData;
            int times = element.Times;

            var itemSlot = new ItemSlotForEdit(i, this)
            {
                RealItem = itemTypeData.Item.Clone()
            };
            itemSlot.RealItem.stack = times;
            itemSlot.JoinParent(_currentChain.ListView);

            new AmmoGapElement(i + 1, this).JoinParent(_currentChain.ListView);
        }

        Recalculate();
    }

    private void SetupColorGrids(SUIScrollView2 scrollView)
    {
        new WhiteColorGrid(this) { RelativeMode = RelativeMode.Horizontal }.JoinParent(scrollView.ListView);
        new OrangeColorGrid(this) { RelativeMode = RelativeMode.Horizontal }.JoinParent(scrollView.ListView);
        new YellowColorGrid(this) { RelativeMode = RelativeMode.Horizontal }.JoinParent(scrollView.ListView);
        new BlueColorGrid(this) { RelativeMode = RelativeMode.Horizontal }.JoinParent(scrollView.ListView);
        new GreenColorGrid(this) { RelativeMode = RelativeMode.Horizontal }.JoinParent(scrollView.ListView);
        new PinkColorGrid(this) { RelativeMode = RelativeMode.Horizontal }.JoinParent(scrollView.ListView);
        new RedColorGrid(this) { RelativeMode = RelativeMode.Horizontal }.JoinParent(scrollView.ListView);
        new BrownColorGrid(this) { RelativeMode = RelativeMode.Horizontal }.JoinParent(scrollView.ListView);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        // 鼠标滚轮锁定
        if (_currentChain.IsMouseHovering || _availableAmmos.IsMouseHovering)
        {
            PlayerInput.LockVanillaMouseScroll("ImproveGame: Ammo Chain UI");
            Main.LocalPlayer.mouseInterface = true;
        }

        if (ShouldResetCurrentChain)
        {
            ShouldResetCurrentChain = false;
            SetupCurrentChain(EditingChain);
        }
    }

    public void DrawImePanel()
    {
        if (!_editableText.IsWritingText)
            return;

        Vector2 position = _editableText.InnerText.GetDimensions().ToRectangle().Bottom();
        position.Y += 32f;
        Main.instance.SetIMEPanelAnchor(position, 0.5f);
    }
}