// Copyright 2014-2017 ClassicalSharp | Licensed under BSD-3
using System;
using ClassicalSharp.GraphicsAPI;
using OpenTK;

namespace ClassicalSharp {

	/// <summary> Draws the vertices for a cuboid region. </summary>
	public sealed class CuboidDrawer {

		public int elementsPerAtlas1D;
		public float invVerElementSize;
		
		public Vector3 topNormal = new Vector3(0, 1, 0);
		public Vector3 bottomNormal = new Vector3(0, -1, 0);
		public Vector3 leftNormal = new Vector3(-1, 0, 0);
		public Vector3 rightNormal = new Vector3(1, 0, 0);
		public Vector3 frontNormal = new Vector3(0, 0, 1);
		public Vector3 backNormal = new Vector3(0, 0, -1);
		
		public Vector3 x1y1z1;
		public Vector3 x1y1z2;
		public Vector3 x2y1z1;
		public Vector3 x2y1z2;
		
		public Vector3 x1y2z1;
		public Vector3 x1y2z2;
		public Vector3 x2y2z1;
		public Vector3 x2y2z2;
		
		/// <summary> Whether a colour tinting effect should be applied to all faces. </summary>
		public bool Tinted;
		
		/// <summary> The tint colour to multiply colour of faces by. </summary>
		public FastColour TintColour;
		
		/// <summary> Whether all vertices are specified individually. </summary>
		public bool Complex = false;
		
		public Vector3 minBB, maxBB;
		public float x1, y1, z1, x2, y2, z2;
		
		
		/// <summary> Draws the left face of the given cuboid region. </summary>
		public void Left(int count, int col, int texLoc, VertexP3fT2fC4b[] vertices, ref int index) {
			float vOrigin = (texLoc % elementsPerAtlas1D) * invVerElementSize;
			float u1 = minBB.Z, u2 = (count - 1) + maxBB.Z * 15.99f/16f;
			float v1 = vOrigin + maxBB.Y * invVerElementSize;
			float v2 = vOrigin + minBB.Y * invVerElementSize * 15.99f/16f;
			if (Tinted) col = TintBlock(col);
			float x1 = this.x1;
			float x2 = this.x1;
			float x3 = this.x1;
			float x4 = this.x1;
			float y1 = this.y2;
			float y2 = this.y2;
			float y3 = this.y1;
			float y4 = this.y1;
			float z2 = this.z1;
			float z1 = this.z2 + (count - 1);
			float z3 = this.z2 + (count - 1);
			float z4 = this.z1;
			
			if (Complex) {
				x1 = x1y2z2.X;
				x2 = x1y2z1.X;
				x3 = x1y1z2.X;
				x4 = x1y1z1.X;
				y1 = x1y2z2.Y;
				y2 = x1y2z1.Y;
				y3 = x1y1z2.Y;
				y4 = x1y1z1.Y;
				z3 = x1y2z2.Z + (count - 1);
				z4 = x1y2z1.Z;
				z1 = x1y1z2.Z + (count - 1);
				z2 = x1y1z1.Z;
			}
			
			VertexP3fT2fC4b v; v.X = x1; v.Colour = col;
			v.X = x2; v.Y = y1; v.Z = z3; v.U = u2; v.V = v1; vertices[index++] = v; 
			v.X = x1; v.Y = y2; v.Z = z4; v.U = u1;           vertices[index++] = v;
			v.X = x3; v.Y = y3; v.Z = z2;           v.V = v2; vertices[index++] = v;
			v.X = x4; v.Y = y4; v.Z = z1; v.U = u2;           vertices[index++] = v;
		}

		/// <summary> Draws the right face of the given cuboid region. </summary>
		public void Right(int count, int col, int texLoc, VertexP3fT2fC4b[] vertices, ref int index) {
			float vOrigin = (texLoc % elementsPerAtlas1D) * invVerElementSize;
			float u1 = (count - minBB.Z), u2 = (1 - maxBB.Z) * 15.99f/16f;
			float v1 = vOrigin + maxBB.Y * invVerElementSize;
			float v2 = vOrigin + minBB.Y * invVerElementSize * 15.99f/16f;
			if (Tinted) col = TintBlock(col);
			float x1 = this.x2;
			float x2 = this.x2;
			float x3 = this.x2;
			float x4 = this.x2;
			float y1 = this.y2;
			float y2 = this.y2;
			float y3 = this.y1;
			float y4 = this.y1;
			float z2 = this.z1;
			float z1 = this.z2 + (count - 1);
			float z3 = this.z2 + (count - 1);
			float z4 = this.z1;
			
			if (Complex) {
				x1 = x2y2z2.X;
				x2 = x2y2z1.X;
				x3 = x2y1z2.X;
				x4 = x2y1z1.X;
				y1 = x2y2z2.Y;
				y2 = x2y2z1.Y;
				y3 = x2y1z2.Y;
				y4 = x2y1z1.Y;
				z1 = x2y2z2.Z + (count - 1);
				z2 = x2y2z1.Z;
				z3 = x2y1z2.Z + (count - 1);
				z4 = x2y1z1.Z;
			}
			
			VertexP3fT2fC4b v; v.Colour = col;
			v.X = x2; v.Y = y2; v.Z = z2;  v.U = u1; v.V = v1; vertices[index++] = v;
			v.X = x1; v.Y = y1; v.Z = z1;  v.U = u2;           vertices[index++] = v;
			v.X = x3; v.Y = y3; v.Z = z3;            v.V = v2; vertices[index++] = v;
			v.X = x4; v.Y = y4; v.Z = z4;  v.U = u1;           vertices[index++] = v;
		}

