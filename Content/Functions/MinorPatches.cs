using ImproveGame.Common.Configs;
using ImproveGame.Common.GlobalNPCs;
using ImproveGame.Common.GlobalProjectiles;
using ImproveGame.Content.Items;
using ImproveGame.Packets;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Reflection;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Drawing;
using Terraria.Graphics;
using Terraria.Graphics.Light;
using Terraria.Graphics.Renderers;

namespace ImproveGame.Content.Functions;

/// <summary>
/// 为了方便管理，这里主要放一些不成体系的小修改，比如一些单独的On, IL
/// </summary>
public class MinorPatches : ModSystem
{
    #region 类

    private class ShakeTreeTweak
    {
        private class ShakeTreeItem : GlobalItem
        {
            public override void OnSpawn(WorldItem item, IEntitySource source)
            {
                if (_isShakingTree && source is EntitySource_ShakeTree)
                    _hasItemDropped = true;
            }
        }

        private static bool _isShakingTree;
        private static bool _hasItemDropped; // 检测是否在摇树过程中有物品掉落

        public static void Load()
        {
            // 暂时没空写，不要动 2025.9.6
            // IL_WorldGen.ShakeTree += il =>
            // {
            //     var c = new ILCursor(il);

            //     if (!c.TryGotoNext(MoveType.Before, i => i.MatchLdsfld<NPCLoader>(""))) return;
            // };

            // 暂时保持即可，因为后续会被 IL 替换
            On_WorldGen.ShakeTree += (orig, i, j) =>
            {
                if (!ImproveConfigs.Instance.ShakeTreeFruit)
                {
                    orig(i, j);
                    return;
                }

                _isShakingTree = true;
                _hasItemDropped = false;

                // 在orig前获取树是否被摇过，因为orig会修改WorldGen.treeShakeX,Y的值，标记为被摇过
                bool treeShaken = false;

                WorldGen.GetTreeBottom(i, j, out var x, out var y);
                for (int k = 0; k < WorldGen.numTreeShakes; k++)
                {
                    if (WorldGen.treeShakeX[k] == x && WorldGen.treeShakeY[k] == y)
                    {
                        treeShaken = true;
                        break;
                    }
                }

                orig(i, j);

                _isShakingTree = false;

                if (WorldGen.numTreeShakes == WorldGen.maxTreeShakes || _hasItemDropped || treeShaken)
                    return;

                TreeTypes treeType = WorldGen.GetTreeType(Main.tile[x, y].TileType);
                if (treeType == TreeTypes.None)
                    return;

                y--;
                while (y > 10 && Main.tile[x, y].HasTile && TileID.Sets.IsShakeable[Main.tile[x, y].TileType])
                {
                    y--;
                }

                y++;
                if (!WorldGen.IsTileALeafyTreeTop(x, y) || Collision.SolidTiles(x - 2, x + 2, y - 2, y + 2))
                    return;

                int fruit = CollectHelper.GetShakeTreeFruit(treeType);
                if (fruit > -1)
                    Item.NewItem(WorldGen.GetItemSource_FromTreeShake(x, y), x * 16, y * 16, 16, 16, fruit);
            };
        }
    }

    private class HairstylesUnlock
    {
        private static bool _rebuilt;

        public static void Load()
        {
            On_HairstyleUnlocksHelper.ListWarrantsRemake += RebuildPatch;
            On_HairstyleUnlocksHelper.RebuildList += UnlockPatch;
        }

        private static bool RebuildPatch(On_HairstyleUnlocksHelper.orig_ListWarrantsRemake orig,
            HairstyleUnlocksHelper self)
        {
            if (!_rebuilt)
            {
                _rebuilt = true;
                return true;
            }

            return false;
        }

        private static void UnlockPatch(On_HairstyleUnlocksHelper.orig_RebuildList orig,
            HairstyleUnlocksHelper self)
        {
            self.AvailableHairstyles.Clear();
            for (int i = 0; i < TextureAssets.PlayerHair.Length; i++)
            {
                self.AvailableHairstyles.Add(i);
            }
        }
    }

    private class TranslucentInvisPotion : ModPlayer
    {
        private bool _actuallyInvis = false;

        public override void Load()
        {
            // 隐身药水半透明
            On_LegacyPlayerRenderer.DrawPlayerFull += PatchDrawPlayerFull;
        }

        private void PatchDrawPlayerFull(On_LegacyPlayerRenderer.orig_DrawPlayerFull orig,
            LegacyPlayerRenderer self, Camera camera, Player drawPlayer)
        {
            if (drawPlayer.whoAmI != Main.myPlayer || !drawPlayer.invis ||
                UIConfigs.Instance.InvisibleTransparency is 0f ||
                !drawPlayer.TryGetModPlayer<TranslucentInvisPotion>(out var modPlayer))
            {
                orig.Invoke(self, camera, drawPlayer);
                return;
            }

            drawPlayer.invis = false;
            modPlayer._actuallyInvis = true;

            orig.Invoke(self, camera, drawPlayer);

            drawPlayer.invis = true;
            modPlayer._actuallyInvis = false;
        }

        public override void DrawEffects(PlayerDrawSet drawPlayer, ref float r, ref float g, ref float b,
            ref float a, ref bool fullBright)
        {
            if (Player.invis || !_actuallyInvis) return;

            var opacity = UIConfigs.Instance.InvisibleTransparency;
            r *= opacity;
            g *= opacity;
            b *= opacity;
            a *= opacity;
        }
    }

