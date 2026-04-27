using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ConstellationsOfOrion.Content.Tiles.Relics
{
	public class AstreusRelicTile : ModTile
	{
		public const int FrameWidth = 18 * 3;
		public const int FrameHeight = 18 * 4;

		public Asset<Texture2D> RelicTexture;

		// ⭐ THIS POINTS TO YOUR FLOATING HEAD SPRITE
		public virtual string RelicTextureName => "ConstellationsOfOrion/Content/Tiles/Relics/AstreusRelic";

		// ⭐ THIS IS YOUR PEDESTAL TILE
		public override string Texture => "ConstellationsOfOrion/Content/Tiles/Relics/AstreusRelicTile";

		public override void Load()
		{
			RelicTexture = ModContent.Request<Texture2D>(RelicTextureName);
		}

		public override void SetStaticDefaults()
		{
			Main.tileShine[Type] = 400;
			Main.tileFrameImportant[Type] = true;
			TileID.Sets.InteractibleByNPCs[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.DrawYOffset = 2;
			TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;

			TileObjectData.newTile.StyleHorizontal = false;

			TileObjectData.newTile.StyleWrapLimitVisualOverride = 2;
			TileObjectData.newTile.StyleMultiplier = 2;
			TileObjectData.newTile.StyleWrapLimit = 2;
			TileObjectData.newTile.styleLineSkipVisualOverride = 0;

			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
			TileObjectData.addAlternate(1);

			TileObjectData.addTile(Type);

			AddMapEntry(new Color(233, 207, 94), Language.GetText("MapObject.Relic"));
		}

		public override bool CreateDust(int i, int j, ref int type)
		{
			return false;
		}

		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
		{
			tileFrameX %= FrameWidth;
			tileFrameY %= FrameHeight * 2;
		}

		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
		{
			if (drawData.tileFrameX % FrameWidth == 0 && drawData.tileFrameY % FrameHeight == 0)
			{
				Main.instance.TilesRenderer.AddSpecialPoint(i, j, Terraria.GameContent.Drawing.TileDrawing.TileCounterType.CustomNonSolid);
			}
		}

		public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
		{
			Point p = new Point(i, j);
			Tile tile = Main.tile[p.X, p.Y];

			if (!tile.HasTile)
				return;

			Texture2D texture = RelicTexture.Value;

			Rectangle frame = texture.Frame();

			Vector2 origin = frame.Size() / 2f;
			Vector2 worldPos = p.ToWorldCoordinates(24f, 64f);

			Color color = Lighting.GetColor(p.X, p.Y);

			bool direction = tile.TileFrameY / FrameHeight != 0;
			SpriteEffects effects = direction ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

			const float TwoPi = (float)Math.PI * 2f;

			float offset = (float)Math.Sin(Main.GlobalTimeWrappedHourly * TwoPi / 5f);

			Vector2 drawPos = worldPos - Main.screenPosition +
				new Vector2(0f, -40f + offset * 4f);

			// ⭐ DRAW HEAD
			spriteBatch.Draw(texture, drawPos, frame, color, 0f, origin, 1f, effects, 0f);

			// ⭐ GLOW EFFECT
			float scale = (float)Math.Sin(Main.GlobalTimeWrappedHourly * TwoPi / 2f) * 0.3f + 0.7f;

			Color glow = color;
			glow.A = 0;
			glow *= 0.2f * scale;

			for (float i2 = 0; i2 < 1f; i2 += 0.25f)
			{
				Vector2 offsetGlow = (TwoPi * i2).ToRotationVector2() * 6f;
				spriteBatch.Draw(texture, drawPos + offsetGlow, frame, glow, 0f, origin, 1f, effects, 0f);
			}
		}
	}
}
