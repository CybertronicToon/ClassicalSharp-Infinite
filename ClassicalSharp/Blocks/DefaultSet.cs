// Copyright 2014-2017 ClassicalSharp | Licensed under BSD-3
using System;
using OpenTK;
using BlockID = System.UInt16;

namespace ClassicalSharp.Blocks {
	
	/// <summary> Stores default properties for blocks in Minecraft Classic. </summary>
	public static class DefaultSet {
		
		public static float Height(BlockID b) {
			if (b == Block.Slab) return 8/16f;
			if (b == Block.Snow) return 2/16f;
			#if !ALPHA
			if (b == Block.CobblestoneSlab) return 8/16f;
			#else
			if (b == Block.FarmLand) return 15/16f;
			if (b == Block.StonePressurePlate) return 1/16f;
			if (b == Block.WoodPressurePlate) return 1/16f;
			if (b == Block.Torch ||
			    b == Block.RedstoneTorchOff || b == Block.RedstoneTorchOn) return 10/16f;
			if (b == Block.Redstone) return 1f/16f;
			#endif
			return 1;
		}
		
		public static bool FullBright(BlockID b) {
			#if !ALPHA
			return b == Block.Lava || b == Block.StillLava
				|| b == Block.Magma || b == Block.Fire;
			#else
			return b == Block.Lava || b == Block.StillLava
				|| b == Block.Torch || b == Block.Fire;
			#endif
		}
		
		public static float FogDensity(BlockID b) {
			if (b == Block.Water || b == Block.StillWater)
				return 0.1f;
			if (b == Block.Lava || b == Block.StillLava)
				return 1.8f;
			return 0;
		}
		
		public static FastColour FogColour(BlockID b) {
			if (b == Block.Water || b == Block.StillWater)
				return new FastColour(5, 5, 51);
			if (b == Block.Lava || b == Block.StillLava)
				return new FastColour(153, 25, 0);
			return default(FastColour);
		}
		
		public static byte Collide(BlockID b) {
			if (b == Block.Ice) return CollideType.Ice;
			if (b == Block.Water || b == Block.StillWater)
				return CollideType.LiquidWater;
			if (b == Block.Lava || b == Block.StillLava)
				return CollideType.LiquidLava;
			
			if (b == Block.Snow || b == Block.Air || Draw(b) == DrawType.Sprite)
				return CollideType.Gas;
			
			#if ALPHA
			if (b == Block.Redstone)
				return CollideType.Gas;
			if (b == Block.Ladder)
				return CollideType.ClimbRope;
			if (b == Block.RedstoneTorchOff || b == Block.RedstoneTorchOn
			    || b == Block.Torch)
				return CollideType.Gas;
			#endif
			return CollideType.Solid;
		}
		
		public static byte MapOldCollide(BlockID b, byte collide) {
			#if !ALPHA
			if (b == Block.Rope && collide == CollideType.Gas)
				return CollideType.ClimbRope;
			#else
			if (b == Block.Ladder && collide == CollideType.Gas)
				return CollideType.ClimbRope;
			#endif
			if (b == Block.Ice && collide == CollideType.Solid) 
				return CollideType.Ice;
			if ((b == Block.Water || b == Block.StillWater) && collide == CollideType.Liquid)
				return CollideType.LiquidWater;
			if ((b == Block.Lava || b == Block.StillLava) && collide == CollideType.Liquid)
				return CollideType.LiquidLava;
			return collide;
		}
		
		public static bool BlocksLight(BlockID b) {
			#if !ALPHA
			return !(b == Block.Glass || b == Block.Leaves 
			         || b == Block.Air || Draw(b) == DrawType.Sprite);
			#else
			return !(b == Block.Glass || b == Block.Leaves || b == Block.Ladder
			         || b == Block.Torch || b == Block.Redstone || b == Block.Cactus
			         || b == Block.RedstoneTorchOff || b == Block.RedstoneTorchOn
			         || b == Block.Air || b == Block.Fence || b == Block.WoodPressurePlate
			         || b == Block.StonePressurePlate || Draw(b) == DrawType.Sprite);
			#endif
		}

		public static byte StepSound(BlockID b) {
			if (b == Block.Glass) return SoundType.Stone;
			#if !ALPHA
			if (b == Block.Rope) return SoundType.Cloth;
			#else
			#endif
			if (Draw(b) == DrawType.Sprite) return SoundType.None;
			return DigSound(b);
		}
		
		
		public static byte Draw(BlockID b) {
			if (b == Block.Air) return DrawType.Gas;
			if (b == Block.Leaves) return DrawType.TransparentThick;
			
			#if ALPHA
			if (b == Block.Redstone) return DrawType.Transparent;
			if (b == Block.MobSpawner) return DrawType.TransparentThick;
			if (b == Block.Ladder) return DrawType.Always;
			if (b == Block.Fence) return DrawType.Always;
			#endif

			if (b == Block.Ice || b == Block.Water || b == Block.StillWater) 
				return DrawType.Translucent;
			if (b == Block.Glass || b == Block.Leaves)
				return DrawType.Transparent;
			
			if (b >= Block.Dandelion && b <= Block.RedMushroom)
				return DrawType.Sprite;
			#if !ALPHA
			if (b == Block.Sapling || b == Block.Rope || b == Block.Fire)
				return DrawType.Sprite;
			#else
			if (b == Block.Sapling || b == Block.Fire || b == Block.SugarCane ||
			    b == Block.Wheat)
				return DrawType.Sprite;
			#endif
			return DrawType.Opaque;
		}		

		public static byte DigSound(BlockID b) {
			if (b >= Block.Red && b <= Block.White) 
				return SoundType.Cloth;
			#if !ALPHA
			if (b >= Block.LightPink && b <= Block.Turquoise) 
				return SoundType.Cloth;
			#endif
			if (b == Block.Iron || b == Block.Gold)
				return SoundType.Metal;
			
			#if !ALPHA
			if (b == Block.Bookshelf || b == Block.Wood 
			   || b == Block.Log || b == Block.Crate || b == Block.Fire)
				return SoundType.Wood;
			#else
			if (b == Block.Bookshelf || b == Block.Wood 
			   || b == Block.Log || b == Block.CraftingTable || b == Block.Fire
			   || b == Block.Fence || b == Block.Ladder || b == Block.Door
			   || b == Block.Chest || b == Block.WoodStairs || b == Block.WoodStairs
			   || b == Block.FloorSign || b == Block.WallSign || b == Block.Torch
			   || b == Block.RedstoneTorchOff || b == Block.RedstoneTorchOn
			   || b == Block.Lever || b == Block.Jukebox || b == Block.WoodPressurePlate)
				return SoundType.Wood;
			#endif
			
			#if !ALPHA
			if (b == Block.Rope) return SoundType.Cloth;
			#endif
			if (b == Block.Sand) return SoundType.Sand;
			if (b == Block.Snow) return SoundType.Snow;
			if (b == Block.Glass) return SoundType.Glass;
			if (b == Block.Dirt || b == Block.Gravel)
				return SoundType.Gravel;
			
			if (b == Block.Grass || b == Block.Sapling || b == Block.TNT
			   || b == Block.Leaves || b == Block.Sponge)
				return SoundType.Grass;
			
			if (b >= Block.Dandelion && b <= Block.RedMushroom)
				return SoundType.Grass;
			if (b >= Block.Water && b <= Block.StillLava)
				return SoundType.None;
			#if !ALPHA
			if (b >= Block.Stone && b <= Block.StoneBrick)
				return SoundType.Stone;
			#endif
			return SoundType.None;
		}
	}
}