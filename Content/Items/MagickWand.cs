using ImproveGame.Common;
using ImproveGame.Common.Conditions;
using ImproveGame.Common.Configs;
using ImproveGame.Common.GlobalItems;
using ImproveGame.Common.ModSystems;
using ImproveGame.Packets;
using ImproveGame.UI;
using ImproveGame.UIFramework;

namespace ImproveGame.Content.Items;

/// <summary>
/// 用于批量拆除方块、墙体与箱子的工具物品。
/// </summary>
public class MagickWand : SelectorItem, IConditionItem
{
    /// <summary>
    /// 物品可用性条件，受配置开关控制。
    /// </summary>
    public Condition UseCondition => ConfigCondition.AvailableMagickWandC;

    /// <summary>
    /// 是否启用墙体拆除模式。
    /// </summary>
    public bool WallMode;

    /// <summary>
    /// 是否启用方块拆除模式。
    /// </summary>
    public bool TileMode;

    /// <summary>
    /// 是否启用箱子拆除模式。
    /// </summary>
    public bool ChestMode;

    /// <summary>
    /// 在框选模式中处理单个图格的拆除流程。
    /// </summary>
    public override bool ModifySelectedTiles(Player player, int i, int j)
    {
        // 单机直接读取系统开关；字段值主要用于随物品网络同步到其他客户端。
        if (Main.netMode is not NetmodeID.Server)
        {
            TileMode = WandSystem.TileMode;
            WallMode = WandSystem.WallMode;
            ChestMode = WandSystem.ChestMode;
        }

        if (UIConfigs.Instance.ExplosionEffect && Main.netMode is not NetmodeID.Server)
        {
            // 服务端不播放本地音效，避免重复或无意义调用。
            SoundEngine.PlaySound(SoundID.Item14, Main.MouseWorld);
            MyUtils.BongBong(new Vector2(i, j) * 16f, 16, 16);
        }

        var tile = Main.tile[i, j];
        if (tile.WallType > WallID.None && WallMode)
        {
            WorldGen.KillWall(i, j);
        }

        if (TileMode && tile.HasTile)
        {
            MyUtils.TryKillTile(i, j, player);
            CheckChestDestroy(player, i, j);
        }

        return true;
    }

    /// <summary>
    /// 若目标为可破坏箱子，则先掉落内容物再拆除箱体。
    /// </summary>
    private bool CheckChestDestroy(Player player, int i, int j)
    {
        var tile = Main.tile[i, j];
        // 先做低成本过滤，避免频繁调用 FindChest 带来额外开销。
        if (!TileID.Sets.IsAContainer[tile.TileType] || !ChestMode)
            return false;

        var origin = MyUtils.GetTileOrigin(i, j);
        int chestIndex = Chest.FindChest(origin.X, origin.Y);
        if (chestIndex == -1 || !Main.chest.IndexInRange(chestIndex))
            return false;

        var chest = Main.chest[chestIndex];
        if (Chest.IsLocked(chest.x, chest.y) || chest?.item is null)
        {
            return false;
        }

        // 先掉落内容物。
        for (int k = 0; k < chest.item.Length; k++)
            if (!chest.item[k].IsAir)
                MyUtils.SpawnTileBreakItem(i, j, chest.item[k], "ChestBrokenFromBlastsWand");
        // 再破坏箱体本身。
        MyUtils.TryKillTile(i, j, player);
        return true;
    }

    /// <summary>
    /// 批量拆除结束后同步图格改动，并统一广播音效与特效。
    /// </summary>
    public override void PostModifyTiles(Player player, int minI, int minJ, int maxI, int maxJ)
    {
        var size = new Point(maxI - minI, maxJ - minJ).Abs();
        // 额外扩一圈，覆盖边界图格与相邻联动更新。
        size.X += 2;
        size.Y += 2;
        var center = new Point(minI + size.X / 2, minJ + size.Y / 2).ToWorldCoordinates();
        var rect = new Rectangle(minI, minJ, size.X, size.Y);

        // 仅服务端负责广播图格区域更新。
        if (Main.netMode is NetmodeID.Server)
            NetMessage.SendTileSquare(-1, minI - 1, minJ - 1, size.X, size.Y);

        // 特效与音效走统一数据包，保证多人端表现一致。
        DoBoomPacket.Send(rect);
        PlaySoundPacket.SendSound(LegacySoundIDs.Item, center, style: 14);
    }

