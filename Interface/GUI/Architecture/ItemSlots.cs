namespace ImproveGame.Interface.GUI.Architecture;

public class WoodSlot : ArchitectureItemSlot
{
    public WoodSlot() : base(ModAsset.IconBlock.Value) {
    }

    public override Item GetItem() => Wand.Wood;

    public override void SetItem(Item item) => Wand.Wood = item;

    public override bool CheckCanPlace(Item item) =>
        item.type is ItemID.Wood;
}

public class BlockSlot : ArchitectureItemSlot
{
    public BlockSlot() : base(ModAsset.IconBlock.Value) {
    }

    public override Item GetItem() => Wand.Block;

    public override void SetItem(Item item) => Wand.Block = item;

    public override bool CheckCanPlace(Item item) =>
        item.createTile > -1 && Main.tileSolid[item.createTile] && !Main.tileSolidTop[item.createTile];
}

public class WallSlot : ArchitectureItemSlot
{
    public WallSlot() : base(ModAsset.IconWall.Value) {
    }

    public override Item GetItem() => Wand.Wall;

    public override void SetItem(Item item) => Wand.Wall = item;

    public override bool CheckCanPlace(Item item) => item.createWall > -1;
}

public class PlatformSlot : ArchitectureItemSlot
{
    public PlatformSlot() : base(ModAsset.IconPlatform.Value) {
    }

    public override Item GetItem() => Wand.Platform;

    public override void SetItem(Item item) => Wand.Platform = item;

    public override bool CheckCanPlace(Item item) =>
        item.createTile > -1 && Main.tileSolid[item.createTile] && !Main.tileSolidTop[item.createTile];
}

public class TorchSlot : ArchitectureItemSlot
{
    public TorchSlot() : base(ModAsset.IconTorch.Value) {
    }

    public override Item GetItem() => Wand.Torch;

    public override void SetItem(Item item) => Wand.Torch = item;

    public override bool CheckCanPlace(Item item) =>
        item.createTile > -1 && TileID.Sets.Torch[item.createTile];
}

public class ChairSlot : ArchitectureItemSlot
{
    public ChairSlot() : base(ModAsset.IconChair.Value) {
    }

    public override Item GetItem() => Wand.Chair;

    public override void SetItem(Item item) => Wand.Chair = item;

    public override bool CheckCanPlace(Item item) =>
        item.createTile is TileID.Chairs;
}

public class WorkbenchSlot : ArchitectureItemSlot
{
    public WorkbenchSlot() : base(ModAsset.IconWorkbench.Value) {
    }

    public override Item GetItem() => Wand.Workbench;

    public override void SetItem(Item item) => Wand.Workbench = item;

    public override bool CheckCanPlace(Item item) =>
        item.createTile is TileID.WorkBenches;
}

public class BedSlot : ArchitectureItemSlot
{
    public BedSlot() : base(ModAsset.IconBed.Value) {
    }

    public override Item GetItem() => Wand.Bed;

    public override void SetItem(Item item) => Wand.Bed = item;

    public override bool CheckCanPlace(Item item) =>
        item.createTile is TileID.Beds;
}