    private class NoPylonRestrictions : GlobalPylon
    {
        // 这个无视的条件有点多，所以不用这个重写函数
        /*public override void PostValidTeleportCheck(TeleportPylonInfo destinationPylonInfo, TeleportPylonInfo nearbyPylonInfo,
            ref bool destinationPylonValid, ref bool validNearbyPylonFound, ref string errorKey)
        {
            destinationPylonValid |= Config.NoPylonRestrictions;
            // 下面这条会允许任意地点传送到任意地点，暂不考虑加入
            // validNearbyPylonFound |= Config.NoPylonRestrictions;
        }*/
        /*public override bool? ValidTeleportCheck_PreAnyDanger(TeleportPylonInfo pylonInfo)
        {
            if (Config.NoPylonRestrictions)
                return true; // 无视原版的威胁检测
            return null;
        }
        public override bool? ValidTeleportCheck_PreNPCCount(TeleportPylonInfo pylonInfo, ref int defaultNecessaryNPCCount)
        {
            if (Config.NoPylonRestrictions)
                return true; // 无视原版的npc数量检测
            return null;
        }*/
    }

    // 使存钱罐中物品生效，如同放入背包一样
    private sealed class InfoAccessoryHelper : ModPlayer
    {
        public override void UpdateEquips()
        {
            if (Main.myPlayer != Player.whoAmI)
                return;

            var items = GetAllInventoryItemsList(Player, "inv void");
            foreach (var item in items)
            {
                if (item.type != ItemID.EncumberingStone)
                {
                    ItemLoader.UpdateInventory(item, Player);

                    Player.RefreshInfoAccsFromItemType(item);
                    Player.RefreshMechanicalAccsFromItemType(item.type);
                }
            }
        }
    }

    #endregion

    public override void ModifyTimeRate(ref double timeRate, ref double tileUpdateRate, ref double eventUpdateRate)
    {
        if (Main.IsFastForwardingTime()) return;

        bool allPlayersSleeping =
            Main.CurrentFrameFlags.SleepingPlayersCount == Main.CurrentFrameFlags.ActivePlayersCount &&
            Main.CurrentFrameFlags.SleepingPlayersCount > 0;
        allPlayersSleeping |= ImproveConfigs.Instance.BedOnlyOne && Main.CurrentFrameFlags.SleepingPlayersCount > 0;

        if (!Main.gameMenu && allPlayersSleeping)
        {
            timeRate *= ImproveConfigs.Instance.BedTimeRate / 5f;
            tileUpdateRate *= ImproveConfigs.Instance.BedTimeRate / 5f;
            eventUpdateRate *= ImproveConfigs.Instance.BedTimeRate / 5f;
        }
    }

    public override void PostUpdateEverything()
    {
        if (Main.anglerQuestFinished || Main.anglerWhoFinishedToday.Count > 0)
        {
            switch (ImproveConfigs.Instance.NoCD_FishermanQuest)
            {
                case FishQuestResetType.NotResetFish:
                    Main.anglerQuestFinished = false;
                    Main.anglerWhoFinishedToday.Clear();
                    AddNotification(GetText("Tips.AnglerQuest"), Color.Cyan);
                    break;
                case FishQuestResetType.ResetFish:
                    Main.AnglerQuestSwap();
                    AddNotification(GetText("Tips.AnglerQuest"), Color.Cyan);
                    break;
            }
        }
    }