    /// <summary>
    /// 启用右键功能。
    /// </summary>
    public override bool AltFunctionUse(Player player) => true;

    /// <summary>
    /// 鼠标选点可额外延伸的交互范围。
    /// </summary>
    protected Point ExtraRange;

    /// <summary>
    /// 固定模式下一次拆除的区域尺寸。
    /// </summary>
    protected Point KillSize;

    /// <summary>
    /// 判断当前帧是否需要继续执行拆除。
    /// </summary>
    public override bool IsNeedKill()
    {
        // 固定模式由独立流程触发，不走持续按住检测。
        if (WandSystem.FixedMode)
            return false;
        return !Main.mouseLeft;
    }

    /// <summary>
    /// 设置物品基础属性及选择器行为参数。
    /// </summary>
    public override void SetItemDefaults()
    {
        Item.rare = ItemRarityID.Cyan;
        Item.value = Item.sellPrice(0, 2, 0, 0);

        MaxTilesPerFrame = 100;
        // 普通框选上限与固定模式矩形尺寸分离配置，便于平衡性能与手感。
        SelectRange = new(20, 20);
        KillSize = new(5, 3);
        ExtraRange = new(5, 3);
        // 在服务端执行拆除逻辑，避免客户端各自判定造成状态分叉。
        RunOnServer = true;
    }

    /// <summary>
    /// 开始使用时执行固定模式逻辑，并拦截右键触发。
    /// </summary>
    public override bool StartUseItem(Player player)
    {
        // 左键时允许立即执行一次固定模式拆除。
        if (player.altFunctionUse == 0)
        {
            FixedModeAction(player);
        }
        else if (player.altFunctionUse == 2)
        {
            // 右键由 CanUseItem 管理 UI 开关，这里阻止进入普通使用流程。
            return false;
        }

        return base.StartUseItem(player);
    }

    /// <summary>
    /// 根据玩家挖掘速度计算使用速度倍率。
    /// </summary>
    public override float UseSpeedMultiplier(Player player)
    {
        return 1f + (1f - player.pickSpeed);
    }

    /// <summary>
    /// 在使用动画起始帧触发固定模式动作。
    /// </summary>
    public override bool? UseItem(Player player)
    {
        if (player.ItemAnimationJustStarted)
        {
            // 动画首帧触发可避免一次挥动中重复执行固定模式。
            FixedModeAction(player);
        }

        return base.UseItem(player);
    }

