using ImproveGame.Common.Configs;
using Terraria.ModLoader.IO;

namespace ImproveGame.UI.AutoTrash;

public class AutoTrashPlayer : ModPlayer
{
    #region 设置 Instance
    public static AutoTrashPlayer Instance { get; private set; }
    public override void OnEnterWorld() => Instance = this;
    public override void Unload() => Instance = null;
    #endregion

    #region 基本属性
    public const int MaxCapacity = 6;

    /// <summary>
    /// 已扔的物品
    /// </summary>
    public List<Item> ThrowAwayItems { get; init; } = [];

    /// <summary>
    /// 最近丢弃的的 <see cref="MaxCapacity"/> 件物品
    /// </summary>
    public List<Item> RecentlyThrownAwayItems { get; init; } = [with(MaxCapacity)];
    #endregion

    public override bool ShiftClickSlot(Item[] inventory, int context, int slot)
    {
        Item item = inventory[slot];

        if (UIConfigs.Instance.QoLAutoTrash && !item.IsAir &&
            context is ItemSlot.Context.InventoryItem or ItemSlot.Context.InventoryCoin or ItemSlot.Context.InventoryAmmo &&
            Main.keyState.PressingControl())
        {
            EnterThrowAwayItems(item);
            EnterRecentlyThrownAwayItems(item);
            item.TurnToAir();
            SoundEngine.PlaySound(SoundID.Grab);
        }

        return false;
    }

    /// <summary>
    /// 清理 <see cref="RecentlyThrownAwayItems"/> <see cref="ThrowAwayItems"/> 内对应的物品
    /// </summary>
    /// <param name="item"></param>
    public void RemoveItem(Item item)
    {
        ThrowAwayItems.RemoveAll(i => i.type == item.type);
        RecentlyThrownAwayItems.RemoveAll(i => i.type == item.type);
    }

    /// <summary>
    /// 添加到丢弃列表, 不会修改输入 <see cref="Item"/>
    /// </summary>
    public void EnterThrowAwayItems(Item thrownAwayItem)
    {
        if (thrownAwayItem.IsAir)
            return;

        if (ThrowAwayItems.All(i => i.type != thrownAwayItem.type))
        {
            ThrowAwayItems.Add(thrownAwayItem.Clone());
        }
    }

    /// <summary>
    /// 添加到最近丢弃的物品, 不会修改输入 <see cref="Item"/>
    /// </summary>
    public void EnterRecentlyThrownAwayItems(Item thrownAwayItem)
    {
        if (thrownAwayItem.IsAir)
            return;

        int index = RecentlyThrownAwayItems.FindIndex(i => i.type == thrownAwayItem.type);
        if (index >= 0)
        {
            Item item = RecentlyThrownAwayItems[index];
            item.stack = Math.Min(item.stack + thrownAwayItem.stack, item.maxStack);

            // 放到首位
            if (index > 0)
            {
                RecentlyThrownAwayItems.RemoveAt(index);
                RecentlyThrownAwayItems.Insert(0, item);
            }
        }
        else
        {
            RecentlyThrownAwayItems.Insert(0, thrownAwayItem.Clone());
        }

        CleanUpRecentlyThrownAwayItems();
    }

    /// <summary>
    /// 清理 <see cref="Item.IsAir"/> 和溢出的物品
    /// </summary>
    public void CleanUpRecentlyThrownAwayItems()
    {
        // 清理空物品
        for (int i = 0; i < RecentlyThrownAwayItems.Count; i++)
        {
            if (RecentlyThrownAwayItems[i] is null || RecentlyThrownAwayItems[i].IsAir)
            {
                RecentlyThrownAwayItems.RemoveAt(i--);
            }
        }

        // 清理超过 MaxCapacity 限制的物品
        if (RecentlyThrownAwayItems.Count > MaxCapacity)
        {
            RecentlyThrownAwayItems.RemoveRange(MaxCapacity, RecentlyThrownAwayItems.Count - MaxCapacity);
        }
    }

    public override void SaveData(TagCompound tag)
    {
        tag[nameof(ThrowAwayItems)] = ThrowAwayItems;
        tag[nameof(RecentlyThrownAwayItems)] = RecentlyThrownAwayItems;
    }

    public override void LoadData(TagCompound tag)
    {
        if (tag.TryGet(nameof(ThrowAwayItems), out List<Item> throwAwayItems))
        {
            ThrowAwayItems.Clear();
            ThrowAwayItems.AddRange(throwAwayItems);
        }

        if (tag.TryGet(nameof(RecentlyThrownAwayItems), out List<Item> recentlyThrownAwayItems))
        {
            RecentlyThrownAwayItems.Clear();
            RecentlyThrownAwayItems.AddRange(recentlyThrownAwayItems);
        }
    }
}
