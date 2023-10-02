using ImproveGame.Common.Configs;
using ImproveGame.Common.GlobalItems;
using ImproveGame.Content.Items;
using ImproveGame.Content.Tiles;
using ImproveGame.Interface.ExtremeStorage;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Reflection;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Drawing;

namespace ImproveGame.Content.Patches
{
    /// <summary>
    /// 为了方便管理，这里主要放一些不成体系的小修改，比如一些单独的On, IL
    /// </summary>
    public class MinorPatches : ModSystem
    {
        private class ShakeTreeTweak
        {
            private class ShakeTreeItem : GlobalItem
            {
                public override void OnSpawn(Item item, IEntitySource source)
                {
                    if (_isShakingTree && source is EntitySource_ShakeTree)
                        _hasItemDropped = true;
                }
            }

            private static bool _isShakingTree;
            private static bool _hasItemDropped; // 检测是否在摇树过程中有物品掉落

            public static void Load()
            {
                On_WorldGen.ShakeTree += (orig, i, j) =>
                {
                    if (!Config.ShakeTreeFruit)
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

                    TreeTypes treeType = WorldGen.GetTreeType(Main.tile[x, y].type);
                    if (treeType == TreeTypes.None)
                        return;

                    y--;
                    while (y > 10 && Main.tile[x, y].active() && TileID.Sets.IsShakeable[Main.tile[x, y].type])
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

            private static bool RebuildPatch(On_HairstyleUnlocksHelper.orig_ListWarrantsRemake orig,HairstyleUnlocksHelper self)
            {
                if (!_rebuilt)
                {
                    _rebuilt = true;
                    return true;
                }

                return false;
            }

            private static void UnlockPatch(On_HairstyleUnlocksHelper.orig_RebuildList orig, HairstyleUnlocksHelper self)
            {
                self.AvailableHairstyles.Clear();
                for (int i = 0; i < TextureAssets.PlayerHair.Length; i++)
                {
                    self.AvailableHairstyles.Add(i);
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
            // 使存钱罐中物品生效，如同放入背包一样
            On_Player.UpdateEquips += TweakExtraUpdateInventory;
            // 摇树总是掉落水果
            ShakeTreeTweak.Load();
            // “草药” 生长速度
            IL_WorldGen.GrowAlch += WorldGen_GrowAlch;
            // “草药” 绘制的是否是开花图案
            On_TileDrawing.IsAlchemyPlantHarvestable += TileDrawing_IsAlchemyPlantHarvestable;
            // “草药” 是否可以被 “再生法杖” 收割
            IL_Player.PlaceThing_Tiles_BlockPlacementForAssortedThings +=
                Player_PlaceThing_Tiles_BlockPlacementForAssortedThings;
            // “草药” 是否掉落成熟时候物品
            On_WorldGen.IsHarvestableHerbWithSeed += WorldGen_IsHarvestableHerbWithSeed;
            // 旅商永远不离开
            On_WorldGen.UnspawnTravelNPC += TravelNPCStay;
            // 修改旗帜需求
            On_NPC.CountKillForBannersAndDropThem += NPC_CountKillForBannersAndDropThem;
            // 熔岩史莱姆不生成熔岩
            IL_NPC.VanillaHitEffect += LavalessLavaSlime;
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
                        return;
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
                    return;
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
                if (Config.NoLakeSizePenalty)
                    numWaters = 114514;
            };
            // 失焦运行
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
            // 专家/大师延长Debuff
            On_Player.AddBuff_DetermineBuffTimeToAdd += (orig, self, type, time1) =>
                Config.LongerExpertDebuff ? orig.Invoke(self, type, time1) : time1;
            // 床随地设置重生点
            On_Player.CheckSpawn += (orig, i, j) =>
                Config.BedEverywhere || orig.Invoke(i, j);
            // 固定 NPC 快乐度为指定数值
            IL_ShopHelper.GetShoppingSettings += ModifyNPCHappiness;
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
                return;
            // < 50则会设置为0，开选项的时候把这个设置成114514就行了
            c.EmitDelegate<Func<int, int>>((returnValue) => Config.NPCLiveInEvil ? 114514 : returnValue);
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
                return;

            var label = c.DefineLabel();
            c.Emit<MyUtils>(OpCodes.Ldsfld, nameof(Config));
            c.Emit<ImproveConfigs>(OpCodes.Ldfld, nameof(Config.NoBiomeSpread));
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
                return;

            c.Emit(OpCodes.Ldarg_0); // Player实例
            c.Emit(OpCodes.Ldfld,
                typeof(Player).GetField(nameof(Player.buffType),
                    BindingFlags.Instance | BindingFlags.Public)); // buffType数组
            c.Emit(OpCodes.Ldloc_2); // 索引 i
            c.Emit(OpCodes.Ldelem_I4); // 结合出int32
            c.EmitDelegate<Func<bool, int, bool>>((returnValue, buffType) =>
            {
                if (Config.DontDeleteBuff)
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
                    i => i.MatchCall(typeof(Main), "get_expertMode"),
                    i => i.Match(OpCodes.Brfalse),
                    i => i.Match(OpCodes.Ldarg_0),
                    i => i.MatchLdfld(typeof(NPC), nameof(NPC.type)),
                    i => i.Match(OpCodes.Ldc_I4_S, (sbyte)NPCID.LavaSlime)
                ))
                return;

            c.EmitDelegate<Func<int, int>>(returnValue => Config.LavalessLavaSlime ? NPCLoader.NPCCount : returnValue);
        }

        private void NPC_CountKillForBannersAndDropThem(Terraria.On_NPC.orig_CountKillForBannersAndDropThem orig,
            NPC npc)
        {
            int bannerID = Item.NPCtoBanner(npc.BannerID());
            int itemID = Item.BannerToItem(bannerID);
            int originalRequirement = ItemID.Sets.KillsToBanner[itemID];
            ItemID.Sets.KillsToBanner[itemID] = (int)(ItemID.Sets.KillsToBanner[itemID] * Config.BannerRequirement);
            orig.Invoke(npc);
            ItemID.Sets.KillsToBanner[itemID] = originalRequirement;
        }

        private void TravelNPCStay(On_WorldGen.orig_UnspawnTravelNPC orig)
        {
            if (!Config.TravellingMerchantStay)
                orig.Invoke();
        }

        private bool TileDrawing_IsAlchemyPlantHarvestable(On_TileDrawing.orig_IsAlchemyPlantHarvestable orig,
            TileDrawing self, int style)
        {
            return Config.AlchemyGrassAlwaysBlooms || orig.Invoke(self, style);
        }

        private bool WorldGen_IsHarvestableHerbWithSeed(On_WorldGen.orig_IsHarvestableHerbWithSeed orig, int type,
            int style)
        {
            return Config.AlchemyGrassAlwaysBlooms || orig.Invoke(type, style);
        }

        // “草药” 是否可以被 “再生法杖” 收割
        private static int _herbStyle;
        private static int _herbType;

        private static void Player_PlaceThing_Tiles_BlockPlacementForAssortedThings(ILContext il)
        {
            var c = new ILCursor(il);
            if (!c.TryGotoNext(MoveType.After,
                    i => i.Match(OpCodes.Ldc_I4_S, (sbyte)84),
                    i => i.Match(OpCodes.Bne_Un_S)))
                return;
            c.EmitDelegate(() =>
            {
                if (Config.StaffOfRegenerationAutomaticPlanting)
                {
                    var tile = Main.tile[Player.tileTargetX, Player.tileTargetY];
                    _herbStyle = tile.TileFrameX / 18;
                    _herbType = tile.TileType;
                }
            });

            if (!c.TryGotoNext(MoveType.After,
                    i => i.Match(OpCodes.Ldc_I4_0),
                    i => i.Match(OpCodes.Ldc_I4_0),
                    i => i.Match(OpCodes.Ldc_I4_0),
                    i => i.Match(OpCodes.Call)))
                return;
            c.EmitDelegate(() =>
            {
                if (!Config.StaffOfRegenerationAutomaticPlanting ||
                    _herbType is not TileID.BloomingHerbs and not TileID.MatureHerbs)
                    return;

                WorldGen.PlaceTile(Player.tileTargetX, Player.tileTargetY, TileID.ImmatureHerbs, true, false, -1,
                    _herbStyle);
                NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 1, Player.tileTargetX, Player.tileTargetY,
                    TileID.ImmatureHerbs, _herbStyle);
            });

            if (!c.TryGotoNext(MoveType.After,
                    i => i.Match(OpCodes.Ldc_R8, 40500d),
                    i => i.Match(OpCodes.Ble_Un_S),
                    i => i.Match(OpCodes.Ldc_I4_1),
                    i => i.Match(OpCodes.Stloc_S),
                    i => i.Match(OpCodes.Ldloc_S)))
                return;
            c.EmitDelegate<Func<bool, bool>>(flag =>
            {
                if (Config.StaffOfRegenerationAutomaticPlanting)
                {
                    var tile = Main.tile[Player.tileTargetX, Player.tileTargetY];
                    _herbStyle = tile.TileFrameX / 18;
                    _herbType = tile.TileType;
                }

                return Config.AlchemyGrassAlwaysBlooms || flag;
            });

            if (!c.TryGotoNext(MoveType.After,
                    i => i.Match(OpCodes.Ldc_R4),
                    i => i.Match(OpCodes.Ldc_I4_0),
                    i => i.Match(OpCodes.Ldc_I4_0),
                    i => i.Match(OpCodes.Ldc_I4_0),
                    i => i.Match(OpCodes.Call)))
                return;
            c.EmitDelegate(() =>
            {
                if (!Config.StaffOfRegenerationAutomaticPlanting ||
                    _herbType is not TileID.BloomingHerbs and not TileID.MatureHerbs)
                    return;

                WorldGen.PlaceTile(Player.tileTargetX, Player.tileTargetY, TileID.ImmatureHerbs, true, false, -1,
                    _herbStyle);
                NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 1, Player.tileTargetX, Player.tileTargetY,
                    TileID.ImmatureHerbs, _herbStyle);
            });
        }

