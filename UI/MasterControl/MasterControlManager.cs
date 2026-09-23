using ImproveGame.Common.Configs;
using ImproveGame.Common.ModPlayers;
using ImproveGame.Content.Functions;
using ImproveGame.Content.Items;
using ImproveGame.UI.AmmoChainPanel;
using ImproveGame.UI.ItemContainer;
using ImproveGame.UI.ItemSearcher;
using ImproveGame.UI.ModernConfig;
using ImproveGame.UI.OpenBag;
using ImproveGame.UI.PlayerStats;
using ImproveGame.UI.QuickShimmer;
using ImproveGame.UI.WeatherControl;
using ImproveGame.UI.WorldFeature;
using ImproveGame.UserInterfaces.BigBag;

namespace ImproveGame.UI.MasterControl;

public class MasterControlManager : ModSystem
{
    public static MasterControlManager Instance => ModContent.GetInstance<MasterControlManager>();

    /// <summary>
    /// 注册
    /// </summary>
    public MasterControlFunction Register(MasterControlFunction item)
    {
        if (!_originalMCFunctions.Contains(item))
        {
            _originalMCFunctions.Add(item);
        }

        return item;
    }

    /// <summary>
    /// 原功能列表
    /// </summary>
    public IReadOnlyList<MasterControlFunction> OriginalMCFuncions => _originalMCFunctions;

    private readonly List<MasterControlFunction> _originalMCFunctions = [];

    /// <summary>
    /// 排序之后的可用功能列表
    /// </summary>
    public IReadOnlyList<MasterControlFunction> AvailableOrderedMCFunctions => _availableOrderedMCFunctions;

    private readonly List<MasterControlFunction> _availableOrderedMCFunctions = [];

    /// <summary>
    /// 排序之后的不可用功能列表
    /// </summary>
    public IReadOnlyList<MasterControlFunction> UnavailableOrderedMCFunctions => _unavailableOrderedMCFunctions;

    private readonly List<MasterControlFunction> _unavailableOrderedMCFunctions = [];

    /// <summary>
    /// 排列功能列表
    /// </summary>
    public void OrderMCFunctions()
    {
        _availableOrderedMCFunctions.Clear();
        _unavailableOrderedMCFunctions.Clear();

        foreach (var item in _originalMCFunctions.OrderBy(i => i))
        {
            if (item.IsAvailable)
            {
                _availableOrderedMCFunctions.Add(item);
            }
            else
            {
                _unavailableOrderedMCFunctions.Add(item);
            }
        }
    }

