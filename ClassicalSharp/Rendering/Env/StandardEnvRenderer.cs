﻿// Copyright 2014-2017 ClassicalSharp | Licensed under BSD-3
using System;
using ClassicalSharp.Events;
using ClassicalSharp.GraphicsAPI;
using OpenTK;
using BlockID = System.UInt16;

namespace ClassicalSharp.Renderers {

	public unsafe class StandardEnvRenderer : EnvRenderer {
		
		int cloudsVb = -1, cloudVertices, skyVb = -1, skyVertices;
		
		public override void UseLegacyMode(bool legacy) {
			this.legacy = legacy;
			ContextRecreated();
		}
		
		public override void UseMinimalMode(bool minimal) {
			this.minimal = minimal;
			ContextRecreated();
		}
		
		public override void Render(double deltaTime) {
			if (minimal) { RenderMinimal(deltaTime); return; }
			if (skyVb == -1 || cloudsVb == -1) return;
			
			if (!game.SkyboxRenderer.ShouldRender) {
				RenderSky(deltaTime);
				//game.CelestialRenderer.RenderStars(deltaTime);
				game.CelestialRenderer.Render2(deltaTime);
				RenderClouds(deltaTime);
			} else if (game.World.Env.SkyboxClouds) {
				RenderClouds(deltaTime);
			}
			UpdateFog();
		}
		
		protected override void EnvVariableChanged(object sender, EnvVarEventArgs e) {
			if (minimal) return;
			
			if (e.Var == EnvVar.SkyColour) {
				ResetSky();
			} else if (e.Var == EnvVar.FogColour) {
				UpdateFog();
			} else if (e.Var == EnvVar.CloudsColour) {
				ResetClouds();
			} else if (e.Var == EnvVar.CloudsLevel) {
				ResetSky();
				ResetClouds();
			}
		}
		
		public override void Init(Game game) {
			base.Init(game);
			ResetAllEnv(null, null);
			
			game.Events.ViewDistanceChanged += ResetAllEnv;
			game.Graphics.ContextLost += ContextLost;
			game.Graphics.ContextRecreated += ContextRecreated;
			game.SetViewDistance(game.UserViewDistance, false);
		}
		
		public override void OnNewMap(Game game) {
			game.Graphics.Fog = false;
			ContextLost();
		}
		
		public override void OnNewMapLoaded(Game game) {
			game.Graphics.Fog = !minimal;
			ResetAllEnv(null, null);
		}
		
		void ResetAllEnv(object sender, EventArgs e) {
			UpdateFog();
			ContextRecreated();
		}
		
		public override void Dispose() {
			base.Dispose();
			ContextLost();
			
			game.Events.ViewDistanceChanged -= ResetAllEnv;
			game.Graphics.ContextLost -= ContextLost;
			game.Graphics.ContextRecreated -= ContextRecreated;
		}
		
		void ContextLost() {
			game.Graphics.DeleteVb(ref skyVb);
			game.Graphics.DeleteVb(ref cloudsVb);
		}
		
		void ContextRecreated() {
			ContextLost();
			game.Graphics.Fog = !minimal;
			
			if (minimal) {
				game.Graphics.ClearColour(map.Env.SkyCol);
			} else {
				game.Graphics.SetFogStart(0);
				ResetClouds();
				ResetSky();
			}
		}
		
		void RenderMinimal(double deltaTime) {
			if (!map.HasBlocks) return;
			FastColour fogCol = FastColour.White;
			float fogDensity = 0;
			BlockOn(out fogDensity, out fogCol);
			game.Graphics.ClearColour(fogCol);
			
			// TODO: rewrite this to avoid raising the event? want to avoid recreating vbos too many times often
			if (fogDensity != 0) {
				// Exp fog mode: f = e^(-density*coord)
				// Solve for f = 0.05 to figure out coord (good approx for fog end)
				float dist = (float)Math.Log(0.05) / -fogDensity;
				game.SetViewDistance(dist, false);
			} else {
				game.SetViewDistance(game.UserViewDistance, false);
			}
		}
		