        // 提升草药生长速度
        private void WorldGen_GrowAlch(ILContext il)
        {
            var c = new ILCursor(il);

            if (!c.TryGotoNext(MoveType.After,
                    i => i.Match(OpCodes.Call),
                    i => i.Match(OpCodes.Ldc_I4_S)))
                return;
            c.EmitDelegate<Func<int, int>>(num => Config.AlchemyGrassGrowsFaster ? 1 : num);

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
        /// 旗帜BUFF在背包生效
        /// </summary>
        private static void AddBannerBuff(Item item)
        {
            if (item is null)
                return;

            bool globalItemNotNull = item.TryGetGlobalItem<GlobalItemData>(out var itemData);
            if (globalItemNotNull)
                itemData.ShouldHaveInvGlowForBanner = false;

            int bannerID = ItemToBanner(item);
            if (bannerID != -1)
            {
                Main.SceneMetrics.NPCBannerBuff[bannerID] = true;
                Main.SceneMetrics.hasBanner = true;
                if (globalItemNotNull)
                    itemData.ShouldHaveInvGlowForBanner = true;
            }
        }

        public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
        {
            // 不要模拟
            if (TileCounter.Simulating)
                return;
            // 随身旗帜（增益站）
            if (Config.NoPlace_BUFFTile_Banner && Main.netMode is not NetmodeID.Server)
            {
                TryAddBuff(Main.LocalPlayer);
                if (Config.ShareInfBuffs)
                    CheckTeamPlayers(Main.myPlayer, TryAddBuff);

                // 从TE中获取所有的无尽Buff物品
                foreach ((int _, TileEntity tileEntity) in TileEntity.ByID)
                {
                    if (tileEntity is not TEExtremeStorage {UsePortableBanner: true} storage)
                    {
                        continue;
                    }

                    var banners = storage.FindAllNearbyChestsWithGroup(ItemGroup.Furniture);
                    banners.ForEach(i => CheckBanners(Main.chest[i].item));
                }
            }
        }

        private static void TryAddBuff(Player player) =>
            CheckBanners(GetAllInventoryItemsList(player));

        /// <summary>
        /// 从某个玩家的各种物品栏中拿效果
        /// </summary>
        private static void CheckBanners(IEnumerable<Item> items)
        {
            foreach (var item in items)
            {
                AddBannerBuff(item);
                if (item is not null && !item.IsAir && item.ModItem is not null &&
                    item.ModItem is BannerChest bannerChest && bannerChest.StoredBanners.Count > 0)
                {
                    foreach (var p in bannerChest.StoredBanners)
                    {
                        AddBannerBuff(p);
                    }
                }
            }
        }

        /// <summary>
        /// 使存钱罐中物品如同放在背包
        /// </summary>
        private void TweakExtraUpdateInventory(On_Player.orig_UpdateEquips orig, Player self, int i)
        {
            orig(self, i);

            if (Main.myPlayer != self.whoAmI)
                return;

            var items = GetAllInventoryItemsList(self, "inv");
            foreach (var item in items)
            {
                if (item.type != ItemID.EncumberingStone)
                {
                    ItemLoader.UpdateInventory(item, self);

                    self.RefreshInfoAccsFromItemType(item.type);
                    self.RefreshMechanicalAccsFromItemType(item.type);
                }
            }
        }

        /// <summary>
        /// 伤害波动
        /// </summary>
        private int DisableDamageVar(On_Main.orig_DamageVar_float_int_float orig, float dmg, int percent, float luck)
        {
            if (Config.BanDamageVar)
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
                    i => i.Match(OpCodes.Pop),
                    i => i.Match(OpCodes.Ldc_I4_M1)))
                return;
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
            if (!Config.BanTombstone)
            {
                orig(self, coinsOwned, deathText, hitDirection);
            }
        }

