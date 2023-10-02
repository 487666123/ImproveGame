using Terraria.GameContent.ItemDropRules;

namespace ImproveGame.Common.GlobalNPCs
{
    public class ImproveNPC : GlobalNPC
    {
        public override void SetDefaults(NPC npc)
        {
            npc.value *= Config.NPCCoinDropRate;
        }

        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            if (npc.type == NPCID.KingSlime)
            {
                int itemType = ModContent.ItemType<Content.Items.SpaceWand>();
                npcLoot.Add(new DropPerPlayerOnThePlayer(itemType, 1, 1, 1, new WandDrop(itemType)));
                itemType = ModContent.ItemType<Content.Items.WallPlace>();
                npcLoot.Add(new DropPerPlayerOnThePlayer(itemType, 1, 1, 1, new WandDrop(itemType)));
            }

            // 钱币掉落倍率开到最大时（x25）有 0.5% 概率掉落幸运金币，它唯一的作用就是出售。
            if (!Config.LoadModItems.CoinOne) return;
            var leadingRule = new LeadingConditionRule(new CoinOneDrop());
            leadingRule.OnSuccess(new CommonDrop(ModContent.ItemType<Content.Items.Coin.CoinOne>(), 200));
            npcLoot.Add(leadingRule);
        }
    }

    public class WandDrop : IItemDropRuleCondition
    {
        public int itemType;
        public WandDrop(int itemType)
        {
            this.itemType = itemType;
        }

        public bool CanDrop(DropAttemptInfo info)
        {
            if (!info.IsInSimulation)
            {
                return !info.player.HasItem(itemType);
            }
            return false;
        }

        public bool CanShowItemDropInUI() => true;

        public string GetConditionDescription() => GetText("ItemDropRule.WandDrop");
    }
    
    public class CoinOneDrop : IItemDropRuleCondition
    {
        public bool CanDrop(DropAttemptInfo info)
            => Config.NPCCoinDropRate is 25;

        public bool CanShowItemDropInUI() => Config.NPCCoinDropRate is 25;
        
        public string GetConditionDescription() => null;
    }
}
