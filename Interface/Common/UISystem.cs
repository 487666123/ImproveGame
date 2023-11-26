﻿using ImproveGame.Common.Configs;
using ImproveGame.Interface.ExtremeStorage;
using ImproveGame.Interface.GUI;
using ImproveGame.Interface.GUI.AutoTrash;
using ImproveGame.Interface.GUI.BannerChest;
using ImproveGame.Interface.GUI.ItemSearcher;
using ImproveGame.Interface.GUI.OpenBag;
using ImproveGame.Interface.GUI.PlayerStats;
using ImproveGame.Interface.GUI.WorldFeature;
using ImproveGame.Interface.UIStructs;

namespace ImproveGame.Interface.Common
{
    /// <summary>
    /// 用户界面
    /// </summary>
    public class UISystem : ModSystem
    {
        internal static UISystem Instance;

        #region 定义

        // 检测主题是否变化，变化了就重设UI，Config的OnChanged无法确定是哪个变量发生了变化，所以放这里了
        private ThemeType _themeLastTick;
        private bool _acrylicVfxLastTick;

        public BrustGUI BrustGUI;
        internal static UserInterface BrustInterface;

        public SpaceWandGUI SpaceWandGUI;
        internal static UserInterface SpaceWandInterface;

        public PaintWandGUI PaintWandGUI;
        internal static UserInterface PaintWandInterface;

        public GrabBagInfoGUI GrabBagInfoGUI;
        public EventTrigger GrabBagInfoTrigger;

        // 生命体检测仪筛选
        public LifeformAnalyzerGUI LifeformAnalyzerGUI;
        public EventTrigger LifeformAnalyzerTrigger;

        // 玩家信息表
        public PlayerStatsGUI PlayerInfoGUI;
        public EventTrigger PlayerInfoTrigger;

        // 自动垃圾桶
        public GarbageListGUI AutoTrashGUI;
        public EventTrigger AutoTrashTrigger;

        // 物品栏下方多个垃圾桶
        public InventoryTrashGUI InventoryTrashGUI;
        public EventTrigger InventoryTrashTrigger;

        // 药水袋 & 旗帜盒 UI
        public PackageGUI PackageGUI;
        public EventTrigger PackageTrigger;

        // 大背包 UI
        public BigBagGUI BigBagGUI;
        public EventTrigger BigBagTrigger;

        // Buff 追踪站
        public BuffTrackerGUI BuffTrackerGUI;
        public EventTrigger BuffTrackerTrigger;

        // 液体法杖
        public LiquidWandGUI LiquidWandGUI;
        public EventTrigger LiquidWandTrigger;

        // 建筑法杖
        public ArchitectureGUI ArchitectureGUI;
        public EventTrigger ArchitectureTrigger;

        // 构造法杖
        public StructureGUI StructureGUI;
        public EventTrigger StructureTrigger;

        // 世界特性
        public WorldFeatureGUI WorldFeatureGUI;
        public EventTrigger WorldFeatureTrigger;

        // 物品搜索
        public ItemSearcherGUI ItemSearcherGUI;
        public EventTrigger ItemSearcherTrigger;

        // 快速开袋
        public OpenBagGUI OpenBagGUI;
        public EventTrigger OpenBagTrigger;

        // 侧栏 UI
        public AutofisherGUI AutofisherGUI;
        public ExtremeStorageGUI ExtremeStorageGUI;
        public PrefixRecallGUI PrefixRecallGUI;
        public SidedEventTrigger SidedEventTrigger;

        #endregion

        #region 卸载 & 加载

        public override void Unload()
        {
            Instance = null;

            PlayerInfoGUI = null;
            PlayerInfoTrigger = null;

            // 侧栏GUI
            ExtremeStorageGUI = null;
            AutofisherGUI = null;
            PrefixRecallGUI = null;
            SidedEventTrigger = null;

            BuffTrackerGUI = null;
            BuffTrackerTrigger = null;

            LiquidWandGUI = null;
            LiquidWandTrigger = null;

            ArchitectureGUI = null;
            ArchitectureTrigger = null;

            BrustGUI = null;
            BrustInterface = null;

            AutoTrashGUI = null;
            AutoTrashTrigger = null;

            InventoryTrashGUI = null;
            InventoryTrashTrigger = null;

            BigBagGUI = null;
            BigBagTrigger = null;

            SpaceWandGUI = null;
            SpaceWandInterface = null;

            PaintWandGUI = null;
            PaintWandInterface = null;

            GrabBagInfoGUI = null;
            GrabBagInfoTrigger = null;

            LifeformAnalyzerGUI = null;
            LifeformAnalyzerTrigger = null;

            StructureGUI = null;
            StructureTrigger = null;

            WorldFeatureGUI = null;
            WorldFeatureTrigger = null;

            ItemSearcherGUI = null;
            ItemSearcherTrigger = null;

            OpenBagGUI = null;
            OpenBagTrigger = null;

            PackageGUI = null;
            PackageTrigger = null;
        }

