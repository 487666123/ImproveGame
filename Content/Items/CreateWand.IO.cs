using Terraria.ModLoader.IO;

namespace ImproveGame.Content.Items;

public partial class CreateWand
{
    /*
    [CloneByReference] public Item Block = new Item();
    [CloneByReference] public Item Platform = new Item();
    [CloneByReference] public Item Workbench = new Item();
    [CloneByReference] public Item Table = new Item();
    [CloneByReference] public Item Chair = new Item();
    [CloneByReference] public Item Door = new Item();
    [CloneByReference] public Item Chest = new Item();
    [CloneByReference] public Item Bed = new Item();
    [CloneByReference] public Item Bookcase = new Item();
    [CloneByReference] public Item Bathtub = new Item();
    [CloneByReference] public Item Candelabra = new Item();
    [CloneByReference] public Item Candle = new Item();
    [CloneByReference] public Item Chandelier = new Item();
    [CloneByReference] public Item Clock = new Item();
    [CloneByReference] public Item Dresser = new Item();
    [CloneByReference] public Item Lamp = new Item();
    [CloneByReference] public Item Lantern = new Item();
    [CloneByReference] public Item Piano = new Item();
    [CloneByReference] public Item Sink = new Item();
    [CloneByReference] public Item Sofa = new Item();
    [CloneByReference] public Item Toilet = new Item();
    [CloneByReference] public Item Torch = new Item();
    [CloneByReference] public Item Campfire = new Item();
    [CloneByReference] public Item Wall = new Item(); 
    */
    private void HandleLegacyData(TagCompound tag)
    {
        if (tag.TryGet("Block", out Item block))
            BuildingMaterials[0] = block;
        if (tag.TryGet("Platform", out Item platform))
            BuildingMaterials[1] = platform;
        if (tag.TryGet("Workbench", out Item workbench))
            BuildingMaterials[2] = workbench;
        if (tag.TryGet("Table", out Item table))
            BuildingMaterials[3] = table;
        if (tag.TryGet("Chair", out Item chair))
            BuildingMaterials[4] = chair;
        if (tag.TryGet("Door", out Item door))
            BuildingMaterials[5] = door;
        if (tag.TryGet("Chest", out Item chest))
            BuildingMaterials[6] = chest;
        if (tag.TryGet("Bed", out Item bed))
            BuildingMaterials[7] = bed;
        if (tag.TryGet("Bookcase", out Item bookcase))
            BuildingMaterials[8] = bookcase;
        if (tag.TryGet("Bathtub", out Item bathtub))
            BuildingMaterials[9] = bathtub;
        if (tag.TryGet("Candelabra", out Item candelabra))
            BuildingMaterials[10] = candelabra;
        if (tag.TryGet("Candle", out Item candle))
            BuildingMaterials[11] = candle;
        if (tag.TryGet("Chandelier", out Item chandelier))
            BuildingMaterials[12] = chandelier;
        if (tag.TryGet("Clock", out Item clock))
            BuildingMaterials[13] = clock;
        if (tag.TryGet("Dresser", out Item dresser))
            BuildingMaterials[14] = dresser;
        if (tag.TryGet("Lamp", out Item lamp))
            BuildingMaterials[15] = lamp;
        if (tag.TryGet("Lantern", out Item lantern))
            BuildingMaterials[16] = lantern;
        if (tag.TryGet("Piano", out Item piano))
            BuildingMaterials[17] = piano;
        if (tag.TryGet("Sink", out Item sink))
            BuildingMaterials[18] = sink;
        if (tag.TryGet("Sofa", out Item sofa))
            BuildingMaterials[19] = sofa;
        if (tag.TryGet("Toilet", out Item toliet))
            BuildingMaterials[20] = toliet;
        if (tag.TryGet("Torch", out Item torch))
            BuildingMaterials[21] = torch;
        if (tag.TryGet("Campfire", out Item campfire))
            BuildingMaterials[22] = campfire;
        if (tag.TryGet("Wall", out Item wall))
            BuildingMaterials[23] = wall;
    }

    [CloneByReference]
    public readonly Item[] BuildingMaterials = new Item[30];

    public CreateWand()
    {
        for (int n = 0; n < BuildingMaterials.Length; n++)
            BuildingMaterials[n] = new();
    }

    public override void SaveData(TagCompound tag)
    {
        for (int n = 0; n < BuildingMaterials.Length; n++)
            tag[$"m_{n}"] = BuildingMaterials[n];
    }

    public override void LoadData(TagCompound tag)
    {
        //HandleLegacyData(tag);
        for (int n = 0; n < BuildingMaterials.Length; n++)
        {
            if (tag.TryGet($"m_{n}", out Item item))
                BuildingMaterials[n] = item;
        }
    }

    public override void NetSend(BinaryWriter writer)
    {
        for (int n = 0; n < BuildingMaterials.Length; n++)
            ItemIO.Send(BuildingMaterials[n], writer, true, true);
    }

    public override void NetReceive(BinaryReader reader)
    {
        for (int n = 0; n < BuildingMaterials.Length; n++)
            BuildingMaterials[n] = ItemIO.Receive(reader, true, true);
    }
}
