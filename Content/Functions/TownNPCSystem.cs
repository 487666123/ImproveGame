using ImproveGame.Common.Configs;
using Terraria.GameContent.Bestiary;

namespace ImproveGame.Content.Functions
{
    public class TownNPCSystem : ModSystem
    {
        private static bool _isExtraUpdate; // 是否处于源于模组的额外更新中
        private static HashSet<int> _activeTownNPCs = new();
        private static BestiaryUnlockProgressReport _cachedReport = new();

        public override void Load()
        {
            // 更好的NPC生成机制 + NPC生成加速
            On_Main.UpdateTime_SpawnTownNPCs += orig =>
            {
                _isExtraUpdate = false;
                orig.Invoke();
                TrySpawnNPCsThatNeedsToUntie();

                // 2023.3.25: 调用次数从指数级改为乘数，发现实际速度并无差异，减少开销的操作也可有可无了，不过还是保留吧
                int times = ImproveConfigs.Instance.TownNPCSpawnSpeed - 1;
                // 仅获取一次，避免重复调用增加开销
                double worldUpdateRate = WorldGen.GetWorldUpdateRate();
                _cachedReport = Main.GetBestiaryProgressReport();
                // 在标记为“额外更新”的情况下调用 orig
                _isExtraUpdate = true;
                for (int i = 0; i < times; i++)
                {
                    TrySetNPCSpawn(orig, worldUpdateRate);
                }

                // 重置标记防止影响其他模组或原版代码
                _isExtraUpdate = false;
            };

            // 在加速的NPC生成中，跳过寻找家的步骤
            On_WorldGen.QuickFindHome += (orig, npc) =>
            {
                if (!_isExtraUpdate)
                    orig.Invoke(npc);
            };

            // 在加速的NPC生成中，避免多次调用GetBestiaryProgressReport开销过大，这里缓存一个
            On_Main.GetBestiaryProgressReport += orig =>
            {
                if (!_isExtraUpdate)
                    return orig.Invoke();
                return _cachedReport;
            };
        }

        private static void TrySetNPCSpawn(On_Main.orig_UpdateTime_SpawnTownNPCs orig, double worldUpdateRate)
        {
            orig.Invoke();

            // 确保原版在生成NPC的阶段
            if (Main.netMode is NetmodeID.MultiplayerClient || worldUpdateRate <= 0 || Main.checkForSpawns != 0)
                return;

            // 设立存活城镇NPC速查表
            SetupActiveTownNPCList();

            // 钱币包括猪猪存钱罐里面的
            if (HasEnoughMoneyForMerchant())
                TrySetNPCSpawn(NPCID.Merchant);

            TrySpawnNPCsThatNeedsToUntie();
        }

        private static void TrySpawnNPCsThatNeedsToUntie()
        {
            // 更好的生成机制
            if (!ImproveConfigs.Instance.TownNPCGetTFIntoHouse)
                return;

            if (NPC.downedSlimeKing || NPC.downedBoss1 || NPC.downedBoss2 || NPC.downedBoss3 || NPC.downedQueenBee ||
                Main.hardMode || NPC.downedDeerclops)
                TrySetNPCSpawn(NPCID.Stylist);
            if (NPC.downedGoblins)
                TrySetNPCSpawn(NPCID.GoblinTinkerer);
            if (NPC.downedBoss2)
                TrySetNPCSpawn(NPCID.DD2Bartender);
            if (NPC.downedBoss3)
                TrySetNPCSpawn(NPCID.Mechanic);
            if (Main.hardMode)
            {
                TrySetNPCSpawn(NPCID.Wizard);
                TrySetNPCSpawn(NPCID.TaxCollector);
            }
        }

        private static bool HasEnoughMoneyForMerchant()
        {
            int moneyCount = 0;
            for (int l = 0; l < Main.maxPlayers; l++)
            {
                var player = Main.player[l];
                if (!player.active)
                    continue;

                for (int m = 0; m < 40; m++)
                {
                    if (player.bank.item[m] is null || player.bank.item[m].stack <= 0)
                        continue;

                    var item = player.bank.item[m];
                    switch (item.type)
                    {
                        case ItemID.CopperCoin:
                            moneyCount += item.stack;
                            break;
                        case ItemID.SilverCoin:
                            moneyCount += item.stack * 100;
                            break;
                        case ItemID.GoldCoin:
                            return true;
                        case ItemID.PlatinumCoin:
                            return true;
                    }

                    if (moneyCount >= 5000) return true;
                }
            }

            return false;
        }

        public override void PostUpdateTime()
        {
            if (!Main.dayTime && ImproveConfigs.Instance.TownNPCGetTFIntoHouse) 
            {
                double rate = WorldGen.GetWorldUpdateRate();
                if (rate >= 0)
                    Main.checkForSpawns += (int)(7200 / rate);
                Main.UpdateTime_SpawnTownNPCs();
            }
        }

        private static void TrySetNPCSpawn(int npcId)
        {
            if (_activeTownNPCs.Contains(npcId))
                return;

            Main.townNPCCanSpawn[npcId] = true;
            if (WorldGen.prioritizedTownNPCType == 0)
            {
                WorldGen.prioritizedTownNPCType = npcId;
            }
        }

        private static void SetupActiveTownNPCList()
        {
            _activeTownNPCs = new HashSet<int>();
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                var npc = Main.npc[i];
                if (npc.active && npc.townNPC && npc.friendly)
                {
                    _activeTownNPCs.Add(npc.type);
                }
            }
        }
    }
}