﻿// Copyright 2014-2017 ClassicalSharp | Licensed under BSD-3
using System;
using System.Drawing;
using ClassicalSharp.GraphicsAPI;

namespace ClassicalSharp {
	
	/// <summary> Stores the four texture coordinates that describe a textured quad. </summary>
	public struct TextureRec {
		public float U1, V1, U2, V2;
		
		public TextureRec(float u, float v, float uWidth, float vHeight) {
			U1 = u; V1 = v;
			U2 = u + uWidth; V2 = v + vHeight;
		}
	}
	
	/// <summary> Contains the information necessary to describe a 2D textured quad. </summary>
	public struct Texture {
		
		public int ID;
		public short X, Y;
		public ushort Width, Height;
		public float U1, V1;
		public float U2, V2;

		public Texture(int id, int x, int y, int width, int height, 
		               float u2, float v2)
			: this(id, x, y, width, height, 0, u2, 0, v2)	{
		}
		
		public Texture(int id, int x, int y, int width, int height, 
		               TextureRec rec)
			: this(id, x, y, width, height, rec.U1, rec.U2, rec.V1, rec.V2) {
		}
		
		public Texture(int id, int x, int y, int width, int height, 
		               float u1, float u2, float v1, float v2) {
			ID = id;
			X = (short)x; Y = (short)y;
			Width = (ushort)width; Height = (ushort)height;
			U1 = u1; V1 = v1;
			U2 = u2; V2 = v2;
		}
		
		public bool IsValid { get { return ID > 0; } }
		public int X1 { get { return X; } set { X = (short)value; } }
		public int Y1 { get { return Y; } set { Y = (short)value; } }
		
		public void Render(IGraphicsApi gfx) {
			gfx.BindTexture(ID);
			gfx.Draw2DTexture(ref this, FastColour.White);
		}
		
		public void Render(IGraphicsApi gfx, FastColour colour) {
			gfx.BindTexture(ID);
			gfx.Draw2DTexture(ref this, colour);
		}
		
		public override string ToString() {
			return ID + String.Format("({0}, {1} -> {2},{3}", X, Y, Width, Height);
		}
	}
}
