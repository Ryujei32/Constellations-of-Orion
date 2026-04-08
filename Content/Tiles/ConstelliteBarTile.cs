using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ConstellationsOfOrion.Content.Tiles
{
	public class ConstelliteBarTile : ModTile
	{
		public override void SetStaticDefaults()
		{
			// ⭐ BEHAVIOR (same as vanilla bars)
			Main.tileSolidTop[Type] = true;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileTable[Type] = true;

			// ⭐ THIS MAKES IT PLACE CORRECTLY (1x1 bar)
			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.LavaDeath = true;
			TileObjectData.addTile(Type);

			// ⭐ MAP COLOR
			AddMapEntry(new Color(200, 100, 255), CreateMapEntryName());

			DustType = DustID.GemAmethyst;
			HitSound = SoundID.Tink;
		}
	}
}
