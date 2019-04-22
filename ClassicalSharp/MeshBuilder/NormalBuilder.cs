// Copyright 2014-2017 ClassicalSharp | Licensed under BSD-3
using System;
using ClassicalSharp.GraphicsAPI;
using ClassicalSharp.Map;
using OpenTK;
using BlockID = System.UInt16;

namespace ClassicalSharp {

	public unsafe sealed class NormalMeshBuilder : ChunkMeshBuilder {
		
		CuboidDrawer drawer = new CuboidDrawer();
		
		protected override int StretchXLiquid(int countIndex, int x, int y, int z, int chunkIndex, BlockID block) {
			if (OccludedLiquid(chunkIndex)) return 0;
			int count = 1;
			x++;
			chunkIndex++;
			countIndex += Side.Sides;
			bool stretchTile = (BlockInfo.CanStretch[block] & (1 << Side.Top)) != 0;
			
			while (x < chunkEndX && stretchTile && CanStretch(block, chunkIndex, x, y, z, Side.Top) && !OccludedLiquid(chunkIndex)) {
				counts[countIndex] = 0;
				count++;
				x++;
				chunkIndex++;
				countIndex += Side.Sides;
			}
			return count;
		}
		
		protected override int StretchX(int countIndex, int x, int y, int z, int chunkIndex, BlockID block, int face) {
			int count = 1;
			x++;
			chunkIndex++;
			countIndex += Side.Sides;
			bool stretchTile = (BlockInfo.CanStretch[block] & (1 << face)) != 0;
			
			while (x < chunkEndX && stretchTile && CanStretch(block, chunkIndex, x, y, z, face)) {
				counts[countIndex] = 0;
				count++;
				x++;
				chunkIndex++;
				countIndex += Side.Sides;
			}
			return count;
		}
		
		protected override int StretchZ(int countIndex, int x, int y, int z, int chunkIndex, BlockID block, int face) {
			int count = 1;
			z++;
			chunkIndex += extChunkSize;
			countIndex += chunkSize * Side.Sides;
			bool stretchTile = (BlockInfo.CanStretch[block] & (1 << face)) != 0;
			
			while (z < chunkEndZ && stretchTile && CanStretch(block, chunkIndex, x, y, z, face)) {
				counts[countIndex] = 0;
				count++;
				z++;
				chunkIndex += extChunkSize;
				countIndex += chunkSize * Side.Sides;
			}
			return count;
		}
		
		bool CanStretch(BlockID initial, int chunkIndex, int x, int y, int z, int face) {
			BlockID cur = chunk[chunkIndex];
			return cur == initial
				&& !BlockInfo.IsFaceHidden(cur, chunk[chunkIndex + offsets[face]], face)
				&& (fullBright || (LightCol(X, Y, Z, face, initial) == LightCol(x, y, z, face, cur)));
		}
		
		int LightCol(int x, int y, int z, int face, BlockID block) {
			int offset = (BlockInfo.LightOffset[block] >> face) & 1;
			switch (face) {
				case Side.Left:
					return x < offset          ? light.OutsideXSide   : light.LightCol_XSide_Fast(x - offset, y, z);
				case Side.Right:
					return x > (maxX - offset) ? light.OutsideXSide   : light.LightCol_XSide_Fast(x + offset, y, z);
				case Side.Front:
					return z < offset          ? light.OutsideZSide   : light.LightCol_ZSide_Fast(x, y, z - offset);
				case Side.Back:
					return z > (maxZ - offset) ? light.OutsideZSide   : light.LightCol_ZSide_Fast(x, y, z + offset);
				case Side.Bottom:
					return y <= 0              ? light.OutsideYBottom : light.LightCol_YBottom_Fast(x, y - offset, z);
				case Side.Top:
					return y >= maxY           ? light.Outside        : light.LightCol_YTop_Fast(x, (y + 1) - offset, z);
			}
			return 0;
		}
		
		protected override void PreStretchTiles(int x1, int y1, int z1) {
			base.PreStretchTiles(x1, y1, z1);
			drawer.invVerElementSize  = invVerElementSize;
			drawer.elementsPerAtlas1D = elementsPerAtlas1D;
		}
		
