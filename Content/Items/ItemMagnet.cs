using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using ImproveGame.Common.ModPlayers;

namespace ImproveGame.Content.Items
{
	public class ItemMagnet : ModItem //TODO port to 1.4.4
    {
        public static string TooltipKey = "Mods.ItemMagnetPro.Tooltips.";
		public static string SelectionKey = TooltipKey + "Selections.";
		public static string ItemActionKey = TooltipKey + "ItemActions.";
		public static string ApproachKey = TooltipKey + "Approaches.";
        private TooltipLine selection;
		private TooltipLine itemAction;
		private TooltipLine approach;
        public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 30;
			Item.useTime = 24;
			Item.useAnimation = 24;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.value = 12;
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.CoinPickup;
			Item.autoReuse = false;

            selection = new TooltipLine(Mod, "Selection", string.Empty);
            itemAction = new TooltipLine(Mod, "ItemAction", string.Empty);
            approach = new TooltipLine(Mod, "Approach", string.Empty);
			selection.OverrideColor = Color.LawnGreen;
			itemAction.OverrideColor = Color.Yellow;
			approach.OverrideColor = Color.Aqua;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
			if (!Main.LocalPlayer.TryGetModPlayer(out MagPlayer mp))
			{
				return;
			}
			selection.Text = Language.GetTextValue(SelectionKey + mp.Selection.ToString());
			itemAction.Text = Language.GetTextValue(ItemActionKey + mp.ItemAction.ToString());
			approach.Text = Language.GetTextValue(ApproachKey + mp.Approach.ToString());
			int index = 0;
            tooltips.ForEach(t =>
			{
				if (t.Name == "ItemName")
				{
					index = tooltips.IndexOf(t);
                }
			});
            tooltips.Insert(index + 1, selection);
            tooltips.Insert(index + 2, itemAction);
            tooltips.Insert(index + 3, approach);
        }
        public override bool AltFunctionUse(Player player)
		{
			return true;
		}
		public override bool? UseItem(Player player)
		{
			MagPlayer mag = player.GetModPlayer<MagPlayer>();
			if (player.itemAnimation == 1)
			{
				if (player.altFunctionUse == 2)
				{
					mag.ItemAction += 1;
				}
				else
				{
					mag.Selection += 1;
				}
			}
			return base.UseItem(player);
		}
		public override bool CanRightClick()
		{
			return true;
		}
		public override bool ConsumeItem(Player player)
		{
			return false;
		}
		public override void RightClick(Player player)
		{
			player.GetModPlayer<MagPlayer>().Approach += 1;
		}
		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.DirtBlock, 4);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
}