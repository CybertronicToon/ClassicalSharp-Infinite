﻿// Copyright 2014-2017 ClassicalSharp | Licensed under BSD-3
//#define DEBUG_OCCLUSION
using System;
using ClassicalSharp.GraphicsAPI;
using ClassicalSharp.Map;
using ClassicalSharp.Renderers;
using OpenTK;
using BlockID = System.UInt16;
using BlockRaw = System.Byte;

namespace ClassicalSharp {
	
	/// <summary> Class responsible for converting a 16x16x16 into an optimised mesh of vertices. </summary>
	/// <remarks> This class is heavily optimised and as such may suffer from slightly unreadable code. </remarks>
	public unsafe abstract partial class ChunkMeshBuilder {
		
		protected int X, Y, Z;
		protected BlockID curBlock;
		protected World map;
		protected IWorldLighting light;
		protected WorldEnv env;
		protected Game game;
		protected const int chunkSize = 16, extChunkSize = 18;
		protected const int chunkSize2 = 16 * 16, extChunkSize2 = 18 * 18;
		protected const int chunkSize3 = 16 * 16 * 16, extChunkSize3 = 18 * 18 * 18;
		
		public void Init(Game game) {
			this.game = game;
			game.Events.TerrainAtlasChanged += TerrainAtlasChanged;
		}
		
		protected internal int width, length, height, sidesLevel, edgeLevel;
		protected int maxX, maxY, maxZ, chunkEndX, chunkEndZ;
		protected int cIndex;
		protected BlockID* chunk;
		protected byte* counts;
		protected int* bitFlags;
		protected bool useBitFlags;
		
		protected int numChunks;
		protected ChunkHandler ChunkHandler;
		
		bool BuildChunk(int x1, int y1, int z1, ref bool allAir) {
			if (ChunkHandler == null) {
				ChunkHandler = map.ChunkHandler;
			}
			light = game.Lighting;
			PreStretchTiles(x1, y1, z1);
			BlockID* chunkPtr = stackalloc BlockID[extChunkSize3]; chunk = chunkPtr;
			byte* countsPtr = stackalloc byte[chunkSize3 * Side.Sides]; counts = countsPtr;
			int* bitsPtr = stackalloc int[extChunkSize3]; bitFlags = bitsPtr;
			MemUtils.memset((IntPtr)chunkPtr, 0, 0, extChunkSize3 * sizeof(BlockID));
			
			bool allSolid = false;
			//fixed (BlockRaw* mapPtr = map.blocks1) {
				#if USE_16_BIT
				if (BlockInfo.MaxDefined >= 256) {
					ReadChunkData_16Bit(x1, y1, z1, mapPtr, ref allAir, ref allSolid);
				} else {
					ReadChunkData_8Bit(x1, y1, z1, mapPtr, ref allAir, ref allSolid);
				}
				#else
				ReadChunkData_8Bit_2(x1, y1, z1, ref allAir, ref allSolid);
				#endif
				
				if (x1 == 0 || y1 == 0 || z1 == 0 || x1 + chunkSize >= width ||
				    y1 + chunkSize >= height || z1 + chunkSize >= length) allSolid = false;
				
				if (allAir || allSolid) return false;
				light.LightHint(x1 - 1, z1 - 1);
			//}
			
			MemUtils.memset((IntPtr)countsPtr, 1, 0, chunkSize3 * Side.Sides);
			int xMax = Math.Min(width, x1 + chunkSize);
			int yMax = Math.Min(height, y1 + chunkSize);
			int zMax = Math.Min(length, z1 + chunkSize);
			
			chunkEndX = xMax; chunkEndZ = zMax;
			Stretch(x1, y1, z1);
			PostStretchTiles(x1, y1, z1);
			
			for (int y = y1, yy = 0; y < yMax; y++, yy++) {
				for (int z = z1, zz = 0; z < zMax; z++, zz++) {
					
					int chunkIndex = (yy + 1) * extChunkSize2 + (zz + 1) * extChunkSize + (0 + 1);
					for (int x = x1, xx = 0; x < xMax; x++, xx++) {
						curBlock = chunk[chunkIndex];
						if (BlockInfo.Draw[curBlock] != DrawType.Gas) {
							int index = ((yy << 8) | (zz << 4) | xx) * Side.Sides;
							X = x; Y = y; Z = z;
							cIndex = chunkIndex;
							RenderTile(index);
						}
						chunkIndex++;
					}
				}
			}
			return true;
		}
		
