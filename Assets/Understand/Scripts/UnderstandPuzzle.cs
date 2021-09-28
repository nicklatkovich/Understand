using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnderstandPuzzle {
	public const int SIZE = 7;
	public const int LEVELS_COUNT = 7;

	public IRule[] rules;
	public ShapeComponent.Shape[][][] maps = new ShapeComponent.Shape[LEVELS_COUNT][][];
	public List<Vector2Int>[] pathes = new List<Vector2Int>[LEVELS_COUNT];

	public UnderstandPuzzle() {
		List<IRule> rules = new List<IRule>();
		rules.Add(GetRandomStartShapeRule());
		int minLength = 2;
		for (int levelIndex = 0; levelIndex < LEVELS_COUNT; levelIndex++) {
			maps[levelIndex] = new ShapeComponent.Shape[SIZE][];
			pathes[levelIndex] = GenerateRandomPath(minLength);
			bool[][] filledCell = new bool[SIZE][];
			for (int x = 0; x < SIZE; x++) {
				maps[levelIndex][x] = new ShapeComponent.Shape[SIZE];
				filledCell[x] = new bool[SIZE];
			}
			rules.Sort((a, b) => a.fillingOrder - b.fillingOrder);
			foreach (IRule rule in rules) rule.FillGrid(maps[levelIndex], filledCell, pathes[levelIndex]);
		}
		this.rules = rules.ToArray();
		this.rules.Shuffle();
	}

	public bool RuleValid(int levelIndex, int ruleIndex, List<Vector2Int> path) {
		return rules[ruleIndex].Valid(maps[levelIndex], path);
	}

	public StartAtRule GetRandomStartShapeRule() {
		int rnd = Random.Range(0, 9);
		switch (rnd) {
			case 0:
				return new StartAtRule(new HashSet<ShapeComponent.Shape>(new[] {
					ShapeComponent.Shape.TRIANGLE_UP,
					ShapeComponent.Shape.TRIANGLE_RIGHT,
					ShapeComponent.Shape.TRIANGLE_DOWN,
					ShapeComponent.Shape.TRIANGLE_LEFT,
				}), "Start at triangle");
			case 1: return new StartAtRule(new HashSet<ShapeComponent.Shape>(new[] { ShapeComponent.Shape.SQUARE }), "Start at square");
			case 2: return new StartAtRule(new HashSet<ShapeComponent.Shape>(new[] { ShapeComponent.Shape.DIAMOND }), "Start at diamond");
			case 3: return new StartAtRule(new HashSet<ShapeComponent.Shape>(new[] { ShapeComponent.Shape.CIRCLE }), "Start at circle");
			case 4: return new StartAtRule(new HashSet<ShapeComponent.Shape>(new[] { ShapeComponent.Shape.STAR }), "Start at star");
			case 5: return new StartAtRule(new HashSet<ShapeComponent.Shape>(new[] { ShapeComponent.Shape.HEART }), "Start at heart");
			case 6: return new StartAtRule(new HashSet<ShapeComponent.Shape>(new[] { ShapeComponent.Shape.NONE }), "Start at empty cell");
			case 7: {
					int rnd1 = Random.Range(0, 4);
					switch (rnd1) {
						case 0: return new StartAtRule(new HashSet<ShapeComponent.Shape>(new[] { ShapeComponent.Shape.TRIANGLE_UP }), "Start at up-triangle");
						case 1: return new StartAtRule(new HashSet<ShapeComponent.Shape>(new[] { ShapeComponent.Shape.TRIANGLE_RIGHT }), "Start at right-triangle");
						case 2: return new StartAtRule(new HashSet<ShapeComponent.Shape>(new[] { ShapeComponent.Shape.TRIANGLE_DOWN }), "Start at down-triangle");
						default: return new StartAtRule(new HashSet<ShapeComponent.Shape>(new[] { ShapeComponent.Shape.TRIANGLE_LEFT }), "Start at left-triangle");
					}
				}
			default:
				return new StartAtRule(new HashSet<ShapeComponent.Shape>(new[] {
					ShapeComponent.Shape.SQUARE,
					ShapeComponent.Shape.DIAMOND,
				}), "Start at shape with 4 vertices");
		}
	}

	public List<Vector2Int> GenerateRandomPath(int minLength) {
		Maze maze = new Maze(SIZE);
		List<Vector2Int> result = maze.GenerateRandomPath(minLength);
		int length = Random.Range(minLength, result.Count + 1);
		int skip = Random.Range(0, result.Count - length + 1);
		return result.Skip(skip).Take(length).ToList();
	}

	private static bool IsValidCoord(Vector2Int a) {
		return a.x >= 0 && a.x < SIZE && a.y >= 0 && a.y < SIZE;
	}
}
