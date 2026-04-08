using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ConstellationsOfOrion.Content.Tiles.Banners
{
	public class StarliteSlimeBannerTile : ModTile
	{
		public override void SetStaticDefaults()
		{
			// ⭐ Required tile settings
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;

			// ⭐ Makes banner sway like vanilla
			TileID.Sets.MultiTileSway[Type] = true;

			// ⭐ Vanilla banner placement (THIS is what matters)
			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2Top);

			// ⭐ Match your sprite (16x48)
			TileObjectData.newTile.Height = 3;
			TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16 };

			// ⭐ Allow multiple styles (Calamity-style system)
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.StyleWrapLimit = 111;

			TileObjectData.addTile(Type);

			// ⭐ Map entry
			AddMapEntry(new Color(200, 100, 255), CreateMapEntryName());

			DustType = DustID.t_Slime;
		}

		// ⭐ Fix slight draw offset (common banner issue)
		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
		{
			offsetY = -2;
		}
	}
}
