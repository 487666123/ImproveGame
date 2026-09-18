using ImproveGame.Common.Configs;
using ImproveGame.Common.ModHooks;
using ImproveGame.Content.Items;
using ImproveGame.Content.Items.IconDummies;
using ImproveGame.Core;
using ImproveGame.UI.AmmoChainPanel;
using Terraria.ModLoader.IO;

namespace ImproveGame.Content.Functions.ChainedAmmo;

public class AmmoChainGlobalItem : GlobalItem, IItemOverrideHover, IItemOverrideLeftClick
{
    public int Count;
    public int Index;
    public AmmoChain Chain = new();
    public static bool IsPickingAmmo = false;

    public override void Load()
    {
        On_Player.PickAmmo_Item_refInt32_refSingle_refBoolean_refInt32_refSingle_refInt32_bool += OnPickAmmo;
        On_Player.PickAmmo_PickAmmoItem += OnChooseAmmo;
    }

    private void OnPickAmmo(
        On_Player.orig_PickAmmo_Item_refInt32_refSingle_refBoolean_refInt32_refSingle_refInt32_bool orig, Player self,
        Item sitem, ref int projtoshoot, ref float speed, ref bool canshoot, ref int totaldamage, ref float knockback,
        out int usedammoitemid, bool dontconsume)
    {
        IsPickingAmmo = true;

        orig.Invoke(self, sitem, ref projtoshoot, ref speed, ref canshoot, ref totaldamage, ref knockback,
            out usedammoitemid, dontconsume);

        IsPickingAmmo = false;
    }

    private Item OnChooseAmmo(On_Player.orig_PickAmmo_PickAmmoItem orig, Player player, Item weapon)
    {
        if (!ImproveConfigs.Instance.AmmoChain || !IsPickingAmmo || !weapon.TryGetGlobalItem<AmmoChainGlobalItem>(out var globalItem) ||
            globalItem.Chain is null || globalItem.Chain.Chain.Count is 0)
        {
            return DoVanillaChooseAmmoPlusBigBagAmmo(orig, player, weapon);
        }

        // 超界重置
        if (!globalItem.Chain.Chain.IndexInRange(globalItem.Index))
            globalItem.ResetToZero();

        var ammoType = globalItem.Chain.Chain[globalItem.Index];

        // 至少要有能用的弹药
        int failCounter = 0;
        var bigBagItems = GetAllInventoryItemsList(player, "portable inv", 110);

        bool isNotUniversalAmmo = ammoType.ItemData.Item.type != ModContent.ItemType<UniversalAmmoIcon>();
        bool ammoDoesntFitWeapon = ammoType.ItemData.Item.ammo != weapon.useAmmo;
        bool anyAmmoInInventories = player.inventory.All(i => ammoType.ItemData.Item.type != i.type) &&
                                    bigBagItems.All(i => ammoType.ItemData.Item.type != i.type);
        while (isNotUniversalAmmo && (ammoDoesntFitWeapon || anyAmmoInInventories))
        {
            globalItem.Index++;
            Count = 0;
            if (!globalItem.Chain.Chain.IndexInRange(globalItem.Index))
            {
                failCounter++;
                globalItem.ResetToZero();

                // 两轮没找到，说明就没有适合的，直接原版行为
                if (failCounter >= 2)
                    return DoVanillaChooseAmmoPlusBigBagAmmo(orig, player, weapon);
            }

            // 更新变量
            ammoType = globalItem.Chain.Chain[globalItem.Index];
            isNotUniversalAmmo = ammoType.ItemData.Item.type != ModContent.ItemType<UniversalAmmoIcon>();
            ammoDoesntFitWeapon = ammoType.ItemData.Item.ammo != weapon.useAmmo;
            anyAmmoInInventories = player.inventory.All(i => ammoType.ItemData.Item.type != i.type) &&
                                   bigBagItems.All(i => ammoType.ItemData.Item.type != i.type);
        }

        // 不限弹药，即为沿用原版弹药
        if (ammoType.ItemData.Item.type == ModContent.ItemType<UniversalAmmoIcon>())
        {
            var foundedAmmo = DoVanillaChooseAmmoPlusBigBagAmmo(orig, player, weapon);
            // 这里不判断是否null，无论实际有没有找到弹药，都直接跳到下一个弹药
            GoToNextAmmo(globalItem);
            return foundedAmmo;
        }

        // 找到对应弹药
        Item item = null;
        bool itemFound = false;
        for (int j = 54; j < 58; j++)
        {
            var ammo = player.inventory[j];
            if (ammo.stack > 0 && ammoType.ItemData.Item.type == ammo.type &&
                ItemLoader.CanChooseAmmo(weapon, ammo, player))
            {
                item = ammo;
                itemFound = true;
                break;
            }
        }

        if (!itemFound)
        {
            for (int k = 0; k < 54; k++)
            {
                var ammo = player.inventory[k];
                if (ammo.stack > 0 && ammoType.ItemData.Item.type == ammo.type &&
                    ItemLoader.CanChooseAmmo(weapon, ammo, player))
                {
                    item = ammo;
                    itemFound = true;
                    break;
                }
            }
        }

        // 找大背包里的
        if (!itemFound)
        {
            foreach (var ammo in bigBagItems.Where(ammo =>
                         ammo.stack > 0 && ammoType.ItemData.Item.type == ammo.type &&
                         ItemLoader.CanChooseAmmo(weapon, ammo, player)))
            {
                item = ammo;
                itemFound = true;
                break;
            }
        }

        // 找到了，前进
        if (itemFound)
            GoToNextAmmo(globalItem);

        return item;
    }