		void ReadChunkData_8Bit(int x1, int y1, int z1, BlockRaw* mapPtr, ref bool outAllAir, ref bool outAllSolid) { // only assign this variable once
			bool allAir = true, allSolid = true;
			Console.WriteLine(x1);
			for (int yy = -1; yy < 17; yy++) {
				int y = yy + y1;
				if (y < 0) continue;
				if (y >= height) break;
				for (int zz = -1; zz < 17; zz++) {
					int z = zz + z1;
					if (z < 0) continue;
					if (z >= length) break;
					
					int index = (y * length + z) * width + (x1 - 1 - 1);
					int chunkIndex = (yy + 1) * extChunkSize2 + (zz + 1) * extChunkSize + (-1 + 1) - 1;
					
					for (int xx = -1; xx < 17; xx++) {
						int x = xx + x1;
						index++;
						chunkIndex++;
						if (x < 0) continue;
						if (x >= width) break;
						BlockID rawBlock = mapPtr[index];
						
						allAir = allAir && BlockInfo.Draw[rawBlock] == DrawType.Gas;
						allSolid = allSolid && BlockInfo.FullOpaque[rawBlock];
						chunk[chunkIndex] = rawBlock;
					}
				}
				outAllAir = allAir; outAllSolid = allSolid;
			}
		}
		
		void ReadChunkData_8Bit_2(int x1, int y1, int z1, ref bool outAllAir, ref bool outAllSolid) { // only assign this variable once
			bool allAir = true, allSolid = true;
			for (int yy = -1; yy < 17; yy++) {
				int y = yy + y1;
				if (y < 0) continue;
				if (y >= height) break;
				for (int zz = -1; zz < 17; zz++) {
					int z = zz + z1;
					if (z < 0) continue;
					if (z >= length) break;
					
					int index = (y * length + z) * width + (x1 - 1 - 1);
					int chunkIndex = (yy + 1) * extChunkSize2 + (zz + 1) * extChunkSize + (-1 + 1) - 1;
					
					for (int xx = -1; xx < 17; xx++) {
						int x = xx + x1;
						index++;
						chunkIndex++;
						if (x < 0) continue;
						if (x >= width) break;
						BlockID rawBlock = ChunkHandler.GetBlock(x, y, z);
						
						allAir = allAir && BlockInfo.Draw[rawBlock] == DrawType.Gas;
						allSolid = allSolid && BlockInfo.FullOpaque[rawBlock];
						chunk[chunkIndex] = rawBlock;
					}
				}
				outAllAir = allAir; outAllSolid = allSolid;
			}
		}
		
		#if USE_16_BIT
		void ReadChunkData_16Bit(int x1, int y1, int z1, BlockRaw* mapPtr, ref bool outAllAir, ref bool outAllSolid) { // only assign this variable once
			bool allAir = true, allSolid = true;
			fixed (BlockRaw* mapPtr2 = map.blocks2) {
				for (int yy = -1; yy < 17; yy++) {
					int y = yy + y1;
					if (y < 0) continue;
					if (y >= height) break;
					for (int zz = -1; zz < 17; zz++) {
						int z = zz + z1;
						if (z < 0) continue;
						if (z >= length) break;
						
						int index = (y * length + z) * width + (x1 - 1 - 1);
						int chunkIndex = (yy + 1) * extChunkSize2 + (zz + 1) * extChunkSize + (-1 + 1) - 1;
						
						for (int xx = -1; xx < 17; xx++) {
							int x = xx + x1;
							index++;
							chunkIndex++;
							if (x < 0) continue;
							if (x >= width) break;
							BlockID rawBlock = (BlockID)(mapPtr[index] | (mapPtr2[index] << 8));
							
							allAir = allAir && BlockInfo.Draw[rawBlock] == DrawType.Gas;
							allSolid = allSolid && BlockInfo.FullOpaque[rawBlock];
							chunk[chunkIndex] = rawBlock;
						}
					}
					outAllAir = allAir; outAllSolid = allSolid;
				}
			}
		}
		#endif
		
		public void MakeChunk(ChunkInfo info) {
			int x = info.CentreX - 8, y = info.CentreY - 8, z = info.CentreZ - 8;
			if (!BuildChunk(x, y, z, ref info.AllAir)) return;
			
			for (int i = 0; i < arraysCount; i++) {
				SetPartInfo(normalParts[i], i, ref info.NormalParts);
				SetPartInfo(liquidParts[i], i, ref info.LiquidParts);
				SetPartInfo(translucentParts[i], i, ref info.TranslucentParts);
			}
			
			#if OCCLUSION
			if (info.NormalParts != null || info.LiquidParts != null || info.TranslucentParts != null)
				info.occlusionFlags = (byte)ComputeOcclusion();
			#endif
		}
		
