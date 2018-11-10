﻿// Copyright 2014-2017 ClassicalSharp | Licensed under BSD-3
using System;
using ClassicalSharp.Entities;
using ClassicalSharp.Events;
using ClassicalSharp.GraphicsAPI;
using ClassicalSharp.Model;
using OpenTK;
using BlockID = System.UInt16;

namespace ClassicalSharp.Renderers {
	
	public class HeldBlockRenderer : IGameComponent {
		
		BlockID block;
		Game game;
		Entity held;
		Matrix4 heldBlockProj;
		
		public void Init(Game game) {
			this.game = game;
			held = new FakeHeldEntity(game);
			if (game.Mode is ClassicalSharp.Mode.CreativeGameMode) 	{
				lastBlock = game.Inventory.Selected;
			} else {
				lastBlock = game.SurvInv.Selected;
			}
			
			game.Events.ProjectionChanged += ProjectionChanged;
			game.Events.HeldBlockChanged += DoSwitchBlockAnim;
			game.UserEvents.BlockChanged += BlockChanged;
		}

		public void Ready(Game game) { }
		public void Reset(Game game) { }
		public void OnNewMap(Game game) { }
		public void OnNewMapLoaded(Game game) { }
		
		public void Dispose() {
			game.Events.ProjectionChanged -= ProjectionChanged;
			game.Events.HeldBlockChanged -= DoSwitchBlockAnim;
			game.UserEvents.BlockChanged -= BlockChanged;
		}
		
		public void Render(double delta) {
			if (!game.ShowBlockInHand) return;
			game.Graphics.Lighting = true;
			game.Graphics.UpdateLightsHeldBlock();

			float lastSwingY = swingY; swingY = 0;
			if (game.Mode is ClassicalSharp.Mode.CreativeGameMode) 	{
				block = game.Inventory.Selected;
			} else {
				block = game.SurvInv.Selected;
			}

			game.Graphics.SetMatrixMode(MatrixType.Projection);
			game.Graphics.LoadMatrix(ref heldBlockProj);
			game.Graphics.SetMatrixMode(MatrixType.Modelview);
			Matrix4 view = game.Graphics.View;
			SetMatrix();
			
			ResetHeldState();
			DoAnimation(delta, lastSwingY);
			SetBaseOffset();
			if (!game.Camera.IsThirdPerson) RenderModel();
			
			game.Graphics.View = view;
			game.Graphics.SetMatrixMode(MatrixType.Projection);
			game.Graphics.LoadMatrix(ref game.Graphics.Projection);
			game.Graphics.SetMatrixMode(MatrixType.Modelview);
			game.Graphics.Lighting = false;
		}
		
		void RenderModel() {
			game.Graphics.FaceCulling = true;
			game.Graphics.Texturing = true;
			game.Graphics.SetupAlphaState(BlockInfo.Draw[block]);
			game.Graphics.DepthTest = false;
			
			IModel model;
			if (BlockInfo.Draw[block] == DrawType.Gas) {
				model = game.ModelCache.Get("arm");
				held.ModelScale = new Vector3(1.0f);
			} else {
				model = game.ModelCache.Get("block");
				held.ModelScale = new Vector3(0.4f);
			}
			model.isHeld = true;
			model.Render(held);
			
			game.Graphics.Texturing = false;
			game.Graphics.RestoreAlphaState(BlockInfo.Draw[block]);
			game.Graphics.DepthTest = true;
			game.Graphics.FaceCulling = false;
		}
		
		static Vector3 nOffset = new Vector3(0.56f, -0.72f, -0.72f);
		static Vector3 sOffset = new Vector3(0.46f, -0.52f, -0.72f);
		void SetMatrix() {
			Player p = game.LocalPlayer;
			Vector3 eyePos = p.EyePosition;
			
			Matrix4 m, lookAt;
			Matrix4.LookAt(eyePos, eyePos - Vector3.UnitZ, Vector3.UnitY, out lookAt);
			Matrix4.Mult(out m, ref lookAt, ref game.Camera.tiltM);
			game.Graphics.View = m;
		}
		
		void ResetHeldState() {
			// Based off details from http://pastebin.com/KFV0HkmD (Thanks goodlyay!)
			Player p = game.LocalPlayer;
			held.Position = p.EyePosition;
			
			held.Position.X -= game.Camera.bobbingHor;
			held.Position.Y -= game.Camera.bobbingVer;
			held.Position.Z -= game.Camera.bobbingHor;
			
			held.HeadY = -45; held.RotY = -45;
			held.HeadX = 0; held.RotX = 0;
			held.ModelBlock = block;
			held.SkinType = p.SkinType;
			held.TextureId = p.TextureId;
			held.uScale = p.uScale;
			held.vScale = p.vScale;
		}
		
		void SetBaseOffset() {
			bool sprite = BlockInfo.Draw[block] == DrawType.Sprite;
			Vector3 offset = sprite ? sOffset : nOffset;
			
			held.Position += offset;
			if (!sprite && BlockInfo.Draw[block] != DrawType.Gas) {
				float height = BlockInfo.MaxBB[block].Y - BlockInfo.MinBB[block].Y;
				held.Position.Y += 0.2f * (1 - height);
			}
		}
		