    /// <summary>
    /// 固定模式下按矩形范围拆除，并同步多人可见的音效与特效。
    /// </summary>
    private void FixedModeAction(Player player)
    {
        // 仅由本地玩家触发，且必须处于固定模式。
        if (player.whoAmI == Main.myPlayer && WandSystem.FixedMode)
        {
            Rectangle rectangle = GetRectangle(player);
            // 音效通过数据包广播，保证多人环境中的听感一致。
            if (UIConfigs.Instance.ExplosionEffect)
                PlaySoundPacket.PlaySound(LegacySoundIDs.Item, Main.MouseWorld, style: 14);
            MyUtils.ForeachTile(rectangle, (x, y) =>
            {
                if (Main.tile[x, y].WallType > WallID.None && WandSystem.WallMode)
                {
                    WorldGen.KillWall(x, y);
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                        NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 2, x, y);
                }

                if (WandSystem.TileMode && Main.tile[x, y].HasTile)
                {
                    MyUtils.TryKillTile(x, y, player);
                    // 固定模式下每格读取一次当前开关，确保与 UI 最新状态一致。
                    ChestMode = WandSystem.ChestMode;
                    CheckChestDestroy(player, x, y);
                }

                if (UIConfigs.Instance.ExplosionEffect)
                    MyUtils.BongBong(new Vector2(x, y) * 16f, 16, 16);
            }, (x, y, wid, hei) => DoBoomPacket.Send(x, y, wid, hei)); // 同步爆炸特效到其他客户端。
        }
    }

    /// <summary>
    /// 固定模式下禁用选择器框选。
    /// </summary>
    public override bool CanUseSelector(Player player)
    {
        return !WandSystem.FixedMode;
    }

    /// <summary>
    /// 持有物品时，在固定模式下渲染拆除范围提示框。
    /// </summary>
    public override void HoldItem(Player player)
    {
        // 仅本地客户端绘制提示框，避免服务器端与他人角色重复创建 UI。
        if (!Main.dedServ && Main.myPlayer == player.whoAmI)
        {
            if (WandSystem.FixedMode)
            {
                GameRectangle.Create(this, () => !WandSystem.FixedMode, GetRectangle(player), Color.Red * 0.35f,
                    Color.Red);
            }

        }
    }

    /// <summary>
    /// 校验物品可用性，并处理右键切换爆破面板。
    /// </summary>
    public override bool CanUseItem(Player player)
    {
        // 建筑被禁用时直接阻止拆除行为。
        if (player.noBuilding)
            return false;

        if (player.altFunctionUse == 2)
        {
            // 右键仅切换面板，不触发物品实际使用。
            if (BurstGUI.Visible && UISystem.Instance.BurstGUI.Timer.AnyOpen)
                UISystem.Instance.BurstGUI.Close();
            else
                UISystem.Instance.BurstGUI.Open();
            return false;
        }

        return base.CanUseItem(player);
    }

    /// <summary>
    /// 计算固定模式的拆除矩形（以鼠标图格为中心并受交互距离限制）。
    /// </summary>
    protected Rectangle GetRectangle(Player player)
    {
        Rectangle rect = new();
        Point playerCenter = player.Center.ToTileCoordinates();
        Point mousePosition = Main.MouseWorld.ToTileCoordinates();
        // 先把鼠标目标裁剪到可交互范围内，再生成最终拆除矩形。
        mousePosition = MyUtils.ModifySize(playerCenter, mousePosition, Player.tileRangeX + ExtraRange.X,
            Player.tileRangeY + ExtraRange.Y);
        rect.X = mousePosition.X - KillSize.X / 2;
        rect.Y = mousePosition.Y - KillSize.Y / 2;
        rect.Width = KillSize.X;
        rect.Height = KillSize.Y;
        return rect;
    }

    /// <summary>
    /// 将三种拆除模式写入网络数据，供多人同步。
    /// </summary>
    public override void NetSend(BinaryWriter writer)
    {
        // 写包前从系统开关刷新一次，避免发送过期状态。
        WallMode = WandSystem.WallMode;
        TileMode = WandSystem.TileMode;
        ChestMode = WandSystem.ChestMode;
        writer.Write(new BitsByte(WallMode, TileMode, ChestMode));
    }

    /// <summary>
    /// 从网络数据恢复三种拆除模式状态。
    /// </summary>
    public override void NetReceive(BinaryReader reader)
    {
        var bitsByte = (BitsByte)reader.ReadByte();
        // 位压缩顺序需与 NetSend 保持一致。
        WallMode = bitsByte[0];
        TileMode = bitsByte[1];
        ChestMode = bitsByte[2];
    }

    /// <summary>
    /// 注册魔棒合成配方。
    /// </summary>
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddRecipeGroup(RecipeGroups.Wood, 18)
            .AddIngredient(ItemID.JungleSpores, 6)
            .AddIngredient(ItemID.Ruby, 1)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}