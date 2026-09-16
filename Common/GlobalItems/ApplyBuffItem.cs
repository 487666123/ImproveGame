using ImproveGame.Common.Configs;
using ImproveGame.Common.ModSystems;
using ImproveGame.Content.Functions;
using ImproveGame.Core;
using ImproveGame.Modules.InfiniteBuff;
using ImproveGame.Modules.InfiniteBuff.UserInterface;
using SilkyUIFramework;
using System.Collections.ObjectModel;

namespace ImproveGame.Common.GlobalItems;

public class ApplyBuffItem : GlobalItem
{
    // 不能用同一个List，因为要求的堆叠量不一样，否则会串台
    private readonly static Dictionary<int, List<int>> _potionToBuffs = [];
    private readonly static Dictionary<int, List<int>> _stationToBuffs = [];

    public static bool IsItemAvailable(Item item)
    {
        if (Main.gameMenu) return false;

        if (Main.LocalPlayer.TryGetModPlayer<InfiniteBuffModPlayer>(out var infBuffPlayer) &&
            infBuffPlayer.BuffItems.Contains(item))
            return true;

        // 非增益药剂
        if (ImproveConfigs.Instance.NoConsume_Potion && item.stack >= ImproveConfigs.Instance.NoConsume_PotionRequirement &&
            Lookups.SpecialPotions.Contains(item.type))
            return true;

        // 红药水扩展
        if (item.CanActivateRedPotionExtension())
            return true;

        // 随身增益站：旗帜
        if (ImproveConfigs.Instance.NoPlace_BUFFTile_Banner && BannerPatches.AvailableBanners.Contains(item))
            return true;

        // 弹药
        if (ImproveConfigs.Instance.NoConsume_Ammo && item.stack >= 3996 && item.ammo > 0)
            return true;

        // 电线
        if (ImproveConfigs.Instance.NoConsume_Wire && item.stack >= 3996 && item.type == ItemID.Wire)
            return true;

        // 花园侏儒
        if (item.type is ItemID.GardenGnome)
            return true;

        return false;
    }

    public static List<int> GetItemBuffTypes(Item item)
    {
        if (ModIntegrationsSystem.ModdedInfBuffsIgnore.Contains(item.type)) return [];

        if (ImproveConfigs.Instance.NoConsume_Potion)
        {
            // 普通药水
            if (item.stack >= ImproveConfigs.Instance.NoConsume_PotionRequirement)
            {
                if (_potionToBuffs.TryGetValue(item.type, out var buffsInTable))
                {
                    return buffsInTable;
                }

                var buffs = new List<int>();
                // 自带buffType的物品，小于60s持续时间的不算
                if (item.buffType > 0 && item.buffTime >= 60 * 60)
                    buffs.Add(item.buffType);
                // 其他Mod的，自行添加了引用
                if (ModIntegrationsSystem.ModdedPotionBuffs.TryGetValue(item.type, out List<int> buffTypes))
                    buffs.AddRange(buffTypes);
                if (buffs.Count > 0)
                {
                    _potionToBuffs[item.type] = buffs;
                    return buffs;
                }
            }
        }

        // 随身增益站：普通
        if (ImproveConfigs.Instance.NoPlace_BUFFTile)
        {
            if (_stationToBuffs.TryGetValue(item.type, out var buffsInTable))
                return buffsInTable;

            IsBuffTileItem(item, out List<int> buffTypes);
            if (item.type is ItemID.HoneyBucket)
                buffTypes.Add(BuffID.Honey);
            if (buffTypes.Count > 0)
            {
                _stationToBuffs[item.type] = buffTypes;
                return buffTypes;
            }
        }

        return [];
    }

    public static bool IsBuffTileItem(Item item, out List<int> buffTypes)
    {
        // 会给玩家buff的雕像
        buffTypes = (from t in Lookups.BuffTiles
                     where item.createTile == t.TileID && (item.placeStyle == t.Style || t.Style == -1) select t.BuffID)
            .ToList();

        // 其他Mod的，自行添加了引用
        if (ModIntegrationsSystem.ModdedPlaceableItemBuffs.TryGetValue(item.type, out var moddedBuffs))
            buffTypes.AddRange(moddedBuffs);
        return buffTypes.Count > 0;
    }

