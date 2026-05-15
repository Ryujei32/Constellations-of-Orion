// CONTENT/ITEMS/MATERIALS/SUNSTONEORE.CS

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ConstellationsOfOrion.Content.Items.Materials
{
	public class SunstoneOre : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 100;
		}

		public override void SetDefaults()
		{
			Item.width = 14;
			Item.height = 14;

			Item.maxStack = 9999;

			Item.value = Item.sellPrice(silver: 5);
			Item.rare = ItemRarityID.Orange;

			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.useStyle = ItemUseStyleID.Swing;

			Item.consumable = true;

			Item.createTile = ModContent.TileType<Content.Tiles.SunstoneOreTile>();
		}
	}
}