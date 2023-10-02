namespace ImproveGame.Content.Items.Coin
{
    public class CoinOne : ModItem
    {
        public override bool IsLoadingEnabled(Mod mod) => Config.LoadModItems.CoinOne;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 5;
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Red;
            Item.value = Item.sellPrice(1);
            Item.maxStack = 9999;
        }
    }
}
