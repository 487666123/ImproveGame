using ImproveGame.Common.Configs;
using ImproveGame.Common.ModPlayers;
using ImproveGame.Common.ModSystems;
using ImproveGame.Core;
using ImproveGame.UI;
using ImproveGame.UI.Autofisher;
using ImproveGame.UI.ExtremeStorage;
using ImproveGame.UI.ItemSearcher;
using ImproveGame.UI.MasterControl;
using ImproveGame.UI.OpenBag;
using ImproveGame.UI.PlayerStats;
using ImproveGame.UI.WorldFeature;
using ImproveGame.UIFramework.BaseViews;
using System.Collections;

namespace ImproveGame.UIFramework.Common;

// 仅在客户端加载
[Autoload(Side = ModSide.Client)]
public class UIPlayer : ModPlayer
{
    private static CoroutineRunner _uiSetupDelayRunner = new();
    internal static readonly Vector2 HugeInventoryDefPosition = new(150, 340);
    internal static Vector2 HugeInventoryUIPosition;
    //internal static readonly Vector2 BuffTrackerDefPosition = new(630, 160);
    //internal static Vector2 BuffTrackerPosition;
    internal static readonly Vector2 WorldFeatureDefPosition = new(250, 280);
    internal static Vector2 WorldFeaturePosition;
    internal static readonly Vector2 ItemSearcherDefPosition = new(620, 400);
    internal static Vector2 ItemSearcherPosition;
    internal static readonly Vector2 OpenBagDefPosition = new(410, 360);
    internal static Vector2 OpenBagPosition;
    internal static readonly Vector2 PlayerInfoToggleDefPosition = new(572, 20);
    internal static Vector2 PlayerInfoTogglePosition;
    internal static bool ShouldShowUI; // 防止进入世界时UI闪一下

    // 函数在玩家进入地图时候调用, 不会在服务器调用, 用来加载 UI, 可以避免一些因 HJson 未加载产生的问题.
    public override void OnEnterWorld()
    {
        // 协程延时执行可以防止进入世界时UI闪一下
        InitUI();
    }

    public override void Unload()
    {
        _uiSetupDelayRunner = null;
    }

    public static void InitUI()
    {
        _uiSetupDelayRunner.StopAll();
        _uiSetupDelayRunner.Run(2f, SetupUI());
    }

    static IEnumerator SetupUI()
    {
        UIStyle.SetUIColors(UIConfigs.Instance.ThemeType);

        ShouldShowUI = true;
        UISystem uiSystem = UISystem.Instance;
        DataPlayer dataPlayer = Main.LocalPlayer.GetModPlayer<DataPlayer>();

        // 遍历继承了 BodyView 类的类型
        foreach (Type baseBodyType in UISystem.Instance.AutoCreateBodyTypes)
        {
            // 根据类型获取对应事件触发器
            EventTrigger eventTrigger = UISystem.Instance.EventTriggerInstances[baseBodyType];

            if (eventTrigger != null && Activator.CreateInstance(baseBodyType) is BaseBody bodyView)
            {
                UISystem.Instance.BaseBodyInstances[baseBodyType] = bodyView;
                eventTrigger.SetRootBody(bodyView);
            }
        }

        // 玩家信息
        CheckPositionValid(ref PlayerInfoTogglePosition, PlayerInfoToggleDefPosition);
        PlayerStatsGUI.Visible = true;
        PlayerStatsGUI.Instance.ControllerSwitch.SetPos(PlayerInfoTogglePosition).Recalculate();
        PlayerStatsGUI.Instance.LoadAndSetupFavorites();

        BigBagGUI.Instance.ItemGrid.SetInventory(dataPlayer.SuperVault);
        CheckPositionValid(ref HugeInventoryUIPosition, HugeInventoryDefPosition);
        BigBagGUI.Instance.MainPanel.SetPos(HugeInventoryUIPosition).Recalculate();

        // 增益追踪器
        //uiSystem.BuffTrackerGUI = new BuffTrackerGUI();
        //uiSystem.BuffTrackerTrigger.SetRootBody(uiSystem.BuffTrackerGUI);
        //CheckPositionValid(ref BuffTrackerPosition, BuffTrackerDefPosition);
        //uiSystem.BuffTrackerGUI.MainPanel.SetPos(BuffTrackerPosition).Recalculate();
        //UISystem.Instance.BuffTrackerGUI.BuffTrackerBattler.ResetDataForNewPlayer(Main.LocalPlayer.whoAmI);

        // 建筑法杖
        //uiSystem.ArchitectureGUI = new ArchitectureGUI();
        //uiSystem.ArchitectureTrigger.SetRootBody(uiSystem.ArchitectureGUI);

        // 构造法杖
        uiSystem.StructureGUI = new StructureGUI();
        uiSystem.StructureTrigger.SetRootBody(uiSystem.StructureGUI);

        // 世界特性
        CheckPositionValid(ref WorldFeaturePosition, WorldFeatureDefPosition);
        WorldFeatureGUI.Instance?.MainPanel.SetPos(WorldFeaturePosition).Recalculate();

        // 物品搜索
        CheckPositionValid(ref ItemSearcherPosition, ItemSearcherDefPosition);
        ItemSearcherGUI.Instance.MainPanel.SetPos(ItemSearcherPosition).Recalculate();

        // 快速开袋
        CheckPositionValid(ref OpenBagPosition, OpenBagDefPosition);
        OpenBagGUI.Instance?.MainPanel.SetPos(OpenBagPosition).Recalculate();

        // 生命体检测仪筛选
        uiSystem.LifeformAnalyzerGUI = new LifeformAnalyzerGUI();
        uiSystem.LifeformAnalyzerTrigger.SetRootBody(uiSystem.LifeformAnalyzerGUI);

        // 左侧栏
        uiSystem.ExtremeStorageGUI = new ExtremeStorageGUI();
        uiSystem.AutofisherGUI = new AutofisherGUI();
        uiSystem.PrefixRecallGUI = new PrefixRecallGUI();
        SidedEventTrigger.Clear();
        SidedEventTrigger.RegisterViewBody(uiSystem.ExtremeStorageGUI);
        SidedEventTrigger.RegisterViewBody(uiSystem.AutofisherGUI);
        SidedEventTrigger.RegisterViewBody(uiSystem.PrefixRecallGUI);

        // 总控快捷键弹窗提示
        bool hasKeybind = MyUtils.TryGetKeybindString(KeybindSystem.MasterControlKeybind, out _);
        if (!UIConfigs.Instance.FckKeybindPopup && !hasKeybind && UISystem.TryGetBaseBody<PopupPanel>(out var panel) && panel is not null)
            panel.Open();
        yield return null;
    }

    public static void CheckPositionValid(ref Vector2 position, Vector2 defaultPosition)
    {
        if (position.X <= 0 || position.X >= Main.screenWidth - 16)
            position.X = defaultPosition.X;
        if (position.Y <= 0 || position.Y >= Main.screenHeight - 16)
            position.Y = defaultPosition.Y;
    }

    public override void PreUpdate()
    {
        // 这里的判断其实是不需要的，因为SetupUI只会在进入世界的玩家的客户端运行
        // 但是为了防止其他情况下的报错，还是加上了
        if (Main.myPlayer != Player.whoAmI || Main.netMode is NetmodeID.Server)
            return;
        _uiSetupDelayRunner.Update(1);
    }
}