		private bool IsBlockRedstone1(int x, int y, int z) {
			#if ALPHA
			if (game.World.SafeGetBlock(x, y, z) == Block.Redstone ||
			    game.World.SafeGetBlock(x, y, z) == Block.RedstoneTorchOff ||
			    game.World.SafeGetBlock(x, y, z) == Block.RedstoneTorchOn ||
			    game.World.SafeGetBlock(x, y, z) == Block.StonePressurePlate ||
			    game.World.SafeGetBlock(x, y, z) == Block.WoodPressurePlate ||
			    game.World.SafeGetBlock(x, y, z) == Block.StoneButton)
					return true;
			if (game.World.SafeGetBlock(x, y, z) == Block.Air) {
			if (game.World.SafeGetBlock(x, y - 1, z) == Block.Redstone ||
			    game.World.SafeGetBlock(x, y - 1, z) == Block.RedstoneTorchOff ||
			    game.World.SafeGetBlock(x, y - 1, z) == Block.RedstoneTorchOn ||
			    game.World.SafeGetBlock(x, y - 1, z) == Block.StonePressurePlate ||
			    game.World.SafeGetBlock(x, y - 1, z) == Block.WoodPressurePlate ||
			    game.World.SafeGetBlock(x, y - 1, z) == Block.StoneButton)
					return true;
			}
			#endif
			return false;
		}
		
		private bool IsBlockRedstone2(int x, int y, int z) {
			#if ALPHA
			if (game.World.SafeGetBlock(x, y, z) == Block.Redstone ||
			    game.World.SafeGetBlock(x, y, z) == Block.RedstoneTorchOff ||
			    game.World.SafeGetBlock(x, y, z) == Block.RedstoneTorchOn ||
			    game.World.SafeGetBlock(x, y, z) == Block.StonePressurePlate ||
			    game.World.SafeGetBlock(x, y, z) == Block.WoodPressurePlate ||
			    game.World.SafeGetBlock(x, y, z) == Block.StoneButton)
					return true;
			#endif
			return false;
		}
		
		private float MaxLiquidData(int x, int y, int z, bool x2, bool z2) {
			int xmin = x;
			int zmin = z;
			if (!x2) xmin -= 1;
			if (!z2) zmin -= 1;
			byte[] data = new Byte[4];
			bool[] isLiq = new bool[4];
			bool highLiq = false;
			data[0] = game.World.SafeGetData(xmin, y, zmin);
			isLiq[0] = (game.World.SafeGetBlock(xmin, y, zmin) >= Block.Water &&
			            game.World.SafeGetBlock(xmin, y, zmin) <= Block.StillLava);
			data[1] = game.World.SafeGetData(xmin, y, zmin + 1);
			isLiq[1] = (game.World.SafeGetBlock(xmin, y, zmin + 1) >= Block.Water &&
			            game.World.SafeGetBlock(xmin, y, zmin + 1) <= Block.StillLava);
			data[2] = game.World.SafeGetData(xmin + 1, y, zmin);
			isLiq[2] = (game.World.SafeGetBlock(xmin + 1, y, zmin) >= Block.Water &&
			            game.World.SafeGetBlock(xmin + 1, y, zmin) <= Block.StillLava);
			data[3] = game.World.SafeGetData(xmin + 1, y, zmin + 1);
			isLiq[3] = (game.World.SafeGetBlock(xmin + 1, y, zmin + 1) >= Block.Water &&
			            game.World.SafeGetBlock(xmin + 1, y, zmin + 1) <= Block.StillLava);
			
			highLiq = ((game.World.SafeGetBlock(xmin, y + 1, zmin) >= Block.Water &&
			             game.World.SafeGetBlock(xmin, y + 1, zmin) <= Block.StillLava && isLiq[0]) ||
			            (game.World.SafeGetBlock(xmin, y + 1, zmin + 1) >= Block.Water &&
			             game.World.SafeGetBlock(xmin, y + 1, zmin + 1) <= Block.StillLava && isLiq[1]) ||
			            (game.World.SafeGetBlock(xmin + 1, y + 1, zmin) >= Block.Water &&
			             game.World.SafeGetBlock(xmin + 1, y + 1, zmin) <= Block.StillLava && isLiq[2]) ||
			            (game.World.SafeGetBlock(xmin + 1, y + 1, zmin + 1) >= Block.Water &&
			             game.World.SafeGetBlock(xmin + 1, y + 1, zmin + 1) <= Block.StillLava && isLiq[3]));
			
			byte maxdata = 0x7;
			bool didLiq = false;
			for (int i = 0; i < data.Length; i++) {
				if (data[i] > 0x7) continue;
				if (!isLiq[i]) continue;
				didLiq = true;
				if (maxdata > data[i]) maxdata = data[i];
			}
			//Console.WriteLine(maxdata);
			if (highLiq) return -0.75f;
			if (!didLiq) return 0x0;
			return maxdata;
		}
		
