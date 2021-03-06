﻿// Copyright 2014-2017 ClassicalSharp | Licensed under BSD-3
using System;
using System.Drawing;
using System.IO;
using ClassicalSharp.Generator;
using ClassicalSharp.Gui.Widgets;
using ClassicalSharp.Singleplayer;
using OpenTK.Input;

namespace ClassicalSharp.Gui.Screens {
	public sealed class GenLevelScreen : MenuScreen {		
		public GenLevelScreen(Game game) : base(game) { }

		MenuInputWidget selected;
		
		public override bool HandlesMouseClick(int mouseX, int mouseY, MouseButton button) {
			return HandleMouseClick(widgets, mouseX, mouseY, button);
		}
		
		public override bool HandlesKeyPress(char key) {
			return selected == null ? true :
				selected.HandlesKeyPress(key);
		}
		
		public override bool HandlesKeyDown(Key key) {
			if (key == Key.Escape) {
				game.Gui.SetNewScreen(null);
				return true;
			}
			return selected == null ? (key < Key.F1 || key > Key.F35) :
				selected.HandlesKeyDown(key);
		}
		
		public override bool HandlesKeyUp(Key key) {
			return selected == null ? true :
				selected.HandlesKeyUp(key);
		}
		
		public override void Init() {
			base.Init();
			game.Keyboard.KeyRepeat = true;
			titleFont = new Font(game.FontName, 16, FontStyle.Bold);
			regularFont = new Font(game.FontName, 16);
			ContextRecreated();
		}
		
		protected override void ContextRecreated() {
			widgets = new Widget[] {
				MakeInput(-80, false, game.World.Width.ToString()),
				MakeInput(-40, false, game.World.Height.ToString()),
				MakeInput(0, false, game.World.Length.ToString()),
				MakeInput(40, true, ""),
				
				MakeLabel(-150, -80, "Width:"), MakeLabel(-150, -40, "Height:"),
				MakeLabel(-150, 0, "Length:"), MakeLabel(-140, 40, "Seed:"),
				TextWidget.Create(game, "Generate new level", regularFont)
					.SetLocation(Anchor.Centre, Anchor.Centre, 0, -130),
				
				ButtonWidget.Create(game, 200, "Flatgrass", titleFont, GenFlatgrassClick)
					.SetLocation(Anchor.Centre, Anchor.Centre, -120, 100),
				ButtonWidget.Create(game, 200, "Vanilla", titleFont, GenNotchyClick)
					.SetLocation(Anchor.Centre, Anchor.Centre, 120, 100),
				MakeBack(false, titleFont, SwitchPause),
			};
		}
		
		InputWidget MakeInput(int y, bool seed, string value) {
			MenuInputValidator validator;
			if (seed) {
				validator = new SeedValidator();
			} else {
				validator = new IntegerValidator(1, 8192);
			}
			InputWidget input = MenuInputWidget.Create(game, 200, 30, value,
			                                                regularFont, validator)
				.SetLocation(Anchor.Centre, Anchor.Centre, 0, y);
			
			input.Active = false;
			input.OnClick = InputClick;
			return input;
		}
		
		TextWidget MakeLabel(int x, int y, string text) {
			TextWidget label = TextWidget.Create(game, text, regularFont)
				.SetLocation(Anchor.Centre, Anchor.Centre, x, y);
			
			label.XOffset = -110 - label.Width / 2;
			label.Reposition();
			label.Colour = new FastColour(224, 224, 224);
			return label;
		}
		
		public override void Dispose() {
			game.Keyboard.KeyRepeat = false;
			base.Dispose();
		}
		
		void InputClick(Game game, Widget widget, MouseButton btn, int x, int y) {
			if (btn != MouseButton.Left) return;
			if (selected != null) selected.ShowCaret = false;
			
			selected = (MenuInputWidget)widget;
			selected.HandlesMouseClick(x, y, btn);
			selected.ShowCaret = true;
		}
		
		void GenFlatgrassClick(Game game, Widget widget, MouseButton btn, int x, int y) {
			if (btn != MouseButton.Left) return;
			GenerateMap(new FlatGrassGenerator());
		}
		
		void GenNotchyClick(Game game, Widget widget, MouseButton btn, int x, int y) {
			if (btn != MouseButton.Left) return;
			GenerateMap(new NotchyGenerator());
		}
		
		void GenerateMap(IMapGenerator gen) {
			int width = GetInt(0), height = GetInt(1);
			int length = GetInt(2);
			long seed = GetSeedInt(3);
			
			long volume = (long)width * height * length;
			if (volume > int.MaxValue) {
				game.Chat.Add("&cThe generated map's volume is too big.");
			} else if (width == 0 || height == 0 || length == 0) {
				game.Chat.Add("&cOne of the map dimensions is invalid.");
			} else {
				game.Server.BeginGeneration(width, height, length, seed, gen);
			}
		}
		
		int GetInt(int index) {
			MenuInputWidget input = (MenuInputWidget)widgets[index];
			string text = input.Text.ToString();
			if (!input.Validator.IsValidValue(text))
				return 0;
			return text == "" ? 0 : Int32.Parse(text);
		}
		
		long GetSeedInt(long index) {
			MenuInputWidget input = (MenuInputWidget)widgets[index];
			string text = input.Text.ToString();
			if (text == "") return Utils.CurrentTimeMillis();
			
			if (!input.Validator.IsValidValue(text))
				return 0;
			return text == "" ? 0 : Int64.Parse(text);
		}
	}
	
	public sealed class ClassicGenLevelScreen : MenuScreen {	
		public ClassicGenLevelScreen(Game game) : base(game) { }
		
		public override bool HandlesMouseClick(int mouseX, int mouseY, MouseButton button) {
			return HandleMouseClick(widgets, mouseX, mouseY, button);
		}
		
		public override bool HandlesKeyDown(Key key) {
			if (key == Key.Escape) {
				game.Gui.SetNewScreen(null);
				return true;
			}
			return true;
		}
		
		public override void Init() {
			base.Init();
			titleFont = new Font(game.FontName, 16, FontStyle.Bold);
			regularFont = new Font(game.FontName, 16);
			ContextRecreated();
		}
		
		protected override void ContextRecreated() {
			widgets = new Widget[] {
				ButtonWidget.Create(game, 400, "Small", titleFont, GenSmallClick)
					.SetLocation(Anchor.Centre, Anchor.Centre, 0, -100),
				ButtonWidget.Create(game, 400, "Normal", titleFont, GenMediumClick)
					.SetLocation(Anchor.Centre, Anchor.Centre, 0, -50),
				ButtonWidget.Create(game, 400, "Huge", titleFont, GenHugeClick)
					.SetLocation(Anchor.Centre, Anchor.Centre, 0, 0),
				MakeBack(false, titleFont, SwitchPause),
			};
		}
		
		void GenSmallClick(Game game, Widget widget, MouseButton btn, int x, int y) {
			if (btn == MouseButton.Left) DoGen(128);
		}
		
		void GenMediumClick(Game game, Widget widget, MouseButton btn, int x, int y) {
			if (btn == MouseButton.Left) DoGen(256);
		}
		
		void GenHugeClick(Game game, Widget widget, MouseButton btn, int x, int y) {
			if (btn == MouseButton.Left) DoGen(512);
		}
		
		void DoGen(int size) {
			//int seed = new Random().Next();
			long seed = Utils.CurrentTimeMillis();
			IMapGenerator gen = new NotchyGenerator();
			game.Server.BeginGeneration(size, 128, size, seed, gen);
		}
	}
}