// Copyright 2014-2017 ClassicalSharp | Licensed under BSD-3
using System;
using System.Drawing;
using ClassicalSharp.GraphicsAPI;
using ClassicalSharp.Gui.Widgets;
using OpenTK.Input;

namespace ClassicalSharp.Gui.Screens {
	public class HudScreen : Screen, IGameComponent {
		
		public HudScreen(Game game) : base(game) { }
		
		ChatScreen chat;
		internal Widget hotbar;
		PlayerListWidget playerList;
		Font playerFont;
		
		public void Init(Game game) { }
		public void Ready(Game game) { Init(); }
		public void Reset(Game game) { }
		public void OnNewMap(Game game) { }
		public void OnNewMapLoaded(Game game) { }
		
		internal int BottomOffset { get { return hotbar.Height; } }
		
		public override void Render(double delta) {
			IGraphicsApi gfx = game.Graphics;
			if (game.HideGui && chat.HandlesAllInput) {
				gfx.Texturing = true;
				chat.input.Render(delta);
				gfx.Texturing = false;
			}
			if (game.HideGui) return;
			bool showMinimal = game.Gui.ActiveScreen.BlocksWorld;
			
			if (playerList == null && !showMinimal) {
				gfx.Texturing = true;
				DrawCrosshairs();
				gfx.Texturing = false;
			}			
			if (chat.HandlesAllInput && !game.PureClassic) {
				chat.RenderBackground();
			}
			
			gfx.Texturing = true;
			if (!showMinimal) hotbar.Render(delta);
			chat.Render(delta);
			
			if (playerList != null && game.Gui.ActiveScreen == this) {
				playerList.Active = chat.HandlesAllInput;
				playerList.Render(delta);
				// NOTE: Should usually be caught by KeyUp, but just in case.
				if (!game.IsKeyDown(KeyBind.PlayerList)) {
					playerList.Dispose();
					playerList = null;
				}
			}
			
			gfx.Texturing = false;
		}
		
		/*const int chExtent = 16, chWeight = 2;
		static TextureRec chRec = new TextureRec(0, 0, 15/256f, 15/256f);
		void DrawCrosshairs() {			
			if (game.Gui.IconsTex <= 0) return;
			
			int cenX = game.Width / 2, cenY = game.Height / 2;
			int extent = (int)(chExtent * game.Scale(game.Height / 480f));
			Texture chTex = new Texture(game.Gui.IconsTex, cenX - extent,
			                            cenY - extent, extent * 2, extent * 2, chRec);
			chTex.Render(game.Graphics);
		}*/
		
		const int chExtent = 16, chWeight = 2;
		const int chAdd = 24;
		const int barSize = 22;
		static TextureRec chRec = new TextureRec(0, 0, 16/256f, 16/256f);
		void DrawCrosshairs() {			
			if (game.Gui.IconsTex <= 0) return;
			game.Graphics.AlphaTest = true;
			game.Graphics.RGBAlphaBlendFunc(BlendFunc.InvDestColor, BlendFunc.Zero, BlendFunc.SourceAlpha, BlendFunc.InvSourceAlpha);
			
			int scale = Utils.Floor(2 * (game.Height / 480f));
			if (scale <= 0) scale = 1;
			int cenX = game.Width;
			int cenY = game.Height;
			//int cenXInit = ((((int)cenX) / (scale * 2)) * (scale * 2));
			//int cenYInit = ((((int)cenY) / (scale * 2)) * (scale * 2));
			//int cenXInit = (int)(Math.Round((double)cenX / (scale * 2)) * (scale * 2));
			//int cenYInit = (int)(Math.Round((double)cenY / (scale * 2)) * (scale * 2));
			//int cenXInit = (int)((float)(cenX + (scale * 2) - 1) / (float)(scale * 2) * (float)(scale * 2));
			//int cenYInit = (int)((float)(cenY + (scale * 2) - 1) / (float)(scale * 2) * (float)(scale * 2));
			int cenXInit = ((cenX + (scale * 2) - 1) / (scale * 2)) * (scale * 2);
			int cenYInit = ((cenY + (scale * 2) - 1) / (scale * 2)) * (scale * 2);
			int guiCenX = cenXInit / scale;
			int guiCenY = cenYInit / scale;
			guiCenX = (int)Math.Round((double)guiCenX / 2);
			guiCenY = (int)Math.Round((double)guiCenY / 2);
			cenX = guiCenX * scale;
			cenY = guiCenY * scale;
			/*Console.WriteLine("cenX: " + cenX);
			Console.WriteLine((cenX % (scale * 2)));
			cenX += (cenX % (scale * 2));
			Console.WriteLine("cenX 2: " + cenX);
			cenY += (cenY % (scale * 2));
			cenX /= 2;
			cenY /= 2;*/
			int extent = (int)(chExtent * scale);
			int guiSub = extent + (int)(chAdd * scale) + (int)(barSize * scale);
			int numY = ((cenYInit - guiSub) / 2) + (chAdd * scale);
			Texture chTex = new Texture(game.Gui.IconsTex, cenX - (extent / 2),
			                            numY, extent, extent, chRec);
			chTex.Render(game.Graphics);
			game.Graphics.AlphaBlendFunc(BlendFunc.SourceAlpha, BlendFunc.InvSourceAlpha);
			game.Graphics.AlphaTest = false;
		}
		
