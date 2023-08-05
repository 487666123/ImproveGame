﻿namespace ImproveGame.Core;

public class CoroutineSystem : ModSystem
{
    internal static CoroutineRunner TileRunner = new();
    internal static CoroutineRunner GenerateRunner = new();
    internal static CoroutineRunner MiscRunner = new();

    public override void PreUpdateTime()
    {
        TileRunner.Update(1);
        GenerateRunner.Update(1);
        MiscRunner.Update(1);
    }
}