    public override void Load()
    {
        // 死亡是否掉落墓碑
        On_Player.DropTombstone += DisableDropTombstone;
        // 修改空间法杖显示平台剩余数量
        IL_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += TweakDrawCountInventory;
        // 伤害波动
        On_Main.DamageVar_float_int_float += DisableDamageVar;
        // 摇树总是掉落水果
        ShakeTreeTweak.Load();
        // “草药” 生长速度
        IL_WorldGen.GrowAlch += WorldGen_GrowAlch;

        //// “草药” 是否可以被 “再生法杖” 收割
        //IL_Player.PlaceThing_Tiles_BlockPlacementForAssortedThings +=
        //    Player_PlaceThing_Tiles_BlockPlacementForAssortedThings;

        // “草药” 永久开花
        On_WorldGen.IsAlchemyPlantHarvestable += AlwaysBloomModify;

        // “草药” 是否掉落成熟时候物品
        On_WorldGen.IsHarvestableHerbWithSeed += WorldGen_IsHarvestableHerbWithSeed;
        // “南瓜”生长速度
        On_WorldGen.GrowPumpkin += On_WorldGenOnGrowPumpkin;
        // “生命果”生长解限and生长速度
        IL_WorldGen.UpdateWorld_GrassGrowth += IL_WorldGen_UpdateWorld_GrassGrowth;
        // 旅商永远不离开
        On_WorldGen.UnspawnTravelNPC += TravelNPCStay;
        // 修改旗帜需求
        On_NPC.CountKillForBannersAndDropThem += NPC_CountKillForBannersAndDropThem;
        // 熔岩史莱姆不生成熔岩
        IL_NPC.VanillaHitEffect += LavalessLavaSlime;
        On_NPC.VanillaHitEffect += LavalessGFB;
        // 死后保存Buff
        IL_Player.UpdateDead += KeepBuffOnUpdateDead;
        // 禁止腐化蔓延
        IL_WorldGen.UpdateWorld_Inner += DisableBiomeSpread;
        // NPC住在腐化
        IL_WorldGen.ScoreRoom += LiveInCorrupt;
        // 发型解锁
        HairstylesUnlock.Load();
        // 移除Social和Favorite提示
        IL_Main.MouseText_DrawItemTooltip_GetLinesInfo += il =>
        {
            var c = new ILCursor(il);

            QuickModify(nameof(Item.favorited));
            QuickModify(nameof(Item.social));
            return;

            void QuickModify(string name)
            {
                if (!c.TryGotoNext(
                        MoveType.After,
                        i => i.Match(OpCodes.Ldarg_0),
                        i => i.MatchLdfld<Item>(name)))
                {
                    ILMatchLog(nameof(IL_Main.MouseText_DrawItemTooltip_GetLinesInfo), il);
                    return;
                }
                c.Emit(OpCodes.Pop);
                c.Emit(OpCodes.Ldc_I4_0);
            }
        };
        // 大背包内弹药可直接被使用
        On_Player.ChooseAmmo += (orig, player, weapon) =>
            orig.Invoke(player, weapon) ??
            GetAllInventoryItemsList(player, "inv portable")
                .FirstOrDefault(i => i.stack > 0 && ItemLoader.CanChooseAmmo(weapon, i, player), null);
        // 大背包内弹药在UI的数值显示
        IL_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += il =>
        {
            var c = new ILCursor(il);
            if (!c.TryGotoNext(MoveType.After,
                    i => i.Match(OpCodes.Ldloc_1),
                    i => i.MatchLdfld<Item>(nameof(Item.useAmmo)),
                    i => i.Match(OpCodes.Ldc_I4_0),
                    i => i.Match(OpCodes.Ble_S),
                    i => i.Match(OpCodes.Ldloc_1),
                    i => i.MatchLdfld<Item>(nameof(Item.useAmmo)),
                    i => i.Match(OpCodes.Pop),
                    i => i.Match(OpCodes.Ldc_I4_0)))
            {
                ILMatchLog(nameof(IL_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color), il);
                return;
            }
            c.Emit(OpCodes.Ldloc_1); // 将weapon读入
            c.EmitDelegate<Func<int, Item, int>>((_, weapon) =>
            {
                ItemCount(GetAllInventoryItemsList(Main.LocalPlayer, "inv portable").ToArray(),
                    i => i.stack > 0 && ItemLoader.CanChooseAmmo(weapon, i, Main.LocalPlayer), out int count);
                return count;
            });
        };
        // 池子无渔力大小惩罚
        On_Projectile.GetFishingPondState += (On_Projectile.orig_GetFishingPondState orig, int x, int y,
            out bool lava, out bool honey, out int numWaters, out int chumCount) =>
        {
            orig.Invoke(x, y, out lava, out honey, out numWaters, out chumCount);
            if (ImproveConfigs.Instance.NoLakeSizePenalty)
                numWaters = 114514;
        };
        // 失焦运行
        /*
        IL_Main.DoUpdate += il =>
        {
            try
            {
                // // hasFocus = ((Game)this).IsActive;
                // IL_07db: ldarg.0
                // IL_07dc: call instance bool [FNA]Microsoft.Xna.Framework.Game::get_IsActive()
                // IL_07e1: stsfld bool Terraria.Main::hasFocus
                // // if (!hasFocus && netMode == 0)
                // IL_07e6: ldsfld bool Terraria.Main::hasFocus
                // IL_07eb: brtrue.s IL_0854
                // IL_07ed: ldsfld int32 Terraria.Main::netMode
                // IL_07f2: brtrue.s IL_0854
                var c = new ILCursor(il);
                if (!c.TryGotoNext(MoveType.After,
                        i => i.Match(OpCodes.Ldarg_0),
                        i => i.MatchCall(typeof(Game), $"get_{nameof(Game.IsActive)}"),
                        i => i.MatchStsfld(typeof(Main), nameof(Main.hasFocus)),
                        i => i.MatchLdsfld(typeof(Main), nameof(Main.hasFocus)),
                        i => i.Match(OpCodes.Brtrue_S),
                        i => i.MatchLdsfld(typeof(Main), nameof(Main.netMode))))
                    return;

                c.EmitDelegate<Func<int, int>>(returnValue =>
                {
                    if (UIConfigs.Instance.KeepFocus && returnValue is NetmodeID.SinglePlayer)
                        return 3;
                    return returnValue;
                });
            }
            catch
            {
                MonoModHooks.DumpIL(Mod, il);
            }
        };
        */
        // 专家/大师延长Debuff
        On_Player.AddBuff_DetermineBuffTimeToAdd += (orig, self, type, time1) => ImproveConfigs.Instance.LongerExpertDebuff ? orig.Invoke(self, type, time1) : time1;
        // 床随地设置重生点
        On_Player.CheckSpawn += BetterCheckSpawn;
        // 固定 NPC 快乐度为指定数值
        IL_ShopHelper.GetShoppingSettings += ModifyNPCHappiness;
        // 狱火圈半透明
        IL_Main.DrawInfernoRings += TranslucentInfernoRings;
        // 任务鱼可堆叠
        On_Projectile.FishingCheck_ProbeForQuestFish += PatchQuestFishCheck;
        // 是否无视睡觉限制
        On_PlayerSleepingHelper.DoesPlayerHaveReasonToActUpInBed += NoSleepRestrictions;
        // 光照是否无视物块传播
        On_TileLightScanner.LightIsBlocked += LightNotBlocked;
        // 无条件队内传送
        On_Player.HasUnityPotion += (orig, self) => ImproveConfigs.Instance.NoConditionTP || orig(self);
        // 无条件队内传送-虫洞药水无消耗
        On_Player.TakeUnityPotion += NotTakeUnityPotion;
        // 禁止非玩家爆炸物破坏物块
        On_Projectile.ExplodeTiles += NoExplodeTiles;
        // 墓地特效
        On_SceneState.UpdateGraveyard += GraveyardVisualRemoval;
        // 墓地音乐
        On_Main.UpdateAudio += GraveyardMusicRemoval;
        // 墓地迷雾
        On_AmbientWindSystem.Update += GraveyardMistRemoval;
    }

    private bool AlwaysBloomModify(On_WorldGen.orig_IsAlchemyPlantHarvestable orig, int style, int y)
    {
        return ImproveConfigs.Instance.AlchemyGrassAlwaysBlooms || orig.Invoke(style, y);
    }