		bool hadPlayerList;
		protected override void ContextLost() {
			DisposePlayerList();
			hotbar.Dispose();
		}
		
		void DisposePlayerList() {
			hadPlayerList = playerList != null;
			if (playerList != null)
				playerList.Dispose();
			playerList = null;
		}
		
		protected override void ContextRecreated() {
			hotbar.Dispose();
			hotbar.Init();
			
			if (!hadPlayerList) return;		
			bool extended = game.Server.UsingExtPlayerList && !game.UseClassicTabList;
			playerList = new PlayerListWidget(game, playerFont, !extended);

			playerList.Init();
			playerList.Reposition();
		}
		
		public override void Dispose() {
			playerFont.Dispose();
			chat.Dispose();
			ContextLost();
			
			game.WorldEvents.OnNewMap -= OnNewMap;
			game.Graphics.ContextLost -= ContextLost;
			game.Graphics.ContextRecreated -= ContextRecreated;
		}
		
		public override void OnResize(int width, int height) {
			chat.OnResize(width, height);
			hotbar.Reposition();
			
			if (playerList != null) {
				playerList.Reposition();
			}
		}
		
		public override void Init() {
			int size = game.Drawer2D.UseBitmappedChat ? 16 : 11;
			playerFont = new Font(game.FontName, size);
			hotbar = game.Mode.MakeHotbar();
			hotbar.Init();
			chat = new ChatScreen(game, this);
			chat.Init();
			
			game.WorldEvents.OnNewMap += OnNewMap;
			game.Graphics.ContextLost += ContextLost;
			game.Graphics.ContextRecreated += ContextRecreated;
		}

		void OnNewMap(object sender, EventArgs e) { DisposePlayerList(); }

		public override bool HandlesKeyPress(char key) { return chat.HandlesKeyPress(key); }
		
		public override bool HandlesKeyDown(Key key) {
			Key playerListKey = game.Mapping(KeyBind.PlayerList);
			bool handles = playerListKey != Key.Tab || !game.TabAutocomplete || !chat.HandlesAllInput;
			if (key == playerListKey && handles) {
				if (playerList == null && !game.Server.IsSinglePlayer) {
					hadPlayerList = true;
					ContextRecreated();
				}
				return true;
			}
			
			return chat.HandlesKeyDown(key) || hotbar.HandlesKeyDown(key);
		}
		
		public override bool HandlesKeyUp(Key key) {
			if (key == game.Mapping(KeyBind.PlayerList)) {
				if (playerList != null) {
					hadPlayerList = false;
					playerList.Dispose();
					playerList = null;
					return true;
				}
			}
			
			return chat.HandlesKeyUp(key) || hotbar.HandlesKeyUp(key);
		}
		
		public void OpenInput(string text) { chat.OpenInput(text); }
		
		public void AppendInput(string text) { chat.input.Append(text); }
		
		public override bool HandlesMouseScroll(float delta) {
			return chat.HandlesMouseScroll(delta);
		}
		
		public override bool HandlesMouseClick(int mouseX, int mouseY, MouseButton button) {
			if (button != MouseButton.Left || !HandlesAllInput) return false;
			
			string name;
			if (playerList == null || (name = playerList.GetNameUnder(mouseX, mouseY)) == null)
				return chat.HandlesMouseClick(mouseX, mouseY, button);
			chat.AppendTextToInput(name + " ");
			return true;
		}
	}
}
