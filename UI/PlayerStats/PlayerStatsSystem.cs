namespace ImproveGame.UI.PlayerStats;

/// <summary>
/// 抽象就是对世界的鞭挞
/// </summary>
public class PlayerStatsSystem : ModSystem
{
    public static PlayerStatsSystem Instance { get; private set; }

    public Dictionary<string, BaseStatsCategory> StatsCategories { get; private set; } = new();

    public override void Load()
    {
        Instance = this;

        #region 近战属性

        BaseStatsCategory melee =
            new BaseStatsCategory(ModAsset.Melee.Value, "UI.PlayerStats.Melee");

        // 近战伤害、暴击、攻速与穿甲
        AddDamageStat(melee, DamageClass.Melee);
        AddCritStat(melee, DamageClass.Melee);
        AddAttackSpeedStat(melee, DamageClass.Melee);
        AddArmorPenetrationStat(melee, DamageClass.Melee);

        #endregion

        #region 远程

        BaseStatsCategory ranged =
            new BaseStatsCategory(ModAsset.Ranged.Value, "UI.PlayerStats.Ranged");

        // 远程伤害
        AddDamageStat(ranged, DamageClass.Ranged);

        // 弹药伤害
        ranged.BaseProperties.Add(new BaseStat(ranged, "UI.PlayerStats.BulletDamage",
            () =>
            {
                var sm = Main.LocalPlayer.bulletDamage;
                return BonusSyntax((sm.Additive * sm.Multiplicative - 1f) * 100, true);
            }));

        // 弓箭伤害
        ranged.BaseProperties.Add(new BaseStat(ranged, "UI.PlayerStats.ArrowDamage",
            () =>
            {
                var sm = Main.LocalPlayer.arrowDamage;
                return BonusSyntax((sm.Additive * sm.Multiplicative - 1f) * 100, true);
            }));

        // 其他伤害
        ranged.BaseProperties.Add(new BaseStat(ranged, "UI.PlayerStats.SpecialistDamage",
            () =>
            {
                var sm = Main.LocalPlayer.specialistDamage;
                return BonusSyntax((sm.Additive * sm.Multiplicative - 1f) * 100, true);
            }));

        // 远程暴击攻速与穿甲
        AddCritStat(ranged, DamageClass.Ranged);
        AddAttackSpeedStat(ranged, DamageClass.Ranged);
        AddArmorPenetrationStat(ranged, DamageClass.Ranged);

        #endregion

        #region 魔法

        BaseStatsCategory magic =
            new BaseStatsCategory(ModAsset.Magic.Value, "UI.PlayerStats.Magic");

        // 法术伤害暴击与攻速
        AddDamageStat(magic, DamageClass.Magic);
        AddCritStat(magic, DamageClass.Magic);
        AddAttackSpeedStat(magic, DamageClass.Magic);

        // 法术回复
        magic.BaseProperties.Add(new BaseStat(magic, "UI.PlayerStats.Regen",
            () => $"{Main.LocalPlayer.manaRegen / 2f}/s"));

        // 法术消耗减免
        magic.BaseProperties.Add(new BaseStat(magic, "UI.PlayerStats.Cost",
            () => BonusSyntax((Main.LocalPlayer.manaCost - 1f) * 100f, true)));

        // 法术穿甲
        AddArmorPenetrationStat(magic, DamageClass.Magic);

        #endregion

        #region 召唤

        BaseStatsCategory summon =
            new BaseStatsCategory(ModAsset.Summon.Value, "UI.PlayerStats.Summon");

        // 召唤伤害
        AddDamageStat(summon, DamageClass.Summon);

        // 鞭子速度
        summon.BaseProperties.Add(new BaseStat(summon, "UI.PlayerStats.SummonMeleeSpeed",
            () => BonusSyntax(Main.LocalPlayer.GetAttackSpeed(DamageClass.SummonMeleeSpeed) * 100f - 100f, true)));

        // 召唤穿甲
        AddArmorPenetrationStat(summon, DamageClass.Summon);

        // 召唤栏
        summon.BaseProperties.Add(new BaseStat(summon, "UI.PlayerStats.MaxMinions",
            () => $"{Main.LocalPlayer.slotsMinions}/{Main.LocalPlayer.maxMinions}"));

        // 哨兵栏
        summon.BaseProperties.Add(new BaseStat(summon, "UI.PlayerStats.MaxTurrets",
            () =>
                $"{Main.projectile.Count(proj => proj.active && proj.owner == Main.LocalPlayer.whoAmI && proj.WipableTurret)}/{Main.LocalPlayer.maxTurrets}"));

        #endregion

        #region 投掷属性 (出于跨模组考虑)

        var throwingIcon = ModLoader.IsEnabled("ThoriumMod")
            ? ModAsset.ThrowingThorium.Value
            : ModAsset.Throwing.Value;
        BaseStatsCategory throwing =
            new BaseStatsCategory(throwingIcon, "UI.PlayerStats.Throwing");

        // 投掷伤害、暴击、攻速与穿甲
        AddDamageStat(throwing, DamageClass.Throwing);
        AddCritStat(throwing, DamageClass.Throwing);
        AddAttackSpeedStat(throwing, DamageClass.Throwing);
        AddArmorPenetrationStat(throwing, DamageClass.Throwing);

        #endregion

        #region 其他

        BaseStatsCategory other =
            new BaseStatsCategory(MyUtils.GetTexture("UI/PlayerStats/Luck").Value, "UI.PlayerStats.Other");

        // 生命回复
        other.BaseProperties.Add(new BaseStat(other, "UI.PlayerStats.LifeRegen",
            () => $"{Main.LocalPlayer.lifeRegen / 2f}/s"));

        // 免伤
        other.BaseProperties.Add(new BaseStat(other, "UI.PlayerStats.Endurance",
            () => BonusSyntax(Main.LocalPlayer.endurance * 100f)));

        // 仇恨
        other.BaseProperties.Add(new BaseStat(other, "UI.PlayerStats.Aggro",
            () => $"{Main.LocalPlayer.aggro}"));

        // 幸运
        other.BaseProperties.Add(new BaseStat(other, "UI.PlayerStats.Luck",
            () => $"{Math.Round(Main.LocalPlayer.luck, 2)}"));

        #endregion

        StatsCategories.Add("Melee", melee);
        StatsCategories.Add("Ranged", ranged);
        StatsCategories.Add("Magic", magic);
        StatsCategories.Add("Summon", summon);
        StatsCategories.Add("Throwing", throwing);
        StatsCategories.Add("Other", other);

        // tipPanel.Append(new PlyInfoCard(PlyInfo("WingTime")}:", () => $"{MathF.Round((player.wingTime + player.rocketTime * 6) / 60f, 2)}s", "Flying").Join(_cardPanel);
        // new PlyInfoCard(GetHJSON("WingTimeMax"),
        //     () => $"{MathF.Round((_player.wingTimeMax + _player.rocketTimeMax * 6) / 60f, 2)}s", "Flying").Join(_cardPanel);
    }

