﻿// Copyright 2014-2017 ClassicalSharp | Licensed under BSD-3
using System;
using OpenTK;
using ClassicalSharp.GraphicsAPI;

namespace ClassicalSharp.Model {
	
	/// <summary> Describes the starting index of this part within a model's array of vertices,
	/// and the number of vertices following the starting index that this part uses. </summary>
	public struct ModelPart {
		
		public int Offset, Count;
		public float RotX, RotY, RotZ;
		
		public ModelPart(int offset, int count, float rotX, float rotY, float rotZ) {
			Offset = offset; Count = count;
			RotX = rotX; RotY = rotY; RotZ = rotZ;
		}
	}
	
	public struct ModelVertex {
		public float X, Y, Z;
		public ushort U, V;
		public Vector3 D;
		public bool N;
		
		public ModelVertex(float x, float y, float z, int u, int v) {
			X = x; Y = y; Z = z;
			U = (ushort)u; V = (ushort)v;
			D = new Vector3(0, 0, 1);
			N = false;
		}
		
		public ModelVertex(float x, float y, float z, int u, int v, Vector3 d) {
			X = x; Y = y; Z = z;
			U = (ushort)u; V = (ushort)v;
			D = d;
			N = false;
		}
	}
}