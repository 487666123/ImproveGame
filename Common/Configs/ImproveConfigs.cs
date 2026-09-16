using ImproveGame.Common.Configs.Elements;
using ImproveGame.Common.GlobalNPCs;
using ImproveGame.Common.GlobalProjectiles;
using ImproveGame.Content.Functions;
using ImproveGame.UI.ModernConfig;
using Newtonsoft.Json;
using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace ImproveGame.Common.Configs;

public class ImproveConfigs : ModConfig
{
    // 获取配置
    // 使用了属性，这样如果值是 null 也不会崩了，防止各种奇奇怪怪的跨 mod 崩溃情况
    // 坏的支持，不应该返回假值，null 判断是调用方责任，这是基本开发守则 [26.4.2](局长)
    public static ImproveConfigs Instance { get; private set; }

    public override ConfigScope Mode => ConfigScope.ServerSide;
    public override void OnLoaded() => Instance = this;

    [JsonIgnore]
    // 用于Config选项收藏功能，判别这个Config实例是不是真正的ImproveConfigs
    // 以决定要不要显示“已收藏”文本
    public bool IsRealImproveConfigs = true;

    #region 其他

    [Header("Others")]

    [CustomModConfigItem(typeof(OtherFunctionsElement))]
    public object OtherFunctions;

    [CustomModConfigItem(typeof(OpenConfigElement))]
    public object OpenConfig;

    [CustomModConfigItem(typeof(FaqElement))]
    public object OpenFaq;

    #endregion

    #region 预设

    [Header("Presets")]
    [CustomModConfigItem(typeof(ILoveBalanceElement))]
    public object ILoveBalance;

    [CustomModConfigItem(typeof(FukMeCalamityElement))]
    public object FukMeCalamity;

    [CustomModConfigItem(typeof(AllOffElement))]
    public object AllOff;

    #endregion

    #region 物品设置

    [Header("Item")]
    /// <summary> 物品最大堆叠 </summary>
    [DefaultValue(9999)]
    [Range(1, int.MaxValue)]
    [ReloadRequired]
    public int ItemMaxStack;

    /// <summary> 任务鱼可堆叠 </summary>
    [DefaultValue(true)]
    [ReloadRequired]
    public bool QuestFishStack;

    /// <summary> 大背包 </summary>
    [DefaultValue(false)]
    public bool SuperVault;

    /// <summary> 独立收纳 自动拾取 </summary>
    [DefaultValue(false)]
    public bool SuperVoidVault;

    /// <summary> 独立收纳 智能拾取 </summary>
    [DefaultValue(false)]
    public bool SmartVoidVault;

    /// <summary> 物品拾取范围提升 (单位：格子) </summary>
    [DefaultValue(0)]
    [Slider]
    [Range(0, 75)]
    public int GrabDistance;

    /// <summary> 工具挖掘速度提升 </summary>
    [DefaultValue(0f)]
    [Range(0, 1f)]
    [Slider]
    [Increment(0.125f)]
    [CustomModConfigItem(typeof(Round4FloatElement))]
    public float ExtraToolSpeed;

    /// <summary> 提高物块放置速度 </summary>
    [DefaultValue(false)]
    public bool ModifyPlayerPlaceSpeed;

    /// <summary> 提高物块放置距离 </summary>
    [DefaultValue(0)]
    [Slider]
    [Range(0, 20)]
    [Increment(2)]
    public int ModifyPlayerTileRange;

    /// <summary> 物块放置速度和距离提升黑名单 </summary>
    public List<string> TileSpeed_Blacklist = ["torch"];

    /// <summary> 召唤物不消耗 </summary>
    [DefaultValue(false)]
    public bool NoConsume_SummonItem;

    /// <summary> 无限弹药 </summary>
    [DefaultValue(true)]
    public bool NoConsume_Ammo;

    /// <summary> 无限投掷物 </summary>
    [DefaultValue(true)]
    public bool NoConsume_Projectile;

    /// <summary> 无限电线 </summary>
    [DefaultValue(true)]
    public bool NoConsume_Wire;

    /// <summary> 可追溯重铸 </summary>
    [DefaultValue(true)]
    public bool ImprovePrefix;

    /// <summary> 中键打开便携收纳 </summary>
    [DefaultValue(true)]
    public bool MiddleEnableBank;

    /// <summary> 加速提炼机 </summary>
    [DefaultValue(true)]
    public bool FasterExtractinator;

    /// <summary> 任何地点都可放置床 </summary>
    [DefaultValue(true)]
    public bool BedEverywhere;

    /// <summary> 取消睡觉限制 </summary>
    [DefaultValue(true)]
    public bool NoSleepRestrictions;

