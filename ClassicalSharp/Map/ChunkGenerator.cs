// Copyright 2014-2017 ClassicalSharp | Licensed under BSD-3
using System;
using ClassicalSharp.Events;
using ClassicalSharp.Renderers;
using ClassicalSharp.Generator;
using OpenTK;
using BlockID = System.UInt16;
using BlockRaw = System.Byte;

namespace ClassicalSharp.Map {
	public class ChunkGenerator {
		
		Game game;
		
		public long Seed;
		
		long chunkXMul, chunkZMul, chunkSeed, chunkDecSeed;
		
		const short waterLevel = 64;
		const int Height = 128;
		const int oneY = 16 * 16;
		
		int minHeight;
		
		public JavaRandom rnd, rnd2;
		
		ChunkHandler ChunkHandler;
		
		public ChunkGenerator(ChunkHandler ChunkHandler) {
			rnd = new JavaRandom();
			rnd2 = new JavaRandom();
			this.Seed = rnd.GetSeed();
			//chunkXMul = rnd.nextLong();
			//chunkZMul = rnd.nextLong();
			
			n1 = new CombinedNoise(
				new OctaveNoise(8, rnd), new OctaveNoise(8, rnd));
			n2 = new CombinedNoise(
				new OctaveNoise(8, rnd), new OctaveNoise(8, rnd));
			n3 = new OctaveNoise(6, rnd);
			
			n4 = new OctaveNoise(8, rnd);
			
			n5 = new OctaveNoise(8, rnd);
			n6 = new OctaveNoise(8, rnd);
			
			n7 = new OctaveNoise(8, rnd);
			
			this.ChunkHandler = ChunkHandler;
		}
		
		public ChunkGenerator(ChunkHandler ChunkHandler, long Seed) {
			rnd = new JavaRandom();
			rnd2 = new JavaRandom();
			rnd.SetSeed(Seed);
			this.Seed = Seed;
			chunkXMul = rnd.nextLong();
			chunkZMul = rnd.nextLong();
			
			n1 = new CombinedNoise(
				new OctaveNoise(8, rnd), new OctaveNoise(8, rnd));
			n2 = new CombinedNoise(
				new OctaveNoise(8, rnd), new OctaveNoise(8, rnd));
			n3 = new OctaveNoise(6, rnd);
			
			n4 = new OctaveNoise(8, rnd);
			
			n5 = new OctaveNoise(8, rnd);
			n6 = new OctaveNoise(8, rnd);
			
			n7 = new OctaveNoise(8, rnd);
			
			this.ChunkHandler = ChunkHandler;
		}
		
		CombinedNoise n1, n2;
		
		OctaveNoise n3, n4, n5, n6, n7;
		
		short[] heightmap;
		
		public void GenerateChunk(ref Chunk chunk, int chunkX, int chunkZ) {
			chunkSeed = (chunkX * chunkXMul) ^ (chunkZ * chunkZMul) ^ Seed;
			
			heightmap = new short[16*16];
			minHeight = Height;
			
			CreateHeightmap(ref chunk, chunkX, chunkZ, n1, n2, n3);
			CreateStrata(ref chunk, chunkX, chunkZ, n4);
			FloodFillWater(ref chunk);
			CreateSurfaceLayer(ref chunk, chunkX, chunkZ, n5, n6);
			
		}
		
		int decX, decZ;
		
		public void DecorateChunk(int chunkX, int chunkZ) {
			chunkDecSeed = (chunkX) ^ (chunkZ) ^ Seed;
			rnd2.SetSeed(chunkDecSeed);
			
			decX = chunkX * 16;
			decX -= 1;
			decZ = chunkZ * 16;
			
			PlantTrees(chunkX, chunkZ, n7);
		}
		
		public int GetIndex(int x, int y, int z) {
			#if ALPHA
			//return (y + (z * 128) + (x * 128 * 16));
			return (y + 128 *(z + 16 * x));
			#else
			return ((y * 16 + z) * 16 + x);
			#endif
		}
		
