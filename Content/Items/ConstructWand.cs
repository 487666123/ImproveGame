using ImproveGame.Common.Conditions;
using ImproveGame.Common.GlobalItems;
using ImproveGame.Common.ModSystems;
using ImproveGame.Content.Functions.Construction;
using ImproveGame.Core;
using ImproveGame.UI;
using ImproveGame.UIFramework;
using System.Threading;

namespace ImproveGame.Content.Items
{
    public class ConstructWand : SelectorItem, IConditionItem
    {
        public Condition UseCondition => ConfigCondition.AvailableConstructWandC;

        public override bool ModifySelectedTiles(Player player, int i, int j) => true;

        public override void PostModifyTiles(Player player, int minI, int minJ, int maxI, int maxJ)
        {
            var rectangle = new Rectangle(minI, minJ, maxI - minI, maxJ - minJ);
            var saveTileThread = new Thread(() =>
            {
                TipRenderer.CurrentState = TipRenderer.State.Saving;
                FileOperator.SaveAsFile(rectangle);
                SoundEngine.PlaySound(SoundID.ResearchComplete);
                TipRenderer.CurrentState = TipRenderer.State.Saved;
                TipRenderer.TextDisplayCountdown = 60;
            });
            saveTileThread.Start();
        }

        public override void SetItemDefaults()
        {
            Item.rare = ItemRarityID.Red;
            Item.value = Item.sellPrice(0, 8, 0, 0);
            // Item.mana = 20;

            SelectRange = new Point(200, 200);
            UseNewThread = true;
        }

        public override bool StartUseItem(Player player)
        {
            if (player.altFunctionUse == 0)
            {
                MyUtils.ItemRotation(player);

                if (!Main.dedServ && Main.myPlayer == player.whoAmI && // 多人客户端或者单人
                    WandSystem.ConstructMode is WandSystem.Construct.Place && // 模式为放置
                    !string.IsNullOrEmpty(WandSystem.ConstructFilePath) && // 有路径
                    File.Exists(WandSystem.ConstructFilePath) && // 路径存在文件
                    CoroutineSystem.GenerateRunner.Count is 0) // 没有任务
                {
                    var tag = FileOperator.GetTagFromFile(WandSystem.ConstructFilePath);

                    if (tag is null)
                        return false;

                    GenerateCore.GenerateFromTag(tag, Main.MouseWorld.ToTileCoordinates());
                }
                else if (CoroutineSystem.GenerateRunner.Count > 0) // 有任务 还使用了
                {
                    CoroutineSystem.GenerateRunner.StopAll();

                    TipRenderer.CurrentState = TipRenderer.State.Stopped;
                    TipRenderer.TextDisplayCountdown = 60;
                }
            }
            else if (player.altFunctionUse == 2)
            {
                if (StructureGUI.Visible)
                    UISystem.Instance.StructureGUI.Close();
                else
                    UISystem.Instance.StructureGUI.Open();
                return false;
            }

            return base.StartUseItem(player);
        }

        public override bool IsNeedKill() => !Main.mouseLeft;

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseSelector(Player player)
        {
            return WandSystem.ConstructMode is WandSystem.Construct.Save;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddRecipeGroup(RecipeSystem.AnyMythrilBar, 18)
                .AddIngredient(ItemID.Amber, 8)
                .AddRecipeGroup(RecipeGroups.Wood, 80)
                .AddIngredient(ItemID.StoneBlock, 80)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