    public static void CalamityIntegration(Mod calamityMod)
    {
        if (!calamityMod.TryFind<DamageClass>("RogueDamageClass", out var damageClass))
            return;

        BaseStatsCategory rogue = new(ModAsset.Rogue.Value, "UI.PlayerStats.CalamityMod.Rogue", false,
            ModContent.Request<Texture2D>("CalamityMod/icon_small", AssetRequestMode.ImmediateLoad).Value);

        AddDamageStat(rogue, damageClass);
        AddCritStat(rogue, damageClass);

        // 灾厄特有，盗贼弹幕速度
        rogue.BaseProperties.Add(new BaseStat(rogue, "UI.PlayerStats.CalamityMod.RogueVelocity",
            () => BonusSyntax((float)calamityMod.Call("GetRogueVelocity", Main.LocalPlayer) * 100f - 100f, true)));

        AddAttackSpeedStat(rogue, damageClass);
        AddArmorPenetrationStat(rogue, damageClass);

        Instance.StatsCategories.Add("CalamityMod.Rogue", rogue);
    }

    public static void ThoriumIntegration(Mod thoriumMod)
    {
        var thoriumIcon = ModContent.Request<Texture2D>("ThoriumMod/icon_small", AssetRequestMode.ImmediateLoad).Value;

        if (thoriumMod.TryFind<DamageClass>("HealerDamage", out var healerDamageClass) &&
            thoriumMod.TryFind<DamageClass>("HealerTool", out var healerToolClass))
        {
            BaseStatsCategory healer = new(ModAsset.Healer.Value, "UI.PlayerStats.ThoriumMod.Healer", false,
                thoriumIcon);

            AddDamageStat(healer, healerDamageClass);
            AddCritStat(healer, healerDamageClass);
            // 光辉施法速度
            AddAttackSpeedStat(healer, healerDamageClass);
            // 治疗速度（法术施法速度）
            healer.BaseProperties.Add(new BaseStat(healer, "UI.PlayerStats.ThoriumMod.HealingSpeed",
                () => BonusSyntax(PlayerAttackSpeed(healerToolClass), true)));
            // 法术回血量加成
            healer.BaseProperties.Add(new BaseStat(healer, "UI.PlayerStats.ThoriumMod.HealBonus",
                () =>
                {
                    int healBonus = (int)thoriumMod.Call("GetHealBonus", Main.LocalPlayer);
                    return healBonus > 0 ? $"+{healBonus}" : healBonus.ToString();
                }));
            AddArmorPenetrationStat(healer, healerDamageClass);

            Instance.StatsCategories.Add("ThoriumMod.Healer", healer);
        }

        if (thoriumMod.TryFind<DamageClass>("BardDamage", out var bardDamageClass))
        {
            BaseStatsCategory bard = new(ModAsset.Bard.Value, "UI.PlayerStats.ThoriumMod.Bard", false, thoriumIcon);

            // 乐师伤害、暴击、攻速与穿甲
            AddDamageStat(bard, bardDamageClass);
            AddCritStat(bard, bardDamageClass);
            AddAttackSpeedStat(bard, bardDamageClass);
            AddArmorPenetrationStat(bard, bardDamageClass);

            Instance.StatsCategories.Add("ThoriumMod.Bard", bard);
        }
    }