		public Vector3I GetCoords(int index) {
			Vector3I result;
			#if ALPHA
			result.X = index >> 11;
			result.Y = index & 0x7F;
			result.Z = (index >> 7) & 0x0F;
			#else
			result.X = index % 16;
			result.Y = index / oneY;
			result.Z = (index / 16) % 16;
			#endif
			return result;
		}
		
		void CreateHeightmap(ref Chunk chunk, int chunkX, int chunkZ, CombinedNoise n1, CombinedNoise n2, OctaveNoise n3) {
			int index = 0;
			int offsetX = (chunkX * 16);
			int offsetZ = (chunkZ * 16);
			
			for (int z = 0; z < 16; z++) {
				int zCur = z + offsetZ;
				for (int x = 0; x < 16; x++) {
					int xCur = x + offsetX;
					double hLow = n1.Compute(xCur * 1.3f, zCur * 1.3f) / 6 - 4, height = hLow;
					
					if (n3.Compute(xCur, zCur) <= 0) {
						double hHigh = n2.Compute(xCur * 1.3f, zCur * 1.3f) / 5 + 6;
						height = Math.Max(hLow, hHigh);
					}
					
					height *= 0.5;
					if (height < 0) height *= 0.8f;
					
					short adjHeight = (short)(height + waterLevel);
					minHeight = adjHeight < minHeight ? adjHeight : minHeight;
					index = z * 16 + x;
					heightmap[index] = adjHeight;
				}
			}
			//heightmap = hMap;
		}
		
		void CreateStrata(ref Chunk chunk, int chunkX, int chunkZ, OctaveNoise n) {
			int maxY = Height - 1, mapIndex = 0;
			// Try to bulk fill bottom of the map if possible
			int minStoneY = CreateStrataFast(ref chunk, chunkX, chunkZ);
			int offsetX = (chunkX * 16);
			int offsetZ = (chunkZ * 16);

			for (int z = 0; z < 16; z++) {
				int zCur = z + offsetZ;
				for (int x = 0; x < 16; x++) {
					int xCur = x + offsetX;
					int index = z * 16 + x;
					int dirtThickness = (int)(n.Compute(x, z) / 24 - 4);
					int dirtHeight = heightmap[index];
					int stoneHeight = dirtHeight + dirtThickness;	
					
					stoneHeight = Math.Min(stoneHeight, maxY);
					dirtHeight  = Math.Min(dirtHeight,  maxY);
					
					mapIndex = GetIndex(x, minStoneY, z);
					for (int y = minStoneY; y <= stoneHeight; y++) {
						chunk.blocks[mapIndex] = Block.Stone; mapIndex = GetIndex(x, y + 1, z);
					}
					
					stoneHeight = Math.Max(stoneHeight, 0);
					mapIndex = GetIndex(x, stoneHeight + 1, z);
					for (int y = stoneHeight + 1; y <= dirtHeight; y++) {
						chunk.blocks[mapIndex] = Block.Dirt; mapIndex = GetIndex(x, y, z);
					}
				}
			}
		}
		
		int CreateStrataFast(ref Chunk chunk, int chunkX, int chunkZ) {
			// Make lava layer at bottom
			int index = 0;
			int offsetX = (chunkX * 16);
			int offsetZ = (chunkZ * 16);
			for (int z = 0; z < 16; z++)
				for (int x = 0; x < 16; x++)
			{
				index = GetIndex(x, 0, z);
				chunk.blocks[index] = Block.Lava;
			}
			
			// Invariant: the lowest value dirtThickness can possible be is -14
			int stoneHeight = minHeight - 14;
			if (stoneHeight <= 0) return 1; // no layer is fully stone
			
			// We can quickly fill in bottom solid layers
			for (int y = 1; y <= stoneHeight; y++)
				for (int z = 0; z < 16; z++)
					for (int x = 0; x < 16; x++)
			{
				index = GetIndex(x, y, z);
				chunk.blocks[index] = Block.Stone;
			}
			return stoneHeight;
		}
		
