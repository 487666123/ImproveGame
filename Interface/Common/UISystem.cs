using ImproveGame.Common.Configs;
using ImproveGame.Interface.ExtremeStorage;
using ImproveGame.Interface.GUI;
using ImproveGame.Interface.GUI.Architecture;
using ImproveGame.Interface.GUI.AutoTrash;
using ImproveGame.Interface.GUI.BannerChest;
using ImproveGame.Interface.GUI.WorldFeature;
using ImproveGame.Interface.PlayerInfo;
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

        public BrustGUI BrustGUI;
        internal static UserInterface BrustInterface;

        public SpaceWandGUI SpaceWandGUI;
        internal static UserInterface SpaceWandInterface;

        public PaintWandGUI PaintWandGUI;
        internal static UserInterface PaintWandInterface;

        public GrabBagInfoGUI GrabBagInfoGUI;
        internal static UserInterface GrabBagInfoInterface;

        // 生命体检测仪筛选
        public LifeformAnalyzerGUI LifeformAnalyzerGUI;
        public EventTrigger LifeformAnalyzerTrigger;

        // 玩家信息表
        public PlayerInfoGUI PlayerInfoGUI;
        public EventTrigger PlayerInfoTrigger;

        // 自动垃圾桶
        public AutoTrashGUI AutoTrashGUI;
        public EventTrigger AutoTrashTrigger;

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

            BigBagGUI = null;
            BigBagTrigger = null;

            SpaceWandGUI = null;
            SpaceWandInterface = null;

            PaintWandGUI = null;
            PaintWandInterface = null;

            GrabBagInfoGUI = null;
            GrabBagInfoInterface = null;

            LifeformAnalyzerGUI = null;
            LifeformAnalyzerTrigger = null;

            StructureGUI = null;
            StructureTrigger = null;

            WorldFeatureGUI = null;
            WorldFeatureTrigger = null;

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
            PackageTrigger = new EventTrigger("Radial Hotbars", "Package");
            BigBagTrigger = new EventTrigger("Radial Hotbars", "Big Bag");
            PlayerInfoTrigger = new EventTrigger("Radial Hotbars", "Player Info");
            LiquidWandTrigger = new EventTrigger("Radial Hotbars", "Liquid Wand");
            ArchitectureTrigger = new EventTrigger("Radial Hotbars", "Architecture");
            StructureTrigger = new EventTrigger("Radial Hotbars", "Structure");
            LifeformAnalyzerTrigger = new EventTrigger("Radial Hotbars", "Lifeform Analyzer");
            WorldFeatureTrigger = new EventTrigger("Radial Hotbars", "World Feature");

            BuffTrackerTrigger = new EventTrigger("Radial Hotbars", "Buff Tracker GUI");
            BuffTrackerGUI = new BuffTrackerGUI();
            BuffTrackerTrigger.SetCarrier(BuffTrackerGUI);

            SidedEventTrigger = new SidedEventTrigger();

            BrustGUI = new BrustGUI();
            SpaceWandGUI = new SpaceWandGUI();
            PaintWandGUI = new PaintWandGUI();
            GrabBagInfoGUI = new GrabBagInfoGUI();
            LoadGUI(ref BrustGUI, out BrustInterface);
            LoadGUI(ref SpaceWandGUI, out SpaceWandInterface);
            LoadGUI(ref PaintWandGUI, out PaintWandInterface);
            LoadGUI(ref GrabBagInfoGUI, out GrabBagInfoInterface);
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
            if (GrabBagInfoGUI.Visible)
                GrabBagInfoInterface?.Update(gameTime);
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

            // 背包
            layers.FindVanilla("Inventory", index =>
            {
                layers.Insert(index, "Grab Bag Info GUI", GrabBagInfoGUI, () => GrabBagInfoGUI.Visible);
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
    }
}