// ============================================
// CONTENT/TILES/SUNSTONEBARTILE.CS
// ============================================

using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ConstellationsOfOrion.Content.Tiles
{
	public class SunstoneBarTile : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileSolidTop[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileTable[Type] = true;
			Main.tileLavaDeath[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.LavaDeath = true;

			TileObjectData.addTile(Type);

			AddMapEntry(new Color(221, 193, 122), CreateMapEntryName());

			DustType = DustID.GoldFlame;
			HitSound = SoundID.Tink;
		}
	}
}