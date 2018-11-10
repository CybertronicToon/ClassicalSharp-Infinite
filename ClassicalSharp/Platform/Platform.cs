﻿// Copyright 2014-2017 ClassicalSharp | Licensed under BSD-3
using System;
using System.Drawing;
using System.IO;
#if ANDROID
using Android.Graphics;
#else
using System.Drawing.Imaging;
#endif

namespace ClassicalSharp {

	/// <summary> Abstracts away platform specific operations. </summary>
	public static class Platform {
	
		public static bool ValidBitmap(Bitmap bmp) {
			// Mono seems to be returning a bitmap with a native pointer of zero in some weird cases.
			// We can detect this as property access raises an ArgumentException.
			try {
				int height = bmp.Height;
				PixelFormat format = bmp.PixelFormat;
				// make sure these are not optimised out
				return height != -1 && format != PixelFormat.Undefined;
			} catch (ArgumentException) {
				return false;
			}
		}
		
		public static Bitmap ReadBmp32Bpp(IDrawer2D drawer, byte[] data) {
			return ReadBmp32Bpp(drawer, new MemoryStream(data));
		}

		public static Bitmap ReadBmp32Bpp(IDrawer2D drawer, Stream src) {
			Bitmap bmp = ReadBmp(src);
			if (!ValidBitmap(bmp)) return null;
			if (!Is32Bpp(bmp)) drawer.ConvertTo32Bpp(ref bmp);
			return bmp;
		}

		public static Bitmap CreateBmp(int width, int height) {
			#if !ANDROID
			return new Bitmap(width, height);
			#else
			return Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);
			#endif
		}
	
		public static Bitmap ReadBmp(Stream src) {
			#if !ANDROID
			return new Bitmap(src);
			#else
			return BitmapFactory.DecodeStream(src);
			#endif
		}

		public static void WriteBmp(Bitmap bmp, Stream dst) {
			#if !ANDROID
			bmp.Save(dst, ImageFormat.Png);
			#else
			bmp.Compress(Bitmap.CompressFormat.Png, 100, dst);
			#endif
		}

		public static bool Is32Bpp(Bitmap bmp) {
			#if !ANDROID
			PixelFormat format = bmp.PixelFormat;
			return format == PixelFormat.Format32bppRgb || format == PixelFormat.Format32bppArgb;
			#else
			Bitmap.Config config = bmp.GetConfig();
			return config != null && config == Bitmap.Config.Argb8888;
			#endif
		}
	}
}
