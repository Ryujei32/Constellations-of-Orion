using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace ConstellationsOfOrion.Content.Items.Materials
{
	public class ConstelliteOre : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 20;
			Item.height = 20;
			Item.maxStack = 9999;
			Item.ResearchUnlockCount = 100;

			Item.value = Item.sellPrice(silver: 2);
			Item.rare = ItemRarityID.LightRed;

			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.useStyle = ItemUseStyleID.Swing;

			Item.consumable = true;
			Item.createTile = ModContent.TileType<Tiles.ConstelliteOreTile>();
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "Flavor",
				"\"A fragment of a fallen constellation\""));
		}
	}
}
