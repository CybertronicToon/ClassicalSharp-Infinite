// Copyright 2014-2017 ClassicalSharp | Licensed under BSD-3
using System;
using ClassicalSharp.Events;
using ClassicalSharp.Renderers;
using OpenTK;
using BlockID = System.UInt16;
using BlockRaw = System.Byte;

namespace ClassicalSharp.Map {
	public class ChunkHandler {
		
		Game game;
		
		public Chunk[,] ChunkArray;
		
		public int curChunkX;
		public int curChunkY;
		
		public ChunkGenerator ChunkGenerator;
		
		public ChunkHandler(int size, Game game) {
			this.game = game;
			ChunkArray = new Chunk[size, size];
			ChunkGenerator = new ChunkGenerator(this);
		}
		
		public ChunkHandler(BlockRaw[] blocks, int size, Game game) {
			this.game = game;
			ChunkGenerator = new ChunkGenerator(this);
			
			int numChunks = size / 16;
			ChunkArray = new Chunk[numChunks, numChunks];
			for (int x = 0; x < numChunks; x++)
				for (int y = 0; y < numChunks; y++) {
				int chunkAdjX = (x * 16);
				int chunkAdjY = (y * 16);
				Chunk newChunk = new Chunk(x, y);
				for (int bX = 0; bX < 16; bX++)
					for (int bY = 0; bY < 128; bY++)
						for(int bZ = 0; bZ < 16; bZ++) {
					newChunk.blocks[(bY * 16 + bZ) * 16 + bX] = blocks[(bY * size + (bZ + chunkAdjY)) * size + (bX + chunkAdjX)];
				}
				ChunkArray[y, x] = newChunk;
			}
		}
		
		public void UpdateCurChunk() {
			int oldChunkX = curChunkX;
			int oldChunkY = curChunkY;
			Vector3 curPos = game.LocalPlayer.Position;
			int x = Utils.Floor(curPos.X);
			int y = Utils.Floor(curPos.Z);
			bool isNegX = false;
			bool isNegY = false;
			if (x < 0) {
				x -= 1;
				isNegX = true;
			}
			if (y < 0) {
				y -= 1;
				isNegY = true;
			}
			curChunkX = x / 16;
			if (isNegX) curChunkX -= 1;
			curChunkY = y / 16;
			if (isNegY) curChunkY -= 1;
			if (oldChunkX != curChunkX || oldChunkY != curChunkY) {
				AdjChunks(oldChunkX, oldChunkY);
			}
		}
		
		public void AdjChunks(int oldChunkX, int oldChunkY) {
			int length = ChunkArray.GetLength(0);
			bool xNeg = false;
			bool yNeg = false;
			if (oldChunkX > curChunkX) xNeg = true;
			if (oldChunkY > curChunkY) yNeg = true;
			int adjX = curChunkX - oldChunkX;
			int adjY = curChunkY - oldChunkY;
			int curX = 0;
			if (xNeg) curX = length - 1;
			int curY = 0;
			if (yNeg) curY = length - 1;
			for (int x = 0; x < length; x++) {
				for (int y = 0; y < length; y++) {
					int newX = curX - adjX;
					int newY = curY - adjY;
					if (newX > 0 && newY > 0 && newX < length && newY < length) {
						Chunk chunk = ChunkArray[ curY, curX];
						ChunkArray[newY, newX] = chunk;
						ChunkArray[curY, curX] = null;
					} else {
						ChunkArray[curY, curX] = null;
					}
					if (yNeg) {
						curY -= 1;
					} else {
						curY += 1;
					}
				}
				if (xNeg) {
					curX -= 1;
				} else {
					curX += 1;
				}
				curY = 0;
				if (yNeg) curY = length - 1;
			}
			
			GenNullChunks();
			game.MapRenderer.Refresh();
			//game.MapRenderer.AdjChunks(oldChunkX, oldChunkY);
			game.Lighting.Refresh();
		}
		
		public void GenNullChunks() {
			int length = ChunkArray.GetLength(0);
			
			for (int x = 0; x < length; x++)
				for (int y = 0; y < length; y++) {
				int chunkX = x + (curChunkX - 4);
				int chunkY = y + (curChunkY - 4);
				if (ChunkArray[y, x] != null) continue;
				Chunk chunk = new Chunk(chunkX, chunkY);
				ChunkGenerator.GenerateChunk(ref chunk, chunkX, chunkY);
				ChunkArray[y, x] = chunk;
			}
		}
		
