﻿// Copyright 2014-2017 ClassicalSharp | Licensed under BSD-3
//#define TEST_VANILLA
using System;
using System.Net;
using ClassicalSharp.Entities;
using ClassicalSharp.Generator;
using ClassicalSharp.Gui.Screens;
using ClassicalSharp.Physics;
using OpenTK;
using OpenTK.Input;

namespace ClassicalSharp.Singleplayer {

	public sealed class SinglePlayerServer : IServerConnection {
		
		internal PhysicsBase physics;
		
		public SinglePlayerServer(Game window) {
			game = window;
			physics = new PhysicsBase(game);
			SupportsFullCP437 = !game.ClassicMode;
			SupportsPartialMessages = true;
			IsSinglePlayer = true;
		}
		
		public override void Connect(IPAddress address, int port) {
			game.Chat.SetLogName("Singleplayer");
			game.SupportsCPEBlocks = game.UseCPE;
			int max = game.SupportsCPEBlocks ? Block.MaxCpeBlock : Block.MaxOriginalBlock;
			for (int i = 1; i <= max; i++) {
				BlockInfo.CanPlace[i] = true;
				BlockInfo.CanDelete[i] = true;
			}
			game.Downloader.AsyncGetSkin(game.LocalPlayer.SkinName, game.LocalPlayer.SkinName);
			
			game.Events.RaiseBlockPermissionsChanged();
			//int seed = new Random().Next();
			long seed = Utils.CurrentTimeMillis();
			BeginGeneration(144, 128, 144, seed, new NotchyGenerator());
		}
		
		char lastCol = '\0';
		public override void SendChat(string text) {
			if (String.IsNullOrEmpty(text)) return;
			lastCol = '\0';
			
			while (text.Length > Utils.StringLength) {
				AddChat(text.Substring(0, Utils.StringLength));
				text = text.Substring(Utils.StringLength);
			}
			AddChat(text);
		}
		
		void AddChat(string text) {
			text = text.TrimEnd().Replace('%', '&');
			if (!IDrawer2D.IsWhiteCol(lastCol))
				text = "&" + lastCol + text;
			
			char col = IDrawer2D.LastCol(text, text.Length);
			if (col != '\0') lastCol = col;
			game.Chat.Add(text, MessageType.Normal);
		}
		
		public override void SendPosition(Vector3 pos, float rotY, float headX) {
		}
		
		public override void SendPlayerClick(MouseButton button, bool buttonDown, byte targetId, PickedPos pos) {
		}
		
		public override void Dispose() {
			physics.Dispose();
		}
		
		int curChunkX, curChunkY;
		
		public override void Tick(ScheduledTask task) {
			if (Disconnected) return;
			// Network ticked 60 times a second, only do physics 20 times a second
			if ((netTicks % 3) == 0) {
				if (DoDayNightCycle) {
					Ticks++;
				}
				physics.Tick();
				game.Mode.Tick();
				CheckAsyncResources();
				
				int x, y;
				if (game.World.ChunkHandler != null) {
					game.World.ChunkHandler.GetChunkCoords(game.LocalPlayer.Position, out x, out y);
					if (x != curChunkX || y != curChunkY) {
						curChunkX = x;
						curChunkY = y;
						int cenChunk = game.World.ChunkHandler.ChunkArray.GetLength(0) / 2;
						int maxChunk = game.World.ChunkHandler.ChunkArray.GetLength(0);
						int minChunk = 0;
						//Console.WriteLine("curChunkX: " + curChunkX + ", curChunkY: " + curChunkY);
						if (curChunkX == cenChunk && curChunkY == cenChunk) {
							//Console.WriteLine("In center chunk");
							//Console.WriteLine(maxChunk);
						} else if (curChunkX >= ((maxChunk - 1) + maxChunk) ||
						           curChunkY >= ((maxChunk - 1) + maxChunk) ||
						           curChunkX <= (minChunk - maxChunk) ||
						           curChunkY <= (minChunk - maxChunk)) {
							//Console.WriteLine("outside max bounds");
						}
					}
				}
			}
			netTicks++;			
		}
	}
}