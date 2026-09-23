using ImproveGame.Common.Configs;
using ImproveGame.Common.GlobalItems;
using ImproveGame.UIFramework;
using ImproveGame.UIFramework.BaseViews;
using ImproveGame.UIFramework.Common;
using ImproveGame.UIFramework.SUIElements;
using ImproveGame.UIFramework.UIElements;
using Terraria.GameInput;
using Terraria.UI.Chat;
using UIImage = Terraria.GameContent.UI.Elements.UIImage;

namespace ImproveGame.UI;

public class PrefixRecallGUI : BaseBody, ISidedView
{
    public static bool Visible => ImproveConfigs.Instance.ImprovePrefix && !Main.reforgeItem.IsAir &&
        Main.reforgeItem.TryGetGlobalItem<ImprovePrefixItem>(out var modItem) && modItem.Prefixs.Count > 0;

    public override bool Enabled { get => true; set { } }

    public float SwapSlideFactor { get; set; }

    public override bool CanSetFocusTarget(UIElement target)
        => (target != this && _basePanel.IsMouseHovering) || _basePanel.IsLeftMousePressed;

    private static int _oldItemType; // 用于监测type以及时更新uiList
    private static int _oldPrefixCount; // 用于监测词缀数量以及时更新uiList
    private static bool _oldVisible; // 用于监测显示条件是否满足以开启/关闭UI

    private const float PanelLeft = 54f;
    private const float PanelTop = 340f;
    private const float PanelHeight = 330f;
    private const float PanelWidth = 388f;

    private SUIPanel _basePanel; // 背景板
    public SUIScrollBar Scrollbar; // 拖动条
    public UIList UIList; // 明细列表

    public bool ForceCloseCondition() => Main.LocalPlayer.chest != -1 || !Main.playerInventory ||
                                         Main.LocalPlayer.sign > -1 || Main.LocalPlayer.talkNPC <= -1 ||
                                         Main.npc[Main.LocalPlayer.talkNPC] is not { type: NPCID.GoblinTinkerer };

    // 检测是否满足显示条件并执行显示/隐藏命令
    public void TrackDisplayment()
    {
        bool opened = SidedEventTrigger.IsOpened(this);

        switch (Visible)
        {
            case false when _oldVisible && opened:
            case true when !_oldVisible && !opened:
                SidedEventTrigger.ToggleViewBody(this);
                break;
        }

        _oldVisible = Visible;
    }

    public override void OnInitialize()
    {
        Append(_basePanel = new SUIPanel(UIStyle.PanelBorder, UIStyle.PanelBg)
        {
            Shaded = true,
            Left = { Pixels = PanelLeft },
            Top = { Pixels = PanelTop },
            Width = { Pixels = PanelWidth },
            Height = { Pixels = PanelHeight }
        });

        UIList = new UIList
        {
            Width = new(-28f, 1f),
            Height = new(0, 1f),
            ListPadding = 4f,
            ManualSortMethod = _ => { }
        };
        _basePanel.Append(UIList);

        Scrollbar = new SUIScrollBar
        {
            HAlign = 1f,
            Height = new(0, 1f)
        };
        Scrollbar.SetView(100f, 1000f);
        SetupScrollBar();
        _basePanel.Append(Scrollbar);
    }

    private void SetupScrollBar(bool resetViewPosition = true)
    {
        float height = UIList.GetInnerDimensions().Height;
        Scrollbar.SetView(height, UIList.GetTotalHeight());
        if (resetViewPosition)
            Scrollbar.BarTop = 0f;
    }

    public override void ScrollWheel(UIScrollWheelEvent evt)
    {
        base.ScrollWheel(evt);
        if (_basePanel.GetOuterDimensions().ToRectangle().Contains(evt.MousePosition.ToPoint()))
            Scrollbar.BarTopBuffer += evt.ScrollWheelValue;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (Scrollbar.IsMouseHovering) // 不知道为啥默认没有
            Main.LocalPlayer.mouseInterface = true;

        UpdateList();

        // 进入和退出动画
        float oldLeft = _basePanel.Left.Pixels;
        float widthNext = _basePanel.GetDimensions().Width;
        float hiddenPositionNext = -widthNext - 78;
        _basePanel.Left.Set((int)MathHelper.Lerp(hiddenPositionNext, PanelLeft, SwapSlideFactor), 0f);
        if (oldLeft != _basePanel.Left.Pixels)
            _basePanel.Recalculate();
    }

    private void UpdateList()
    {
        if (!Main.reforgeItem.TryGetGlobalItem<ImprovePrefixItem>(out var prefixItem))
        {
            Close();
            return;
        }
        if (_oldItemType == Main.reforgeItem.type &&
            _oldPrefixCount == prefixItem.Prefixs.Count)
            return;

        SetupList();
        _oldItemType = Main.reforgeItem.type;
        _oldPrefixCount = prefixItem.Prefixs.Count;
    }

    public void Open()
    {
        UpdateList();
    }

    public void Close()
    {
        _oldItemType = ItemID.None;
    }

    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        if (Scrollbar is not null)
        {
            UIList._innerList.Top.Set(-Scrollbar.BarTop, 0);
        }

        UIList.Recalculate();

        if (_basePanel.IsMouseHovering || Scrollbar.IsMouseHovering)
        {
            PlayerInput.LockVanillaMouseScroll("ImproveGame: Prefix Recall GUI");
        }

        base.DrawSelf(spriteBatch);
    }

    public void SetupList()
    {
        UIList.Clear();

        var prefixes = Main.reforgeItem.GetGlobalItem<ImprovePrefixItem>().Prefixs.ToArray();
        var sortedResult = from pair in prefixes orderby pair.Cost descending select pair; // 排序
        foreach (var prefix in sortedResult)
        {
            UIList.Add(new PrefixTab(prefix.PrefixId, prefix.Cost));
        }

        Recalculate();
        SetupScrollBar();
    }
}