    private void IL_WorldGen_UpdateWorld_GrassGrowth(ILContext il)
    {
        var c = new ILCursor(il);

        if (c.TryGotoNext(MoveType.Before, i => i.MatchStloc(14)))
        {
            c.EmitDelegate<Func<int, int>>(maxValue2 =>
            {
                return (ImproveConfigs.Instance.LifeFruitGrowsFaster ? 1 : maxValue2);
            });
        }
        else
        {
            ILMatchLog(nameof(IL_WorldGen_UpdateWorld_GrassGrowth), il);
        }

        if (c.TryGotoNext(MoveType.Before, i => i.MatchStloc(20)))
        {
            c.EmitDelegate<Func<int, int>>((num7) =>
            {
                int limit = ImproveConfigs.Instance.LifeFruitLimit;

                if (limit == -1)
                    return num7;

                if (Main.expertMode)
                    limit = Math.Max(limit, 11);
                else
                    limit = Math.Max(limit, 1);

                return limit;
            });
        }
        else
        {
            ILMatchLog(nameof(IL_WorldGen_UpdateWorld_GrassGrowth), il);
        }
    }

    private void On_WorldGenOnGrowPumpkin(On_WorldGen.orig_GrowPumpkin orig, int i, int j, int type)
    {
        orig.Invoke(i, j, type);
        if (!ImproveConfigs.Instance.PumpkinGrowsFaster)
            return;
        orig.Invoke(i, j, type);
        orig.Invoke(i, j, type);
        orig.Invoke(i, j, type);
        orig.Invoke(i, j, type);
        orig.Invoke(i, j, type);
    }

    private void GraveyardMistRemoval(On_AmbientWindSystem.orig_Update orig, AmbientWindSystem self)
    {
        if (UIConfigs.Instance.RemoveGraveyardVisual && UIConfigs.Instance.RemoveGraveyardMist)
        {
            bool inGraveyard = Main.LocalPlayer.ZoneGraveyard;
            Main.LocalPlayer.ZoneGraveyard = false;
            orig.Invoke(self);
            Main.LocalPlayer.ZoneGraveyard = inGraveyard;
            return;
        }

        orig.Invoke(self);
    }

    private void GraveyardMusicRemoval(On_Main.orig_UpdateAudio orig, Main self)
    {
        if (UIConfigs.Instance.RemoveGraveyardVisual && UIConfigs.Instance.RemoveGraveyardMusic)
        {
            bool inGraveyard = Main.LocalPlayer.ZoneGraveyard;
            Main.LocalPlayer.ZoneGraveyard = false;
            orig.Invoke(self);
            Main.LocalPlayer.ZoneGraveyard = inGraveyard;
            return;
        }

        orig.Invoke(self);
    }

    private static void GraveyardVisualRemoval(On_SceneState.orig_UpdateGraveyard orig, SceneState self, SceneMetrics metrics)
    {
        orig.Invoke(self, metrics);

        if (UIConfigs.Instance.RemoveGraveyardVisual)
            Main.GraveyardVisualIntensity = 0f;
    }

    private void NoExplodeTiles(On_Projectile.orig_ExplodeTiles orig, Projectile self, Vector2 compareSpot,
        int radius, int minI, int maxI, int minJ, int maxJ, bool wallSplode, bool explodeHardmodeOres)
    {
        if (ImproveConfigs.Instance.DisableNonPlayerBombsExplosions != DisableNonPlayerBombsExplosionsType.Disabled &&
            (self.type == ProjectileID.Bomb && self.GetGlobalProjectile<ImproveProjectile>().NonPlayer ||
             self.type == ProjectileID.BombSkeletronPrime))
            return;

        if (ImproveConfigs.Instance.DisableNonPlayerBombsExplosions == DisableNonPlayerBombsExplosionsType.FTWAndExplosives &&
            self.type == ProjectileID.Explosives)
            return;

        orig.Invoke(self, compareSpot, radius, minI, maxI, minJ, maxJ, wallSplode, explodeHardmodeOres);
    }

    private bool BetterCheckSpawn(On_Player.orig_CheckSpawn orig, int x, int y)
    {
        if (ImproveConfigs.Instance.BedEverywhere)
        {
            // 相比于原版判定，把床被阻挡和房间判定给删了
            if (x < 10 || x > Main.maxTilesX - 10 || y < 10 || y > Main.maxTilesX - 10)
                return false;

            if (Main.tile[x, y - 1] == null)
                return false;

            if (!Main.tile[x, y - 1].HasTile || !TileID.Sets.IsValidSpawnPoint[Main.tile[x, y - 1].TileType])
                return false;

            return true;
        }

        return orig(x, y);
    }

    private void NotTakeUnityPotion(On_Player.orig_TakeUnityPotion orig, Player self)
    {
        if (ImproveConfigs.Instance.NoConditionTP) return;

        orig(self);
    }

    private bool LightNotBlocked(On_TileLightScanner.orig_LightIsBlocked orig, TileLightScanner self, Tile tile)
    {
        if (ImproveConfigs.Instance.LightNotBlocked) return false;
        else return orig(self, tile);
    }

    private bool NoSleepRestrictions(On_PlayerSleepingHelper.orig_DoesPlayerHaveReasonToActUpInBed orig,
        ref PlayerSleepingHelper self, Player player)
    {
        // 谁也无法阻止我睡觉！不管是Boss还是事件！
        if (ImproveConfigs.Instance.NoSleepRestrictions) return false;
        else return orig(ref self, player);
    }

    private void PatchQuestFishCheck(On_Projectile.orig_FishingCheck_ProbeForQuestFish orig, Projectile self,
        ref FishingAttempt fisher)
    {
        if (!ImproveConfigs.Instance.QuestFishStack)
        {
            orig.Invoke(self, ref fisher);
            return;
        }

        fisher.questFish = Main.anglerQuestItemNetIDs[Main.anglerQuest];
        // if (Main.player[owner].HasItem(fisher.questFish))
        //     fisher.questFish = -1;

        if (!NPC.AnyNPCs(369))
            fisher.questFish = -1;

        if (Main.anglerQuestFinished)
            fisher.questFish = -1;
    }