		void RenderSky(double delta) {
			Vector3 pos = game.CurrentCameraPos;
			float normalY = map.Height + 8;
			float skyY = Math.Max(pos.Y + 8, normalY);
			IGraphicsApi gfx = game.Graphics;
			
			gfx.SetBatchFormat(VertexFormat.P3fC4b);
			gfx.BindVb(skyVb);
			if (skyY == normalY) {
				gfx.DrawVb_IndexedTris(skyVertices);
			} else {
				Matrix4 m = game.Graphics.View;
				float dy = skyY - normalY; // inlined Y translation matrix multiply
				m.Row3.X += dy * m.Row1.X; m.Row3.Y += dy * m.Row1.Y;
				m.Row3.Z += dy * m.Row1.Z; m.Row3.W += dy * m.Row1.W;
				
				gfx.LoadMatrix(ref m);
				gfx.DrawVb_IndexedTris(skyVertices);
				gfx.LoadMatrix(ref game.Graphics.View);
			}
		}
		
		void RenderClouds(double delta) {
			if (game.World.Env.CloudHeight < -2000) return;
			double time = game.accumulator;
			float posX = (game.World.ChunkHandler.curChunkX * 16f) / 2048f;
			float posY = (game.World.ChunkHandler.curChunkY * 16f) / 2048f;
			float offsetX = (float)(time / 2048f * 0.6f * map.Env.CloudsSpeed);
			IGraphicsApi gfx = game.Graphics;
			
			Matrix4 xMat, yMat;
			gfx.SetMatrixMode(MatrixType.Texture);
			Matrix4 matrix = Matrix4.Identity; matrix.Row3.X = offsetX + posX; // translate X axis
			matrix.Row3.Y = posY;
			gfx.LoadMatrix(ref matrix);
			gfx.SetMatrixMode(MatrixType.Modelview);
			
			gfx.AlphaTest = true;
			gfx.Texturing = true;
			gfx.BindTexture(game.CloudsTex);
			gfx.SetBatchFormat(VertexFormat.P3fT2fC4b);
			gfx.BindVb(cloudsVb);
			gfx.DrawVb_IndexedTris(cloudVertices);
			gfx.AlphaTest = false;
			gfx.Texturing = false;
			
			gfx.SetMatrixMode(MatrixType.Texture);
			gfx.LoadIdentityMatrix();
			gfx.SetMatrixMode(MatrixType.Modelview);
		}
		
		void UpdateFog() {
			if (!map.HasBlocks || minimal) return;
			FastColour fogCol = FastColour.White;
			float fogDensity = 0;
			BlockOn(out fogDensity, out fogCol);
			IGraphicsApi gfx = game.Graphics;
			
			if (fogDensity != 0) {
				gfx.SetFogMode(Fog.Exp);
				gfx.SetFogDensity(fogDensity);
			} else if (game.World.Env.ExpFog) {
				gfx.SetFogMode(Fog.Exp);
				// f = 1-z/end   f = e^(-dz)
				//   solve for f = 0.01 gives:
				// e^(-dz)=0.01 --> -dz=ln(0.01)
				// 0.99=z/end   --> z=end*0.99
				//   therefore
				// d = -ln(0.01)/(end*0.99)
				double density = -Math.Log(0.01) / (game.ViewDistance * 0.99);
				gfx.SetFogDensity((float)density);
			} else {
				gfx.SetFogMode(Fog.Linear);
				gfx.SetFogEnd(game.ViewDistance);
			}
			gfx.ClearColour(fogCol);
			gfx.SetFogColour(fogCol);
		}
		
		void ResetClouds() {
			if (!map.HasBlocks || game.Graphics.LostContext) return;
			game.Graphics.DeleteVb(ref cloudsVb);
			RebuildClouds((int)game.ViewDistance, legacy ? 128 : 65536);
		}
		
