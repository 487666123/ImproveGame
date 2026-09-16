using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Reflection;
using Terraria.DataStructures;
using Terraria.WorldBuilding;

namespace ImproveGame.Helpers;

public class TileCounter : ILoadable
{
    public int CorruptionTileCount { get; set; }

    public int HallowTileCount { get; set; }

    public int SandTileCount { get; set; }

    public int MushroomTileCount { get; set; }

    public int SnowTileCount { get; set; }

    public int CrimsonTileCount { get; set; }

    public int JungleTileCount { get; set; }

    public int DungeonTileCount { get; set; }

    public int MeteorTileCount { get; set; }

    public int GraveyardTileCount { get; set; }

    public int ActiveFountainColor { get; private set; }

    public bool EnoughTilesForJungle => JungleTileCount >= SceneMetrics.JungleTileThreshold;

    public bool EnoughTilesForHallow => HallowTileCount >= SceneMetrics.HallowTileThreshold;

    public bool EnoughTilesForSnow => SnowTileCount >= SceneMetrics.SnowTileThreshold;

    public bool EnoughTilesForGlowingMushroom => MushroomTileCount >= SceneMetrics.MushroomTileThreshold;

    public bool EnoughTilesForDesert => SandTileCount >= SceneMetrics.DesertTileThreshold;

    public bool EnoughTilesForCorruption => CorruptionTileCount >= SceneMetrics.CorruptionTileThreshold;

    public bool EnoughTilesForCrimson => CrimsonTileCount >= SceneMetrics.CrimsonTileThreshold;

    public bool EnoughTilesForMeteor => MeteorTileCount >= SceneMetrics.MeteorTileThreshold;

    public bool EnoughTilesForGraveyard => GraveyardTileCount >= SceneMetrics.GraveyardTileThreshold;

    internal int[] _tileCounts = new int[TileLoader.TileCount];

    public static bool Simulating { get; private set; }

    public TileCounter()
    {
        Reset();
    }

    public void Load(Mod mod)
    {
        IL_Player.UpdateBiomes += SimulateUpdateBiomes;
    }

    private void SimulateUpdateBiomes(ILContext il)
    {
        var c = new ILCursor(il);
        if (!c.TryGotoNext(MoveType.After, i => i.MatchCallvirt<BiomeLoader>(nameof(BiomeLoader.UpdateBiomes))))
        {
            ILMatchLog(nameof(SimulateUpdateBiomes), il);
            return;
        }

        var label = c.DefineLabel(); // 记录位置
        c.Emit(OpCodes.Call,
            typeof(TileCounter).GetMethod($"get_{nameof(Simulating)}", BindingFlags.Static | BindingFlags.Public));
        c.Emit(OpCodes.Brfalse_S, label); // 为false就跳到下面
        c.Emit(OpCodes.Ret); // 为true直接return
        c.MarkLabel(label);
    }

    public void Unload() { }

    public void Simulate(Player player)
    {
        Simulating = true;

        int evilTileCount = Main.SceneMetrics.EvilTileCount;
        int holyTileCount = Main.SceneMetrics.HolyTileCount;
        int sandTileCount = Main.SceneMetrics.SandTileCount;
        int mushroomTileCount = Main.SceneMetrics.MushroomTileCount;
        int snowTileCount = Main.SceneMetrics.SnowTileCount;
        int bloodTileCount = Main.SceneMetrics.BloodTileCount;
        int jungleTileCount = Main.SceneMetrics.JungleTileCount;
        int meteorTileCount = Main.SceneMetrics.MeteorTileCount;
        int graveyardTileCount = Main.SceneMetrics.GraveyardTileCount;
        int dungeonTileCount = Main.SceneMetrics.DungeonTileCount;

        Main.SceneMetrics.EvilTileCount = CorruptionTileCount;
        Main.SceneMetrics.HolyTileCount = HallowTileCount;
        Main.SceneMetrics.SandTileCount = SandTileCount;
        Main.SceneMetrics.MushroomTileCount = MushroomTileCount;
        Main.SceneMetrics.SnowTileCount = SnowTileCount;
        Main.SceneMetrics.BloodTileCount = CrimsonTileCount;
        Main.SceneMetrics.JungleTileCount = JungleTileCount;
        Main.SceneMetrics.MeteorTileCount = MeteorTileCount;
        Main.SceneMetrics.GraveyardTileCount = GraveyardTileCount;
        Main.SceneMetrics.DungeonTileCount = DungeonTileCount;

        player.UpdateBiomes();

        Main.SceneMetrics.EvilTileCount = evilTileCount;
        Main.SceneMetrics.HolyTileCount = holyTileCount;
        Main.SceneMetrics.SandTileCount = sandTileCount;
        Main.SceneMetrics.MushroomTileCount = mushroomTileCount;
        Main.SceneMetrics.SnowTileCount = snowTileCount;
        Main.SceneMetrics.BloodTileCount = bloodTileCount;
        Main.SceneMetrics.JungleTileCount = jungleTileCount;
        Main.SceneMetrics.MeteorTileCount = meteorTileCount;
        Main.SceneMetrics.GraveyardTileCount = graveyardTileCount;
        Main.SceneMetrics.DungeonTileCount = dungeonTileCount;

        Simulating = false;
    }