    private void TranslucentInfernoRings(ILContext il)
    {
        // IL_016d: ldc.r4       0.8
        // IL_0172: ldc.r4       0.0
        // IL_0177: ldloc.s      scale
        // IL_0179: ldloc.3      // num2
        // IL_017a: sub
        // IL_017b: call         float32 [System.Runtime]System.Math::Abs(float32)
        // IL_0180: ldc.r4       10
        // IL_0185: mul
        // IL_0186: call         float32 [FNA]Microsoft.Xna.Framework.MathHelper::Lerp(float32, float32, float32)
        // IL_018b: stloc.s      num3
        var c = new ILCursor(il);
        if (!c.TryGotoNext(
                MoveType.After,
                i => i.MatchLdcR4(0.8f),
                i => i.MatchLdcR4(0.0f),
                i => i.MatchLdloc(out _),
                i => i.MatchLdloc(out _),
                i => i.MatchSub(),
                i => i.Match(OpCodes.Call),
                i => i.MatchLdcR4(10),
                i => i.MatchMul(),
                i => i.Match(OpCodes.Call)))
        {
            ILMatchLog(nameof(TranslucentInfernoRings), il);
            return;
        }

        c.EmitCall(typeof(UIConfigs).GetProperty(nameof(UIConfigs.Instance))!.GetGetMethod()!);
        c.EmitLdfld(typeof(UIConfigs).GetField("InfernoTransparency")!);
        c.EmitMul();
    }

    private void LiveInCorrupt(ILContext il)
    {
        // int num3 = -WorldGen.GetTileTypeCountByCategory(tileTypeCounts, TileScanGroup.TotalGoodEvil);
        // if (num3 < 50) { ... }
        // IL_005F: call      int32 Terraria.WorldGen::GetTileTypeCountByCategory(int32[], valuetype Terraria.Enums.TileScanGroup)
        // IL_0064: neg
        // IL_0065: stloc.s   num3
        // IL_0067: ldloc.s   num3
        // IL_0069: ldc.i4.s  50
        // IL_006B: bge.s     IL_0070
        var c = new ILCursor(il);
        if (!c.TryGotoNext(
                MoveType.After,
                i => i.MatchCall(typeof(WorldGen).GetMethod("GetTileTypeCountByCategory")),
                i => i.Match(OpCodes.Neg),
                i => i.Match(OpCodes.Stloc_S),
                i => i.Match(OpCodes.Ldloc_S),
                i => i.Match(OpCodes.Ldc_I4_S)))
        {
            ILMatchLog(nameof(LiveInCorrupt), il);
            return;
        }
        // < 50则会设置为0，开选项的时候把这个设置成114514就行了
        c.EmitDelegate<Func<int, int>>((returnValue) => ImproveConfigs.Instance.NPCLiveInEvil ? 114514 : returnValue);
    }

    private void DisableBiomeSpread(ILContext il)
    {
        var c = new ILCursor(il);
        /* IL_0022: ldc.i4.0
         * IL_0023: ceq
         * IL_0025: stsfld    bool Terraria.WorldGen::AllowedToSpreadInfections
         * (原版设置的后面 插入)
         * IL_002A: ldc.i4.3
         * IL_002B: stloc.1
         */
        if (!c.TryGotoNext(
                MoveType.After,
                i => i.Match(OpCodes.Ldc_I4_0),
                i => i.Match(OpCodes.Ceq),
                i => i.MatchStsfld<WorldGen>(nameof(WorldGen.AllowedToSpreadInfections))
            ))
        {
            ILMatchLog(nameof(DisableBiomeSpread), il);
            return;
        }

        var label = c.DefineLabel();

        // 获取 Instance 属性的 Get 方法
        var getterMethod = typeof(ImproveConfigs).GetProperty("Instance").GetGetMethod();
        // 发出调用指令
        c.Emit(OpCodes.Call, getterMethod);

        c.Emit<ImproveConfigs>(OpCodes.Ldfld, nameof(ImproveConfigs.Instance.NoBiomeSpread));
        c.Emit(OpCodes.Brfalse, label); // 为False，跳走
        c.Emit(OpCodes.Ldc_I4_0); // 推一个0，也就是False，设置到AllowedToSpreadInfections
        c.Emit<WorldGen>(OpCodes.Stsfld, nameof(WorldGen.AllowedToSpreadInfections));
        c.MarkLabel(label);
    }

    // 只想要保存增益，不要减益，于是复杂了起来
    private void KeepBuffOnUpdateDead(ILContext il)
    {
        var c = new ILCursor(il);

        /* IL_01C0: ldsfld    bool[] Terraria.Main::persistentBuff
         * IL_01C5: ldarg.0
         * IL_01C6: ldfld     int32[] Terraria.Player::buffType
         * IL_01CB: ldloc.2
         * IL_01CC: ldelem.i4
         * IL_01CD: ldelem.u1
         * IL_01CE: brtrue.s  IL_01E2
         */
        if (!c.TryGotoNext(
                MoveType.After,
                i => i.MatchLdsfld<Main>(nameof(Main.persistentBuff)),
                i => i.Match(OpCodes.Ldarg_0),
                i => i.MatchLdfld<Player>(nameof(Player.buffType)),
                i => i.Match(OpCodes.Ldloc_2),
                i => i.Match(OpCodes.Ldelem_I4),
                i => i.Match(OpCodes.Ldelem_U1)
            ))
        {
            ILMatchLog(nameof(KeepBuffOnUpdateDead), il);
            return;
        }

        c.Emit(OpCodes.Ldarg_0); // Player实例
        c.Emit(OpCodes.Ldfld,
            typeof(Player).GetField(nameof(Player.buffType),
                BindingFlags.Instance | BindingFlags.Public)); // buffType数组
        c.Emit(OpCodes.Ldloc_2); // 索引 i
        c.Emit(OpCodes.Ldelem_I4); // 结合出int32
        c.EmitDelegate<Func<bool, int, bool>>((returnValue, buffType) =>
        {
            if (ImproveConfigs.Instance.DontDeleteBuff)
            {
                // 返回false就会进入删除
                return !Main.debuff[buffType] && !Main.buffNoSave[buffType] && !Main.lightPet[buffType] &&
                       !Main.vanityPet[buffType];
            }

            return returnValue;
        });
    }

