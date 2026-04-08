using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace ConstellationsOfOrion.Content.Items.Armor
{
	[AutoloadEquip(EquipType.Head)]
	public class OrionHelmet : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 24;
			Item.defense = 14;
			Item.value = Item.buyPrice(gold: 5);
			Item.rare = ItemRarityID.Pink;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			Player player = Main.LocalPlayer;

			bool inSocialSlot =
				player.armor[10].type == Type;

			if (inSocialSlot)
				return;

			int setBonusIndex = tooltips.FindIndex(t => t.Name == "SetBonus");

			if (setBonusIndex != -1)
			{
				tooltips.Insert(setBonusIndex,
					new TooltipLine(Mod, "DamageBonus", "8% increased damage"));

				tooltips.Insert(setBonusIndex,
					new TooltipLine(Mod, "CritBonus", "8% increased critical strike chance"));
			}
			else
			{
				tooltips.Add(new TooltipLine(Mod, "DamageBonus", "8% increased damage"));
				tooltips.Add(new TooltipLine(Mod, "CritBonus", "8% increased critical strike chance"));
			}
		}

		public override void UpdateEquip(Player player)
		{
			player.GetDamage(DamageClass.Generic) += 0.08f;
			player.GetCritChance(DamageClass.Generic) += 8f;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return body.type == ModContent.ItemType<OrionChestplate>()
				&& legs.type == ModContent.ItemType<OrionLeggings>();
		}

		public override void UpdateArmorSet(Player player)
		{
			player.setBonus =
				"Melee attacks inflict Poisoned and Venom\n" +
				"+200 maximum mana\n" +
				"+3 max minions\n" +
				"20% increased ranged damage";

			player.statManaMax2 += 200;
			player.maxMinions += 3;
			player.GetDamage(DamageClass.Ranged) += 0.20f;

			player.GetModPlayer<OrionPlayer>().orionSet = true;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
                .AddIngredient(ModContent.ItemType<Content.Items.Materials.ConstelliteBar>(), 12)
				.AddIngredient(ItemID.FallenStar, 10)
				.AddIngredient(ItemID.SoulofLight, 5)
				.AddTile(TileID.MythrilAnvil)
				.Register();
		}
	}
}