		void FloodFillWater(ref Chunk chunk) {
			int waterY = waterLevel - 1;
			int index1 = GetIndex(0, waterY, 0);
			int index2 = GetIndex(0, waterY, (16 - 1));
			
			for (int x = 0; x < 16; x++) {
				index1 = GetIndex(x, waterY, 0);
				index2 = GetIndex(x, waterY, (16 - 1));
				FloodFill(index1, Block.Water, chunk);
				FloodFill(index2, Block.Water, chunk);
				//index1++; index2++;
			}
			
			index1 = GetIndex(0, waterY, 0);
			index2 = GetIndex((16 - 1), waterY, 0);
			for (int z = 0; z < 16; z++) {
				index1 = GetIndex(0, waterY, z);
				index2 = GetIndex((16 - 1), waterY, z);
				FloodFill(index1, Block.Water, chunk);
				FloodFill(index2, Block.Water, chunk);
				//index1 += 16; index2 += 16;
			}
		}
		
		void FloodFill(int startIndex, BlockRaw block, Chunk chunk) {
			if (startIndex < 0) return; // y below map, immediately ignore
			Generator.NotchyGenerator.FastIntStack stack = new Generator.NotchyGenerator.FastIntStack(4);
			stack.Push(startIndex);
			
			while (stack.Size > 0) {
				int index = stack.Pop();
				if (chunk.blocks[index] != Block.Air) continue;
				chunk.blocks[index] = block;
				
				Vector3I coords = GetCoords(index);
				int x = coords.X;
				int y = coords.Y;
				int z = coords.Z;
				
				if (x > 0) stack.Push(GetIndex(x - 1, y, z));
				if (x < 16 - 1) stack.Push(GetIndex(x + 1, y, z));
				if (z > 0) stack.Push(GetIndex(x, y, z - 1));
				if (z < 16 - 1) stack.Push(GetIndex(x, y, z + 1));
				if (y > 0) stack.Push(GetIndex(x, y - 1, z));
			}
		}
		
		void CreateSurfaceLayer(ref Chunk chunk, int chunkX, int chunkZ, OctaveNoise n1, OctaveNoise n2) {
			// TODO: update heightmap
			int offsetX = (chunkX * 16);
			int offsetZ = (chunkZ * 16);
			for (int z = 0; z < 16; z++) {
				int zCur = z + offsetZ;
				for (int x = 0; x < 16; x++) {
					int xCur = x + offsetX;
					int hMapIndex = z * 16 + x;
					int y = heightmap[hMapIndex];
					if (y < 0 || y >= Height) continue;
					
					int index = GetIndex(x, y, z);
					BlockRaw blockAbove = y >= (Height - 1) ? Block.Air : chunk.blocks[GetIndex(x, y + 1, z)];
					if (blockAbove == Block.Water && (n2.Compute(xCur, zCur) > 12)) {
						chunk.blocks[index] = Block.Gravel;
					} else if (blockAbove == Block.Air) {
						chunk.blocks[index] = (y <= waterLevel && (n1.Compute(xCur, zCur) > 8)) ? Block.Sand : Block.Grass;
					}
				}
			}
		}
		
		int GetMaxBlock(int x, int z) {
			for (int i = 127; i >= 0; i--) {
				BlockRaw block = ChunkHandler.GetBlockAdjSafe(x, i, z);
				if (block != Block.Air) return i;
			}
			return 0;
		}
		