		void SetPartInfo(DrawInfo part, int i, ref ChunkPartInfo[] parts) {
			int vertCount = part.VerticesCount();
			if (vertCount == 0) return;
			
			ChunkPartInfo info;
			fixed (VertexP3fT2fC4b* ptr = part.vertices) {
				// add an extra element to fix crashing on some GPUs
				info.VbId = game.Graphics.CreateVb((IntPtr)ptr, VertexFormat.P3fT2fC4b, vertCount + 1);
			}
			info.VerticesCount = vertCount;
			
			info.LeftCount =   (ushort)part.vCount[Side.Left];
			info.RightCount =  (ushort)part.vCount[Side.Right];
			info.FrontCount =  (ushort)part.vCount[Side.Front];
			info.BackCount =   (ushort)part.vCount[Side.Back];
			info.BottomCount = (ushort)part.vCount[Side.Bottom];
			info.TopCount =    (ushort)part.vCount[Side.Top];
			info.SpriteCount = part.spriteCount;
			
			// Lazy initalize part arrays so we can save time in MapRenderer for chunks that only contain 1 or 2 part types.
			if (parts == null)
				parts = new ChunkPartInfo[arraysCount];
			parts[i] = info;
		}
		
		void Stretch(int x1, int y1, int z1) {
			int xMax = Math.Min(width,  x1 + chunkSize);
			int yMax = Math.Min(height, y1 + chunkSize);
			int zMax = Math.Min(length, z1 + chunkSize);
			#if OCCLUSION
			int flags = ComputeOcclusion();
			#endif
			#if DEBUG_OCCLUSION
			FastColour col = new FastColour(60, 60, 60, 255);
			if ((flags & 1) != 0) col.R = 255; // x
			if ((flags & 4) != 0) col.G = 255; // y
			if ((flags & 2) != 0) col.B = 255; // z
			map.Sunlight = map.Shadowlight = col;
			map.SunlightXSide = map.ShadowlightXSide = col;
			map.SunlightZSide = map.ShadowlightZSide = col;
			map.SunlightYBottom = map.ShadowlightYBottom = col;
			#endif
			byte[] hidden = BlockInfo.hidden;
			
			int xTexMax = width;
			int xTexMin = -1;
			int zTexMax = length;
			int zTexMin = -1;
			int yTexMin = -1;
			
			for (int y = y1, yy = 0; y < yMax; y++, yy++) {
				for (int z = z1, zz = 0; z < zMax; z++, zz++) {
					int cIndex = (yy + 1) * extChunkSize2 + (zz + 1) * extChunkSize + (-1 + 1);
					for (int x = x1, xx = 0; x < xMax; x++, xx++) {
						cIndex++;
						BlockID b = chunk[cIndex];
						if (BlockInfo.Draw[b] == DrawType.Gas) continue;
						int index = ((yy << 8) | (zz << 4) | xx) * Side.Sides;
						bool isLiq = IsBlockLiquid(b);
						
						// Sprites only use one face to indicate stretching count, so we can take a shortcut here.
						// Note that sprites are not drawn with any of the DrawXFace, they are drawn using DrawSprite.
						if (BlockInfo.Draw[b] == DrawType.Sprite) {
							index += Side.Top;
							if (counts[index] != 0) {
								X = x; Y = y; Z = z;
								AddSpriteVertices(b);
								counts[index] = 1;
							}
							continue;
						}
						
						X = x; Y = y; Z = z;
						fullBright = BlockInfo.FullBright[b];
						int tileIdx = b * BlockInfo.Count;
						// All of these function calls are inlined as they can be called tens of millions to hundreds of millions of times.
						
						if (counts[index] == 0 ||
						    (x == xTexMin + 1 && isLiq && y < edgeLevel) ||
						    (x != xTexMin && (hidden[tileIdx + chunk[cIndex - 1]] & (1 << Side.Left)) != 0)) {
							counts[index] = 0;
						} else {
							int count = StretchZ(index, x, y, z, cIndex, b, Side.Left);
							AddVertices(b, Side.Left);/*AddVertices(b, Side.Left);*/ counts[index] = (byte)count;
						}
						
						index++;
						if (counts[index] == 0 ||
						    (x == xTexMax - 1 && isLiq && y < edgeLevel) ||
						    (x != xTexMax && (hidden[tileIdx + chunk[cIndex + 1]] & (1 << Side.Right)) != 0)) {
							counts[index] = 0;
						} else {
							int count = StretchZ(index, x, y, z, cIndex, b, Side.Right);
							AddVertices(b, Side.Right); counts[index] = (byte)count;
						}
						
						index++;
						if (counts[index] == 0 ||
						    (z == zTexMin + 1 && isLiq && y < edgeLevel) ||
						    (z != zTexMin && (hidden[tileIdx + chunk[cIndex - 18]] & (1 << Side.Front)) != 0)) {
							counts[index] = 0;
						} else {
							int count = StretchX(index, x, y, z, cIndex, b, Side.Front);
							AddVertices(b, Side.Front); counts[index] = (byte)count;
						}
						
						index++;
						if (counts[index] == 0 ||
						    (z == zTexMax - 1 && isLiq && y < edgeLevel) ||
						    (z != zTexMax && (hidden[tileIdx + chunk[cIndex + 18]] & (1 << Side.Back)) != 0)) {
							counts[index] = 0;
						} else {
							int count = StretchX(index, x, y, z, cIndex, b, Side.Back);
							AddVertices(b, Side.Back); counts[index] = (byte)count;
						}
						
						index++;
						if (counts[index] == 0 || y == yTexMin ||
						    (hidden[tileIdx + chunk[cIndex - 324]] & (1 << Side.Bottom)) != 0 ||
						    (y == yTexMin + 1 && isLiq)) {
							counts[index] = 0;
						} else {
							int count = StretchX(index, x, y, z, cIndex, b, Side.Bottom);
							AddVertices(b, Side.Bottom); counts[index] = (byte)count;
						}
						
						index++;
						if (counts[index] == 0 ||
						    (hidden[tileIdx + chunk[cIndex + 324]] & (1 << Side.Top)) != 0) {
							counts[index] = 0;
						} else if (!isLiq) {
							int count = StretchX(index, x, y, z, cIndex, b, Side.Top);
							AddVertices(b, Side.Top); counts[index] = (byte)count;
						} else {
							int count = StretchXLiquid(index, x, y, z, cIndex, b);
							if (count > 0) AddVertices(b, Side.Top);
							counts[index] = (byte)count;
						}
					}
				}
			}
		}
		
