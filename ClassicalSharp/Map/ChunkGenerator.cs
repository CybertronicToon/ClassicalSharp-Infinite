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
		
		long chunkXMul, chunkZMul, chunkSeed;
		
		const short waterLevel = 64;
		const int Height = 128;
		const int oneY = 16 * 16;
		
		int minHeight;
		
		public JavaRandom rnd;
		
		ChunkHandler ChunkHandler;
		
		public ChunkGenerator(ChunkHandler ChunkHandler) {
			rnd = new JavaRandom();
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
			
			this.ChunkHandler = ChunkHandler;
		}
		
		public ChunkGenerator(ChunkHandler ChunkHandler, long Seed) {
			rnd = new JavaRandom();
			rnd.SetSeed(Seed);
			this.Seed = Seed;
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
			
			this.ChunkHandler = ChunkHandler;
		}
		
		CombinedNoise n1, n2;
		
		OctaveNoise n3, n4, n5, n6;
		
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
		
		void CreateHeightmap(ref Chunk chunk, int chunkX, int chunkZ, CombinedNoise n1, CombinedNoise n2, OctaveNoise n3) {
			/*CombinedNoise n1 = new CombinedNoise(
				new OctaveNoise(8, rnd), new OctaveNoise(8, rnd));
			CombinedNoise n2 = new CombinedNoise(
				new OctaveNoise(8, rnd), new OctaveNoise(8, rnd));
			OctaveNoise n3 = new OctaveNoise(6, rnd);*/
			int index = 0;
			//short[] hMap = new short[16 * 16];
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
			int hMapIndex = 0, maxY = Height - 1, mapIndex = 0;
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
					
					mapIndex = minStoneY * oneY + z * 16 + x;
					for (int y = minStoneY; y <= stoneHeight; y++) {
						chunk.blocks[mapIndex] = Block.Stone; mapIndex += oneY;
					}
					
					stoneHeight = Math.Max(stoneHeight, 0);
					mapIndex = (stoneHeight + 1) * oneY + z * 16 + x;
					for (int y = stoneHeight + 1; y <= dirtHeight; y++) {
						chunk.blocks[mapIndex] = Block.Dirt; mapIndex += oneY;
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
				index = z * 16 + x;
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
				index = (y * 16 + z) * 16 + x;
				chunk.blocks[index] = Block.Stone;
			}
			return stoneHeight;
		}
		
		void FloodFillWater(ref Chunk chunk) {
			int waterY = waterLevel - 1;
			int index1 = (waterY * 16 + 0) * 16 + 0;
			int index2 = (waterY * 16 + (16 - 1)) * 16 + 0;
			
			for (int x = 0; x < 16; x++) {
				FloodFill(index1, Block.Water, chunk);
				FloodFill(index2, Block.Water, chunk);
				index1++; index2++;
			}
			
			index1 = (waterY * 16 + 0) * 16 + 0;
			index2 = (waterY * 16 + 0) * 16 + (16 - 1);
			for (int z = 0; z < 16; z++) {
				FloodFill(index1, Block.Water, chunk);
				FloodFill(index2, Block.Water, chunk);
				index1 += 16; index2 += 16;
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
				
				int x = index % 16;
				int y = index / oneY;
				int z = (index / 16) % 16;
				
				if (x > 0) stack.Push(index - 1);
				if (x < 16 - 1) stack.Push(index + 1);
				if (z > 0) stack.Push(index - 16);
				if (z < 16 - 1) stack.Push(index + 16);
				if (y > 0) stack.Push(index - oneY);
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
					
					int index = (y * 16 + z) * 16 + x;
					BlockRaw blockAbove = y >= (Height - 1) ? Block.Air : chunk.blocks[index + oneY];
					if (blockAbove == Block.Water && (n2.Compute(xCur, zCur) > 12)) {
						chunk.blocks[index] = Block.Gravel;
					} else if (blockAbove == Block.Air) {
						chunk.blocks[index] = (y <= waterLevel && (n1.Compute(xCur, zCur) > 8)) ? Block.Sand : Block.Grass;
					}
				}
			}
		}
	}
}