    /// <summary> 睡觉速度倍率 </summary>
    [DefaultValue(5)]
    [Increment(5)]
    [Range(5, 100)]
    [Slider]
    public int BedTimeRate;

    #endregion

    #region 晶塔限制

    [Header("PylonMechanics")]

    /// <summary> 晶塔放置无限制 </summary>
    [DefaultValue(false)]
    public bool PylonPlaceNoRestriction;

    /// <summary> 晶塔传送无需玩家在附近 </summary>
    [DefaultValue(false)]
    public bool PylonTeleNoNear;

    /// <summary> 晶塔传送无需附近有NPC </summary>
    [DefaultValue(false)]
    public bool PylonTeleNoNPC;

    /// <summary> 晶塔传送无视危险环境 </summary>
    [DefaultValue(false)]
    public bool PylonTeleNoDanger;

    /// <summary> 晶塔传送无视生物群系 </summary>
    [DefaultValue(false)]
    public bool PylonTeleNoBiome;

    #endregion

    #region 物品效果设置

    [Header("ItemEffect")]
    /// <summary> 便携制作站 </summary>
    [DefaultValue(true)]
    public bool PortableCraftingStation;

    /// <summary> 随身增益站 </summary>
    [DefaultValue(true)]
    public bool NoPlace_BUFFTile;

    /// <summary> 随身增益站：旗帜 </summary>
    [DefaultValue(true)]
    public bool NoPlace_BUFFTile_Banner;

    /// <summary> 药剂 无限续杯 </summary>
    [DefaultValue(true)]
    public bool NoConsume_Potion;

    /// <summary> 药剂 无限续杯 需求量 </summary>
    [DisplayCondition(nameof(ImproveConfigs), nameof(NoConsume_Potion))]
    [DefaultValue(30)]
    [Range(10, 300)]
    public int NoConsume_PotionRequirement;

    /// <summary> 无限红药水 </summary>
    [DefaultValue(false)]
    public bool InfiniteRedPotion;

    /// <summary> 红药水随处可用 </summary>
    [DefaultValue(false)]
    [DisplayCondition(nameof(ImproveConfigs), nameof(InfiniteRedPotion))]
    public bool RedPotionEverywhere;

    /// <summary> 红药水无限使用需求量 </summary>
    [DefaultValue(30)]
    [Range(10, 300)]
    [DisplayCondition(nameof(ImproveConfigs), nameof(InfiniteRedPotion))]
    public int RedPotionRequirement;

    #endregion

    #region NPC设置

    [Header("NPC")]
    /// <summary> 城镇NPC分配房屋 </summary>
    [DefaultValue(true)]
    public bool TownNPCHome;

    /// <summary> 更好的城镇NPC入住机制 </summary>
    [DefaultValue(false)]
    public bool TownNPCGetTFIntoHouse;

    /// <summary> 城镇NPC可住在邪恶环境 </summary>
    [DefaultValue(true)]
    public bool NPCLiveInEvil;

    /// <summary> 城镇NPC刷新速度乘数 </summary>
    [Slider]
    [Range(1, 10)]
    [DefaultValue(1)]
    public int TownNPCSpawnSpeed;

    /// <summary> 渔夫任务立即刷新 </summary>
    [DefaultValue(FishQuestResetType.NotResetFish)]
    [DrawTicks]
    public FishQuestResetType NoCD_FishermanQuest;

    /// <summary> 敌人钱币掉落倍率 </summary>
    [Range(1, 25)]
    [DefaultValue(1)]
    [Slider]
    public int NPCCoinDropRate;

    /// <summary> 修改NPC快乐度 </summary>
    [DefaultValue(false)]
    public bool ModifyNPCHappiness;

    /// <summary> NPC快乐度值 </summary>
    [DisplayCondition(nameof(ImproveConfigs), nameof(ModifyNPCHappiness))]
    [DefaultValue(75)]
    [Range(75, 150)]
    [Slider]
    public int NPCHappiness;

    /// <summary> 史莱姆必定内含物品 </summary>
    [DefaultValue(false)]
    public bool SlimeExDrop;

    /// <summary> 熔岩史莱姆/地狱蝙蝠不生成熔岩 </summary>
    [DefaultValue(false)]
    public bool LavalessLavaSlime;

    /// <summary> 旅商永远不离开 </summary>
    [DefaultValue(false)]
    public bool TravellingMerchantStay;

    /// <summary> 旅商商店可刷新 </summary>
    [DefaultValue(true)]
    public bool TravellingMerchantRefresh;

    /// <summary> 快捷护士 </summary>
    [DefaultValue(true)]
    public bool QuickNurse;

