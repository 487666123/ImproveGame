using ImproveGame.Common.Configs;
using ImproveGame.Core;
using System.Collections;
using Terraria.DataStructures;

namespace ImproveGame.Content.Functions.Summon;

[Autoload(Side = ModSide.Client)] // 原版召唤都是在客户端运行的
public class AutoSummonLogic : ModPlayer
{
    private static bool Enabled => !ModLoader.HasMod("SummonersAssociation") && UIConfigs.Instance.AutoSummon;

    private static readonly CoroutineRunner SummonRunner = new();
    private int _lastUsedStaffType;

    public override bool Shoot(Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity,
        int type, int damage, float knockback)
    {
        // 有人报索引超出数组界限了，非常抽象，加个判定
        if (!ProjectileID.Sets.MinionTargetingFeature.IndexInRange(type) ||
            !ContentSamples.ProjectilesByType.ContainsKey(type))
            return base.Shoot(item, source, position, velocity, type, damage, knockback);

        // 至少我们能适配灾厄
        if (Enabled && Player.altFunctionUse != 2 && ProjectileID.Sets.MinionTargetingFeature[type] &&
            ContentSamples.ProjectilesByType[type].minion && _lastUsedStaffType != item.type)
        {
            _lastUsedStaffType = item.type;
            var text = MyUtils.GetText("AutoSummon.Set", item.Name);
            MyUtils.AddNotification(text, default, item.type);
        }

        return base.Shoot(item, source, position, velocity, type, damage, knockback);
    }

    public override void PreUpdate()
    {
        if (Main.myPlayer != Player.whoAmI)
            return;
        SummonRunner.Update(1);
    }

    public override void OnRespawn()
    {
        if (!Enabled || Main.myPlayer != Player.whoAmI || !MyUtils.LocalPlayerHasItemFast(_lastUsedStaffType))
            return;

        SummonRunner.StopAll();
        // 等待3帧，确保玩家已经刷新
        SummonRunner.Run(3, SummonMinions());
    }

    IEnumerator SummonMinions()
    {
        // 还是要遍历，因为要正确处理词缀，找到正确的物品
        var allItems = MyUtils.GetAllInventoryItemsList(Player);
        var item = allItems.Find(item => item.type == _lastUsedStaffType);
        Player.slotsMinions = 0f;

        if (item is null)
            yield break;

        float slotsPerSpawn = ItemID.Sets.StaffMinionSlotsRequired[item.type];
        int iterationTimes = (int)(Player.maxMinions / slotsPerSpawn);

        SoundEngine.PlaySound(item.UseSound);
        var text = MyUtils.GetText("AutoSummon.Summoned", iterationTimes, item.Name);
        MyUtils.AddNotification(text, Color.Pink, item.type);

        for (int i = 0; i < iterationTimes; i++)
        {
            var mouseX = Main.mouseX;
            var mouseY = Main.mouseY;
            Main.mouseX = (int)(Player.Center.X - Main.screenPosition.X);
            Main.mouseY = (int)(Player.Top.Y - Main.screenPosition.Y) - 32;

            if (CombinedHooks.CanUseItem(Player, item) && CombinedHooks.CanShoot(Player, item))
            {
                Player.ItemCheck_Shoot(Player.whoAmI, item, Player.GetWeaponDamage(item));
                Player.ItemCheck_ApplyPetBuffs(item);
            }

            Main.mouseX = mouseX;
            Main.mouseY = mouseY;

            // 等2帧再召唤，有一些Mod通过检测场上召唤物数量来改变下一个召唤物的行为（说的就是你灾厄）
            yield return 2;
        }

        yield return null;
    }
}