    // 物品消耗
    public override bool ConsumeItem(Item item, Player player)
    {
        if (item.ModItem?.Mod.Name is "Everglow") return base.ConsumeItem(item, player);

        if (ImproveConfigs.Instance.NoConsume_Potion && !ModIntegrationsSystem.ModdedInfBuffsIgnore.Contains(item.type) &&
            item.stack >= ImproveConfigs.Instance.NoConsume_PotionRequirement &&
            !ModIntegrationsSystem.ModdedInfBuffsConsume.Contains(item.type) &&
           (item.buffType > 0 || Lookups.SpecialPotions.Contains(item.type)))
        {
            return false;
        }

        return base.ConsumeItem(item, player);
    }

    private static bool _oldMiddlePressed;

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (!IsItemAvailable(item))
            return;

        if (IsBuffTileItem(item, out _) || item.type is ItemID.HoneyBucket or ItemID.GardenGnome ||
            (item.stack >= ImproveConfigs.Instance.NoConsume_PotionRequirement && item.buffType > 0 && item.active))
        {
            var buffTypes = GetItemBuffTypes(item);
            if (buffTypes.Count != 1)
            {
                if (buffTypes.Count is 0) return;

                var buffs = Lang.GetBuffName(buffTypes[0]);
                buffs = buffTypes.ToArray()[1..].Aggregate(buffs, (current, i) => current + $", {Lang.GetBuffName(i)}");
                buffs = $"[{buffs}]";

                tooltips.Add(new TooltipLine(Mod, "AppliedBuffs", buffs)
                {
                    Color = Color.SkyBlue
                });

                TagItem.AddIconHiddenTooltips(Mod, tooltips);
                return;
            }

            var buffType = buffTypes[0];
            if (buffType is -1 && item.type != ItemID.GardenGnome) return;

            if (_oldMiddlePressed)
            {
                _oldMiddlePressed = KeybindSystem.ItemInteractKeybind.JustPressed;
            }

            if (KeybindSystem.ItemInteractKeybind.JustPressed && !_oldMiddlePressed)
            {
                _oldMiddlePressed = true;

                if (UISceneManager.Instance.TryGetInstance<InfiniteBuffController>(out var controller))
                {
                    controller.Enabled = !controller.Enabled;
                }

                // [old]
                //if (BuffTrackerGUI.Visible)
                //    UISystem.Instance.BuffTrackerGUI.Close();
                //else
                //    UISystem.Instance.BuffTrackerGUI.Open();
            }

            TagItem.ModifyBuffTooltips(Mod, item.type, buffType, tooltips);
        }

        // 红药水扩展
        if (item.CanActivateRedPotionExtension())
        {
            tooltips.Add(new TooltipLine(Mod, "TagDetailed.RedPotion", GetText("Tips.TagDetailed.RedPotion"))
            {
                Color = Color.SkyBlue
            });
            TagItem.AddShiftForMoreTooltip(tooltips);
        }
    }

    public override bool PreDrawTooltip(Item item, ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y)
    {
        if (!IsItemAvailable(item))
            return base.PreDrawTooltip(item, lines, ref x, ref y);

        if ((item.CanActivateRedPotionExtension() || item.type is ItemID.GardenGnome) && ItemSlot.ShiftInUse)
        {
            TagItem.DrawTagTooltips(lines, TagItem.GenerateDetailedTags(Mod, lines), x, y);
            return base.PreDrawTooltip(item, lines, ref x, ref y);
        }

        if (IsBuffTileItem(item, out _) || item.type is ItemID.HoneyBucket ||
            (item.stack >= ImproveConfigs.Instance.NoConsume_PotionRequirement && item.buffType > 0 && item.active))
        {
            var buffTypes = GetItemBuffTypes(item);

            if (buffTypes.Count != 1)
                return base.PreDrawTooltip(item, lines, ref x, ref y);

            var buffType = buffTypes[0];
            if (buffType is -1)
                return base.PreDrawTooltip(item, lines, ref x, ref y);

            TryGetKeybindString(KeybindSystem.ItemInteractKeybind, out string keybind);
            object arg = new
            {
                BuffName = Lang.GetBuffName(buffType),
                MaxSpawn = ImproveConfigs.Instance.SpawnRateMaxValue,
                MinSpawn = Math.Min(ImproveConfigs.Instance.SpawnRateMinValue, ImproveConfigs.Instance.SpawnRateMaxValue).ToString("0.0"),
                KeybindName = keybind
            };
            if (ItemSlot.ShiftInUse)
                TagItem.DrawTagTooltips(lines, TagItem.GenerateDetailedTags(Mod, lines, arg), x, y);
        }

        return base.PreDrawTooltip(item, lines, ref x, ref y);
    }
}