    private void LavalessLavaSlime(ILContext il)
    {
        var c = new ILCursor(il);

        if (!c.TryGotoNext(
                MoveType.After,
                i => i.MatchCall<Main>("get_expertMode"),
                i => i.MatchBrtrue(out _),
                i => i.MatchLdsfld(typeof(WorldGen.Skyblock), nameof(WorldGen.Skyblock.lowTiles)),
                i => i.MatchBrfalse(out _),
                i => i.Match(OpCodes.Ldarg_0),
                i => i.MatchLdfld<NPC>(nameof(NPC.type)),
                i => i.Match(OpCodes.Ldc_I4_S, (sbyte)NPCID.LavaSlime)
            ))
        {
            ILMatchLog(nameof(LavalessLavaSlime), il);
            return;
        }

        c.EmitDelegate<Func<int, int>>(returnValue => ImproveConfigs.Instance.LavalessLavaSlime ? NPCLoader.NPCCount : returnValue);
    }

    private void LavalessGFB(On_NPC.orig_VanillaHitEffect orig, NPC self, int hitDirection, double dmg, bool instantKill)
    {
        bool gfb = Main.getGoodWorld;
        if (ImproveConfigs.Instance.LavalessLavaSlime && self.type is NPCID.Hellbat or NPCID.Lavabat)
            Main.getGoodWorld = false;

        orig(self, hitDirection, dmg, instantKill);

        Main.getGoodWorld = gfb;
    }

    private void NPC_CountKillForBannersAndDropThem(On_NPC.orig_CountKillForBannersAndDropThem orig,
        NPC npc)
    {
        int bannerID = BannerSystem.NPCtoBanner(npc.BannerID());
        int itemID = BannerSystem.BannerToItem(bannerID);
        int originalRequirement = ItemID.Sets.KillsToBanner[itemID];
        ItemID.Sets.KillsToBanner[itemID] = (int)(ItemID.Sets.KillsToBanner[itemID] * ImproveConfigs.Instance.BannerRequirement);
        orig.Invoke(npc);
        ItemID.Sets.KillsToBanner[itemID] = originalRequirement;
    }

    private void TravelNPCStay(On_WorldGen.orig_UnspawnTravelNPC orig)
    {
        if (!ImproveConfigs.Instance.TravellingMerchantStay)
            orig.Invoke();
    }


    private bool WorldGen_IsHarvestableHerbWithSeed(On_WorldGen.orig_IsHarvestableHerbWithSeed orig, int type,
        int style, int y)
    {
        return ImproveConfigs.Instance.AlchemyGrassAlwaysBlooms || orig.Invoke(type, style, y);
    }

    // “草药” 是否可以被 “再生法杖” 收割
    // 自动补种已经在1.4.5内置了所以不需要了
    // 永久开花现在可以用On改独立一个函数了
#if false
    private static int _herbStyle;
    private static int _herbType;
    private static int _herbImmatureImmuneTime;
    private static void Player_PlaceThing_Tiles_BlockPlacementForAssortedThings(ILContext il)
    {
        var c = new ILCursor(il);

        // 移动到破坏草/草药的判定处
        if (!c.TryGotoNext(MoveType.After,
                i => i.MatchLdcI4(84),
                i => i.MatchBneUn(out _)))
        {
            ILMatchLog(nameof(Player_PlaceThing_Tiles_BlockPlacementForAssortedThings), il);
            return;
        }

        // 如果开了自动补种就记录当前物块的Type和Style
        c.EmitDelegate(() =>
        {
            if (ImproveConfigs.Instance.StaffOfRegenerationAutomaticPlanting)
            {
                var tile = Main.tile[Player.tileTargetX, Player.tileTargetY];
                _herbStyle = tile.TileFrameX / 18;
                _herbType = tile.TileType;
            }
        });

        // 移动到KillTile后
        if (!c.TryGotoNext(MoveType.After,
                i => i.Match(OpCodes.Ldc_I4_0),
                i => i.Match(OpCodes.Ldc_I4_0),
                i => i.Match(OpCodes.Ldc_I4_0),
                i => i.Match(OpCodes.Call)))
        {
            ILMatchLog(nameof(Player_PlaceThing_Tiles_BlockPlacementForAssortedThings), il);
            return;
        }

        // 旧的逻辑：在KillTile和它的发包之间插入，满足条件时立刻补种并发包，这样有时候会同步失败
        /*c.EmitDelegate(() =>
        {
            if (!Config.StaffOfRegenerationAutomaticPlanting ||
                _herbType is not TileID.BloomingHerbs and not TileID.MatureHerbs)
                return;

            WorldGen.PlaceTile(Player.tileTargetX, Player.tileTargetY, TileID.ImmatureHerbs, true, false, -1,
                _herbStyle);
            NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 1, Player.tileTargetX, Player.tileTargetY,
                TileID.ImmatureHerbs, _herbStyle);
            _herbImmatureImmuneTime = 5;
        });*/

        // 移除原先的KillTile发包
        int startIndex = c.Index;
        if (!c.TryGotoNext(i => i.Match(OpCodes.Br)))
        {
            ILMatchLog(nameof(Player_PlaceThing_Tiles_BlockPlacementForAssortedThings), il);
            return;
        }
        int endIndex = c.Index;
        c.Index = startIndex;
        c.RemoveRange(endIndex - startIndex);

        // 如果开启了再生法杖补种就采用专用发包，否则采用原版操作
        c.EmitDelegate(() =>
        {
            var tileTargetX = Player.tileTargetX;
            var tileTargetY = Player.tileTargetY;
            if (Main.dedServ) return;
            if (ImproveConfigs.Instance.StaffOfRegenerationAutomaticPlanting &&
                _herbType is TileID.BloomingHerbs or TileID.MatureHerbs &&
                _herbStyle >= 0)
            {
                WorldGen.PlaceTile(
                    tileTargetX,
                    tileTargetY,
                    TileID.ImmatureHerbs,
                    true,
                    false,
                    -1,
                    _herbStyle);

                AutomaticPlantingPacket.Get(tileTargetX, tileTargetY, (byte)_herbStyle).Send();

                _herbType = -1;
                _herbImmatureImmuneTime = 5;
            }
            else
            {
                if (!Main.tile[tileTargetX, tileTargetY].HasTile && Main.netMode == NetmodeID.MultiplayerClient)
                    NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, tileTargetX, tileTargetY);
            }
        });