    /// <summary> 图鉴快速解锁 </summary>
    [DefaultValue(true)]
    public bool BestiaryQuickUnlock;

    /// <summary> 旗帜击杀要求倍率 </summary>
    [Slider]
    [Range(0.1f, 10f)]
    [Increment(0.05f)]
    [DefaultValue(1f)]
    [CustomModConfigItem(typeof(Round4FloatElement))]
    public float BannerRequirement;

    #endregion

    #region 游戏机制

    [Header("GameMechanics")]
    /// <summary> 额外BUFF栏 </summary>
    [DefaultValue(99)]
    [Range(0, 99)]
    [Slider]
    [Increment(11)]
    [ReloadRequired]
    public int ExtraPlayerBuffSlots;

    /// <summary> 草药迅速生长 </summary>
    [DefaultValue(false)]
    public bool AlchemyGrassGrowsFaster;

    /// <summary> 草药总是盛开 </summary>
    [DefaultValue(false)]
    public bool AlchemyGrassAlwaysBlooms;

    /// <summary> 南瓜生长加速 </summary>
    [DefaultValue(false)]
    public bool PumpkinGrowsFaster;

    /// <summary> 生命果生长加速 </summary>
    [DefaultValue(false)]
    public bool LifeFruitGrowsFaster;

    /// <summary> 生命果最大数量限制 </summary>
    [DefaultValue(-1)]
    [Range(1, 60)]
    [Increment(1)]
    [Slider]
    public int LifeFruitLimit;

    /// <summary> 再生法杖自动种植, 因为1.4.5已经内置该功能了所以注释掉了 </summary>
    // [DefaultValue(false)]
    // public bool StaffOfRegenerationAutomaticPlanting;

    /// <summary> 禁止邪恶生物群落蔓延 </summary>
    [DefaultValue(true)]
    public bool NoBiomeSpread;

    /// <summary> 满血复活 </summary>
    [DefaultValue(true)]
    public bool RespawnWithFullHP;

    /// <summary> 死亡不清除增益 </summary>
    [DefaultValue(true)]
    public bool DontDeleteBuff;

    /// <summary> 旅行模式研究需求减少 </summary>
    [DefaultValue(false)]
    public bool JourneyResearch;

    /// <summary> 禁用伤害随机波动 </summary>
    [DefaultValue(false)]
    public bool BanDamageVar;

    /// <summary> 池子无渔力大小惩罚 </summary>
    [DefaultValue(true)]
    public bool NoLakeSizePenalty;

    /// <summary> 重生加速 非BOSS战 </summary>
    [DefaultValue(0)]
    [Range(0, 100)]
    [Increment(5)]
    [Slider]
    public int ResurrectionTimeShortened;

    /// <summary> 重生加速 BOSS战 </summary>
    [DefaultValue(0)]
    [Range(0, 100)]
    [Increment(5)]
    [Slider]
    public int BOSSBattleResurrectionTimeShortened;

    /// <summary> 禁止生成墓碑 </summary>
    [DefaultValue(false)]
    public bool BanTombstone;

    /// <summary> 专家Debuff时长延长 </summary>
    [DefaultValue(true)]
    public bool LongerExpertDebuff;

    /// <summary> 光线不被物块阻挡 </summary>
    [DefaultValue(false)]
    public bool LightNotBlocked;

    /// <summary> 炸弹不伤害玩家 </summary>
    [DefaultValue(BombsNotDamageType.Disabled)]
    [DrawTicks]
    public BombsNotDamageType BombsNotDamage;

    /// <summary> 禁用非玩家放置的炸弹爆炸 </summary>
    [DefaultValue(DisableNonPlayerBombsExplosionsType.Disabled)]
    [DrawTicks]
    public DisableNonPlayerBombsExplosionsType DisableNonPlayerBombsExplosions;

    /// <summary> 弹药链效果 </summary>
    [DefaultValue(true)]
    public bool AmmoChain;

    /// <summary> 简单连锁挖矿 </summary>
    [DefaultValue(true)]
    public bool SimpleVeinMining;

    /// <summary> 禁用连锁挖矿提示弹窗 </summary>
    [DisplayCondition(nameof(ImproveConfigs), nameof(SimpleVeinMining))]
    [DefaultValue(false)]
    public bool DisableVeinMiningPopup;

    #endregion

    #region 树木设置

    [Header("Tree")]
    /// <summary> 树木迅速生长 & 无视地层条件 </summary>
    [DefaultValue(true)]
    public bool TreeGrowFaster;

    /// <summary> 摇树总是掉落水果 </summary>
    [DefaultValue(false)]
    public bool ShakeTreeFruit;