		protected abstract int StretchXLiquid(int countIndex, int x, int y, int z, int chunkIndex, BlockID block);
		
		protected abstract int StretchX(int countIndex, int x, int y, int z, int chunkIndex, BlockID block, int face);
		
		protected abstract int StretchZ(int countIndex, int x, int y, int z, int chunkIndex, BlockID block, int face);
		
		protected static int[] offsets = { -1, 1, -extChunkSize, extChunkSize, -extChunkSize2, extChunkSize2 };
		
		protected bool OccludedLiquid(int chunkIndex) {
			chunkIndex += 324;
			return
				BlockInfo.FullOpaque[chunk[chunkIndex]]
				&& BlockInfo.Draw[chunk[chunkIndex - 18]] != DrawType.Gas
				&& BlockInfo.Draw[chunk[chunkIndex - 1]] != DrawType.Gas
				&& BlockInfo.Draw[chunk[chunkIndex + 1]] != DrawType.Gas
				&& BlockInfo.Draw[chunk[chunkIndex + 18]] != DrawType.Gas;
		}
		
		public void OnNewMapLoaded() {
			map = game.World;
			env = game.World.Env;
			width = map.Width; height = map.Height; length = map.Length;
			maxX = width - 1; maxY = height - 1; maxZ = length - 1;
			
			sidesLevel = Math.Max(0, game.World.Env.SidesHeight);
			edgeLevel = Math.Max(0, game.World.Env.EdgeHeight);
		}
		
		public bool IsBlockLiquid(BlockID block) { // TODO: Find out how to cull liquids properly.
			/*if (BlockInfo.Collide[block] == CollideType.LiquidLava
			    || BlockInfo.Collide[block] == CollideType.LiquidWater
			    || BlockInfo.Collide[block] == CollideType.Liquid) return true;*/
			if (block >= Block.Water && block <= Block.StillLava) return true;
			return false;
		}
	}
	
	public struct ChunkPartInfo {
		public int VbId, VerticesCount, SpriteCount;
		public ushort LeftCount, RightCount, FrontCount, BackCount, BottomCount, TopCount;
	}
}