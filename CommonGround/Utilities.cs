using CommonGround.Extensions.Lang;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using TShockAPI;

namespace CommonGround.Utilities
{
	/// <summary>
	/// Utilies for gathering information from and interacting with a Terraria world.
	/// </summary>
	public static class GroundUtils
	{
		/// <summary>
		/// Finds all item stacks with the given item ID.
		/// </summary>
		/// <param name="itemID"></param>
		/// <returns>A list of tuples pairing items to their index in the Main array.</returns>
		public static IEnumerable<(Item item, int index)> GetItemsByType(int itemID) =>
			Main.item
				.Select((item, index) => (item, index))
				.Where(t => t.item.type == itemID && t.item.stack > 0);

		/// <summary>
		/// Removes a specified number of items from a stack.
		/// </summary>
		/// <param name="needed">The desired number of items to remove</param>
		/// <param name="index">The Main array index of the target stack</param>
		/// <param name="goPoof">If true, a puff of smoke will be emitted if the stack is fully drained</param>
		/// <param name="bounceLeftovers">If true, the remaining stack will be bounced into the air</param>
		/// <returns>The difference of the desired number of items and the number that were removed</returns>
		public static int ConsumeItemsFromStack(int needed, int index, bool goPoof = false, bool bounceLeftovers = false)
		{
			var item = Main.item[index];

			// Determine how many are coming off of this stack
			int taking = Math.Min(item.stack, needed);

			// Update the stack
			item.stack -= taking;
			if (item.stack <= 0)
			{
				item.SetDefaults();
				item.active = false;

				if (goPoof)
					TSPlayer.All.SendData(PacketTypes.PoofOfSmoke, number: (int)item.Center.X, number2: (int)item.Center.Y);
			}
			else if (bounceLeftovers)
			{
				item.velocity.Y = Main.rand.Next(-25, -12) * 0.1f;
			}

			TSPlayer.All.SendData(PacketTypes.UpdateItemDrop, number: index);

			return needed - taking;
		}

		/// <summary>
		/// Removes a specified number of items from a list of stacks.
		/// </summary>
		/// <param name="remaining">The desired number of items to remove</param>
		/// <param name="items">A list of tuples pairing item stacks to their IDs</param>
		/// <param name="goPoof">If true, a puff of smoke will be emitted if the stack is fully drained</param>
		/// <param name="bounceLeftovers">If true, the remaining stack will be bounced into the air</param>
		/// <returns>The difference of the desired number of items and the number that were removed</returns>
		public static int ConsumeAvailableItems(int remaining, IEnumerable<(Item item, int index)> items, bool goPoof = false, bool bounceLeftovers = false)
		{
			// Consume as many as we need
			foreach (var (item, index) in items)
			{
				remaining = ConsumeItemsFromStack(remaining, index, goPoof, bounceLeftovers);

				// See if we're done
				if (remaining <= 0)
					break;
			}

			return remaining;
		}

		/// <summary>
		/// Attempts to consume exactly the specified number of items from a list of stacks, or else consumes none.
		/// </summary>
		/// <param name="count">The desired number of items to remove</param>
		/// <param name="items">A list of tuples pairing item stacks to their IDs</param>
		/// <param name="goPoof">If true, a puff of smoke will be emitted if the stack is fully drained</param>
		/// <param name="bounceLeftovers">If true, the remaining stack will be bounced into the air</param>
		/// <returns>Whether the desired number of items could be consumed</returns>
		public static bool ConsumeItems(int count, IEnumerable<(Item item, int index)> items, bool goPoof = false, bool bounceLeftovers = false)
		{
			// Bail if we don't have enough to cover the whole amount
			if (items.Select(t => t.item.stack).Sum() < count)
				return false;

			// Consume the items
			ConsumeAvailableItems(count, items, goPoof, bounceLeftovers);
			return true;
		}

		/// <summary>
		/// Finds a (probably) safe location for teleporation near the specified tile.
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public static Vector2 FindProbablySafeTeleportLocation(Point target)
		{
			// Look for a spot to teleport to
			var settings = new Player.RandomTeleportationAttemptSettings()
			{
				attemptsBeforeGivingUp = 1000,
				avoidAnyLiquid = false,
				avoidHurtTiles = true,
				avoidLava = true,
				avoidWalls = false,
				maximumFallDistanceFromOrignalPoint = 30,
				mostlySolidFloor = true
			};

			var destPos = Vector2.Zero;
			bool canSpawn = false;
			int rangeX = 100;
			int halfRangeX = rangeX / 2;
			int rangeY = 80;

			// TODO I'm not sure why CheckForGoodTeleportationSpot is an instance method, since it seems like the dimensions of a
			// Player object are always the same. Worth double-checking later, but in the meantime we'll just take a random player
			// and call it a day.
			Main.player.Where(p => p.active).FirstOrDefault()?.Let(player =>
			{
				destPos = player.CheckForGoodTeleportationSpot(ref canSpawn, target.X - halfRangeX, rangeX, target.Y, rangeY, settings);
				if (!canSpawn)
					destPos = player.CheckForGoodTeleportationSpot(ref canSpawn, target.X - rangeX, halfRangeX, target.Y, rangeY, settings);
				if (!canSpawn)
					destPos = player.CheckForGoodTeleportationSpot(ref canSpawn, target.X + halfRangeX, halfRangeX, target.Y, rangeY, settings);
			});


			if (canSpawn)
				return destPos;
			else
			{
				TShock.Log.Warn("Fell back to a random teleportation location; this shouldn't happen except very rarely");
				return new Vector2(target.X + Main.rand.NextFloatDirection() * rangeX, target.Y + Main.rand.NextFloatDirection() * rangeY);
			}
		}

		/// <summary>
		/// Plays fairy disappearing effects at the specified position for all clients.
		/// </summary>
		/// <param name="pos">The world coordinate at which to play the effect</param>
		/// <param name="color">The desired color of the effect (0 for pink, 1 for blue, 2 for green); omit for a random color</param>
		public static void DoFairyFX(Vector2 pos, int color = -1) => TSPlayer.All.SendData(PacketTypes.TreeGrowFX, null, 2, pos.X, pos.Y, color >= 0 ? color : Main.rand.Next(3));
	}
}