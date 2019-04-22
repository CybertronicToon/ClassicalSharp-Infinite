// Copyright 2014-2017 ClassicalSharp | Licensed under BSD-3
using System;
using System.Collections.Generic;
using ClassicalSharp.Map;
using BlockRaw = System.Byte;

namespace ClassicalSharp.Singleplayer {

	public class FallingPhysics {
		Game game;
		PhysicsBase physics;
		World map;
		int width, length, height, oneY;
		
		public FallingPhysics(Game game, PhysicsBase physics) {
			this.game = game;
			map = game.World;
			this.physics = physics;
			
			physics.OnPlace[Block.Sand] = DoFalling;
			physics.OnPlace[Block.Gravel] = DoFalling;
			physics.OnActivate[Block.Sand] = DoFalling;
			physics.OnActivate[Block.Gravel] = DoFalling;
			physics.OnRandomTick[Block.Sand] = DoFalling;
			physics.OnRandomTick[Block.Gravel] = DoFalling;
		}
		
		public void ResetMap() {
			width = map.Width;
			height = map.Height;
			length = map.Length;
			oneY = width * length;
		}

		void DoFalling(Vector3I pos, BlockRaw block) {
			int start = pos.Y, y = pos.Y;
			Vector3I found = new Vector3I(-1, -1, -1);
			// Find lowest air block
			//while (index >= oneY) {
			while (y >= 1) {
				//index -= oneY;
				y -= 1;
				BlockRaw other = (BlockRaw)map.GetBlock(pos.X, y, pos.Z);
				if (other == Block.Air || (other >= Block.Water && other <= Block.StillLava))
					found = new Vector3I(pos.X, y, pos.Z);
				else
					break;
			}
			if (found.X == -1 || found.Y == -1 || found.Z == -1) return;

			//int x = found % width;
			//int y = found / oneY; // posIndex / (width * length)
			//int z = (found / width) % length;
			game.UpdateBlock(found.X, found.Y, found.Z, block);
			
			//x = start % width;
			//y = start / oneY; // posIndex / (width * length)
			//z = (start / width) % length;
			game.UpdateBlock(pos.X, pos.Y, pos.Z, Block.Air);
			physics.ActivateNeighbours(pos.X, pos.Y, pos.Z, start);
		}
	}
}