		void PlantTrees(int chunkX, int chunkZ, OctaveNoise n1) {
			int numTrees = (int)Math.Round(n1.Compute(chunkX * 8, chunkZ * 8), MidpointRounding.AwayFromZero);
			
			for (int i = 0; i < numTrees; i++) {
				int patchX = (decX + 8), patchZ = (decZ + 8);
				
				//for (int j = 0; j < 20; j++) {
					int treeX = patchX, treeZ = patchZ;
					for (int k = 0; k < 20; k++) {
						treeX += rnd2.Next(15);
						treeZ += rnd2.Next(15);
						if (treeX < patchX || treeZ < patchZ || treeX >= (patchX + 16) ||
						    treeZ >= (patchZ + 16) || rnd2.NextFloat() >= 0.25)
							continue;
						
						//int treeY = heightmap[treeZ * Width + treeX] + 1;
						int treeY = GetMaxBlock(treeX, treeZ) + 1;
						if (treeY >= Height) continue;
						int treeHeight = 5 + rnd.Next(3);
						
						//int index = (treeY * Length + treeZ) * Width + treeX;
						BlockRaw blockUnder = treeY > 0 ? ChunkHandler.GetBlockAdjSafe(treeX, treeY - 1, treeZ) : Block.Air;
						
						if (blockUnder == Block.Grass && CanGrowTree(treeX, treeY, treeZ, treeHeight)) {
							GrowTree(treeX, treeY, treeZ, treeHeight);
						}
					}
				//}
			}
		}
		
		bool CanGrowTree(int treeX, int treeY, int treeZ, int treeHeight) {
			// check tree base
			int baseHeight = treeHeight - 4;
			for (int y = treeY; y < treeY + baseHeight; y++)
				for (int z = treeZ - 1; z <= treeZ + 1; z++)
					for (int x = treeX - 1; x <= treeX + 1; x++)
			{
				if (ChunkHandler.GetBlockAdjSafe(x, y, z) != 0 &&
				    ChunkHandler.GetBlockAdjSafe(x, y, z) != Block.Leaves) return false;
			}
			
			// and also check canopy
			for (int y = treeY + baseHeight; y < treeY + treeHeight; y++)
				for (int z = treeZ - 2; z <= treeZ + 2; z++)
					for (int x = treeX - 2; x <= treeX + 2; x++)
			{
				if (ChunkHandler.GetBlockAdjSafe(x, y, z) != 0 &&
				    ChunkHandler.GetBlockAdjSafe(x, y, z) != Block.Leaves) return false;
			}
			return true;
		}
		
		void GrowTree(int treeX, int treeY, int treeZ, int height) {
			int baseHeight = height - 4;
			
			// leaves bottom layer
			for (int y = treeY + baseHeight; y < treeY + baseHeight + 2; y++)
				for (int zz = -2; zz <= 2; zz++)
					for (int xx = -2; xx <= 2; xx++)
			{
				int x = xx + treeX, z = zz + treeZ;
				
				if (Math.Abs(xx) == 2 && Math.Abs(zz) == 2) {
					if (rnd.NextFloat() >= 0.5)
						ChunkHandler.SetBlockAdj(x, y, z, Block.Leaves);
				} else {
					ChunkHandler.SetBlockAdj(x, y, z, Block.Leaves);
				}
			}
			
			// leaves top layer
			int bottomY = treeY + baseHeight + 2;
			for (int y = treeY + baseHeight + 2; y < treeY + height; y++)
				for (int zz = -1; zz <= 1; zz++)
					for (int xx = -1; xx <= 1; xx++)
			{
				int x = xx + treeX, z = zz + treeZ;

				if (xx == 0 || zz == 0) {
					ChunkHandler.SetBlockAdj(x, y, z, Block.Leaves);
				} else if (y == bottomY && rnd.NextFloat() >= 0.5) {
					ChunkHandler.SetBlockAdj(x, y, z, Block.Leaves);
				}
			}
			
			// then place trunk
			for (int y = 0; y < height - 1; y++) {
				//blocks[index] = Block.Log;
				ChunkHandler.SetBlockAdj(treeX, (treeY + y), treeZ, Block.Log);
			}
		}
	}
}