		public void SetNewMap(BlockRaw[] blocks, int size) {
			
			Console.WriteLine("new ChunkHandler");
			
			int numChunks = size / 16;
			ChunkArray = new Chunk[numChunks, numChunks];
			for (int x = 0; x < numChunks; x++)
				for (int y = 0; y < numChunks; y++) {
				int chunkAdjX = (x * 16);
				int chunkAdjY = (y * 16);
				Chunk newChunk = new Chunk(x, y);
				for (int bX = 0; bX < 16; bX++)
					for (int bY = 0; bY < 128; bY++)
						for(int bZ = 0; bZ < 16; bZ++) {
					newChunk.blocks[(bY * 16 + bZ) * 16 + bX] = blocks[(bY * size + (bZ + chunkAdjY)) * size + (bX + chunkAdjX)];
				}
				ChunkArray[y, x] = newChunk;
			}
		}
		
		public void GenInitialChunks(long seed, int size) {
			
			Console.WriteLine("new ChunkHandler");
			
			ChunkGenerator = new ChunkGenerator(this, seed);
			
			int numChunks = size / 16;
			ChunkArray = new Chunk[numChunks, numChunks];
			for (int x = 0; x < numChunks; x++)
				for (int y = 0; y < numChunks; y++) {
				int chunkAdjX = (x * 16);
				int chunkAdjY = (y * 16);
				Chunk newChunk = new Chunk(x, y);
				ChunkGenerator.GenerateChunk(ref newChunk, newChunk.x, newChunk.y);
				/*for (int bX = 0; bX < 16; bX++)
					for (int bY = 0; bY < 128; bY++)
						for(int bZ = 0; bZ < 16; bZ++) {
					newChunk.blocks[(bY * 16 + bZ) * 16 + bX] = blocks[(bY * size + (bZ + chunkAdjY)) * size + (bX + chunkAdjX)];
				}*/
				ChunkArray[y, x] = newChunk;
			}
		}
		
		public BlockRaw GetBlock(int x, int y, int z) {
			if (!inMap(x, y, z)) return Block.Air;
			int chunkX = x / 16;
			if (x < 0) chunkX -= 1;
			int blockX = x % 16;
			int chunkY = z / 16;
			if (z < 0) chunkY -= 1;
			int blockZ = z % 16;
			int blockY = y;
			if (blockY < 0) return Block.Lava;
			if (blockY > 127) return Block.Air;
			return ChunkArray[chunkY, chunkX].blocks[(blockY * 16 + blockZ) * 16 + blockX];
		}
		
		public BlockRaw GetBlockAdj(int x, int y, int z) {
			if (!inMap(x, y, z)) return Block.Air;
			//int chunkX = x / 16;
			int chunkX = x >> 4;
			//int blockX = x % 16;
			int blockX = x & 15;
			//if (x < 0) chunkX -= 1;
			//int chunkY = z / 16;
			int chunkY = z >> 4;
			//if (z < 0) chunkY -= 1;
			//int blockZ = z % 16
			int blockZ = z & 15;
			int blockY = y;
			if (blockY < 0) return Block.Lava;
			if (blockY > 127) return Block.Air;
			return ChunkArray[chunkY - (curChunkY - 4), chunkX - (curChunkX - 4)].blocks[(blockY * 16 + blockZ) * 16 + blockX];
		}
		
		public BlockRaw GetBlockAdjSafe(int x, int y, int z) {
			if (!inMap(x, y, z)) return Block.Air;
			//int chunkX = x / 16;
			int chunkX = x >> 4;
			//int blockX = x % 16;
			int blockX = x & 15;
			//if (x < 0) chunkX -= 1;
			//int chunkY = z / 16;
			int chunkY = z >> 4;
			//if (z < 0) chunkY -= 1;
			//int blockZ = z % 16
			int blockZ = z & 15;
			int blockY = y;
			if (blockY < 0) return Block.Lava;
			if (blockY > 127) return Block.Air;
			if (chunkX - (curChunkX - 4) < 0 || chunkY - (curChunkY - 4) < 0) {
				return Block.Air;
			} else if (chunkX - (curChunkX - 4) >= ChunkArray.GetLength(0) ||
			           chunkY - (curChunkY - 4) >= ChunkArray.GetLength(0)) {
				return Block.Air;
			}
			return ChunkArray[chunkY - (curChunkY - 4), chunkX - (curChunkX - 4)].blocks[(blockY * 16 + blockZ) * 16 + blockX];
		}
		