		/// <summary> Draws the front face of the given cuboid region. </summary>
		public void Front(int count, int col, int texLoc, VertexP3fT2fC4b[] vertices, ref int index) {
			float vOrigin = (texLoc % elementsPerAtlas1D) * invVerElementSize;
			float u1 = (count - minBB.X), u2 = (1 - maxBB.X) * 15.99f/16f;
			float v1 = vOrigin + maxBB.Y * invVerElementSize;
			float v2 = vOrigin + minBB.Y * invVerElementSize * 15.99f/16f;
			if (Tinted) col = TintBlock(col);
			float x1 = this.x1;
			float x2 = this.x2 + (count - 1);
			float x3 = this.x1;
			float x4 = this.x2 + (count - 1);
			float y1 = this.y2;
			float y2 = this.y2;
			float y3 = this.y1;
			float y4 = this.y1;
			float z1 = this.z1;
			float z2 = this.z1;
			float z3 = this.z1;
			float z4 = this.z1;
			if (Complex) {
				x3 = x1y2z1.X;
				x4 = x2y2z1.X + (count - 1);
				x1 = x1y1z1.X;
				x2 = x2y1z1.X + (count - 1);
				y1 = x1y2z1.Y;
				y2 = x2y2z1.Y;
				y3 = x1y1z1.Y;
				y4 = x2y1z1.Y;
				z3 = x1y2z1.Z;
				z4 = x2y2z1.Z;
				z1 = x1y1z1.Z;
				z2 = x2y1z1.Z;
			}
			
			VertexP3fT2fC4b v; v.Z = z1; v.Colour = col;
			v.X = x2; v.Y = y3; v.Z = z2; v.U = u2; v.V = v2; vertices[index++] = v;
			v.X = x1; v.Y = y4; v.Z = z1; v.U = u1;           vertices[index++] = v;
			v.X = x3; v.Y = y1; v.Z = z3;           v.V = v1; vertices[index++] = v;
			v.X = x4; v.Y = y2; v.Z = z4; v.U = u2;           vertices[index++] = v;
		}
		
		/// <summary> Draws the back face of the given cuboid region. </summary>
		public void Back(int count, int col, int texLoc, VertexP3fT2fC4b[] vertices, ref int index) {
			float vOrigin = (texLoc % elementsPerAtlas1D) * invVerElementSize;
			float u1 = minBB.X, u2 = (count - 1) + maxBB.X * 15.99f/16f;
			float v1 = vOrigin + maxBB.Y * invVerElementSize;
			float v2 = vOrigin + minBB.Y * invVerElementSize * 15.99f/16f;			
			if (Tinted) col = TintBlock(col);
			float x1 = this.x1;
			float x2 = this.x2 + (count - 1);
			float x3 = this.x1;
			float x4 = this.x2 + (count - 1);
			float y1 = this.y2;
			float y2 = this.y2;
			float y3 = this.y1;
			float y4 = this.y1;
			float z1 = this.z2;
			float z2 = this.z2;
			float z3 = this.z2;
			float z4 = this.z2;
			if (Complex) {
				x1 = x1y2z2.X;
				x2 = x2y2z2.X + (count - 1);
				x3 = x1y1z2.X;
				x4 = x2y1z2.X + (count - 1);
				y1 = x1y2z2.Y;
				y2 = x2y2z2.Y;
				y3 = x1y1z2.Y;
				y4 = x2y1z2.Y;
				z1 = x1y2z2.Z;
				z2 = x2y2z2.Z;
				z3 = x1y1z2.Z;
				z4 = x2y1z2.Z;
			}
			
			VertexP3fT2fC4b v; v.Colour = col;
			v.X = x2; v.Y = y2; v.Z = z2; v.U = u2; v.V = v1; vertices[index++] = v;
			v.X = x1; v.Y = y1; v.Z = z1; v.U = u1;           vertices[index++] = v;
			v.X = x3; v.Y = y3; v.Z = z3; v.V = v2; vertices[index++] = v;
			v.X = x4; v.Y = y4; v.Z = z4; v.U = u2;           vertices[index++] = v;
		}
		
