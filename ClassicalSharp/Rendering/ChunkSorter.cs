﻿// Copyright 2014-2017 ClassicalSharp | Licensed under BSD-3
using System;
using ClassicalSharp.GraphicsAPI;
using OpenTK;

namespace ClassicalSharp.Renderers {
	
	public static class ChunkSorter {

		public static void UpdateSortOrder(Game game, ChunkUpdater updater) {
			Vector3 cameraPos = game.CurrentCameraPos;
			//int adjCamX, adjCamZ;
			//game.Camera.GetCamChunk(out adjCamX, out adjCamZ);
			//cameraPos.X -= adjCamX;
			//cameraPos.Z -= adjCamZ;
			Vector3I newChunkPos = Vector3I.Floor(cameraPos);
			newChunkPos.X = (newChunkPos.X & ~0x0F) + 8;
			newChunkPos.Y = (newChunkPos.Y & ~0x0F) + 8;
			newChunkPos.Z = (newChunkPos.Z & ~0x0F) + 8;
			if (newChunkPos == updater.chunkPos) return;
			
			ChunkInfo[] chunks = game.MapRenderer.chunks;
			int[] distances = updater.distances;
			Vector3I pPos = newChunkPos;
			updater.chunkPos = pPos;
			
			for (int i = 0; i < chunks.Length; i++) {
				ChunkInfo info = chunks[i];
				
				// Calculate distance to chunk centre
				int dx = info.CentreX - pPos.X, dy = info.CentreY - pPos.Y, dz = info.CentreZ - pPos.Z;
				distances[i] = dx * dx + dy * dy + dz * dz;
				
				// Can work out distance to chunk faces as offset from distance to chunk centre on each axis.
				int dXMin = dx - 8, dXMax = dx + 8;
				int dYMin = dy - 8, dYMax = dy + 8;
				int dZMin = dz - 8, dZMax = dz + 8;
				
				// Back face culling: make sure that the chunk is definitely entirely back facing.
				info.DrawLeft = !(dXMin <= 0 && dXMax <= 0);
				info.DrawRight = !(dXMin >= 0 && dXMax >= 0);
				info.DrawFront = !(dZMin <= 0 && dZMax <= 0);
				info.DrawBack = !(dZMin >= 0 && dZMax >= 0);
				info.DrawBottom = !(dYMin <= 0 && dYMax <= 0);
				info.DrawTop = !(dYMin >= 0 && dYMax >= 0);
			}

			// NOTE: Over 5x faster compared to normal comparison of IComparer<ChunkInfo>.Compare
			if (distances.Length > 1)
				QuickSort(distances, chunks, 0, chunks.Length - 1);
			updater.ResetUsedFlags();
			//SimpleOcclusionCulling();
		}
		
		static void QuickSort(int[] keys, ChunkInfo[] values, int left, int right) {
			while (left < right) {
				int i = left, j = right;
				int pivot = keys[(i + j) / 2];
				// partition the list
				while (i <= j) {
					while (pivot > keys[i]) i++;
					while (pivot < keys[j]) j--;
					
					if (i <= j) {
						int key = keys[i]; keys[i] = keys[j]; keys[j] = key;
						ChunkInfo value = values[i]; values[i] = values[j]; values[j] = value;
						i++; j--;
					}
				}
				
				// recurse into the smaller subset
				if (j - left <= right - i) {
					if (left < j)
						QuickSort(keys, values, left, j);
					left = i;
				} else {
					if (i < right)
						QuickSort(keys, values, i, right);
					right = j;
				}
			}
		}
	}
}