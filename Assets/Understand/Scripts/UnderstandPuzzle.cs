using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnderstandPuzzle {
	public const int SIZE = 7;
	public const int LEVELS_COUNT = 7;
	public readonly float[] DIFFICULTIES = new float[SIZE] { .1f, .2f, .4f, .6f, .8f, 1f, 1f };

	public IRule[] rules;
	public ShapeComponent.Shape[][][] maps = new ShapeComponent.Shape[LEVELS_COUNT][][];
	public List<Vector2Int>[] pathes = new List<Vector2Int>[LEVELS_COUNT];

	public UnderstandPuzzle() {
		List<IRule> rules = new List<IRule>();
		rules.Add(GetRandomStartShapeRule());
		rules.Add(GetRandomEndShapeRule());
		int minLength = 2;
		for (int levelIndex = 0; levelIndex < LEVELS_COUNT; levelIndex++) {
			HashSet<ShapeComponent.Shape> offPathShapes = new HashSet<ShapeComponent.Shape>(ShapeComponent.ALL_SHAPES);
			HashSet<ShapeComponent.Shape> onPathShapes = new HashSet<ShapeComponent.Shape>(ShapeComponent.ALL_SHAPES);
			maps[levelIndex] = new ShapeComponent.Shape[SIZE][];
			pathes[levelIndex] = GenerateRandomPath(minLength);
			bool[][] filledCell = new bool[SIZE][];
			for (int x = 0; x < SIZE; x++) {
				maps[levelIndex][x] = new ShapeComponent.Shape[SIZE];
				filledCell[x] = new bool[SIZE];
			}
			rules.Sort((a, b) => a.fillingOrder - b.fillingOrder);
			foreach (IRule rule in rules) rule.FillGrid(maps[levelIndex], filledCell, pathes[levelIndex]);
			for (int x = 0; x < SIZE; x++) {
				for (int y = 0; y < SIZE; y++) {
					if (filledCell[x][y]) continue;
					filledCell[x][y] = true;
					if (Random.Range(0f, 1f) >= DIFFICULTIES[levelIndex]) continue;
					maps[levelIndex][x][y] = ChooseFillingShape(pathes[levelIndex].Contains(new Vector2Int(x, y)) ? onPathShapes : offPathShapes);
				}
			}
		}
		this.rules = rules.ToArray();
		this.rules.Shuffle();
	}

	public bool RuleValid(int levelIndex, int ruleIndex, List<Vector2Int> path) {
		return rules[ruleIndex].Valid(maps[levelIndex], path);
	}

	private struct ShapeRule {
		public readonly HashSet<ShapeComponent.Shape> shapes;
		public readonly string description;
		public ShapeRule(IEnumerable<ShapeComponent.Shape> shapes, string description) {
			this.shapes = new HashSet<ShapeComponent.Shape>(shapes);
			this.description = description;
		}
	}

	private ShapeRule GenerateRandomShapeRule() {
		int rnd = Random.Range(0, 9);
		switch (rnd) {
			case 0:
				return new ShapeRule(new[] {
					ShapeComponent.Shape.TRIANGLE_UP,
					ShapeComponent.Shape.TRIANGLE_RIGHT,
					ShapeComponent.Shape.TRIANGLE_DOWN,
					ShapeComponent.Shape.TRIANGLE_LEFT,
				}, "triangle");
			case 1: return new ShapeRule(new[] { ShapeComponent.Shape.SQUARE }, "square");
			case 2: return new ShapeRule(new[] { ShapeComponent.Shape.DIAMOND }, "diamond");
			case 3: return new ShapeRule(new[] { ShapeComponent.Shape.CIRCLE }, "circle");
			case 4: return new ShapeRule(new[] { ShapeComponent.Shape.STAR }, "star");
			case 5: return new ShapeRule(new[] { ShapeComponent.Shape.HEART }, "heart");
			case 6: return new ShapeRule(new[] { ShapeComponent.Shape.NONE }, "empty cell");
			case 7: {
					int rnd1 = Random.Range(0, 4);
					switch (rnd1) {
						case 0: return new ShapeRule(new[] { ShapeComponent.Shape.TRIANGLE_UP }, "up-triangle");
						case 1: return new ShapeRule(new[] { ShapeComponent.Shape.TRIANGLE_RIGHT }, "right-triangle");
						case 2: return new ShapeRule(new[] { ShapeComponent.Shape.TRIANGLE_DOWN }, "down-triangle");
						default: return new ShapeRule(new[] { ShapeComponent.Shape.TRIANGLE_LEFT }, "left-triangle");
					}
				}
			default: return new ShapeRule(new[] { ShapeComponent.Shape.SQUARE, ShapeComponent.Shape.DIAMOND }, "shape with 4 vertices");
		}
	}

	public IRule GetRandomStartShapeRule() {
		ShapeRule rawRule = GenerateRandomShapeRule();
		return new StartShapeRule(rawRule.shapes, "Start on " + rawRule.description);
	}

	public IRule GetRandomEndShapeRule() {
		ShapeRule rawRule = GenerateRandomShapeRule();
		return new EndShapeRule(rawRule.shapes, "End on " + rawRule.description);
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

	private ShapeComponent.Shape ChooseFillingShape(HashSet<ShapeComponent.Shape> variants) {
		ShapeComponent.Shape[] notTriangles = variants.Where(s => !ShapeComponent.IsTriangle(s)).ToArray();
		if (notTriangles.Length == variants.Count) return variants.PickRandom();
		if (Random.Range(0, notTriangles.Count() + 1) == 0) return variants.Where(s => ShapeComponent.IsTriangle(s)).PickRandom();
		return notTriangles.PickRandom();
	}
}
