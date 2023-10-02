using Terraria.ModLoader.IO;

namespace ImproveGame.Interface.GUI.OpenBag;

public class LootKeeper : ModPlayer
{
    public Item Bag;
    public List<Item> Loots;

    public override void SaveData(TagCompound tag)
    {
        tag["bag"] = Bag;
        tag["loot"] = Loots;
    }

    public override void LoadData(TagCompound tag)
    {
        Bag = tag.Get<Item>("bag") ?? new Item();
        Loots = tag.Get<List<Item>>("loot") ?? new List<Item>();
    }

    public override void PostUpdate()
    {
        base.PostUpdate();
    }
}