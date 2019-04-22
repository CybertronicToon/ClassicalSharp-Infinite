// Copyright 2014-2017 ClassicalSharp | Licensed under BSD-3
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using ClassicalSharp.Events;
using ClassicalSharp.Network;
using ClassicalSharp.Network.Protocols;
using ClassicalSharp.Renderers;
using OpenTK;
using BlockID = System.UInt16;
using BlockRaw = System.Byte;
using NbtCompound = System.Collections.Generic.Dictionary<string, ClassicalSharp.Map.NbtTag>;

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
		
		private void UpdateCurChunkNoAdj() {
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
		}
		
		public void AdjChunks(int oldChunkX, int oldChunkY) {
			int length = ChunkArray.GetLength(0);
			int halfLen = length / 2;
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
					if (ChunkArray[y, x] != null && ChunkArray[y, x].blocks != null && ChunkArray[y, x].populated != false)
					SaveChunk(ChunkArray[y, x], (x + oldChunkX - halfLen), (y + oldChunkY - halfLen));
					Console.WriteLine(oldChunkX);
				}
			}
			for (int x = 0; x < length; x++) {
				for (int y = 0; y < length; y++) {
					int newX = curX - adjX;
					int newY = curY - adjY;
					if (newX > 0 && newY > 0 && newX < length && newY < length) {
						Chunk chunk = ChunkArray[ curY, curX];
						//if (ChunkArray[newY, newX] != null) {
						//	SaveChunk(ChunkArray[newY, newX], (newX + oldChunkX), (newY + oldChunkY));
						//}
						ChunkArray[newY, newX] = chunk;
						ChunkArray[curY, curX] = null;
					} else {
						//SaveChunk(ChunkArray[curY, curX], (curX + oldChunkX), (curY + oldChunkY));
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
			DecorateChunks();
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
				GetChunk(ref chunk, chunkX, chunkY);
				ChunkArray[y, x] = chunk;
			}
		}
		
		public void DecorateChunks() {
			int length = ChunkArray.GetLength(0);
			
			for (int x = 0; x < length - 1; x++)
				for (int y = 0; y < length - 1; y++) {
				int chunkX = x + (curChunkX - 4);
				int chunkY = y + (curChunkY - 4);
				if (ChunkArray[y, x] == null) continue;
				Chunk chunk = ChunkArray[y, x];
				if (!chunk.populated) {
					ChunkGenerator.DecorateChunk(chunkX, chunkY);
					chunk.populated = true;
				}
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
			curChunkX = (ChunkArray.GetLength(0) / 2);
			curChunkY = (ChunkArray.GetLength(0) / 2);
		}
		
		public void GenInitialChunks(long seed, int size) {
			UpdateCurChunkNoAdj();
			
			Console.WriteLine("new ChunkHandler");
			
			ChunkGenerator = new ChunkGenerator(this, seed);
			
			int numChunks = size / 16;
			ChunkArray = new Chunk[numChunks, numChunks];
			for (int x = 0; x < numChunks; x++)
				for (int y = 0; y < numChunks; y++) {
				int chunkAdjX = (x * 16);
				int chunkAdjY = (y * 16);
				Chunk newChunk = new Chunk(x, y);
				GetChunk(ref newChunk, newChunk.x, newChunk.y);
				/*for (int bX = 0; bX < 16; bX++)
					for (int bY = 0; bY < 128; bY++)
						for(int bZ = 0; bZ < 16; bZ++) {
					newChunk.blocks[(bY * 16 + bZ) * 16 + bX] = blocks[(bY * size + (bZ + chunkAdjY)) * size + (bX + chunkAdjX)];
				}*/
				ChunkArray[y, x] = newChunk;
			}
			DecorateChunks();
			UpdateCurChunk();
		}
		
		public void GetChunk(ref Chunk chunk, int chunkX, int chunkZ) {
			#if ALPHA
			string dir1, dir2, file;
			unchecked { 
				dir1 = Base36.Encode((uint)chunkX % (uint)64);
				dir2 = Base36.Encode((uint)chunkZ % (uint)64);
			}
			string fileX = "";
			string fileZ = "";
			int chunkX2 = chunkX;
			int chunkZ2 = chunkZ;
			if (chunkX2 < 0) {
				fileX = "-";
				chunkX2 *= -1;
			}
			fileX += Base36.Encode((long)chunkX2);
			if (chunkZ2 < 0) {
				fileZ = "-";
				chunkZ2 *= -1;
			}
			fileZ += Base36.Encode((long)chunkZ2);
			file = ("c." + fileX + "." + fileZ + ".dat");
			string path = Path.Combine(Program.AppDirectory, "saves");
			path = Path.Combine(path, "World1");
			path = Path.Combine(path, dir1);
			path = Path.Combine(path, dir2);
			path = Path.Combine(path, file);
			if (File.Exists(path)) {
				bool loaded = LoadChunk(ref chunk, path);
				if (loaded) return;
			}
			#endif
			ChunkGenerator.GenerateChunk(ref chunk, chunkX, chunkZ);
		}
		
		public bool LoadChunk(ref Chunk chunk, string path) {
			try {
				using (FileStream fs = File.OpenRead(path)) {
					GZipHeaderReader gsheader = new GZipHeaderReader();
					while (!gsheader.ReadHeader(fs)) { }
					
					using (DeflateStream gs = new DeflateStream(fs, CompressionMode.Decompress)) {
						BinaryReader reader = new BinaryReader(gs);
						if (reader.ReadByte() != (byte)NbtTagType.Compound)
						throw new InvalidDataException("Nbt file must start with Tag_Compound");
						NbtFile file = new NbtFile(reader);
						
						NbtTag root = file.ReadTag((byte)NbtTagType.Compound, true);
						NbtCompound children = (NbtCompound)root.Value;
						if (children.ContainsKey("Level")) {
							NbtCompound levelChildren = (NbtCompound)children["Level"].Value;
							chunk.blocks = (byte[])levelChildren["Blocks"].Value;
							int size = 16 * 128 * 16;
							int halfSize = size / 2;
							byte[] data = (byte[])levelChildren["Data"].Value;
							chunk.metadata = new NibbleSlice(data, 0, halfSize);
							chunk.populated = ((byte)levelChildren["TerrainPopulated"].Value != 0);
							return true;
						}
					}
				}
			} catch(Exception ex) {
				ErrorHandler.LogError("loading chunk", ex);
				game.Chat.Add("Failed to load chunk.");
				return false;
			}
			return false;
		}
		
		public void SaveChunk(Chunk chunk, int chunkX, int chunkZ) {
			//throw new NotImplementedException();
			#if ALPHA
			string dir, dir1, dir2, file;
			unchecked { 
				dir1 = Base36.Encode((uint)chunkX % (uint)64);
				dir2 = Base36.Encode((uint)chunkZ % (uint)64);
			}
			string fileX = "";
			string fileZ = "";
			int chunkX2 = chunkX;
			int chunkZ2 = chunkZ;
			if (chunkX2 < 0) {
				fileX = "-";
				chunkX2 *= -1;
			}
			fileX += Base36.Encode((long)chunkX2);
			if (chunkZ2 < 0) {
				fileZ = "-";
				chunkZ2 *= -1;
			}
			fileZ += Base36.Encode((long)chunkZ2);
			file = ("c." + fileX + "." + fileZ + ".dat");
			string path = Path.Combine(Program.AppDirectory, "saves");
			path = Path.Combine(path, "World1");
			path = Path.Combine(path, dir1);
			path = Path.Combine(path, dir2);
			dir = path;
			path = Path.Combine(path, file);
			if (File.Exists(path)) {
				File.Delete(path);
				WriteChunk(chunk, path);
			} else {
				if (!Directory.Exists(dir)) {
					Directory.CreateDirectory(dir);
				}
				WriteChunk(chunk, path);
			}
			#endif
		}
		
		public void WriteChunk(Chunk chunk, string path) {
			try {
				using (FileStream fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write)) {
					using (GZipStream wrapper = new GZipStream(fs, CompressionMode.Compress)) {
						BinaryWriter writer = new BinaryWriter(wrapper);
						NbtFile nbt = new NbtFile(writer);
						
						nbt.Write(NbtTagType.Compound);
						nbt.Write("");
						
						nbt.Write(NbtTagType.Compound);
						nbt.Write("Level");
						
						nbt.Write(NbtTagType.Int8);
						nbt.Write("TerrainPopulated"); nbt.WriteUInt8((byte)1);
						
						nbt.Write(NbtTagType.Int8Array);
						nbt.Write("Blocks"); nbt.WriteInt32(chunk.blocks.Length);
						nbt.WriteBytes(chunk.blocks);
						
						nbt.Write(NbtTagType.Int8Array);
						nbt.Write("Data"); nbt.WriteInt32(chunk.metadata.Data.Length);
						nbt.WriteBytes(chunk.metadata.Data);
						
						nbt.Write(NbtTagType.End);
						
						nbt.Write(NbtTagType.End);
					}
				}
			} catch(Exception ex) {
				ErrorHandler.LogError("saving chunk", ex);
				game.Chat.Add("Failed to save chunk.");
			}
		}
		
		public Vector3I AdjCoords(Vector3I coords) {
			int coordAdj = (ChunkArray.GetLength(0) / 2);
			coords.X -= (16 * (curChunkX - coordAdj));
			coords.Z -= (16 * (curChunkY - coordAdj));
			return coords;
		}
		
		public Vector3I ReverseAdjCoords(Vector3I coords) {
			int coordAdj = (ChunkArray.GetLength(0) / 2);
			coords.X += (16 * (curChunkX - coordAdj));
			coords.Z += (16 * (curChunkY - coordAdj));
			return coords;
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
			int oneY = 16 * 16;
			result.X = index % 16;
			result.Y = index / oneY;
			result.Z = (index / 16) % 16;
			#endif
			return result;
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
			int index = GetIndex(blockX, blockY, blockZ);
			return ChunkArray[chunkY, chunkX].blocks[index];
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
			Chunk chunk = ChunkArray[chunkY - GetChunkAdj(curChunkY), chunkX - GetChunkAdj(curChunkX)];
			if (chunk == null) return Block.Air;
			int index = GetIndex(blockX, blockY, blockZ);
			return chunk.blocks[index];
		}
		
		public BlockRaw GetBlockSafe(int x, int y, int z) {
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
			/*if (blockY < 0) return Block.Lava;
			if (blockY > 127) return Block.Air;
			if (chunkX < 0 || chunkY < 0) {
				return Block.Air;
			} else if (chunkX >= ChunkArray.GetLength(0) ||
			           chunkY >= ChunkArray.GetLength(0)) {
				return Block.Air;
			}*/
			if (blockY < 0) return Block.Lava;
			if (blockY > 127) return Block.Air;
			if (chunkX < 0 || chunkY < 0) {
				return Block.Air;
			} else if (chunkX >= ChunkArray.GetLength(0) ||
			           chunkY >= ChunkArray.GetLength(0)) {
				return Block.Air;
			}
			int index = GetIndex(blockX, blockY, blockZ);
			return ChunkArray[chunkY, chunkX].blocks[index];
		}
		
		public byte GetDataSafe(int x, int y, int z) {
			int chunkX = x >> 4;
			int blockX = x & 15;
			int chunkY = z >> 4;
			int blockZ = z & 15;
			int blockY = y;
			if (blockY < 0) return Block.Lava;
			if (blockY > 127) return Block.Air;
			if (chunkX < 0 || chunkY < 0) {
				return 0;
			} else if (chunkX >= ChunkArray.GetLength(0) ||
			           chunkY >= ChunkArray.GetLength(0)) {
				return 0;
			}
			int index = GetIndex(blockX, blockY, blockZ);
			return ChunkArray[chunkY, chunkX].metadata[index];
		}
		
		public byte GetDataAdjSafe(int x, int y, int z) {
			int chunkX = x >> 4;
			int blockX = x & 15;
			int chunkY = z >> 4;
			int blockZ = z & 15;
			int blockY = y;
			if (blockY < 0) return Block.Lava;
			if (blockY > 127) return Block.Air;
			if (chunkX - GetChunkAdj(curChunkX) < 0 || chunkY - GetChunkAdj(curChunkY) < 0) {
				return 0x0;
			} else if (chunkX - GetChunkAdj(curChunkX) >= ChunkArray.GetLength(0) ||
			           chunkY - GetChunkAdj(curChunkY) >= ChunkArray.GetLength(0)) {
				return 0x0;
			}
			int index = GetIndex(blockX, blockY, blockZ);
			return ChunkArray[chunkY - GetChunkAdj(curChunkY), chunkX - GetChunkAdj(curChunkX)].metadata[index];
		}
		
		public void SetDataSafe(int x, int y, int z, byte data) {
			if (data > 0xFF) data = 0xFF;
			int chunkX = x >> 4;
			int blockX = x & 15;
			int chunkY = z >> 4;
			int blockZ = z & 15;
			int blockY = y;
			if (blockY < 0) return;
			if (blockY > 127) return;
			if (chunkX < 0 || chunkY < 0) {
				return;
			} else if (chunkX >= ChunkArray.GetLength(0) ||
			           chunkY >= ChunkArray.GetLength(0)) {
				return;
			}
			int index = GetIndex(blockX, blockY, blockZ);
			ChunkArray[chunkY, chunkX].metadata[index] = data;
		}
		
		public BlockRaw GetBlockAdjSafe(int x, int y, int z) {
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
			if (chunkX - GetChunkAdj(curChunkX) < 0 || chunkY - GetChunkAdj(curChunkY) < 0) {
				return Block.Air;
			} else if (chunkX - GetChunkAdj(curChunkX) >= ChunkArray.GetLength(0) ||
			           chunkY - GetChunkAdj(curChunkY) >= ChunkArray.GetLength(0)) {
				return Block.Air;
			}
			int index = GetIndex(blockX, blockY, blockZ);
			return ChunkArray[chunkY - GetChunkAdj(curChunkY), chunkX - GetChunkAdj(curChunkX)].blocks[index];
		}
		
		public void SetBlock(int x, int y, int z, BlockRaw blockId) {
			int chunkX = x / 16;
			if (x < 0) chunkX -= 1;
			int blockX = x % 16;
			int chunkY = z / 16;
			if (z < 0) chunkY -= 1;
			int blockZ = z % 16;
			int blockY = y;
			int index = GetIndex(blockX, blockY, blockZ);
			ChunkArray[chunkY, chunkX].blocks[index] = blockId;
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
			Chunk chunk = ChunkArray[chunkY - GetChunkAdj(curChunkY), chunkX - GetChunkAdj(curChunkX)];
			if (chunk == null) return;
			int index = GetIndex(blockX, blockY, blockZ);
			chunk.blocks[index] = blockId;
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
			int index = GetIndex(blockX, blockY, blockZ);
			return ChunkArray[chunkY, chunkX].blocks[index];
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
			Chunk chunk = ChunkArray[chunkY - GetChunkAdj(curChunkY), chunkX - GetChunkAdj(curChunkX)];
			if (chunk == null) return Block.Air;
			int index = GetIndex(blockX, blockY, blockZ);
			return chunk.blocks[index];
		}
		
		public int GetChunkAdj(int curChunk) {
			return curChunk - (ChunkArray.GetLength(0) / 2);
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
			int size = 16 * 128 * 16;
			int halfSize = size / 2;
			byte[] data = new Byte[halfSize];
			metadata = new NibbleSlice(data, 0, halfSize);
			this.x = x;
			this.y = y;
			this.populated = false;
		}
		
		public int x, y;
		
		public BlockRaw[] blocks;
		public NibbleSlice metadata;
		public byte[] lightMap;
		public bool populated;
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
	
	/// <summary> A Base36 De- and Encoder </summary>
	public static class Base36 {
		private const string CharList = "0123456789abcdefghijklmnopqrstuvwxyz";
		
		/// <summary> Encode the given number into a Base36 string </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static String Encode(long input) {
			if (input < 0) throw new ArgumentOutOfRangeException("input", input, "input cannot be negative");
			
			char[] clistarr = CharList.ToCharArray();
			Stack<char> result = new Stack<char>();
			if (input == 0) {
				result.Push(clistarr[input % 36]);
				input /= 36;
			}
			while (input != 0) {
				result.Push(clistarr[input % 36]);
				input /= 36;
			}
			return new string(result.ToArray());
		}
		
		/// <summary> Decode the Base36 Encoded string into a number </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static Int64 Decode(string input) {
			string str = input.ToLower();
			string reversed = "";
			int length = str.Length - 1;
			while (length >= 0) {
				reversed = reversed + str[length];
				length--;
			}
			long result = 0;
			int pos = 0;
			foreach (char c in reversed) {
				result += CharList.IndexOf(c) * (long)Math.Pow(36, pos);
				pos++;
			}
			return result;
		}
	}
}