    // 调用orig，但同时算入大背包的子弹
    private Item DoVanillaChooseAmmoPlusBigBagAmmo(On_Player.orig_PickAmmo_PickAmmoItem orig, Player player, Item weapon)
    {
        var item = orig.Invoke(player, weapon);
        if (item is null)
        {
            var bigBagItems = GetAllInventoryItemsList(player, "portable, inv", 110);
            foreach (var ammo in bigBagItems.Where(ammo =>
                         ammo.stack > 0 && ItemLoader.CanChooseAmmo(weapon, ammo, player)))
            {
                item = ammo;
                break;
            }
        }

        return item;
    }

    private void GoToNextAmmo(AmmoChainGlobalItem globalItem)
    {
        var ammoType = globalItem.Chain.Chain[globalItem.Index];

        globalItem.Count++;
        if (globalItem.Count >= ammoType.Times)
        {
            globalItem.Index++;
            globalItem.Count = 0;
        }
    }

    private void ResetToZero()
    {
        Index = 0;
        Count = 0;
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (Chain != null && Chain.Chain.Count > 0)
            AmmoChainItem.AddAmmoChainTooltips(Mod, Chain, tooltips);
    }

    // 在打开弹药链编辑界面时，Alt点击一个弹药可以直接将其添加到弹药链末端（作为一个附加便捷功能）
    public bool OverrideHover(Item[] inventory, int context, int slot)
    {
        if (!inventory[slot].CanBeUsedAsAmmo() || !Main.keyState.IsKeyDown(Main.FavoriteKey))
            return false;
        if (AmmoChainUI.Instance is null || AmmoChainUI.Instance.PageSlideTimer is null)
            return false;
        if (!AmmoChainUI.Instance.Enabled || !AmmoChainUI.Instance.PageSlideTimer.Opened)
            return false;

        Main.cursorOverride = CursorOverrideID.GamepadDefaultCursor;
        Main.cursorColor = Color.Lime;
        return true;
    }

    public bool OverrideLeftClick(Item[] inventory, int context, int slot)
    {
        if (!OverrideHover(inventory, context, slot))
            return false;

        AmmoChainUI.Instance.TryAddToChain(inventory[slot]);
        return true;
    }

    public override void SaveData(Item item, TagCompound tag)
    {
        tag["ammoChain"] = Chain;
    }

    public override void LoadData(Item item, TagCompound tag)
    {
        Chain = tag.Get<AmmoChain>("ammoChain") ?? new AmmoChain();
    }

    public override void NetSend(Item item, BinaryWriter writer)
    {
        Chain ??= new AmmoChain();
        writer.Write(Chain);
    }

    public override void NetReceive(Item item, BinaryReader reader)
    {
        Chain = reader.ReadAmmoChain();
    }

    public override GlobalItem Clone(Item from, Item to)
    {
        if (from.TryGetGlobalItem<AmmoChainGlobalItem>(out var globalFrom))
        {
            globalFrom.Chain = (AmmoChain)Chain?.Clone();
            return globalFrom;
        }

        return base.Clone(from, to);
    }

    public override bool AppliesToEntity(Item entity, bool lateInstantiation) => lateInstantiation && entity.useAmmo > 0;

    public override bool InstancePerEntity => true;
}