public class PrefixTab : SUIPanel
{
    private static readonly Dictionary<int, Color> RarityColors = new()
    {
        [-11] = Colors.RarityAmber,
        [-1] = Colors.RarityTrash,
        [0] = Colors.RarityNormal,
        [1] = Colors.RarityBlue,
        [2] = Colors.RarityGreen,
        [3] = Colors.RarityOrange,
        [4] = Colors.RarityRed,
        [5] = Colors.RarityPink,
        [6] = Colors.RarityPurple,
        [7] = Colors.RarityLime,
        [8] = Colors.RarityYellow,
        [9] = Colors.RarityCyan,
        [10] = new Color(255, 40, 100),
        [11] = new Color(180, 40, 255)
    };

    public ref Item Item => ref Main.reforgeItem; // 向原物品的链接
    public ModItemSlot DummyItem; // 当前物品展示
    internal int PrefixId;
    internal long Price;

    public PrefixTab(int prefixId, int value) : base(UIStyle.PanelBorderLight, UIStyle.PanelBg)
    {
        PrefixId = prefixId;
        CalculatePrice(value);
        this.SetSize(new Vector2(0f, 80f), 1f);

        DummyItem = new(1f)
        {
            Interactable = false,
            RoundBorder = true,
            VAlign = .5f,
            Item = new(Item.type)
        };
        DummyItem.Item.Prefix(prefixId);
        Append(DummyItem);

        UIHorizontalSeparator separator = new()
        {
            Top = StyleDimension.FromPixels(20),
            Left = StyleDimension.FromPixels(70),
            Width = StyleDimension.FromPixels(240),
            Color = Color.Lerp(Color.White, new Color(63, 65, 151, 255), 0.85f) * 0.7f
        };
        Append(separator);

        var reforgeButton = new UIImage(TextureAssets.Reforge[0])
        {
            Top = new StyleDimension(separator.Height.Pixels + separator.Top.Pixels + 6f, 0f),
            Left = new StyleDimension(-30f, 1f)
        };
        reforgeButton.SetSize(24f, 24f);
        reforgeButton.OnLeftClick += (_, _) => Reforge(reforgeButton);
        reforgeButton.OnUpdate += _ =>
        {
            string str = Lang.inter[19].Value;
            reforgeButton.Color = Color.White;
            if (PrefixId == Item.prefix)
            {
                reforgeButton.Color = Color.Gray;
                return;
            }

            if (reforgeButton.IsMouseHovering)
            {
                Main.instance.MouseText(str);
            }
        };
        reforgeButton.OnMouseOver += (_, _) =>
        {
            if (PrefixId != Item.prefix)
                reforgeButton.SetImage(TextureAssets.Reforge[1]);
        };
        reforgeButton.OnMouseOut += (_, _) =>
        {
            reforgeButton.SetImage(TextureAssets.Reforge[0]);
        };
        Append(reforgeButton);
    }

    private void CalculatePrice(int value)
    {
        Price = value * Item.stack; // TML: #StackablePrefixWeapons: scale with current stack size
        bool canApplyDiscount = true;
        if (!ItemLoader.ReforgePrice(Item, ref Price, ref canApplyDiscount))
            return;

        if (canApplyDiscount && Main.LocalPlayer.discountAvailable)
            Price = (int)(Price * 0.8);

        Price = (int)(Price * Main.LocalPlayer.currentShoppingSettings.PriceAdjustment);
        Price = (int)(Price * 0.05f);
    }

    private void Reforge(UIImage ui)
    {
        if (PrefixId == Item.prefix || !ItemLoader.CanReforge(Item) || !Main.LocalPlayer.CanAfford(Price))
        {
            return;
        }

        Main.LocalPlayer.BuyItem(Price);
        ItemLoader.PreReforge(Item);
        Item.ResetPrefix();
        Item.Prefix(PrefixId);
        ItemLoader.PostReforge(Item);
        PopupText.NewText(PopupTextContext.ItemReforge, Item, Main.LocalPlayer.Center, Item.stack, noStack: true);
        SoundEngine.PlaySound(SoundID.Item37);
        ui.SetImage(TextureAssets.Reforge[0]);
    }

    public override void DrawSelf(SpriteBatch sb)
    {
        base.DrawSelf(sb);
        var dimensions = GetDimensions();
        var position = dimensions.Center();
        position.Y -= 36f;
        position.X -= 82f;
        ItemSlot.DrawMoney(sb, Lang.inter[46].Value, position.X, position.Y, Utils.CoinsSplit(Price), true);

        var textColor = Color.White;
        var item = DummyItem.Item;
        if (item.master || item.rare == ItemRarityID.Master)
            textColor = new Color(255, (int)(Main.masterColor * 200f), 0);
        else if (item.expert || item.rare == ItemRarityID.Expert)
            textColor = Main.DiscoColor;
        else if (RarityColors.TryGetValue(item.rare, out Color color))
            textColor = color;
        if (item.rare >= ItemRarityID.Count)
            textColor = RarityLoader.GetRarity(item.rare).RarityColor;
        position.Y += 6f;
        string str = PrefixId < 0 || PrefixId >= Lang.prefix.Length
            ? MyUtils.GetText("Configs.ImproveConfigs.ImprovePrefix.Unknown")
            : Lang.prefix[PrefixId].Value;
        ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, str, position, textColor, 0f,
            Vector2.Zero, Vector2.One);
    }
}