        /// <summary>
        /// 修改 NPC 快乐度
        /// </summary>
        private void ModifyNPCHappiness(ILContext il)
        {
            FieldInfo fShoppingSettingsPriceAdjustment = typeof(ShoppingSettings).GetField(nameof(ShoppingSettings.PriceAdjustment), BindingFlags.Instance | BindingFlags.Public);
            FieldInfo fMyUtilsConfig = typeof(MyUtils).GetField(nameof(Config), BindingFlags.Static | BindingFlags.Public);
            FieldInfo fImproveConfigsModifyNPCHappiness = typeof(ImproveConfigs).GetField(nameof(ImproveConfigs.ModifyNPCHappiness), BindingFlags.Instance | BindingFlags.Public);
            FieldInfo fImproveConfigsNPCHappiness = typeof(ImproveConfigs).GetField(nameof(ImproveConfigs.NPCHappiness), BindingFlags.Instance | BindingFlags.Public);

            ILCursor c = new(il);
            if (c.TryGotoNext(MoveType.Before, x => x.MatchLdloc1(), x => x.MatchRet()))
            {
                ILLabel target = c.DefineLabel();
                c.EmitLdsfld(fMyUtilsConfig);
                c.EmitLdfld(fImproveConfigsModifyNPCHappiness);
                c.EmitLdcI4(0);
                c.EmitCeq();
                c.EmitBrtrue(target);
                c.EmitLdloca(1);
                c.EmitLdsfld(fMyUtilsConfig);
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
            }
        }
    }
}