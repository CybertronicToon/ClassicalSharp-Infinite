// Copyright 2014-2017 ClassicalSharp | Licensed under BSD-3
using System;
using System.Collections.Generic;
using OpenTK;
using BlockID = System.UInt16;

namespace ClassicalSharp {
	public class Recipes {
		public Recipes() {
			
		}
		
		public static Recipe[] MakeRecipeList() {
			Recipe[] recipes = new Recipe[0];
			BlockID[] ingredients;
			BlockID[,] pattern;
			
			int num = 0;
			
			/*Array.Resize(ref recipes, recipes.Length + 1);
			pattern = new BlockID[2,2] {
				{Block.Wood, Block.Wood},
				{Block.Wood, Block.Wood},
			};
			recipes[num++] = new Recipe(pattern, Block.Log, 1);*/
			
			Array.Resize(ref recipes, recipes.Length + 1);
			ingredients = new BlockID[1] {Block.Sand};
			recipes[num++] = new Recipe(ingredients, Block.Glass, 1);
			
			Array.Resize(ref recipes, recipes.Length + 1);
			pattern = new BlockID[2,2] {
				{Block.Cobblestone, Block.Cobblestone},
				{Block.Cobblestone, Block.Cobblestone},
			};
			recipes[num++] = new Recipe(pattern, Block.MossyRocks, 4);
			
			Array.Resize(ref recipes, recipes.Length + 1);
			ingredients = new BlockID[1] {Block.Log};
			recipes[num++] = new Recipe(ingredients, Block.Wood, 4);
			
			Array.Resize(ref recipes, recipes.Length + 1);
			ingredients = new BlockID[1] {Block.Cobblestone};
			recipes[num++] = new Recipe(ingredients, Block.Stone, 1);
			
			Array.Resize(ref recipes, recipes.Length + 1);
			pattern = new BlockID[1,3] {
				{Block.Cobblestone, Block.Cobblestone, Block.Cobblestone},
			};
			recipes[num++] = new Recipe(pattern, Block.Slab, 3);
			
			#if ALPHA
			Array.Resize(ref recipes, recipes.Length + 1);
			pattern = new BlockID[3,3] {
				{Block.Cobblestone, Block.Cobblestone, Block.Cobblestone},
				{Block.Cobblestone, Block.Air, Block.Cobblestone},
				{Block.Cobblestone, Block.Cobblestone, Block.Cobblestone},
			};
			recipes[num++] = new Recipe(pattern, Block.Furnace, 1);
			
			Array.Resize(ref recipes, recipes.Length + 1);
			pattern = new BlockID[1,2] {
				{Block.Cobblestone, Block.Cobblestone},
			};
			recipes[num++] = new Recipe(pattern, Block.StonePressurePlate, 3);
			
			Array.Resize(ref recipes, recipes.Length + 1);
			pattern = new BlockID[2,3] {
				{Block.Wood, Block.Wood, Block.Wood},
				{Block.Wood, Block.Wood, Block.Wood},
			};
			recipes[num++] = new Recipe(pattern, Block.Fence, 6);
			
			Array.Resize(ref recipes, recipes.Length + 1);
			pattern = new BlockID[2,2] {
				{Block.Wood, Block.Wood},
				{Block.Wood, Block.Wood},
			};
			recipes[num++] = new Recipe(pattern, Block.CraftingTable, 1);
			
			Array.Resize(ref recipes, recipes.Length + 1);
			pattern = new BlockID[3,3] {
				{Block.Wood, Block.Wood, Block.Wood},
				{Block.Wood, Block.Air, Block.Wood},
				{Block.Wood, Block.Wood, Block.Wood},
			};
			recipes[num++] = new Recipe(pattern, Block.Chest, 1);
			#endif
			
			return recipes;
		}
	}
	
	public sealed class Recipe {
		public Recipe(BlockID[] Ingredients, BlockID output, sbyte count) {
			this.Shapeless = true;
			this.Ingredients = Ingredients;
			this.Output = output;
			this.Count = count;
		}
		public Recipe(BlockID[,] Pattern, BlockID output, sbyte count) {
			this.Shapeless = false;
			this.Pattern = Pattern;
			this.Output = output;
			this.Count = count;
		}
		public bool Shapeless;
		public BlockID[] Ingredients;
		public BlockID[,] Pattern;
		public BlockID Output;
		public sbyte Count;
	}
}