		void ProjectionChanged(object sender, EventArgs e) {
			float fov = 70 * Utils.Deg2Rad;
			float aspectRatio = (float)game.Width / game.Height;
			float zNear = game.Graphics.MinZNear;
			game.Graphics.CalcPerspectiveMatrix(fov, aspectRatio, zNear, game.ViewDistance, out heldBlockProj);
		}

		
		bool animating, breaking, swinging;
		float swingY;
		double time, period = 0.25;
		BlockID lastBlock;

		public void ClickAnim(bool digging) {
			// TODO: timing still not quite right, rotate2 still not quite right
			ResetAnim(true, digging ? 0.35 : 0.25);
			swinging = false;
			breaking = digging;
			animating = true;
			// Start place animation at bottom of cycle
			if (!digging) time = period / 2;
		}
		
		void DoSwitchBlockAnim(object sender, EventArgs e) {
			if (swinging) {
				// Like graph -sin(x) : x=0.5 and x=2.5 have same y values
				// but increasing x causes y to change in opposite directions
				if (time > period * 0.5)
					time = period - time;
			} else {
				if (game.Mode is ClassicalSharp.Mode.CreativeGameMode) 	{
					if (block == game.Inventory.Selected) return;
				} else {
					if (block == game.SurvInv.Selected) return;
				}
				ResetAnim(false, 0.25);
				animating = true;
				swinging = true;
			}
		}
		
		void BlockChanged(object sender, BlockChangedEventArgs e) {
			if (e.Block == 0) return;
			ClickAnim(false);
		}
		
		void DoAnimation(double delta, float lastSwingY) {
			if (!animating) return;
			
			if (swinging || !breaking) {
				double t = time / period;
				swingY = -0.4f * (float)Math.Sin(t * Math.PI);
				held.Position.Y += swingY;
				
				if (swinging) {
					// i.e. the block has gone to bottom of screen and is now returning back up
					// at this point we switch over to the new held block.
					if (swingY > lastSwingY) lastBlock = block;
					block = lastBlock;
					held.ModelBlock = block;
				}
			} else {
				DigAnimation();
			}
			time += delta;
			if (time > period) ResetAnim(true, 0.25);
		}
		
		// Based off incredible gifs from (Thanks goodlyay!)
		// https://dl.dropboxusercontent.com/s/iuazpmpnr89zdgb/slowBreakTranslate.gif
		// https://dl.dropboxusercontent.com/s/z7z8bset914s0ij/slowBreakRotate1.gif
		// https://dl.dropboxusercontent.com/s/pdq79gkzntquld1/slowBreakRotate2.gif
		// https://dl.dropboxusercontent.com/s/w1ego7cy7e5nrk1/slowBreakFull.gif
		
		// https://github.com/UnknownShadow200/ClassicalSharp/wiki/Dig-animation-details
		void DigAnimation() {
			double t = time / period;
			double sinHalfCircle = Math.Sin(t * Math.PI);
			double sqrtLerpPI = Math.Sqrt(t) * Math.PI;
			
			held.Position.X -= (float)(Math.Sin(sqrtLerpPI) * 0.4);
			held.Position.Y += (float)(Math.Sin((sqrtLerpPI * 2)) * 0.2);
			held.Position.Z -= (float)(sinHalfCircle * 0.2);

			double sinHalfCircleWeird = Math.Sin(t * t * Math.PI);
			held.RotY -= (float)(Math.Sin(sqrtLerpPI) * 80);
			held.HeadY -= (float)(Math.Sin(sqrtLerpPI) * 80);
			held.RotX += (float)(sinHalfCircleWeird * 20);
		}
		
		void ResetAnim(bool setLastHeld, double period) {
			time = 0; swingY = 0;
			animating = false; swinging = false;
			this.period = period;
			
			if (game.Mode is ClassicalSharp.Mode.CreativeGameMode) 	{
				if (setLastHeld) lastBlock = game.Inventory.Selected;
			} else {
				if (setLastHeld) lastBlock = game.SurvInv.Selected;
			}
		}
	}
	
	/// <summary> Skeleton implementation of player entity so we can reuse block model rendering code. </summary>
	class FakeHeldEntity : Entity {
		
		public FakeHeldEntity(Game game) : base(game) {
			NoShade = true;
		}
		
		public override void SetLocation(LocationUpdate update, bool interpolate) { }		
		public override void Tick(double delta) { }
		public override void RenderModel(double deltaTime, float t) { }		
		public override void RenderName() { }
		public override void Despawn() { }
		
		public override int Colour() {
			Player realP = game.LocalPlayer;
			int col = realP.Colour();
			
			#if USE_DX
			// Adjust pitch so angle when looking straight down is 0.
			float adjHeadX = realP.HeadX - 90;
			if (adjHeadX < 0) adjHeadX += 360;
			
			// Adjust colour so held block is brighter when looking straght up
			float t = Math.Abs(adjHeadX - 180) / 180;
			float colScale = Utils.Lerp(0.9f, 0.7f, t);
			return FastColour.ScalePacked(col, colScale);
			#else
			return col;
			#endif
		}
	}
}