        // 这里是对草药盆内草药的补充判定？
        if (!c.TryGotoNext(MoveType.After,
                i => i.Match(OpCodes.Ldc_R8, 40500d),
                i => i.Match(OpCodes.Ble_Un_S),
                i => i.Match(OpCodes.Ldc_I4_1),
                i => i.Match(OpCodes.Stloc_S),
                i => i.Match(OpCodes.Ldloc_S)))
        {
            ILMatchLog(nameof(Player_PlaceThing_Tiles_BlockPlacementForAssortedThings), il);
            return;
        }

        // 同样记录Type和Style
        c.EmitDelegate<Func<bool, bool>>(flag =>
        {
            if (ImproveConfigs.Instance.StaffOfRegenerationAutomaticPlanting)
            {
                var tile = Main.tile[Player.tileTargetX, Player.tileTargetY];
                _herbStyle = tile.TileFrameX / 18;
                _herbType = tile.TileType;
            }

            return ImproveConfigs.Instance.AlchemyGrassAlwaysBlooms || flag;
        });

        // 移动到KillTile后面
        if (!c.TryGotoNext(MoveType.After,
                // i => i.Match(OpCodes.Ldc_R4), // 原先是移动到发包后面
                i => i.Match(OpCodes.Ldc_I4_0),
                i => i.Match(OpCodes.Ldc_I4_0),
                i => i.Match(OpCodes.Ldc_I4_0),
                i => i.Match(OpCodes.Call)))
        {
            ILMatchLog(nameof(Player_PlaceThing_Tiles_BlockPlacementForAssortedThings), il);
            return;
        }

        // 旧的逻辑：在KillTile和它的发包之间插入，满足条件时立刻补种并发包，这样有时候会同步失败
        /*c.EmitDelegate(() =>
        {
            if (!Config.StaffOfRegenerationAutomaticPlanting ||
                _herbType is not TileID.BloomingHerbs and not TileID.MatureHerbs)
                return;

            WorldGen.PlaceTile(Player.tileTargetX, Player.tileTargetY, TileID.ImmatureHerbs, true, false, -1,
                _herbStyle);
            NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 1, Player.tileTargetX, Player.tileTargetY,
                TileID.ImmatureHerbs, _herbStyle);
            _herbImmatureImmuneTime = 5;
        });*/

        // 移除原先的KillTile发包
        startIndex = c.Index;
        if (!c.TryGotoNext(MoveType.After, i => i.Match(OpCodes.Call)))
        {
            ILMatchLog(nameof(Player_PlaceThing_Tiles_BlockPlacementForAssortedThings), il);
            return;
        }
        endIndex = c.Index;
        c.Index = startIndex;
        c.RemoveRange(endIndex - startIndex);

        // 如果开启了再生法杖补种就采用专用发包，否则采用原版操作
        c.EmitDelegate(() =>
        {
            var tileTargetX = Player.tileTargetX;
            var tileTargetY = Player.tileTargetY;
            if (Main.dedServ) return;
            if (ImproveConfigs.Instance.StaffOfRegenerationAutomaticPlanting &&
                _herbType is TileID.BloomingHerbs or TileID.MatureHerbs &&
                _herbStyle >= 0)
            {
                WorldGen.PlaceTile(
                    tileTargetX,
                    tileTargetY,
                    TileID.ImmatureHerbs,
                    true,
                    false,
                    -1,
                    _herbStyle);

                AutomaticPlantingPacket.Get(tileTargetX, tileTargetY, (byte)_herbStyle).Send();

                _herbType = -1;
                _herbImmatureImmuneTime = 5;
            }
            else
            {
                if (!Main.tile[tileTargetX, tileTargetY].HasTile && Main.netMode == NetmodeID.MultiplayerClient)
                    NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, tileTargetX, tileTargetY);
            }
        });

        // 阻止再生法杖或者再生之斧破坏非种植盆内药草
        if (!c.TryGotoNext(i => i.MatchLdcI4(380)))
        {
            ILMatchLog(nameof(Player_PlaceThing_Tiles_BlockPlacementForAssortedThings), il);
            return;
        }
        if (!c.TryGotoNext(i => i.MatchLdcI4(0)))
        {
            ILMatchLog(nameof(Player_PlaceThing_Tiles_BlockPlacementForAssortedThings), il);
            return;
        }
        if (!c.TryGotoNext(MoveType.After, i => i.MatchLdcI4(0)))
        {
            ILMatchLog(nameof(Player_PlaceThing_Tiles_BlockPlacementForAssortedThings), il);
            return;
        }

        c.Index++;
        c.EmitPop();
        c.EmitDelegate<Func<bool, bool>>(flag => flag && _herbImmatureImmuneTime-- <= 0);
        c.EmitLdsflda(typeof(Main).GetField(nameof(Main.tile), BindingFlags.Static | BindingFlags.Public));
    }
