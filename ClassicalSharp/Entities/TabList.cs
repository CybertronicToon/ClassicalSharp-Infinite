﻿using System;

namespace ClassicalSharp.Entities {

	public sealed class TabList : IGameComponent {
		public static TabListEntry[] Entries = new TabListEntry[256];
		
		public void Init(Game game) { }		
		public void Ready(Game game) { }
		public void OnNewMapLoaded(Game game) { }
		public void Dispose() { }
		public void OnNewMap(Game game) { }
		
		public void Reset(Game game) {
			for (int i = 0; i < Entries.Length; i++)
				Entries[i] = null;
		}
	}
	
	public sealed class TabListEntry {		
		/// <summary> Plain name of the player for autocompletion, etc. </summary>
		public string PlayerName;		
		/// <summary> Formatted name for display in the player list. </summary>
		public string ListName;
		public string ListNameColourless;
		/// <summary> Name of the group this player is in. </summary>
		public string Group;		
		/// <summary> Player's rank within the group. (0 is highest) </summary>
		/// <remarks> Multiple players can share the same rank, so this is not a unique identifier. </remarks>
		public byte GroupRank;
		
		public TabListEntry(string playerName, string listName,
		                    string groupName, byte groupRank) {
			PlayerName = Utils.StripColours(playerName);
			ListName = listName;
			ListNameColourless = Utils.StripColours(listName);
			Group = groupName;
			GroupRank = groupRank;
		}
	}
}
