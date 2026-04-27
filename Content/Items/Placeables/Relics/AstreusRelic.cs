using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace ConstellationsOfOrion.Content.Items.Placeables.Relics
{
	public class AstreusRelic : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 1;
		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 40;

			Item.maxStack = 9999;

			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.useStyle = ItemUseStyleID.Swing;

			Item.consumable = true;

			Item.rare = ItemRarityID.Master;
			Item.master = true;

			Item.createTile = ModContent.TileType<Content.Tiles.Relics.AstreusRelicTile>();
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "Relic",
				"A celestial slime that ruled the stars"));
		}
	}
}
