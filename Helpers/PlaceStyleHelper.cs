namespace ImproveGame.Helpers;

partial class MyUtils
{
    /// <summary>
    /// cnm啊啊啊啊啊啊啊啊啊啊啊啊啊啊ReLogic
    /// </summary>
    public static int TileFrameToPlaceStyle(int type, int frameX, int frameY)
    {
        switch (type)
        {
            case TileID.Torches: // tnnd火把搞特殊
                return frameY / 22;
            case TileID.GolfCupFlag:
            case TileID.LogicGateLamp:
            case TileID.MetalBars:
            case TileID.Bottles: // 横向排列 一格间隙 单排
                return frameX / 18;
            case TileID.LunarMonolith:
            case TileID.PottedCrystalPlants:
            case TileID.PotsSuspended:
            case TileID.FishingCrate:
            case TileID.AlphabetStatues:
            case TileID.Painting2X3:
            case TileID.WaterFountain:
            case TileID.GrandfatherClocks:
            case TileID.Bowls:
            case TileID.CookingPots:
            case TileID.Tombstones:
            case TileID.Containers:
            case TileID.Containers2:
            case TileID.FakeContainers:
            case TileID.FakeContainers2:
            case TileID.WorkBenches:
            case TileID.Anvils:
            case TileID.MythrilAnvil: // 横向排列 两格间隙 单排
                return frameX / 36;
            case TileID.MasterTrophyBase:
            case TileID.PottedLavaPlants:
            case TileID.TeleportationPylon:
            case TileID.GemSaplings:
            case TileID.Tables2:
            case TileID.Campfire:
            case TileID.Cannon:
            case TileID.AdamantiteForge: // 横向排列 三格间隙 单排
                return frameX / 54;
            case TileID.PicnicTable: // 横向排列 四格间隙 单排
                return frameX / 72;
            case TileID.WeightedPressurePlate:
            case TileID.LogicSensor:
            case TileID.LogicGate:
            case TileID.Traps:
            case TileID.PressurePlates:
            case TileID.Candles:
            case TileID.Platforms: // 竖向排列 一格间隙 单排
                return frameY / 18;
            case TileID.Painting3X2:
            case TileID.Firework:
            case TileID.Sinks:
            case TileID.Candelabras:
            case TileID.Bathtubs:
            case TileID.Beds:
            case TileID.HangingLanterns: // 竖向排列 两格间隙 单排
                return frameY / 36;
            case TileID.Toilets:
            case TileID.Chairs: // 竖向排列 两格间隙 单排 (但椅子哥儿们居然是40px切分的)
                return frameY / 40;
            // 双排特例
            case TileID.Statues: // 横向排列 两格间隙 双排 特殊的转向排列...
                if (frameY is 0 or 162)
                    return frameX / 36;
                else if (frameY is 54 or 162 + 54)
                    return frameX / 36 + 55;
                return 0;
            case TileID.Bookcases:
            case TileID.Benches:
            case TileID.Dressers:
            case TileID.Pianos: // 横向排列 三格间隙 双排
                if (frameY is 0)
                    return frameX / 54;
                else
                    return frameX / 54 + 36;
            case TileID.Tables:
                if (frameY is 0)
                    return frameX / 54;
                else
                    return frameX / 54 + 35;
            case TileID.MusicBoxes: // 竖向排列 两格间隙 双排
                if (frameX is 0 or 18 * 2)
                    return frameY / 54;
                else if (frameX is 18 * 4 or 18 * 6)
                    return frameY / 54 + 56;
                return 0;
            case TileID.Chandeliers: // 竖向排列 三格间隙 双排
                if (frameX is 0 or 18 * 3)
                    return frameY / 54;
                else if (frameX is 18 * 6 or 18 * 9)
                    return frameY / 54 + 37;
                return 0;
            case TileID.OpenDoor: // OpenDoor实际上没用，因为没有物品放置开着的门
                if (frameX is 0 or 18 * 2)
                    return frameY / 54;
                else if (frameX is 18 * 4 or 18 * 6)
                    return frameY / 54 + 36;
                return 0;
            case TileID.ClosedDoor:
                if (frameX is 0 or 18 or 18 * 2)
                    return frameY / 54;
                else if (frameX is 18 * 3 or 18 * 4 or 18 * 5)
                    return frameY / 54 + 36;
                return 0;
            case TileID.Lamps:
                if (frameX is 0 or 18 * 2)
                    return frameY / 54;
                else if (frameX is 18 * 4 or 18 * 6)
                    return frameY / 54 + 37;
                return 0;
            case TileID.Painting6X4: // 竖向排列 四格间隙 双排
                if (frameX is 0)
                    return frameY / 72;
                else if (frameX is 18 * 6)
                    return frameY / 72 + 27;
                return 0;
            // 三排!?!?!?!?
            case TileID.Banners: // 横向 一格间隔
                if (frameY is 0)
                    return frameX / 18;
                else if (frameY is 18 * 3)
                    return frameX / 18 + 111;
                else if (frameY is 18 * 6)
                    return frameX / 18 + 222;
                return 0;
            case TileID.Painting3X3: // 横向 三格间隔
                if (frameY is 0)
                    return frameX / 54;
                else if (frameY is 18 * 3)
                    return frameX / 54 + 36;
                else
                    return frameX / 54 + 72;
            case TileID.ExposedGems:
                return frameX / 18;
        }
        return 0;
    }
}