		public void SetBlock(int x, int y, int z, BlockRaw blockId) {
			int chunkX = x / 16;
			if (x < 0) chunkX -= 1;
			int blockX = x % 16;
			int chunkY = z / 16;
			if (z < 0) chunkY -= 1;
			int blockZ = z % 16;
			int blockY = y;
			ChunkArray[chunkY, chunkX].blocks[(blockY * 16 + blockZ) * 16 + blockX] = blockId;
		}
		
		public void SetBlockAdj(int x, int y, int z, BlockRaw blockId) {
			//int chunkX = x / 16;
			int chunkX = x >> 4;
			//int blockX = x % 16;
			int blockX = x & 15;
			//if (x < 0) chunkX -= 1;
			//int chunkY = z / 16;
			int chunkY = z >> 4;
			//if (z < 0) chunkY -= 1;
			//int blockZ = z % 16
			int blockZ = z & 15;
			int blockY = y;
			ChunkArray[chunkY - (curChunkY - 4), chunkX - (curChunkX - 4)].blocks[(blockY * 16 + blockZ) * 16 + blockX] = blockId;
		}
		
		public BlockRaw GetBlockUnsafe(int x, int y, int z) {
			int chunkX = x / 16;
			int blockX = x % 16;
			if (x < 0) chunkX -= 1;
			int chunkY = z / 16;
			if (z < 0) chunkY -= 1;
			int blockZ = z % 16;
			int blockY = y;
			if (blockY < 0) return Block.Lava;
			if (blockY > 127) return Block.Air;
			return ChunkArray[chunkY, chunkX].blocks[(blockY * 16 + blockZ) * 16 + blockX];
		}
		
		public BlockRaw GetBlockAdjUnsafe(int x, int y, int z) {
			//int chunkX = x / 16;
			int chunkX = x >> 4;
			//int blockX = x % 16;
			int blockX = x & 15;
			//if (x < 0) chunkX -= 1;
			//int chunkY = z / 16;
			int chunkY = z >> 4;
			//if (z < 0) chunkY -= 1;
			//int blockZ = z % 16
			int blockZ = z & 15;
			int blockY = y;
			if (blockY < 0) return Block.Lava;
			if (blockY > 127) return Block.Air;
			return ChunkArray[chunkY - (curChunkY - 4), chunkX - (curChunkX - 4)].blocks[(blockY * 16 + blockZ) * 16 + blockX];
		}
		
		public void GetChunkCoords(int x, int y, out int chunkX, out int chunkY) {
			chunkX = x / 16;
			if (x < 0) chunkX -= 1;
			chunkY = y / 16;
			if (y < 0) chunkY -= 1;
		}
		
		public void GetChunkCoords(Vector3 coords, out int chunkX, out int chunkY) {
			chunkX = (int)coords.X / 16;
			if (coords.X < 0) chunkX -= 1;
			chunkY = (int)coords.Z / 16;
			if (coords.Z < 0) chunkY -= 1;
		}
		
		public void GetChunkCoords(Vector3I coords, out int chunkX, out int chunkY) {
			chunkX = coords.X / 16;
			if (coords.X < 0) chunkX -= 1;
			chunkY = coords.Z / 16;
			if (coords.Z < 0) chunkY -= 1;
		}
		
		public bool inMap(int x, int y, int z) {
			int numBlocks = (ChunkArray.GetLength(1) * 16) - 1;
			if (x > numBlocks || x < 0) return false;
			if (y > 127 || y < 0) return false;
			if (z > numBlocks || z < 0) return false;
			return true;
		}
	}
	
	public class Chunk {
		public Chunk(int x, int y) {
			blocks = new BlockRaw[16 * 16 * 128];
			this.x = x;
			this.y = y;
		}
		
		public int x, y;
		
		public BlockRaw[] blocks;
		public byte[] lightMap;
	}
	
	public class ChunkLoc {
		///<summary>Chunk X.</summary>
		public int CX;
		///<summary>Chunk Y.</summary>
		public int CY;
		///<summary>Block X.</summary>
		public int BX;
		///<summary>Block Y.</summary>
		public int BY;
		/// <summary>Block Z.</summary>
		public int BZ;
	}
}