#endif
    // 提升草药生长速度
    private void WorldGen_GrowAlch(ILContext il)
    {
        var c = new ILCursor(il);

        if (!c.TryGotoNext(MoveType.After,
                i => i.Match(OpCodes.Call),
                i => i.Match(OpCodes.Ldc_I4_S)))
        {
            ILMatchLog(nameof(WorldGen_GrowAlch), il);
            return;
        }
        c.EmitDelegate<Func<int, int>>(num => ImproveConfigs.Instance.AlchemyGrassGrowsFaster ? 1 : num);

        /*if (!c.TryGotoNext(MoveType.After,
            i => i.Match(OpCodes.Ret),
            i => i.Match(OpCodes.Ldsfld)))
            return;
        c.EmitDelegate<Func<bool, bool>>(flag => MyUtils.Config.DisableAlchemyPlantRipeCondition ? true : flag); // “太阳花”

        if (!c.TryGotoNext(MoveType.After,
            i => i.Match(OpCodes.Ret),
            i => i.Match(OpCodes.Ldsfld)))
            return;
        c.EmitDelegate<Func<bool, bool>>(flag => MyUtils.Config.DisableAlchemyPlantRipeCondition ? false : flag); // “月光草”

        if (!c.TryGotoNext(MoveType.After,
            i => i.Match(OpCodes.Ret),
            i => i.Match(OpCodes.Ldsfld)))
            return;
        c.EmitDelegate<Func<bool, bool>>(flag => MyUtils.Config.DisableAlchemyPlantRipeCondition ? true : flag); // “幌菊”

        if (!c.TryGotoNext(MoveType.After,
            i => i.Match(OpCodes.Conv_R8),
            i => i.Match(OpCodes.Ldsfld)))
            return;
        c.EmitDelegate<Func<double, double>>(x => MyUtils.Config.DisableAlchemyPlantRipeCondition ? 0 : x); // “闪耀根”

        if (!c.TryGotoNext(MoveType.After,
            i => i.Match(OpCodes.Ldsfld),
            i => i.Match(OpCodes.Ldc_I4),
            i => i.Match(OpCodes.Sub)))
            return;
        c.EmitDelegate<Func<int, int>>(x => MyUtils.Config.DisableAlchemyPlantRipeCondition ? 0 : x); // “火焰花”*/
    }

    /// <summary>
    /// 伤害波动
    /// </summary>
    private int DisableDamageVar(On_Main.orig_DamageVar_float_int_float orig, float dmg, int percent, float luck)
    {
        if (ImproveConfigs.Instance.BanDamageVar)
            return (int)Math.Round(dmg);
        else
            return orig(dmg, percent, luck);
    }

    /// <summary>
    /// 空间法杖计算剩余平台数
    /// </summary>
    private void TweakDrawCountInventory(ILContext il)
    {
        // 计算剩余平台
        var c = new ILCursor(il);
        if (!c.TryGotoNext(MoveType.After,
                i => i.MatchLdcI4(-1),
                i => i.MatchStloc(48)))
        {
            ILMatchLog(nameof(TweakDrawCountInventory), il);
            return;
        }
        c.Index--;
        c.Emit(OpCodes.Ldarg_1); // 玩家物品槽
        c.Emit(OpCodes.Ldarg_2); // content
        c.Emit(OpCodes.Ldarg_3); // 物品在物品槽的位置
        c.EmitDelegate<Func<int, Item[], int, int, int>>((num11, inv, content, slot) =>
        {
            if (content == 13)
            {
                if (inv[slot].ModItem is SpaceWand)
                {
                    SpaceWand spaceWand = inv[slot].ModItem as SpaceWand;
                    ItemCount(inv, spaceWand.GetConditions(), out int count);
                    return count;
                }
                else if (inv[slot].ModItem is WallPlace)
                {
                    ItemCount(inv, (item) => item.createWall > -1, out int count);
                    return count;
                }

                return -1;
            }
            else
            {
                return -1;
            }
        });
    }

    /// <summary>
    /// 墓碑掉落
    /// </summary>
    private void DisableDropTombstone(On_Player.orig_DropTombstone orig, Player self, long coinsOwned,
        NetworkText deathText, int hitDirection)
    {
        if (!ImproveConfigs.Instance.BanTombstone)
        {
            orig(self, coinsOwned, deathText, hitDirection);
        }
    }

    /// <summary>
    /// 修改 NPC 快乐度
    /// </summary>
    private void ModifyNPCHappiness(ILContext il)
    {
        FieldInfo fShoppingSettingsPriceAdjustment =
            typeof(ShoppingSettings).GetField(nameof(ShoppingSettings.PriceAdjustment),
                BindingFlags.Instance | BindingFlags.Public);
        //FieldInfo fMyUtilsConfig =
        //    typeof(MyUtils).GetField("_config", BindingFlags.Static | BindingFlags.NonPublic);

        // 获取 Instance 属性的 Get 方法
        var getterMethod = typeof(ImproveConfigs).GetProperty("Instance").GetGetMethod();
        // 发出调用指令

        FieldInfo fImproveConfigsModifyNPCHappiness =
            typeof(ImproveConfigs).GetField(nameof(ImproveConfigs.ModifyNPCHappiness),
                BindingFlags.Instance | BindingFlags.Public);
        FieldInfo fImproveConfigsNPCHappiness = typeof(ImproveConfigs).GetField(nameof(ImproveConfigs.NPCHappiness),
            BindingFlags.Instance | BindingFlags.Public);

        ILCursor c = new(il);
        if (c.TryGotoNext(MoveType.Before, x => x.MatchLdloc1(), x => x.MatchRet()))
        {
            ILLabel target = c.DefineLabel();
            c.Emit(OpCodes.Call, getterMethod);
            c.EmitLdfld(fImproveConfigsModifyNPCHappiness);
            c.EmitLdcI4(0);
            c.EmitCeq();
            c.EmitBrtrue(target);
            c.EmitLdloca(1);
            c.Emit(OpCodes.Call, getterMethod);
            c.EmitLdfld(fImproveConfigsNPCHappiness);
            c.EmitConvR8();
            c.EmitLdcR8(100.0);
            c.EmitDiv();
            c.EmitStfld(fShoppingSettingsPriceAdjustment);
            c.MarkLabel(target);
        }
        else
        {
            MonoModHooks.DumpIL(ImproveGame.Instance, il);
            ILMatchLog(nameof(IL_WorldGen_UpdateWorld_GrassGrowth), il);
        }
    }
}