using ImproveGame.Common.Configs;
using ImproveGame.Common.ModPlayers;
using ImproveGame.Core;
using ImproveGame.Interface.GUI.BannerChest;
using ImproveGame.Interface.ExtremeStorage;
using ImproveGame.Interface.GUI;
using ImproveGame.Interface.PlayerInfo;
using ImproveGame.Interface.GUI.AutoTrash;
using ImproveGame.Interface.GUI.ItemSearcher;
using ImproveGame.Interface.GUI.OpenBag;
using ImproveGame.Interface.GUI.WorldFeature;
using System.Collections;

namespace ImproveGame.Interface.Common;

// 仅在客户端加载
[Autoload(Side = ModSide.Client)]
public class UIPlayer : ModPlayer
{
    private static CoroutineRunner _uiSetupDelayRunner = new();
    internal static Vector2 HugeInventoryUIPosition;

    // 函数在玩家进入地图时候调用, 不会在服务器调用, 用来加载 UI, 可以避免一些因 HJson 未加载产生的问题.
    public override void OnEnterWorld()
    {
        // 协程延时执行可以防止进入世界时UI闪一下
        _uiSetupDelayRunner.StopAll();
        _uiSetupDelayRunner.Run(2f, SetupUI());
    }

    public override void Unload()
    {
        _uiSetupDelayRunner = null;
    }

    IEnumerator SetupUI()
    {
        UISystem uiSystem = UISystem.Instance;
        DataPlayer dataPlayer = Player.GetModPlayer<DataPlayer>();

        // 玩家信息
        uiSystem.PlayerInfoGUI = new PlayerInfoGUI();
        uiSystem.PlayerInfoTrigger.SetCarrier(uiSystem.PlayerInfoGUI);
        PlayerInfoGUI.Visible = true;

        // 自动垃圾桶
        uiSystem.AutoTrashGUI = new AutoTrashGUI();
        uiSystem.AutoTrashTrigger.SetCarrier(uiSystem.AutoTrashGUI);

        // 旗帜盒
        uiSystem.PackageGUI = new PackageGUI();
        uiSystem.PackageTrigger.SetCarrier(uiSystem.PackageGUI);

        // 大背包
        uiSystem.BigBagGUI = new BigBagGUI();
        uiSystem.BigBagTrigger.SetCarrier(uiSystem.BigBagGUI);

        uiSystem.BigBagGUI.ItemGrid.SetInventory(dataPlayer.SuperVault);
        if (HugeInventoryUIPosition.X <= 0 && HugeInventoryUIPosition.X >= Main.screenWidth)
            HugeInventoryUIPosition.X = 150;
        if (HugeInventoryUIPosition.Y <= 0 && HugeInventoryUIPosition.Y >= Main.screenHeight)
            HugeInventoryUIPosition.Y = 340;
        uiSystem.BigBagGUI.MainPanel.SetPos(HugeInventoryUIPosition).Recalculate();

        // 液体法杖
        uiSystem.LiquidWandGUI = new LiquidWandGUI();
        uiSystem.LiquidWandTrigger.SetCarrier(uiSystem.LiquidWandGUI);

        // 建筑法杖
        uiSystem.ArchitectureGUI = new ArchitectureGUI();
        uiSystem.ArchitectureTrigger.SetCarrier(uiSystem.ArchitectureGUI);

        // 构造法杖
        uiSystem.StructureGUI = new StructureGUI();
        uiSystem.StructureTrigger.SetCarrier(uiSystem.StructureGUI);

        // 世界特性
        uiSystem.WorldFeatureGUI = new WorldFeatureGUI();
        uiSystem.WorldFeatureTrigger.SetCarrier(uiSystem.WorldFeatureGUI);

        // 物品搜索
        uiSystem.ItemSearcherGUI = new ItemSearcherGUI();
        uiSystem.ItemSearcherTrigger.SetCarrier(uiSystem.ItemSearcherGUI);

        // 快速开袋
        uiSystem.OpenBagGUI = new OpenBagGUI();
        uiSystem.OpenBagTrigger.SetCarrier(uiSystem.OpenBagGUI);

        // 生命体检测仪筛选
        uiSystem.LifeformAnalyzerGUI = new LifeformAnalyzerGUI();
        uiSystem.LifeformAnalyzerTrigger.SetCarrier(uiSystem.LifeformAnalyzerGUI);

        // 左侧栏
        uiSystem.ExtremeStorageGUI = new ExtremeStorageGUI();
        uiSystem.AutofisherGUI = new AutofisherGUI();
        uiSystem.PrefixRecallGUI = new PrefixRecallGUI();
        SidedEventTrigger.Clear();
        SidedEventTrigger.RegisterViewBody(uiSystem.ExtremeStorageGUI);
        SidedEventTrigger.RegisterViewBody(uiSystem.AutofisherGUI);
        SidedEventTrigger.RegisterViewBody(uiSystem.PrefixRecallGUI);
        yield return null;
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