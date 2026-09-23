using ImproveGame.Common.Configs;
using Terraria.GameContent.ItemDropRules;

namespace ImproveGame.Common.GlobalNPCs
{
    public class ImproveNPC : GlobalNPC
    {
        public override void SetDefaults(NPC npc)
        {
            npc.value *= ImproveConfigs.Instance.NPCCoinDropRate;
        }

        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            if (npc.type == NPCID.KingSlime)
            {
                int itemType = ModContent.ItemType<Content.Items.SpaceWand>();
                var leadingRule = new LeadingConditionRule(new SpaceWandDrop());
                leadingRule.OnSuccess(new DropPerPlayerOnThePlayer(itemType, 1, 1, 1, new WandDrop(itemType)));
                npcLoot.Add(leadingRule);

                itemType = ModContent.ItemType<Content.Items.WallPlace>();
                int itemTypeAlt = ModContent.ItemType<Content.Items.WallPlaceSelectorMode>();
                leadingRule = new LeadingConditionRule(new WallPlaceDrop());
                leadingRule.OnSuccess(new DropPerPlayerOnThePlayer(itemType, 1, 1, 1,
                    new WandDrop(itemType, itemTypeAlt)));
                npcLoot.Add(leadingRule);
            }
        }

        public override void ModifyGlobalLoot(GlobalLoot globalLoot)
        {
            // 钱币掉落倍率开到最大时（x25）有 0.5% 概率掉落幸运金币，它唯一的作用就是出售。
            var leadingRule = new LeadingConditionRule(new CoinOneDrop());
            leadingRule.OnSuccess(new CommonDrop(ModContent.ItemType<Content.Items.Coin.CoinOne>(), 200));
            globalLoot.Add(leadingRule);
        }

        public override bool PreAI(NPC npc)
        {
            // 实现史莱姆百分百额外掉落
            if (ImproveConfigs.Instance.SlimeExDrop)
            {
                // 我复制的原版判定，然后把概率改成了100%
                if (npc.type == NPCID.BlueSlime && npc.ai[1] is 0 or -1 && Main.netMode != NetmodeID.MultiplayerClient &&
                    npc.value > 0f)
                {
                    if (Main.remixWorld && npc.ai[0] != -999f && Main.rand.NextBool(3))
                    {
                        npc.ai[1] = ItemID.FallenStar;
                        npc.netUpdate = true;
                    }
                    else
                    {
                        int itemType = npc.AI_001_Slimes_GenerateItemInsideBody(npc.ai[0] == -999f);
                        npc.ai[1] = itemType;
                        npc.netUpdate = true;
                    }
                }
            }

            return true;
        }
    }

    public class WandDrop : IItemDropRuleCondition
    {
        public int[] itemTypes;

        public WandDrop(params int[] itemTypes)
        {
            this.itemTypes = itemTypes;
        }

        public bool CanDrop(DropAttemptInfo info)
        {
            if (!info.IsInSimulation)
            {
                var allItems = MyUtils.GetAllInventoryItemsList(info.player, "portable", 160);
                return !MyUtils.HasItem(allItems, itemTypes);
            }

            return false;
        }

        public bool CanShowItemDropInUI() => true;

        public string GetConditionDescription() => MyUtils.GetText("ItemDropRule.WandDrop");
    }

    public class SpaceWandDrop : IItemDropRuleCondition
    {
        public bool CanDrop(DropAttemptInfo info) => MyUtils.AvailableConfig.AvailableSpaceWand;

        public bool CanShowItemDropInUI() => MyUtils.AvailableConfig.AvailableSpaceWand;

        public string GetConditionDescription() => MyUtils.GetText("ItemDropRule.DropWhenEnabled",
            MyUtils.GetText("Conditions.AvailableSpaceWand"));
    }

    public class WallPlaceDrop : IItemDropRuleCondition
    {
        public bool CanDrop(DropAttemptInfo info) => MyUtils.AvailableConfig.AvailableWallPlace;

        public bool CanShowItemDropInUI() => MyUtils.AvailableConfig.AvailableWallPlace;

        public string GetConditionDescription() => MyUtils.GetText("ItemDropRule.DropWhenEnabled",
            MyUtils.GetText("Conditions.AvailableWallPlace"));
    }

    public class CoinOneDrop : IItemDropRuleCondition
    {
        public bool CanDrop(DropAttemptInfo info)
            => ImproveConfigs.Instance.NPCCoinDropRate is 25 && MyUtils.AvailableConfig.AvailableCoinOne;

        // 彩蛋式物品，不在UI中显示
        public bool CanShowItemDropInUI() => false;
        // public bool CanShowItemDropInUI() => Config.NPCCoinDropRate is 25 && AvailableConfig.AvailableCoinOne;

        public string GetConditionDescription() => MyUtils.GetText("ItemDropRule.DropCoinOne");
    }
}