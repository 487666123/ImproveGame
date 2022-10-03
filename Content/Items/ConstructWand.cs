﻿using ImproveGame.Common.ConstructCore;
using ImproveGame.Common.Systems;
using ImproveGame.Interface.Common;
using ImproveGame.Interface.GUI;
using System.Threading;

namespace ImproveGame.Content.Items
{
    public class ConstructWand : SelectorItem
    {
        public override bool ModifySelectedTiles(Player player, int i, int j) => true;

        public override void PostModifyTiles(Player player, int minI, int minJ, int maxI, int maxJ)
        {
            var rectangle = new Rectangle(minI, minJ, maxI - minI, maxJ - minJ);
            var saveTileThread = new Thread(() => FileOperator.SaveAsFile(rectangle));
            saveTileThread.Start();
        }

        public override void SetItemDefaults()
        {
            Item.rare = ItemRarityID.Lime;
            Item.value = Item.sellPrice(0, 1, 0, 0);
            Item.mana = 20;

            SelectRange = new(200, 200);
        }

        public override bool StartUseItem(Player player)
        {
            if (player.altFunctionUse == 0)
            {
                ItemRotation(player);

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
    }
}