    /// <summary> 宝石树必定掉落宝石 </summary>
    [DefaultValue(false)]
    public bool GemTreeAlwaysDropGem;

    /// <summary> 普通树木最小高度 </summary>
    [Range(1, 80)]
    [DefaultValue(5)]
    public int MostTreeMin;

    /// <summary> 普通树木最大高度 </summary>
    [Range(1, 80)]
    [DefaultValue(16)]
    public int MostTreeMax;

    /// <summary> 棕榈树最小高度 </summary>
    [Range(1, 80)]
    [DefaultValue(10)]
    public int PalmTreeMin;

    /// <summary> 棕榈树最大高度 </summary>
    [Range(1, 80)]
    [DefaultValue(20)]
    public int PalmTreeMax;

    /// <summary> 宝石树最小高度 </summary>
    [Range(1, 80)]
    [DefaultValue(7)]
    public int GemTreeMin;

    /// <summary> 宝石树最大高度 </summary>
    [Range(1, 80)]
    [DefaultValue(12)]
    public int GemTreeMax;

    #endregion

    #region 多人设置

    [Header("Together")]
    /// <summary> 共享便携制作站 </summary>
    [DefaultValue(true)]
    public bool ShareCraftingStation;

    /// <summary> 共享无限增益 </summary>
    [DefaultValue(false)]
    public bool ShareInfBuffs;

    /// <summary> 共享效果范围 </summary>
    [DefaultValue(-1)]
    [Range(-1, 2000)]
    public int ShareRange;

    /// <summary> 自动加入队伍 </summary>
    [DefaultValue(false)]
    public bool TeamAutoJoin;

    /// <summary> 仅需一人睡觉 </summary>
    [DefaultValue(false)]
    public bool BedOnlyOne;

    /// <summary> 无条件队内传送 </summary>
    [DefaultValue(true)]
    public bool NoConditionTP;

    #endregion

    #region 模组设置

    [Header("Server")]
    /// <summary> 仅房主可修改配置 </summary>
    [DefaultValue(false)]
    public bool OnlyHost;

    /// <summary> 仅房主可修改配置（需要密码） </summary>
    [DefaultValue(false)]
    [ReloadRequired]
    public bool OnlyHostByPassword;

    /// <summary> 敌人生成速度倍率最大值 </summary>
    [Slider]
    [DefaultValue(50)]
    [Range(4, 100)]
    public int SpawnRateMaxValue;

    /// <summary> 敌人生成速度倍率最小值 </summary>
    [Slider]
    [DefaultValue(0f)]
    [Range(0f, 1f)]
    [Increment(0.1f)]
    [CustomModConfigItem(typeof(Round4FloatElement))]
    public float SpawnRateMinValue;

    /// <summary> 自动渔夫不消耗鱼饵 </summary>
    [DefaultValue(true)]
    public bool EmptyAutofisher;

    /// <summary> 远程存储搜索距离 </summary>
    [DefaultValue(11)]
    [Range(5, 60)]
    [Slider]
    public int ExStorageSearchDistance;

    /// <summary> 开启全图背包查看 </summary>
    [DefaultValue(false)]
    public bool ICanSeeForeverAllBag;

    /// <summary> 显示世界特性面板 </summary>
    [DefaultValue(true)]
    public bool WorldFeaturePanel;

    /// <summary> 天气控制功能 </summary>
    [DefaultValue(true)]
    public bool WeatherControl;

    /// <summary> 快速微光转换 </summary>
    [DefaultValue(true)]
    public bool QuickShimmer;

    /// <summary> 小地图标记功能 </summary>
    [DefaultValue(true)]
    public bool MinimapMark;

    /// <summary> 建造魔杖不消耗材料 </summary>
    [DefaultValue(true)]
    public bool WandMaterialNoConsume;

    // [DefaultValue(true)]
    // public bool WandManaConsumption;

    /// <summary> 允许自定义假人AI样式 </summary>
    [DefaultValue(false)]
    public bool DummyCustomAIStyleAllowed;
    #endregion

    public override void OnChanged()
    {
        if (MostTreeMin > MostTreeMax)
        {
            MostTreeMin = MostTreeMax;
        }
        if (GemTreeMin > GemTreeMax)
        {
            GemTreeMin = GemTreeMax;
        }
        if (PalmTreeMin > PalmTreeMax)
        {
            PalmTreeMin = PalmTreeMax;
        }
        HigherTreeSystem.SetTreeHeights(GemTreeMin, GemTreeMax, MostTreeMin, MostTreeMax);
    }

    public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref NetworkText message)
    {
        return MyUtils.AcceptClientChanges(ImproveConfigs.Instance, pendingConfig, whoAmI, ref message);
    }
}
