// Copyright 2014-2017 ClassicalSharp | Licensed under BSD-3
using System;
using ClassicalSharp.Map;
using BlockRaw = System.Byte;

namespace ClassicalSharp.Singleplayer {

	public class OtherPhysics {
		Game game;
		World map;
		
		public OtherPhysics(Game game, PhysicsBase physics) {
			this.game = game;
			map = game.World;
			physics.OnPlace[Block.Slab] = HandleSlab;
			#if !ALPHA
			physics.OnPlace[Block.CobblestoneSlab] = HandleCobblestoneSlab;
			#else
			#endif
		}
		
		void HandleSlab(Vector3I pos, BlockRaw block) {
			//if (index < map.Width * map.Length) return; // y < 1
			//if (map.blocks1[index - map.Width * map.Length] != Block.Slab) return;
			if (pos.Y < 1) return;
			if (map.GetBlock(pos.X, pos.Y - 1, pos.Z) != Block.Slab) return;
			
			//int x = index % map.Width;
			//int z = (index / map.Width) % map.Length;
			//int y = (index / map.Width) / map.Length;
			game.UpdateBlock(pos.X, pos.Y, pos.Z, Block.Air);
			game.UpdateBlock(pos.X, pos.Y - 1, pos.Z, Block.DoubleSlab);
		}
		
		void HandleCobblestoneSlab(Vector3I pos, BlockRaw block) {
			#if !ALPHA
			//if (index < map.Width * map.Length) return; // y < 1
			//if (map.blocks1[index - map.Width * map.Length] != Block.CobblestoneSlab) return;
			if (pos.Y < 1) return;
			if (map.GetBlock(pos.X, pos.Y - 1, pos.Z) != Block.CobblestoneSlab) return;
			
			//int x = index % map.Width;
			//int z = (index / map.Width) % map.Length;
			//int y = (index / map.Width) / map.Length;
			game.UpdateBlock(pos.X, pos.Y, pos.Z, Block.Air);
			game.UpdateBlock(pos.X, pos.Y - 1, pos.Z, Block.Cobblestone);
			#else
			return;
			#endif
		}
	}
}