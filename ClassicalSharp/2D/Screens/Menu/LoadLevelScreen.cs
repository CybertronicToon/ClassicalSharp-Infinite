﻿// Copyright 2014-2017 ClassicalSharp | Licensed under BSD-3
using System;
using System.IO;
using ClassicalSharp.Entities;
using ClassicalSharp.Map;
using ClassicalSharp.Gui.Widgets;
using ClassicalSharp.Textures;
using OpenTK.Input;
using BlockID = System.UInt16;

namespace ClassicalSharp.Gui.Screens {
	public sealed class LoadLevelScreen : FilesScreen {
		
		public LoadLevelScreen(Game game) : base(game) {
			titleText = "Select a level";
			string dir = Path.Combine(Program.AppDirectory, "maps");
			string[] rawFiles = Directory.GetFiles(dir);
			int count = 0;
			
			// Only add map files
			for (int i = 0; i < rawFiles.Length; i++) {
				string file = rawFiles[i];
				if (file.EndsWith(".cw") || file.EndsWith(".dat")
				    || file.EndsWith(".fcm") || file.EndsWith(".lvl")) {
					count++;
				} else {
					rawFiles[i] = null;
				}
			}
			
			entries = new string[count];
			for (int i = 0, j = 0; i < rawFiles.Length; i++) {
				string file = rawFiles[i];
				if (file == null) continue;
				entries[j] = Path.GetFileName(file); j++;
			}
			Array.Sort(entries);
		}
		
		protected override void TextButtonClick(Game game, Widget widget, MouseButton btn, int x, int y) {
			if (btn != MouseButton.Left) return;
			string path = Path.Combine(Program.AppDirectory, "maps");
			path = Path.Combine(path, ((ButtonWidget)widget).Text);
			if (File.Exists(path))
				LoadMap(path);
		}
		
		void LoadMap(string path) {
			IMapFormatImporter importer = null;
			game.Server.seed = 0;
			if (path.EndsWith(".dat")) {
				importer = new MapDatImporter();
			} else if (path.EndsWith(".fcm")) {
				importer = new MapFcm3Importer();
			} else if (path.EndsWith(".cw")) {
				importer = new MapCwImporter();
			} else if (path.EndsWith(".lvl")) {
				importer = new MapLvlImporter();
			}
			
			try {
				using (FileStream fs = File.OpenRead(path)) {
					int width, height, length;
					game.World.Reset();
					game.WorldEvents.RaiseOnNewMap();
					
					if (game.World.TextureUrl != null) {
						TexturePack.ExtractDefault(game);
						game.World.TextureUrl = null;
					}
					BlockInfo.Reset();
					game.Inventory.SetDefaultMapping();
					
					byte[] blocks = importer.Load(fs, game, out width, out height, out length);
					game.World.SetNewMap(blocks, width, height, length);
					
					game.WorldEvents.RaiseOnNewMapLoaded();
					if (game.UseServerTextures && game.World.TextureUrl != null)
						game.Server.RetrieveTexturePack(game.World.TextureUrl);
					
					LocalPlayer p = game.LocalPlayer;
					LocationUpdate update = LocationUpdate.MakePosAndOri(p.Spawn, p.SpawnRotY, p.SpawnHeadX, false);
					p.SetLocation(update, false);
				}
			} catch (Exception ex) {
				ErrorHandler.LogError("loading map", ex);
				string file = Path.GetFileName(path);
				game.Chat.Add("&eFailed to load map \"" + file + "\"");
			}
		}
	}
}