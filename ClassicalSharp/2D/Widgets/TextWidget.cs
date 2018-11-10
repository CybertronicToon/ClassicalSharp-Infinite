﻿// Copyright 2014-2017 ClassicalSharp | Licensed under BSD-3
using System;
using System.Drawing;

namespace ClassicalSharp.Gui.Widgets {	
	public class TextWidget : Widget {
		
		public TextWidget(Game game, Font font) : base(game) {
			this.font = font;
		}
		
		public static TextWidget Create(Game game, string text, Font font) {
			TextWidget w = new TextWidget(game, font);
			w.Init();
			w.SetText(text);
			return w;
		}
		
		public TextWidget SetLocation(Anchor horAnchor, Anchor verAnchor, int xOffset, int yOffset) {
			HorizontalAnchor = horAnchor; VerticalAnchor = verAnchor;
			XOffset = xOffset; YOffset = yOffset;
			Reposition();
			return this;
		}
		
		protected Texture texture;
		protected int defaultHeight;
		protected internal Font font;
		
		public bool ReducePadding;
		public FastColour Colour = FastColour.White;
		public bool IsValid { get { return texture.IsValid; } }
		
		public override void Init() {
			int height = game.Drawer2D.FontHeight(font, true);
			SetHeight(height);
		}
		
		protected void SetHeight(int height) {
			if (ReducePadding)
				game.Drawer2D.ReducePadding(ref height, Utils.Floor(font.Size), 4);
			defaultHeight = height;
			Height = height;
		}
		
		public void SetText(string text) {
			game.Graphics.DeleteTexture(ref texture);
			if (IDrawer2D.EmptyText(text)) {
				texture = new Texture();
				Width = 0; Height = defaultHeight;
			} else {
				DrawTextArgs args = new DrawTextArgs(text, font, true);
				texture = game.Drawer2D.MakeTextTexture(ref args, 0, 0);
				if (ReducePadding)
					game.Drawer2D.ReducePadding(ref texture, Utils.Floor(font.Size), 4);
				Width = texture.Width; Height = texture.Height;

				Reposition();
				texture.X1 = X; texture.Y1 = Y;
			}
		}
		
		public override void Render(double delta) {
			if (texture.IsValid) {
				texture.Render(game.Graphics, Colour);
			}
		}
		
		public override void Dispose() {
			game.Graphics.DeleteTexture(ref texture);
		}
		
		public override void Reposition() {
			int oldX = X, oldY = Y;
			base.Reposition();
			
			texture.X1 += X - oldX;
			texture.Y1 += Y - oldY;
		}
	}
}