		void ResetSky() {
			if (!map.HasBlocks || game.Graphics.LostContext) return;
			game.Graphics.DeleteVb(ref skyVb);
			RebuildSky((int)game.ViewDistance, legacy ? 128 : 65536);
		}
		
		void RebuildClouds(int extent, int axisSize) {
			extent = Utils.AdjViewDist(extent);
			int x1 = -extent, x2 = map.Width + extent;
			int z1 = -extent, z2 = map.Length + extent;
			cloudVertices = Utils.CountVertices(x2 - x1, z2 - z1, axisSize);
			
			VertexP3fT2fC4b[] vertices = new VertexP3fT2fC4b[cloudVertices];
			DrawCloudsY(x1, z1, x2, z2, map.Env.CloudHeight, axisSize, map.Env.CloudsCol.Pack(), vertices);
			fixed (VertexP3fT2fC4b* ptr = vertices) {
				cloudsVb = game.Graphics.CreateVb((IntPtr)ptr, VertexFormat.P3fT2fC4b, cloudVertices);
			}
		}
		
		void RebuildSky(int extent, int axisSize) {
			extent = Utils.AdjViewDist(extent);
			int x1 = -extent, x2 = map.Width + extent;
			int z1 = -extent, z2 = map.Length + extent;
			skyVertices = Utils.CountVertices(x2 - x1, z2 - z1, axisSize);
			
			VertexP3fC4b[] vertices = new VertexP3fC4b[skyVertices];
			int height = Math.Max(map.Height + 2 + 6, map.Env.CloudHeight + 6);
			
			DrawSkyY(x1, z1, x2, z2, height, axisSize, map.Env.SkyCol.Pack(), vertices);
			fixed (VertexP3fC4b* ptr = vertices) {
				skyVb = game.Graphics.CreateVb((IntPtr)ptr, VertexFormat.P3fC4b, skyVertices);
			}
		}
		
		void DrawSkyY(int x1, int z1, int x2, int z2, int y, int axisSize, int col, VertexP3fC4b[] vertices) {
			int endX = x2, endZ = z2, startZ = z1;
			int i = 0;
			VertexP3fC4b v;
			v.Y = y; v.Colour = col;
			
			for (; x1 < endX; x1 += axisSize) {
				x2 = x1 + axisSize;
				if (x2 > endX) x2 = endX;
				z1 = startZ;
				for (; z1 < endZ; z1 += axisSize) {
					z2 = z1 + axisSize;
					if (z2 > endZ) z2 = endZ;
					
					v.X = x1; v.Z = z1; vertices[i++] = v;
					v.Z = z2; vertices[i++] = v;
					v.X = x2;           vertices[i++] = v;
					v.Z = z1; vertices[i++] = v;
				}
			}
		}
		
		void DrawCloudsY(int x1, int z1, int x2, int z2, int y, int axisSize, int col, VertexP3fT2fC4b[] vertices) {
			int endX = x2, endZ = z2, startZ = z1;
			// adjust range so that largest negative uv coordinate is shifted to 0 or above.
			float offset = Utils.CeilDiv(-x1, 2048);
			int i = 0;
			VertexP3fT2fC4b v;
			v.Y = y + 0.1f; v.Colour = col;
			
			for (; x1 < endX; x1 += axisSize) {
				x2 = x1 + axisSize;
				if (x2 > endX) x2 = endX;
				z1 = startZ;
				for (; z1 < endZ; z1 += axisSize) {
					z2 = z1 + axisSize;
					if (z2 > endZ) z2 = endZ;
					
					float u1 = x1 / 2048f + offset, u2 = x2 / 2048f + offset;
					float v1 = z1 / 2048f + offset, v2 = z2 / 2048f + offset;
					v.X = x1; v.Z = z1; v.U = u1; v.V = v1; vertices[i++] = v;
					v.Z = z2;           v.V = v2; vertices[i++] = v;
					v.X = x2;           v.U = u2;           vertices[i++] = v;
					v.Z = z1;           v.V = v1; vertices[i++] = v;
				}
			}
		}
	}
}