    public void ScanAndExportToMain(Point16 tileCoord)
    {
        // 咱们是new完了直接用，已经Reset了
        // Reset();

        SystemLoader.ResetNearbyTileEffects();

        Rectangle tileRectangle = Utils.CenteredRectangle(tileCoord, SceneMetrics.ZoneScanSize);
        tileRectangle = WorldUtils.ClampToWorld(tileRectangle);
        for (int i = tileRectangle.Left; i < tileRectangle.Right; i++)
        {
            for (int j = tileRectangle.Top; j < tileRectangle.Bottom; j++)
            {
                if (!tileRectangle.Contains(i, j))
                    continue;

                Tile tile = Main.tile[i, j];
                if (tile == null || !tile.HasTile)
                    continue;

                tileRectangle.Contains(i, j);
                if (!TileID.Sets.isDesertBiomeSand[tile.TileType] || !WorldGen.oceanDepths(i, j))
                    _tileCounts[tile.TileType]++;

                if (tile.TileType is TileID.WaterFountain)
                {
                    if (tile.TileFrameY >= 72)
                    {
                        // 原版代码
                        ActiveFountainColor = (tile.TileFrameX / 36) switch
                        {
                            0 => 0,
                            1 => 12,
                            2 => 3,
                            3 => 5,
                            4 => 2,
                            5 => 10,
                            6 => 4,
                            7 => 9,
                            8 => 8,
                            9 => 6,
                            _ => -1,
                        };
                    }

                    break;
                }
            }
        }

        ExportTileCountsToMain();
        SystemLoader.TileCountsAvailable(_tileCounts);
    }

    /// <summary>
    /// 将Counts应用到各大环境里面
    /// </summary>
    private void ExportTileCountsToMain()
    {
        HallowTileCount = _tileCounts[109] + _tileCounts[492] + _tileCounts[110] + _tileCounts[113] +
                          _tileCounts[117] + _tileCounts[116] + _tileCounts[164] + _tileCounts[403] +
                          _tileCounts[402];
        CorruptionTileCount = _tileCounts[23] + _tileCounts[24] + _tileCounts[25] + _tileCounts[32] +
                              _tileCounts[112] + _tileCounts[163] + _tileCounts[400] + _tileCounts[398] +
                              -10 * _tileCounts[27];
        CrimsonTileCount = _tileCounts[199] + _tileCounts[203] + _tileCounts[200] + _tileCounts[401] +
            _tileCounts[399] + _tileCounts[234] + _tileCounts[352] - 10 * _tileCounts[27];
        SnowTileCount = _tileCounts[147] + _tileCounts[148] + _tileCounts[161] + _tileCounts[162] +
                        _tileCounts[164] + _tileCounts[163] + _tileCounts[200];
        JungleTileCount = _tileCounts[60] + _tileCounts[61] + _tileCounts[62] + _tileCounts[74] + _tileCounts[226] +
                          _tileCounts[225];
        MushroomTileCount = _tileCounts[70] + _tileCounts[71] + _tileCounts[72] + _tileCounts[528];
        MeteorTileCount = _tileCounts[37];
        DungeonTileCount = _tileCounts[41] + _tileCounts[43] + _tileCounts[44] + _tileCounts[481] +
                           _tileCounts[482] + _tileCounts[483];
        SandTileCount = _tileCounts[53] + _tileCounts[112] + _tileCounts[116] + _tileCounts[234] +
                        _tileCounts[397] + _tileCounts[398] + _tileCounts[402] + _tileCounts[399] +
                        _tileCounts[396] + _tileCounts[400] + _tileCounts[403] + _tileCounts[401];
        GraveyardTileCount = _tileCounts[85];
        GraveyardTileCount -= _tileCounts[27] / 2;

        if (GraveyardTileCount < 0)
            GraveyardTileCount = 0;

        if (HallowTileCount < 0)
            HallowTileCount = 0;

        if (CorruptionTileCount < 0)
            CorruptionTileCount = 0;

        if (CrimsonTileCount < 0)
            CrimsonTileCount = 0;

        int hallowTileCount = HallowTileCount;
        HallowTileCount -= CorruptionTileCount;
        HallowTileCount -= CrimsonTileCount;
        CorruptionTileCount -= hallowTileCount;
        CrimsonTileCount -= hallowTileCount;
        if (HallowTileCount < 0)
            HallowTileCount = 0;

        if (CorruptionTileCount < 0)
            CorruptionTileCount = 0;

        if (CrimsonTileCount < 0)
            CrimsonTileCount = 0;
    }

    public void FargosFountainSupport(Player player)
    {
        int activeFountainColor = Main.SceneMetrics.ActiveFountainColor;
        Main.SceneMetrics.ActiveFountainColor = ActiveFountainColor;

        // 暂时用作Fargo喷泉的适配，有问题再改回来
        try
        {
            PlayerLoader.PostUpdateMiscEffects(player);
        }
        catch (Exception e)
        {
            // ignored
        }

        Main.SceneMetrics.ActiveFountainColor = activeFountainColor;
    }

    public int GetTileCount(ushort tileId) => _tileCounts[tileId];

    public void Reset()
    {
        Array.Clear(_tileCounts, 0, _tileCounts.Length);
        SandTileCount = 0;
        CorruptionTileCount = 0;
        CrimsonTileCount = 0;
        GraveyardTileCount = 0;
        MushroomTileCount = 0;
        SnowTileCount = 0;
        HallowTileCount = 0;
        MeteorTileCount = 0;
        JungleTileCount = 0;
        DungeonTileCount = 0;
        ActiveFountainColor = -1;
        SystemLoader.TileCountsAvailable(_tileCounts);
    }
}