		/// <summary> Draws the bottom face of the given cuboid region. </summary>
		public void Bottom(int count, int col, int texLoc, VertexP3fT2fC4b[] vertices, ref int index) {
			float vOrigin = (texLoc % elementsPerAtlas1D) * invVerElementSize;
			float u1 = minBB.X, u2 = (count - 1) + maxBB.X * 15.99f/16f;
			float v1 = vOrigin + minBB.Z * invVerElementSize;
			float v2 = vOrigin + maxBB.Z * invVerElementSize * 15.99f/16f;
			if (Tinted) col = TintBlock(col);
			float x1 = this.x1;
			float x2 = this.x2 + (count - 1);
			float x3 = this.x1;
			float x4 = this.x2 + (count - 1);
			float z1 = this.z1;
			float z2 = this.z1;
			float z3 = this.z2;
			float z4 = this.z2;
			float y1 = this.y1;
			float y2 = this.y1;
			float y3 = this.y1;
			float y4 = this.y1;
			if (Complex) {
				x1 = x1y1z1.X;
				x2 = x2y1z1.X + (count - 1);
				x3 = x1y1z2.X;
				x4 = x2y1z2.X + (count - 1);
				z1 = x1y1z1.Z;
				z2 = x2y1z1.Z;
				z3 = x1y1z2.Z;
				z4 = x2y1z2.Z;
				y1 = x1y1z1.Y;
				y2 = x2y1z1.Y;
				y3 = x1y1z2.Y;
				y4 = x2y1z2.Y;
			}
			
			VertexP3fT2fC4b v; v.Colour = col;
			v.X = x2; v.Y = y2; v.Z = z3; v.U = u2; v.V = v2; vertices[index++] = v;
			v.X = x1; v.Y = y1; v.Z = z4; v.U = u1;           vertices[index++] = v;
			v.X = x3; v.Y = y3; v.Z = z1;           v.V = v1; vertices[index++] = v;
			v.X = x4; v.Y = y4; v.Z = z2; v.U = u2;           vertices[index++] = v;
		}
		
		/* ORDER:
			12
			34 */

		/// <summary> Draws the top face of the given cuboid region. </summary>
		public void Top(int count, int col, int texLoc, int flip, VertexP3fT2fC4b[] vertices, ref int index) {
			float vOrigin = (texLoc % elementsPerAtlas1D) * invVerElementSize;
			float u1 = minBB.X, u2 = (count - 1) + maxBB.X * 15.99f/16f;
			float v1 = vOrigin + minBB.Z * invVerElementSize;
			float v2 = vOrigin + maxBB.Z * invVerElementSize * 15.99f/16f;
			if (Tinted) col = TintBlock(col);
			float x1 = this.x1;
			float x2 = this.x2 + (count - 1);
			float x3 = this.x1;
			float x4 = this.x2 + (count - 1);
			float z1 = this.z1;
			float z2 = this.z1;
			float z3 = this.z2;
			float z4 = this.z2;
			float y1 = this.y2;
			float y2 = this.y2;
			float y3 = this.y2;
			float y4 = this.y2;
			if (Complex) {
				x1 = x1y2z1.X;
				x2 = x2y2z1.X + (count - 1);
				x3 = x1y2z2.X;
				x4 = x2y2z2.X + (count - 1);
				z1 = x1y2z1.Z;
				z2 = x2y2z1.Z;
				z3 = x1y2z2.Z;
				z4 = x2y2z2.Z;
				y1 = x1y2z1.Y;
				y2 = x2y2z1.Y;
				y3 = x1y2z2.Y;
				y4 = x2y2z2.Y;
			}
			
			VertexP3fT2fC4b v; v.Colour = col;
			if (flip == 1) {
				v.X = x2; v.Y = y2; v.Z = z2; v.U = u1; v.V = v2; vertices[index++] = v;
				v.X = x1; v.Y = y1; v.Z = z1; v.U = u1; v.V = v1; vertices[index++] = v;
				v.X = x3; v.Y = y3; v.Z = z3; v.U = u2; v.V = v1; vertices[index++] = v;
				v.X = x4; v.Y = y4; v.Z = z4; v.U = u2; v.V = v2; vertices[index++] = v;
			} else {
				v.X = x2; v.Y = y2; v.Z = z2; v.U = u2; v.V = v1; vertices[index++] = v;
				v.X = x1; v.Y = y1; v.Z = z1; v.U = u1; v.V = v1; vertices[index++] = v;
				v.X = x3; v.Y = y3; v.Z = z3; v.U = u1; v.V = v2; vertices[index++] = v;
				v.X = x4; v.Y = y4; v.Z = z4; v.U = u2; v.V = v2; vertices[index++] = v;
			}
		}

		int TintBlock(int col) {
			FastColour rgbCol = FastColour.Unpack(col);
			rgbCol *= TintColour;
			return rgbCol.Pack();
		}
	}
}