        public override void Load()
        {
            Instance = this;
            if (Main.dedServ)
            {
                return;
            }

            // UserInterface 之 EventTrigger 版
            AutoTrashTrigger = new EventTrigger("Radial Hotbars", "Auto Trash");
            InventoryTrashTrigger = new EventTrigger("Radial Hotbars", "Inventory Trash");
            PackageTrigger = new EventTrigger("Radial Hotbars", "Package");
            BigBagTrigger = new EventTrigger("Radial Hotbars", "Big Bag");
            PlayerInfoTrigger = new EventTrigger("Radial Hotbars", "Player Info");
            LiquidWandTrigger = new EventTrigger("Radial Hotbars", "Liquid Wand");
            ArchitectureTrigger = new EventTrigger("Radial Hotbars", "Architecture");
            StructureTrigger = new EventTrigger("Radial Hotbars", "Structure");
            LifeformAnalyzerTrigger = new EventTrigger("Radial Hotbars", "Lifeform Analyzer");
            WorldFeatureTrigger = new EventTrigger("Radial Hotbars", "World Feature");
            ItemSearcherTrigger = new EventTrigger("Radial Hotbars", "Item Searcher");
            OpenBagTrigger = new EventTrigger("Radial Hotbars", "Open Bag");
            BuffTrackerTrigger = new EventTrigger("Radial Hotbars", "Buff Tracker GUI");
            GrabBagInfoTrigger = new EventTrigger("Radial Hotbars", "Grab Bag Info GUI");

            SidedEventTrigger = new SidedEventTrigger();

            BrustGUI = new BrustGUI();
            SpaceWandGUI = new SpaceWandGUI();
            PaintWandGUI = new PaintWandGUI();
            LoadGUI(ref BrustGUI, out BrustInterface);
            LoadGUI(ref SpaceWandGUI, out SpaceWandInterface);
            LoadGUI(ref PaintWandGUI, out PaintWandInterface);
        }


        private static void LoadGUI<T>(ref T uiState, out UserInterface uiInterface, Action PreActive = null)
            where T : UIState
        {
            uiInterface = new UserInterface();
            PreActive?.Invoke();
            // SetState() 会执行 Activate()
            // uiState.Activate();
            uiInterface.SetState(uiState);
        }

        #endregion

        #region 更新

        public override void UpdateUI(GameTime gameTime)
        {
            if (Main.ingameOptionsWindow || Main.InGameUI.IsVisible)
                return;

            // 特殊处理
            PrefixRecallGUI?.TrackDisplayment();

            // 可以看到，它执行的是最早的。
            EventTrigger.UpdateUI(gameTime);

            if (BrustGUI.Visible)
                BrustInterface?.Update(gameTime);
            if (SpaceWandGUI.Visible)
                SpaceWandInterface?.Update(gameTime);
            if (PaintWandGUI.Visible)
                PaintWandInterface?.Update(gameTime);

            if (_themeLastTick != UIConfigs.Instance.ThemeType || _acrylicVfxLastTick != GlassVfxEnabled)
            {
                UIColor.SetUIColors(UIConfigs.Instance.ThemeType);
                if (GlassVfxEnabled)
                    UIColor.AcrylicRedesign();
                UIPlayer.InitUI();
            }

            _themeLastTick = UIConfigs.Instance.ThemeType;
            _acrylicVfxLastTick = GlassVfxEnabled;
        }

        #endregion

        #region 绘制

        // UserInterface 的 Draw() 里面的 Use() 好像会一直 Recalculate()。没看懂为啥。
        // 多个 UserInterface 实例都在 Draw() 的时候导致 UserInterface.ActiveInstance 变更。
        // UserInterface.ActiveInstance 每次变更都会使 Recalculate() 执行一次。
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            // 如果 Insert 是按照向前插入的逻辑，越早插入越晚绘制，也就是越靠近顶层。
            EventTrigger.ModifyInterfaceLayers(layers);

            // 诊断网络？
            layers.FindVanilla("Diagnose Net", index =>
            {
                layers.Insert(index, "Buff Tracker GUI", () =>
                {
                    if (!UIConfigs.Instance.ShowMoreData)
                    {
                        return true;
                    }

                    bool modNetShouldDraw = ModNet.ShouldDrawModNetDiagnosticsUI;

                    if (modNetShouldDraw)
                    {
                        NetModuleLoader.NetModuleDiagnosticsUI.Draw(Main.spriteBatch, 640, 110);
                    }

                    return true;
                });
            });

            // 精密线控仪
            layers.FindVanilla("Wire Selection", index =>
            {
                layers.Insert(index, "SpaceWand GUI", SpaceWandGUI, () => SpaceWandGUI.Visible);
                layers.Insert(index, "Brust GUI", BrustGUI, () => BrustGUI.Visible);
                layers.Insert(index, "Paint GUI", PaintWandGUI, () => PaintWandGUI.Visible);
            });
        }

        #endregion

        public override void PreSaveAndQuit()
        {
            UIPlayer.ShouldShowUI = false;
        }
    }
}