    public override void PostSetupContent()
    {
        #region 大背包

        var bigBackpack = new MasterControlFunction("SuperVault")
        {
            Icon = ModAsset.BigBackpack.Value,
        }.Register();

        bigBackpack.Available += () => ImproveConfigs.Instance.SuperVault;
        bigBackpack.OnMouseDown += tv =>
        {
            if (!ImproveConfigs.Instance.SuperVault)
            {
                Main.NewText(MyUtils.GetText("MasterControl.NotEnabled"), Color.Pink);
                return;
            }

            BigBagUI.Instance?.Toggle();
        };

        #endregion

        #region 属性面板

        var playerStats = new MasterControlFunction("PlayerStats")
        {
            Icon = ModAsset.PlayerStats.Value,
        }.Register();

        playerStats.Available += () => UIConfigs.Instance.PlyInfo is not UIConfigs.PAPDisplayMode.NotDisplayed;
        playerStats.OnMouseDown += tv =>
        {
            if (UIConfigs.Instance.PlyInfo is UIConfigs.PAPDisplayMode.NotDisplayed)
            {
                Main.NewText(MyUtils.GetText("MasterControl.NotEnabled"), Color.Pink);
                return;
            }

            var body = PlayerStatsGUI.Instance;

            if (body.HasChild(body.Window))
                body.RemoveChild(body.Window);
            else
            {
                MyUtils.OperateInventory(true);
                body.Append(body.Window);
            }
        };

        #endregion

        #region 观战
        /*
                var spectating = new MasterControlFunction("Spectating")
                {
                    Icon = ModAsset.Spectating.Value,
                }.Register();

                spectating.OnMouseDown += tv =>
                {
                    var body = SpectatingGUI.Instance;

                    body.Enabled = !body.Enabled;
                };
        */
        #endregion

        #region 旗帜盒

        var bannerChest = new MasterControlFunction("BannerChest")
        {
            Icon = ModAsset.BannerChestIcon.Value,
        }.Register();

        bannerChest.Available += () =>
        {
            if (Main.LocalPlayer.TryGetModPlayer<ImprovePlayer>(out var improvePlayer))
            {
                return improvePlayer.BannerChest != null;
            }

            return false;
        };

        bannerChest.OnMouseDown += _ =>
        {
            if (Main.LocalPlayer.TryGetModPlayer<ImprovePlayer>(out var improvePlayer) &&
                improvePlayer.BannerChest != null)
            {
                if (ItemContainerGUI.Instace.Enabled &&
                    improvePlayer.BannerChest == ItemContainerGUI.Instace.Container &&
                    ItemContainerGUI.Instace.StartTimer.AnyOpen)
                    ItemContainerGUI.Instace.Close();
                else
                    ItemContainerGUI.Instace.Open(improvePlayer.BannerChest);
            }
        };

        #endregion

        #region 药水袋

        var potionBag = new MasterControlFunction("PotionBag")
        {
            Icon = ModAsset.PotionBagIcon.Value,
        }.Register();

        potionBag.Available += () =>
        {
            if (Main.LocalPlayer.TryGetModPlayer<ImprovePlayer>(out var improvePlayer))
            {
                return improvePlayer.PotionBag != null;
            }

            return false;
        };

        potionBag.OnMouseDown += _ =>
        {
            if (Main.LocalPlayer.TryGetModPlayer<ImprovePlayer>(out var improvePlayer) &&
                improvePlayer.PotionBag != null)
            {
                if (ItemContainerGUI.Instace.Enabled &&
                    improvePlayer.PotionBag == ItemContainerGUI.Instace.Container &&
                    ItemContainerGUI.Instace.StartTimer.AnyOpen)
                    ItemContainerGUI.Instace.Close();
                else
                    ItemContainerGUI.Instace.Open(improvePlayer.PotionBag);
            }
        };

        #endregion

        #region 天气控制

        var weatherControl = new MasterControlFunction("WeatherControl")
        {
            Icon = ModAsset.WeatherControl.Value,
        }.Register();

        weatherControl.Available += () => WeatherController.Enabled;

        weatherControl.OnMouseDown += _ =>
        {
            if (!ImproveConfigs.Instance.WeatherControl)
            {
                Main.NewText(MyUtils.GetText("MasterControl.NotEnabled"), Color.Pink);
                return;
            }

            if (!WeatherController.Unlocked)
            {
                Main.NewText(MyUtils.GetText("UI.WeatherGUI.Locked"), Color.Pink);
                return;
            }

            if (WeatherGUI.Visible && WeatherGUI.Instance.StartTimer.AnyOpen)
                WeatherGUI.Instance.Close();
            else
                WeatherGUI.Instance.Open();
        };

        #endregion

        #region 搜索物品

        var itemSearcher = new MasterControlFunction("ItemSearcher")
        {
            Icon = ModAsset.ItemSearcher.Value,
        }.Register();

        itemSearcher.OnMouseDown += _ =>
        {
            var ui = ItemSearcherGUI.Instance;
            if (ui is null) return;

            if (ui.Enabled && ui.StartTimer.AnyOpen)
                ui.Close();
            else
                ui.Open();
        };

        #endregion

        #region 世界特性

        var worldFeature = new MasterControlFunction("WorldFeature")
        {
            Icon = ModAsset.WorldFeature.Value,
        }.Register();

        worldFeature.Available += () => ImproveConfigs.Instance.WorldFeaturePanel;
        worldFeature.OnMouseDown += _ =>
        {
            if (!ImproveConfigs.Instance.WorldFeaturePanel)
            {
                Main.NewText(MyUtils.GetText("MasterControl.NotEnabled"), Color.Pink);
                return;
            }

            var ui = WorldFeatureGUI.Instance;
            if (ui is null) return;

            if (ui.Enabled && ui.StartTimer.AnyOpen)
                ui.Close();
            else
                ui.Open();
        };

        #endregion

        #region 自动开袋

        var bagOpener = new MasterControlFunction("BagOpener")
        {
            Icon = ModAsset.BagOpener.Value,
        }.Register();

        bagOpener.OnMouseDown += _ =>
        {
            var ui = OpenBagGUI.Instance;
            if (ui is null) return;

            if (ui.Enabled && ui.StartTimer.AnyOpen)
                ui.Close();
            else
                ui.Open();
        };

        #endregion

        #region 快速微光
        if (Main.netMode != NetmodeID.Server) Main.instance.LoadItem(ItemID.BottomlessShimmerBucket);
        var quickShimmer = new MasterControlFunction("QuickShimmer")
        {
            Icon = ModAsset.QuickShimmer.Value,
        }.Register();
        quickShimmer.Available += () => QuickShimmerSystem.Enabled;
        quickShimmer.OnMouseDown += _ =>
        {
            var ui = QuickShimmerGUI.Instance;
            if (ui is null) return;
            if (!ImproveConfigs.Instance.QuickShimmer)
            {
                Main.NewText(MyUtils.GetText("MasterControl.NotEnabled"), Color.Pink);
                return;
            }

            if (!QuickShimmerSystem.Unlocked)
            {
                Main.NewText(MyUtils.GetText("UI.QuickShimmer.Locked"), Color.Pink);
                return;
            }

            if (ui.Enabled && ui.StartTimer.AnyOpen)
                ui.Close();
            else
                ui.Open();
        };

        #endregion

        #region 弹药链

        var ammoChain = new MasterControlFunction("AmmoChain")
        {
            Icon = ModAsset.AmmoChain.Value,
        }.Register();

        ammoChain.Available += () => ImproveConfigs.Instance.AmmoChain;
        ammoChain.OnMouseDown += _ =>
        {
            if (!ImproveConfigs.Instance.AmmoChain)
            {
                Main.NewText(MyUtils.GetText("MasterControl.NotEnabled"), Color.Pink);
                return;
            }

            if (AmmoChainUI.Instance.Enabled && AmmoChainUI.Instance.StartTimer.AnyOpen)
                AmmoChainUI.Instance.Close();
            else
                AmmoChainUI.Instance.Open();
        };

        #endregion

        #region 模组配置

        var modConfig = new MasterControlFunction("ModConfig")
        {
            Icon = ModAsset.ModConfigIcon.Value,
        }.Register();

        modConfig.OnMouseDown += _ =>
        {
            var ui = ModernConfigUI.Instance;
            if (ui is null) return;

            if (ui.Enabled)
            {
                ui.Close();
            }
            else
            {
                ui.Open(Mod);
                ui.OpenFromMasterControl = true;
            }
        };

        #endregion

        OrderMCFunctions();
    }

    public override void UpdateUI(GameTime gameTime)
    {
        foreach (var function in OriginalMCFuncions)
        {
            function.UpdateAlways();
        }

        OrderMCFunctions();
    }
}
