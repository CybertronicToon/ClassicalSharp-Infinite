﻿// Copyright 2014-2017 ClassicalSharp | Licensed under BSD-3
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using OpenTK;

namespace ClassicalSharp.GraphicsAPI {
	
	/// <summary> 3 floats for position (XYZ),<br/>
	/// 4 bytes for colour (RGBA if OpenGL, BGRA if Direct3D) </summary>
	/// <remarks> Use FastColour.Pack to convert colours to the correct swizzling. </remarks>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct VertexP3fC4b {
		public float X, Y, Z;
		public int Colour;
		
		public VertexP3fC4b(float x, float y, float z, int c) {
			X = x; Y = y; Z = z;
			Colour = c;
		}
		
		public const int Size = 16; // (4 + 4 + 4) + (1 + 1 + 1 + 1)
	}
	
	/// <summary> 3 floats for position (XYZ),<br/>
	/// 2 floats for texture coordinates (UV),<br/>
	/// 4 bytes for colour (RGBA if OpenGL, BGRA if Direct3D) </summary>
	/// <remarks> Use FastColour.Pack to convert colours to the correct swizzling. </remarks>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct VertexP3fT2fC4b {
		public float X, Y, Z;	
		public int Colour;
		public float U, V;

		public VertexP3fT2fC4b(float x, float y, float z, float u, float v, int c) {
			X = x; Y = y; Z = z; 
			U = u; V = v;
			Colour = c;
		}
		
		public const int Size = 24; // 3 * 4 + 2 * 4 + 4 * 1
	}
	
	/// <summary> 3 floats for position (XYZ),<br/>
	/// 2 floats for texture coordinates (UV),<br/>
	/// 4 bytes for colour (RGBA if OpenGL, BGRA if Direct3D),<br/>
	/// 1 vector3 for normal. (N) </summary>
	/// <remarks> Use FastColour.Pack to convert colours to the correct swizzling. </remarks>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct VertexP3fT2fC4bN1v {
		public float X, Y, Z;	
		public int Colour;
		public float U, V;
		public Vector3 Normal;

		public VertexP3fT2fC4bN1v(float x, float y, float z, float u, float v, int c) {
			X = x; Y = y; Z = z; 
			U = u; V = v;
			Colour = c;
			Normal = new Vector3(0, 0, 1000);
		}

		public VertexP3fT2fC4bN1v(float x, float y, float z, float u, float v, int c, Vector3 n) {
			X = x; Y = y; Z = z; 
			U = u; V = v;
			Colour = c;
			Normal = n;
		}
		
		public const int Size = 36; // 3 * 4 + 2 * 4 + 4 * 1 + 3 * 4
	}
}