		protected override void RenderTile(int index) {
			if (BlockInfo.Draw[curBlock] == DrawType.Sprite) {
				this.fullBright = BlockInfo.FullBright[curBlock];
				this.tinted = BlockInfo.Tinted[curBlock];
				int count = counts[index + Side.Top];
				if (count != 0) DrawSprite(count);
				return;
			}
			
			int leftCount = counts[index++], rightCount = counts[index++],
			frontCount = counts[index++], backCount = counts[index++],
			bottomCount = counts[index++], topCount = counts[index++];
			if (leftCount == 0 && rightCount == 0 && frontCount == 0 &&
			    backCount == 0 && bottomCount == 0 && topCount == 0) return;
			
			byte data = game.World.SafeGetData(X, Y, Z);
			
			int partCount = 1;
			
			bool fullBright = BlockInfo.FullBright[curBlock];
			bool isTranslucent = BlockInfo.Draw[curBlock] == DrawType.Translucent;
			bool isLava = IsBlockLiquid(curBlock);
			#if ALPHA
			bool isCactus = (curBlock == Block.Cactus);
			bool isRedstone = (curBlock == Block.Redstone);
			bool isGrass = (curBlock == Block.Grass);
			bool isFence = (curBlock == Block.Fence);
			bool isAnyTorch = (curBlock == Block.RedstoneTorchOff || curBlock == Block.RedstoneTorchOn||
			                   curBlock == Block.Torch);
			bool hasSnow = false;
			bool leftFence = (game.World.SafeGetBlock(X + 1, Y, Z) == Block.Fence);
			bool rightFence = (game.World.SafeGetBlock(X - 1, Y, Z) == Block.Fence);
			bool frontFence = (game.World.SafeGetBlock(X, Y, Z + 1) == Block.Fence);
			bool backFence = (game.World.SafeGetBlock(X, Y, Z - 1) == Block.Fence);
			bool isRedstoneTorch = (curBlock == Block.RedstoneTorchOn);
			bool leftRedstone = IsBlockRedstone1(X + 1, Y, Z);
			bool rightRedstone = IsBlockRedstone1(X - 1, Y, Z);
			bool frontRedstone = IsBlockRedstone1(X, Y, Z + 1);
			bool backRedstone = IsBlockRedstone1(X, Y, Z - 1);
			if (game.World.SafeGetBlock(X, Y + 1, Z) == Block.Air) {
				if (IsBlockRedstone2(X + 1, Y + 1, Z) && !leftRedstone) {
					leftRedstone = true;
				}
				if (IsBlockRedstone1(X - 1, Y + 1, Z) && !rightRedstone) {
					rightRedstone = true;
				}
				if (IsBlockRedstone1(X, Y + 1, Z + 1) && !frontRedstone) {
					frontRedstone = true;
				}
				if (IsBlockRedstone1(X, Y + 1, Z - 1) && !backRedstone) {
					backRedstone = true;
				}
			}
			if (isGrass) {
				hasSnow = (game.World.SafeGetBlock(X, Y + 1, Z) == Block.Snow ||
				           game.World.SafeGetBlock(X, Y + 1, Z) == Block.SnowBlock);
			}
			#endif
			int lightFlags = BlockInfo.LightOffset[curBlock];
			
			drawer.Complex = false;
			
			drawer.minBB = BlockInfo.MinBB[curBlock]; drawer.minBB.Y = 1 - drawer.minBB.Y;
			drawer.maxBB = BlockInfo.MaxBB[curBlock]; drawer.maxBB.Y = 1 - drawer.maxBB.Y;
			
			Vector3 min = BlockInfo.RenderMinBB[curBlock], max = BlockInfo.RenderMaxBB[curBlock];
			#if ALPHA
			if (curBlock == Block.Ladder) {
				max.Z -= 1/16f;
				min.Z = max.Z;
				byte flip = 0;
				if (data == 0x3) flip = 2;
				if (data == 0x4) flip = 1;
				if (data == 0x5) flip = 3;
				Utils.FlipBounds(min, max, out min, out max, flip);
				Utils.FlipBounds(drawer.minBB, drawer.maxBB, out drawer.minBB, out drawer.maxBB, flip);
			} else if (curBlock >= Block.Water && curBlock <= Block.StillLava) {
				float sub = ((16f/16f) - (1.5f/16f)) / 8;
				if (data > 0x0 && data < 0x8) {
					//max.Y -= sub * data;
					//drawer.maxBB.Y += sub * data;
					drawer.Complex = true;
				}
			}
			#endif
			drawer.x1 = X + min.X; drawer.y1 = Y + min.Y; drawer.z1 = Z + min.Z;
			drawer.x2 = X + max.X; drawer.y2 = Y + max.Y; drawer.z2 = Z + max.Z;
			drawer.x1y1z1 = new Vector3(drawer.x1, drawer.y1, drawer.z1);
			drawer.x1y1z2 = new Vector3(drawer.x1, drawer.y1, drawer.z2);
			drawer.x2y1z1 = new Vector3(drawer.x2, drawer.y1, drawer.z1);
			drawer.x2y1z2 = new Vector3(drawer.x2, drawer.y1, drawer.z2);
			
			drawer.x1y2z1 = new Vector3(drawer.x1, drawer.y2, drawer.z1);
			drawer.x1y2z2 = new Vector3(drawer.x1, drawer.y2, drawer.z2);
			drawer.x2y2z1 = new Vector3(drawer.x2, drawer.y2, drawer.z1);
			drawer.x2y2z2 = new Vector3(drawer.x2, drawer.y2, drawer.z2);
			
			drawer.Tinted = BlockInfo.Tinted[curBlock];
			drawer.TintColour = BlockInfo.FogColour[curBlock];
			
			#if ALPHA
			if (isRedstone) {
				drawer.y2 -= 0.75f/16f;
				drawer.y1 = drawer.y2;
			} else if (isFence) {
				partCount = 9;
			} else if (isAnyTorch && data >= 1 && data <= 4) {
				drawer.Complex = true;
				
				if (data == 4) { // North.
					drawer.x1y1z1.Z -= 8/16f;
					drawer.x1y1z2.Z -= 8/16f;
					drawer.x2y1z1.Z -= 8/16f;
					drawer.x2y1z2.Z -= 8/16f;
					
					drawer.x1y2z1.Z -= 4/16f;
					drawer.x1y2z2.Z -= 4/16f;
					drawer.x2y2z1.Z -= 4/16f;
					drawer.x2y2z2.Z -= 4/16f;
				} else if (data == 3) { // South.
					drawer.x1y1z1.Z += 8/16f;
					drawer.x1y1z2.Z += 8/16f;
					drawer.x2y1z1.Z += 8/16f;
					drawer.x2y1z2.Z += 8/16f;
					
					drawer.x1y2z1.Z += 4/16f;
					drawer.x1y2z2.Z += 4/16f;
					drawer.x2y2z1.Z += 4/16f;
					drawer.x2y2z2.Z += 4/16f;
				} else if (data == 1) { // East.
					drawer.x1y1z1.X -= 8/16f;
					drawer.x1y1z2.X -= 8/16f;
					drawer.x2y1z1.X -= 8/16f;
					drawer.x2y1z2.X -= 8/16f;
					
					drawer.x1y2z1.X -= 4/16f;
					drawer.x1y2z2.X -= 4/16f;
					drawer.x2y2z1.X -= 4/16f;
					drawer.x2y2z2.X -= 4/16f;
				} else if (data == 2) { // West.
					drawer.x1y1z1.X += 8/16f;
					drawer.x1y1z2.X += 8/16f;
					drawer.x2y1z1.X += 8/16f;
					drawer.x2y1z2.X += 8/16f;
					
					drawer.x1y2z1.X += 4/16f;
					drawer.x1y2z2.X += 4/16f;
					drawer.x2y2z1.X += 4/16f;
					drawer.x2y2z2.X += 4/16f;
				}
				
				
				drawer.x1y1z1.Y += 3.25f/16f;
				drawer.x1y1z2.Y += 3.25f/16f;
				drawer.x2y1z1.Y += 3.25f/16f;
				drawer.x2y1z2.Y += 3.25f/16f;
				
				drawer.x1y2z1.Y += 3.25f/16f;
				drawer.x1y2z2.Y += 3.25f/16f;
				drawer.x2y2z1.Y += 3.25f/16f;
				drawer.x2y2z2.Y += 3.25f/16f;
			} else if (curBlock >= Block.Water && curBlock <= Block.StillLava) {
				float sub = ((16f/16f)) / 8;;
				float x1z1 = sub * MaxLiquidData(X, Y, Z, false, false);
				float x1z2 = sub * MaxLiquidData(X, Y, Z, false, true);
				float x2z1 = sub * MaxLiquidData(X, Y, Z, true, false);
				float x2z2 = sub * MaxLiquidData(X, Y, Z, true, true);
				drawer.x1y2z1.Y -= x1z1;
				drawer.x1y2z2.Y -= x1z2;
				drawer.x2y2z1.Y -= x2z1;
				drawer.x2y2z2.Y -= x2z2;
				
			}
			#endif
			
			for (int p = 0; p < partCount; p++) {
			
			#if ALPHA
			if (isFence) {
				if (p == 1 && leftFence) {
					min = new Vector3(10/16f, 12/16f, 7/16f);
					max = new Vector3(16/16f, 15/16f, 9/16f);
					drawer.x1 = X + min.X; drawer.y1 = Y + min.Y; drawer.z1 = Z + min.Z;
					drawer.x2 = X + max.X; drawer.y2 = Y + max.Y; drawer.z2 = Z + max.Z;
					drawer.minBB = min;
					drawer.maxBB = max;
					drawer.minBB.Y = 1 - min.Y;
					drawer.maxBB.Y = 1 - max.Y;
				} else if (p == 2 && leftFence) {
					min = new Vector3(10/16f, 6/16f, 7/16f);
					max = new Vector3(16/16f, 9/16f, 9/16f);
					drawer.x1 = X + min.X; drawer.y1 = Y + min.Y; drawer.z1 = Z + min.Z;
					drawer.x2 = X + max.X; drawer.y2 = Y + max.Y; drawer.z2 = Z + max.Z;
					drawer.minBB = min;
					drawer.maxBB = max;
					drawer.minBB.Y = 1 - min.Y;
					drawer.maxBB.Y = 1 - max.Y;
				} else if (p == 3 && rightFence) {
					min = new Vector3(0, 12/16f, 7/16f);
					max = new Vector3(6/16f, 15/16f, 9/16f);
					drawer.x1 = X + min.X; drawer.y1 = Y + min.Y; drawer.z1 = Z + min.Z;
					drawer.x2 = X + max.X; drawer.y2 = Y + max.Y; drawer.z2 = Z + max.Z;
					drawer.minBB = min;
					drawer.maxBB = max;
					drawer.minBB.Y = 1 - min.Y;
					drawer.maxBB.Y = 1 - max.Y;
				} else if (p == 4 && rightFence) {
					min = new Vector3(0, 6/16f, 7/16f);
					max = new Vector3(6/16f, 9/16f, 9/16f);
					drawer.x1 = X + min.X; drawer.y1 = Y + min.Y; drawer.z1 = Z + min.Z;
					drawer.x2 = X + max.X; drawer.y2 = Y + max.Y; drawer.z2 = Z + max.Z;
					drawer.minBB = min;
					drawer.maxBB = max;
					drawer.minBB.Y = 1 - min.Y;
					drawer.maxBB.Y = 1 - max.Y;
				} else if (p == 5 && frontFence) {
					min = new Vector3(7/16f, 12/16f, 10/16f);
					max = new Vector3(9/16f, 15/16f, 16/16f);
					drawer.x1 = X + min.X; drawer.y1 = Y + min.Y; drawer.z1 = Z + min.Z;
					drawer.x2 = X + max.X; drawer.y2 = Y + max.Y; drawer.z2 = Z + max.Z;
					drawer.minBB = min;
					drawer.maxBB = max;
					drawer.minBB.Y = 1 - min.Y;
					drawer.maxBB.Y = 1 - max.Y;
				} else if (p == 6 && frontFence) {
					min = new Vector3(7/16f, 6/16f, 10/16f);
					max = new Vector3(9/16f, 9/16f, 16/16f);
					drawer.x1 = X + min.X; drawer.y1 = Y + min.Y; drawer.z1 = Z + min.Z;
					drawer.x2 = X + max.X; drawer.y2 = Y + max.Y; drawer.z2 = Z + max.Z;
					drawer.minBB = min;
					drawer.maxBB = max;
					drawer.minBB.Y = 1 - min.Y;
					drawer.maxBB.Y = 1 - max.Y;
				} else if (p == 7 && backFence) {
					min = new Vector3(7/16f, 12/16f, 0);
					max = new Vector3(9/16f, 15/16f, 6/16f);
					drawer.x1 = X + min.X; drawer.y1 = Y + min.Y; drawer.z1 = Z + min.Z;
					drawer.x2 = X + max.X; drawer.y2 = Y + max.Y; drawer.z2 = Z + max.Z;
					drawer.minBB = min;
					drawer.maxBB = max;
					drawer.minBB.Y = 1 - min.Y;
					drawer.maxBB.Y = 1 - max.Y;
				} else if (p == 8 && backFence) {
					min = new Vector3(7/16f, 6/16f, 0);
					max = new Vector3(9/16f, 9/16f, 6/16f);
					drawer.x1 = X + min.X; drawer.y1 = Y + min.Y; drawer.z1 = Z + min.Z;
					drawer.x2 = X + max.X; drawer.y2 = Y + max.Y; drawer.z2 = Z + max.Z;
					drawer.minBB = min;
					drawer.maxBB = max;
					drawer.minBB.Y = 1 - min.Y;
					drawer.maxBB.Y = 1 - max.Y;
				} else if (p > 0) {
					min = Vector3.Zero;
					max = Vector3.Zero;
					drawer.x1 = X + min.X; drawer.y1 = Y + min.Y; drawer.z1 = Z + min.Z;
					drawer.x2 = X + max.X; drawer.y2 = Y + max.Y; drawer.z2 = Z + max.Z;
					drawer.minBB = min;
					drawer.maxBB = max;
					drawer.minBB.Y = 1 - min.Y;
					drawer.maxBB.Y = 1 - max.Y;
				}
			}
			#endif
			
			if (leftCount != 0) {
				int texLoc = BlockInfo.textures[curBlock * Side.Sides + Side.Left];
				int i = texLoc / elementsPerAtlas1D;
				int offset = (lightFlags >> Side.Left) & 1;
				DrawInfo part;
				
				if (isLava && !isTranslucent) {
					part = liquidParts[i];
				} else {
					part = isTranslucent ? translucentParts[i] : normalParts[i];
				}
				#if ALPHA
				if (isCactus) {
					drawer.z1 -= 1/16f;
					drawer.z2 += 1/16f;
					drawer.minBB.Z -= 1/16f;
					drawer.maxBB.Z += 1/16f;
				} else if (isRedstoneTorch) {
					drawer.z1 -= 1/16f;
					drawer.z2 += 1/16f;
					drawer.y2 += 1/16f;
					drawer.minBB.Z -= 1/16f;
					drawer.maxBB.Z += 1/16f;
					drawer.maxBB.Y -= 1/16f;
				} else if (isGrass && hasSnow) {
					texLoc += 65;
				}
				#endif
				int col = fullBright ? FastColour.WhitePacked :
					X >= offset ? light.LightCol_XSide_Fast(X - offset, Y, Z) : light.OutsideXSide;
				drawer.Left(leftCount, col, texLoc, part.vertices, ref part.vIndex[Side.Left]);
				/*drawer.z1 += 1;
				drawer.z2 += 1;
				drawer.Left(leftCount, col, texLoc, part.vertices, ref part.vIndex[Side.Left]);
				drawer.z1 -= 1;
				drawer.z2 -= 1;*/
				#if ALPHA
				if (isCactus) {
					drawer.z1 += 1/16f;
					drawer.z2 -= 1/16f;
					drawer.minBB.Z += 1/16f;
					drawer.maxBB.Z -= 1/16f;
				} else if (isRedstoneTorch) {
					drawer.z1 += 1/16f;
					drawer.z2 -= 1/16f;
					drawer.y2 -= 1/16f;
					drawer.minBB.Z += 1/16f;
					drawer.maxBB.Z -= 1/16f;
					drawer.maxBB.Y += 1/16f;
				}
				#endif
			}
			
			if (rightCount != 0) {
				int texLoc = BlockInfo.textures[curBlock * Side.Sides + Side.Right];
				int i = texLoc / elementsPerAtlas1D;
				int offset = (lightFlags >> Side.Right) & 1;
				DrawInfo part;
				
				if (isLava && !isTranslucent) {
					part = liquidParts[i];
				} else {
					part = isTranslucent ? translucentParts[i] : normalParts[i];
				}
				#if ALPHA
				if (isCactus) {
					drawer.z1 -= 1/16f;
					drawer.z2 += 1/16f;
					drawer.minBB.Z -= 1/16f;
					drawer.maxBB.Z += 1/16f;
				} else if (isRedstoneTorch) {
					drawer.z1 -= 1/16f;
					drawer.z2 += 1/16f;
					drawer.y2 += 1/16f;
					drawer.minBB.Z -= 1/16f;
					drawer.maxBB.Z += 1/16f;
					drawer.maxBB.Y -= 1/16f;
				} else if (isGrass && hasSnow) {
					texLoc += 65;
				}
				#endif
				int col = fullBright ? FastColour.WhitePacked :
					X <= (maxX - offset) ? light.LightCol_XSide_Fast(X + offset, Y, Z) : light.OutsideXSide;
				drawer.Right(rightCount, col, texLoc, part.vertices, ref part.vIndex[Side.Right]);
				#if ALPHA
				if (isCactus) {
					drawer.z1 += 1/16f;
					drawer.z2 -= 1/16f;
					drawer.minBB.Z += 1/16f;
					drawer.maxBB.Z -= 1/16f;
				} else if (isRedstoneTorch) {
					drawer.z1 += 1/16f;
					drawer.z2 -= 1/16f;
					drawer.y2 -= 1/16f;
					drawer.minBB.Z += 1/16f;
					drawer.maxBB.Z -= 1/16f;
					drawer.maxBB.Y += 1/16f;
				}
				#endif
			}
			
			if (frontCount != 0) {
				int texLoc = BlockInfo.textures[curBlock * Side.Sides + Side.Front];
				int i = texLoc / elementsPerAtlas1D;
				int offset = (lightFlags >> Side.Front) & 1;
				DrawInfo part;
				
				if (isLava && !isTranslucent) {
					part = liquidParts[i];
				} else {
					part = isTranslucent ? translucentParts[i] : normalParts[i];
				}
				#if ALPHA
				if (isCactus) {
					drawer.x1 -= 1/16f;
					drawer.x2 += 1/16f;
					drawer.minBB.X -= 1/16f;
					drawer.maxBB.X += 1/16f;
				} else if (isRedstoneTorch) {
					drawer.x1 -= 1/16f;
					drawer.x2 += 1/16f;
					drawer.y2 += 1/16f;
					drawer.minBB.X -= 1/16f;
					drawer.maxBB.X += 1/16f;
					drawer.maxBB.Y -= 1/16f;
				} else if (isGrass && hasSnow) {
					texLoc += 65;
				}
				#endif
				int col = fullBright ? FastColour.WhitePacked :
					Z >= offset ? light.LightCol_ZSide_Fast(X, Y, Z - offset) : light.OutsideZSide;
				drawer.Front(frontCount, col, texLoc, part.vertices, ref part.vIndex[Side.Front]);
				#if ALPHA
				if (isCactus) {
					drawer.x1 += 1/16f;
					drawer.x2 -= 1/16f;
					drawer.minBB.X += 1/16f;
					drawer.maxBB.X -= 1/16f;
				} else if (isRedstoneTorch) {
					drawer.x1 += 1/16f;
					drawer.x2 -= 1/16f;
					drawer.y2 -= 1/16f;
					drawer.minBB.X += 1/16f;
					drawer.maxBB.X -= 1/16f;
					drawer.maxBB.Y += 1/16f;
				}
				#endif
			}
			
			if (backCount != 0) {
				int texLoc = BlockInfo.textures[curBlock * Side.Sides + Side.Back];
				int i = texLoc / elementsPerAtlas1D;
				int offset = (lightFlags >> Side.Back) & 1;
				DrawInfo part;
				
				if (isLava && !isTranslucent) {
					part = liquidParts[i];
				} else {
					part = isTranslucent ? translucentParts[i] : normalParts[i];
				}
				#if ALPHA
				if (isCactus) {
					drawer.x1 -= 1/16f;
					drawer.x2 += 1/16f;
					drawer.minBB.X -= 1/16f;
					drawer.maxBB.X += 1/16f;
				} else if (isRedstoneTorch) {
					drawer.x1 -= 1/16f;
					drawer.x2 += 1/16f;
					drawer.y2 += 1/16f;
					drawer.minBB.X -= 1/16f;
					drawer.maxBB.X += 1/16f;
					drawer.maxBB.Y -= 1/16f;
				} else if (isGrass && hasSnow) {
					texLoc += 65;
				}
				#endif
				int col = fullBright ? FastColour.WhitePacked :
					Z <= (maxZ - offset) ? light.LightCol_ZSide_Fast(X, Y, Z + offset) : light.OutsideZSide;
				drawer.Back(backCount, col, texLoc, part.vertices, ref part.vIndex[Side.Back]);
				#if ALPHA
				if (isCactus) {
					drawer.x1 += 1/16f;
					drawer.x2 -= 1/16f;
					drawer.minBB.X += 1/16f;
					drawer.maxBB.X -= 1/16f;
				} else if (isRedstoneTorch) {
					drawer.x1 += 1/16f;
					drawer.x2 -= 1/16f;
					drawer.y2 -= 1/16f;
					drawer.minBB.X += 1/16f;
					drawer.maxBB.X -= 1/16f;
					drawer.maxBB.Y += 1/16f;
				}
				#endif
			}
			
			if (bottomCount != 0) {
				int texLoc = BlockInfo.textures[curBlock * Side.Sides + Side.Bottom];
				int i = texLoc / elementsPerAtlas1D;
				int offset = (lightFlags >> Side.Bottom) & 1;
				DrawInfo part;
				
				if (isLava && !isTranslucent) {
					part = liquidParts[i];
				} else {
					part = isTranslucent ? translucentParts[i] : normalParts[i];
				}
				int col = fullBright ? FastColour.WhitePacked : light.LightCol_YBottom_Fast(X, Y - offset, Z);
				drawer.Bottom(bottomCount, col, texLoc, part.vertices, ref part.vIndex[Side.Bottom]);
			}
			
			if (topCount != 0) {
				int texLoc = BlockInfo.textures[curBlock * Side.Sides + Side.Top];
				int i = texLoc / elementsPerAtlas1D;
				int offset = (lightFlags >> Side.Top) & 1;
				int flip = 0;
				DrawInfo part;

				if (isLava && !isTranslucent) {
					part = liquidParts[i];
				} else {
					part = isTranslucent ? translucentParts[i] : normalParts[i];
				}
				#if ALPHA
				if (isRedstoneTorch || curBlock == Block.RedstoneTorchOff || curBlock == Block.Torch) {
					drawer.minBB.Z -= 1/16f;
					drawer.maxBB.Z -= 1/16f;
				} else if (isRedstone) {
					if (data != 0) {
						drawer.minBB.Z += 16;
						drawer.maxBB.Z += 16;
					}
					int numRedstone = (leftRedstone? 1:0) + (rightRedstone? 1:0) + (frontRedstone? 1:0) + 
					    (backRedstone? 1:0);
					if (numRedstone == 1) {
						if (rightRedstone || leftRedstone) {
							drawer.minBB.Z += 1;
							drawer.maxBB.Z += 1;
						} else if (frontRedstone || backRedstone) {
							drawer.minBB.Z += 1;
							drawer.maxBB.Z += 1;
							flip = 1;
						}
					} else if (numRedstone == 2) {
						if (rightRedstone && leftRedstone) {
							drawer.minBB.Z += 1;
							drawer.maxBB.Z += 1;
						} else if (frontRedstone && backRedstone) {
							drawer.minBB.Z += 1;
							drawer.maxBB.Z += 1;
							flip = 1;
						} else {
							if (rightRedstone) {
								drawer.maxBB.X -= 5/16f;
								drawer.x2 -= 5/16f;
							} if (leftRedstone) {
								drawer.minBB.X += 5/16f;
								drawer.x1 += 5/16f;
							} if (backRedstone) {
								drawer.maxBB.Z -= 5/16f;
								drawer.z2 -= 5/16f;
							} if (frontRedstone) {
								drawer.minBB.Z += 5/16f;
								drawer.z1 += 5/16f;
							}
						}
					} else if (numRedstone == 3) {
						if (!rightRedstone) {
							drawer.minBB.X += 5/16f;
							drawer.x1 += 5/16f;
						} else if (!leftRedstone) {
							drawer.maxBB.X -= 5/16f;
							drawer.x2 -= 5/16f;
						} else if (!backRedstone) {
							drawer.minBB.Z += 5/16f;
							drawer.z1 += 5/16f;
						} else if (!frontRedstone) {
							drawer.maxBB.Z -= 5/16f;
							drawer.z2 -= 5/16f;
						}
					}
				}
				#endif
				int col = fullBright ? FastColour.WhitePacked : light.LightCol_YTop_Fast(X, (Y + 1) - offset, Z);
				drawer.Top(topCount, col, texLoc, flip, part.vertices, ref part.vIndex[Side.Top]);
				#if ALPHA
				if (isRedstoneTorch || curBlock == Block.RedstoneTorchOff || curBlock == Block.Torch) {
					drawer.minBB.Z += 1/16f;
					drawer.maxBB.Z += 1/16f;
				} else if (isRedstone) {
					int numRedstone = (leftRedstone? 1:0) + (rightRedstone? 1:0) + (frontRedstone? 1:0) + 
					    (backRedstone? 1:0);
					if (numRedstone == 2) {
						if (rightRedstone) {
							drawer.maxBB.X += 5/16f;
							drawer.x2 += 5/16f;
						}
					}
				}
				#endif
				}
			}
		}
	}
}