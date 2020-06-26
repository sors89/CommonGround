using Microsoft.Xna.Framework;
using OTAPI.Tile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.Utilities;
using TShockAPI;

namespace CommonGround.Extensions.Terraria
{
	public static class TerraExtensions
	{
		public static bool IsInfectious(this ITile thing) => TileID.Sets.Corrupt[thing.type] || TileID.Sets.Crimson[thing.type] || TileID.Sets.Hallow[thing.type];
		public static bool IsStone(this ITile thing) => TileID.Sets.Conversion.Stone[thing.type];

		public static bool NextBoolean(this UnifiedRandom thing) => thing.Next(2) == 0;
		public static int NextDirection(this UnifiedRandom thing) => thing.NextBoolean() ? 1 : -1;

		public static void Deconstruct(this Point thing, out int x, out int y) { x = thing.X; y = thing.Y; }
		public static void Deconstruct(this Vector2 thing, out float x, out float y) { x = thing.X; y = thing.Y; }

		public static void AddGroup(this List<Command> thing, string perm, params (string name, CommandDelegate action)[] commands)
		{
			foreach (var cmd in commands)
				thing.Add(new Command(perm, cmd.action, cmd.name));
		}
	}
}

namespace CommonGround.Extensions.Lang
{
	public static class LangExtensions
	{
		public static void Let<T>(this T thing, Action<T> action) => action.Invoke(thing);
		public static R Let<T, R>(this T thing, Func<T, R> func) => func.Invoke(thing);

		public static TV GetValue<TK, TV>(this IDictionary<TK, TV> dict, TK key, TV defaultValue = default(TV)) => dict.TryGetValue(key, out TV value) ? value : defaultValue;
		public static V GetValue<K, V>(this ConditionalWeakTable<K, V> thing, K key, V defaultValue = default)
			where K : class
			where V : class
		{
			return thing.TryGetValue(key, out V value) ? value : defaultValue;
		}

		public static void Deconstruct<TK, TV>(this KeyValuePair<TK, TV> thing, out TK key, out TV value) { key = thing.Key; value = thing.Value; }
	}
}