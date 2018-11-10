﻿// Copyright 2014-2017 ClassicalSharp | Licensed under BSD-3
using System;
using OpenTK;

namespace ClassicalSharp {
	
	public static partial class Utils {

		/// <summary> Clamps that specified value such that min ≤ value ≤ max </summary>
		public static void Clamp(ref float value, float min, float max) {
			if (value < min) value = min;
			if (value > max) value = max;
		}
		
		/// <summary> Clamps that specified value such that min ≤ value ≤ max </summary>
		public static void Clamp(ref int value, int min, int max) {
			if (value < min) value = min;
			if (value > max) value = max;
		}
		
		/// <summary> Returns the next highest power of 2 that is ≥ to the given value. </summary>
		public static int NextPowerOf2(int value) {
			int next = 1;
			while (value > next)
				next <<= 1;
			return next;
		}
		
		/// <summary> Returns whether the given value is a power of 2. </summary>
		public static bool IsPowerOf2(int value) {
			return value != 0 && (value & (value - 1)) == 0;
		}
		
#if !LAUNCHER	
		/// <summary> Creates a vector with all components at 1E25. </summary>
		public static Vector3 MaxPos() { return new Vector3(1E25f, 1E25f, 1E25f); }
				
		public static Vector3 Mul(Vector3 a, Vector3 scale) {
			a.X *= scale.X; a.Y *= scale.Y; a.Z *= scale.Z;
			return a;
		}
		
		/// <summary> Multiply a value in degrees by this to get its value in radians. </summary>
		public const float Deg2Rad = (float)(Math.PI / 180);
		/// <summary> Multiply a value in radians by this to get its value in degrees. </summary>
		public const float Rad2Deg = (float)(180 / Math.PI);
		
		public static int DegreesToPacked(double degrees, int period) {
			return (int)(degrees * period / 360.0) % period;
		}
		
		public static byte DegreesToPacked(double degrees) {
			return (byte)(degrees * 256 / 360.0);
		}
		
		public static double PackedToDegrees(byte packed) {
			return packed * 360.0 / 256.0;
		}
		
		
		/// <summary> Rotates the given 3D coordinates around the x axis. </summary>
		public static Vector3 RotateX(Vector3 v, float angle) {
			float cosA = (float)Math.Cos(angle), sinA = (float)Math.Sin(angle);
			return new Vector3(v.X, cosA * v.Y + sinA * v.Z, -sinA * v.Y + cosA * v.Z);
		}
		
		/// <summary> Rotates the given 3D coordinates around the y axis. </summary>
		public static Vector3 RotateY(Vector3 v, float angle) {
			float cosA = (float)Math.Cos(angle), sinA = (float)Math.Sin(angle);
			return new Vector3(cosA * v.X - sinA * v.Z, v.Y, sinA * v.X + cosA * v.Z);
		}
		
		/// <summary> Rotates the given 3D coordinates around the y axis. </summary>
		public static Vector3 RotateY(float x, float y, float z, float angle) {
			float cosA = (float)Math.Cos(angle), sinA = (float)Math.Sin(angle);
			return new Vector3(cosA * x - sinA * z, y, sinA * x + cosA * z);
		}
		
		/// <summary> Rotates the given 3D coordinates around the z axis. </summary>
		public static Vector3 RotateZ(Vector3 v, float angle) {
			float cosA = (float)Math.Cos(angle), sinA = (float)Math.Sin(angle);
			return new Vector3(cosA * v.X + sinA * v.Y, -sinA * v.X + cosA * v.Y, v.Z);
		}

		/// <summary> Returns a normalised vector that faces in the direction
		/// described by the given yaw and pitch. </summary>
		public static Vector3 GetDirVector(double yawRad, double pitchRad) {
			double x = -Math.Cos(pitchRad) * -Math.Sin(yawRad);
			double y = -Math.Sin(pitchRad);
			double z = -Math.Cos(pitchRad) * Math.Cos(yawRad);
			return new Vector3((float)x, (float)y, (float)z);
		}
		
		public static void GetHeading(Vector3 dir, out double yaw, out double pitch) {
			pitch = Math.Asin(-dir.Y);
			yaw = Math.Atan2(dir.X, -dir.Z);
		}
#endif
		
		public static int Floor(float value) {
			int valueI = (int)value;
			return value < valueI ? valueI - 1 : valueI;
		}
		
		public static int Ceil(float value) {
			int valueI = (int)value;
			if (value == (float)valueI) {
				return valueI;
			}
			return valueI + 1;
		}
		
		/// <summary> Performs rounding upwards integer division. </summary>
		public static int CeilDiv(int a, int b) {
			return a / b + (a % b != 0 ? 1 : 0);
		}		

		/// <summary> Performs linear interpolation between two values. </summary>
		public static float Lerp(float a, float b, float t) {
			return a + (b - a) * t;
		}
		
		public static int Log2(int value) {
			int shift = 0;
			while (value > 1) { shift++; value >>= 1; }
			return shift;
		}

#if !LAUNCHER	
		/// <summary> Linearly interpolates between a given angle range, adjusting if necessary. </summary>
		public static float LerpAngle(float leftAngle, float rightAngle, float t) {
			// we have to cheat a bit for angles here.
			// Consider 350* --> 0*, we only want to travel 10*,
			// but without adjusting for this case, we would interpolate back the whole 350* degrees.
			bool invertLeft = leftAngle > 270 && rightAngle < 90;
			bool invertRight = rightAngle > 270 && leftAngle < 90;
			if (invertLeft) leftAngle = leftAngle - 360;
			if (invertRight) rightAngle = rightAngle - 360;
			
			return Lerp(leftAngle, rightAngle, t);
		}
#endif
	}
}