    #region 快捷添加伤害类型基础属性

    /// <summary>
    /// 伤害
    /// </summary>
    public static void AddDamageStat(BaseStatsCategory category, DamageClass damageClass)
    {
        category.BaseProperties.Add(new BaseStat(category, "UI.PlayerStats.Damage",
            () => BonusSyntax(PlayerDamage(damageClass), true)));
    }

    /// <summary>
    /// 暴击
    /// </summary>
    public static void AddCritStat(BaseStatsCategory category, DamageClass damageClass)
    {
        category.BaseProperties.Add(new BaseStat(category, "UI.PlayerStats.Crit",
            () => BonusSyntax(PlayerCrit(damageClass), true)));
    }

    /// <summary>
    /// 攻速
    /// </summary>
    public static void AddAttackSpeedStat(BaseStatsCategory category, DamageClass damageClass)
    {
        category.BaseProperties.Add(new BaseStat(category, "UI.PlayerStats.Speed",
            () => BonusSyntax(PlayerAttackSpeed(damageClass), true)));
    }

    /// <summary>
    /// 穿甲
    /// </summary>
    public static void AddArmorPenetrationStat(BaseStatsCategory category, DamageClass damageClass)
    {
        category.BaseProperties.Add(new BaseStat(category, "UI.PlayerStats.ArmorPenetration",
            () => $"{MathF.Round(Main.LocalPlayer.GetTotalArmorPenetration(damageClass), 2)}"));
    }

    #endregion

    #region 各种获取属性的方法

    /// <summary>
    /// 获取 LocalPlayer 的伤害
    /// </summary>
    public static float PlayerDamage(DamageClass damageClass)
    {
        var sm = Main.LocalPlayer.GetTotalDamage(damageClass);
        return (sm.Additive * sm.Multiplicative - 1) * 100f;
    }

    /// <summary>
    /// 获取 LocalPlayer 的暴击
    /// </summary>
    public static float PlayerCrit(DamageClass damageClass)
    {
        return Main.LocalPlayer.GetTotalCritChance(damageClass);
    }

    /// <summary>
    /// 获取 LocalPlayer 的攻速
    /// </summary>
    public static float PlayerAttackSpeed(DamageClass damageClass)
    {
        return Main.LocalPlayer.GetTotalAttackSpeed(damageClass) * 100f - 100f;
    }

    #endregion

    /// <summary>
    /// 对于传入的value，以加成格式显示 (四舍五入并保留两位小数) <br/>
    /// 不带符号：value% <br/>
    /// 带符号：+value%(正值) 或 -value%(负值) 或 -0%(0) <br/>
    /// </summary>
    public static string BonusSyntax(float value, bool sign = false)
    {
        float roundedValue = MathF.Round(value, 2);
        if (!sign)
            return $"{roundedValue}%";
        return roundedValue > 0 ? $"+{roundedValue}%" : $"{roundedValue}%";
    }
}