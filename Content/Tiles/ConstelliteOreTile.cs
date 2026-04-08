using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;

namespace ConstellationsOfOrion.Content.Tiles
{
	public class ConstelliteOreTile : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = true;
			Main.tileMergeDirt[Type] = false;
			Main.tileBlockLight[Type] = true;

			Main.tileSpelunker[Type] = true;
			Main.tileOreFinderPriority[Type] = 420;
			TileID.Sets.Ore[Type] = true;

			MinPick = 180;
			MineResist = 5f;

			AddMapEntry(
				new Color(180, 80, 255),
				Language.GetText("Mods.ConstellationsOfOrion.Tiles.ConstelliteOreTile.MapEntry")
			);

			DustType = DustID.GemAmethyst;
			HitSound = SoundID.Tink;

			RegisterItemDrop(ModContent.ItemType<Content.Items.Materials.ConstelliteOre>());
		}
	}
}
