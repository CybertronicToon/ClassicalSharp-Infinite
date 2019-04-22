﻿// Copyright 2014-2017 ClassicalSharp | Licensed under BSD-3
using System;
using ClassicalSharp.Gui.Screens;
using ClassicalSharp.Gui.Widgets;
using OpenTK.Input;
using BlockID = System.UInt16;

namespace ClassicalSharp.Mode {
	
	public sealed class CreativeGameMode : IGameMode {
		
		Game game;
		
		public void Tick() { }
		
		public bool HandlesKeyDown(Key key) {
			if (key == game.Input.Keys[KeyBind.Inventory] && game.Gui.ActiveScreen == game.Gui.hudScreen) {
				game.Gui.SetNewScreen(new InventoryScreen(game));
				return true;
			} else if (key == game.Input.Keys[KeyBind.DropBlock] && !game.ClassicMode) {
				Inventory inv = game.Inventory;
				if (inv.CanChangeSelected() && inv.Selected != Block.Air) {
					// Don't assign Selected directly, because we don't want held block
					// switching positions if they already have air in their inventory hotbar.
					inv[inv.SelectedIndex] = Block.Air;
					game.Events.RaiseHeldBlockChanged();
				}
				return true;
			}
			return false;
		}
		
		public bool PickingLeft() { 
			// always play delete animations, even if we aren't picking a block.
			game.HeldBlockRenderer.ClickAnim(true);
			return false; 
		}
		
		public bool PickingRight() { return false; }
		
		public void StopPickingLeft() { }
		
		public void PickLeft(BlockID old) {
			Vector3I pos = game.SelectedPos.BlockPos;
			game.UpdateBlock(pos.X, pos.Y, pos.Z, Block.Air);
			game.UserEvents.RaiseBlockChanged(pos, old, Block.Air);
		}
		
		public void PickMiddle(BlockID old) {
			Inventory inv = game.Inventory;
			if (BlockInfo.Draw[old] == DrawType.Gas) return;
			if (!(BlockInfo.CanPlace[old] || BlockInfo.CanDelete[old])) return;
			if (!inv.CanChangeSelected() || inv.Selected == old) return;
			
			// Is the currently selected block an empty slot
			if (inv[inv.SelectedIndex] == Block.Air) { 
				inv.Selected = old; return;
			}
			
			// Try to replace same block
			for (int i = 0; i < Inventory.BlocksPerRow; i++) {
				if (inv[i] != old) continue;
				inv.SelectedIndex = i; return;
			}
			
			// Try to replace empty slots
			for (int i = 0; i < Inventory.BlocksPerRow; i++) {
				if (inv[i] != Block.Air) continue;		
				inv[i] = old;
				inv.SelectedIndex = i; return;
			}
			
			// Finally, replace the currently selected block.
			inv.Selected = old;
		}
		
		public void PickRight(BlockID old, BlockID block, BlockID sel) {
			if (SelRight(sel)) return;
			
			Vector3I pos = game.SelectedPos.TranslatedPos;
			Vector3I posSel = game.SelectedPos.BlockPos;
			
			Console.WriteLine(game.World.SafeGetData(posSel.X, posSel.Y, posSel.Z));
			
			#if ALPHA
			if (block == Block.Ladder) {
				Vector3I posLeft = pos; posLeft.X += 1;
				Vector3I posRight = pos; posRight.X -= 1;
				Vector3I posBack = pos; posBack.Z += 1;
				
				game.World.ChunkHandler.SetDataSafe(pos.X, pos.Y, pos.Z, 0x0);
				if (posSel == posLeft) game.World.ChunkHandler.SetDataSafe(pos.X, pos.Y, pos.Z, 0x4);
				if (posSel == posRight) game.World.ChunkHandler.SetDataSafe(pos.X, pos.Y, pos.Z, 0x5);
				if (posSel == posBack) game.World.ChunkHandler.SetDataSafe(pos.X, pos.Y, pos.Z, 0x3);
			} else if (block == Block.RedstoneTorchOff || block == Block.RedstoneTorchOn||
			           block == Block.Torch) {
				Vector3I posLeft = pos; posLeft.X += 1;
				Vector3I posRight = pos; posRight.X -= 1;
				Vector3I posBack = pos; posBack.Z += 1;
				Vector3I posFront = pos; posFront.Z -= 1;
				
				game.World.ChunkHandler.SetDataSafe(pos.X, pos.Y, pos.Z, 0x5);
				if (posSel == posLeft) game.World.ChunkHandler.SetDataSafe(pos.X, pos.Y, pos.Z, 0x2);
				if (posSel == posRight) game.World.ChunkHandler.SetDataSafe(pos.X, pos.Y, pos.Z, 0x1);
				if (posSel == posBack) game.World.ChunkHandler.SetDataSafe(pos.X, pos.Y, pos.Z, 0x3);
				if (posSel == posFront) game.World.ChunkHandler.SetDataSafe(pos.X, pos.Y, pos.Z, 0x4);
			} else {
				game.World.ChunkHandler.SetDataSafe(pos.X, pos.Y, pos.Z, 0x0);
			}
			#endif
			
			game.UpdateBlock(pos.X, pos.Y, pos.Z, block);
			game.UserEvents.RaiseBlockChanged(pos, old, block);
		}
		
		public bool SelRight(BlockID sel) {
			/*if (sel == Block.Wood) {
				game.Gui.SetNewScreen(new SurvivalInventoryScreen(game));
				return true;
			}*/
			return false;
		}

		public Widget MakeHotbar() { return new HotbarWidget(game); }		
		
		public void OnNewMapLoaded(Game game) { }

		public void Init(Game game) {
			this.game = game;
			Inventory inv = game.Inventory;
			inv[0] = Block.Stone;  inv[1] = Block.Cobblestone; inv[2] = Block.Brick;
			inv[3] = Block.Dirt;   inv[4] = Block.Wood;        inv[5] = Block.Log;
			inv[6] = Block.Leaves; inv[7] = Block.Grass;       inv[8] = Block.Slab;
		}
		
		
		public void Ready(Game game) { }
		public void Reset(Game game) { }
		public void OnNewMap(Game game) { }
		public void Dispose() { }
		
		public void BeginFrame(double delta) { }
		public void EndFrame(double delta) { }
	}
}
