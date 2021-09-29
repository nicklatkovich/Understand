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
		List<IRule> rules = GenerateRules();
		int minLength = 4;
		for (int levelIndex = 0; levelIndex < LEVELS_COUNT; levelIndex++) {
			RuleGeneratorHelper gen = new RuleGeneratorHelper(SIZE);
			maps[levelIndex] = new ShapeComponent.Shape[SIZE][];
			pathes[levelIndex] = GenerateRandomPath(minLength);
			for (int x = 0; x < SIZE; x++) {
				maps[levelIndex][x] = new ShapeComponent.Shape[SIZE];
				gen.filledCell[x] = new bool[SIZE];
			}
			rules.Sort((a, b) => b.priority - a.priority);
			foreach (IRule rule in rules) rule.FillGrid(maps[levelIndex], gen, pathes[levelIndex]);
			ShapeComponent.Shape filler = gen.OnPathShapes.Contains(ShapeComponent.Shape.NONE) && Random.Range(0, 9) != 0
				? ShapeComponent.Shape.NONE
				: gen.OnPathShapes.PickRandom();
			for (int x = 0; x < SIZE; x++) {
				for (int y = 0; y < SIZE; y++) {
					if (gen.filledCell[x][y]) continue;
					gen.filledCell[x][y] = true;
					if (Random.Range(0f, 1f) >= DIFFICULTIES[levelIndex]) maps[levelIndex][x][y] = filler;
					else maps[levelIndex][x][y] = ChooseFillingShape(pathes[levelIndex].Contains(new Vector2Int(x, y)) ? gen.OnPathShapes : gen.OffPathShapes);
				}
			}
		}
		this.rules = rules.ToArray();
		this.rules.Shuffle();
	}

	private List<IRule> GenerateRules() {
		List<IRule> result = new List<IRule>();
		StartShapeRule startShapeRule = GetRandomStartShapeRule();
		result.Add(startShapeRule);
		EndShapeRule endShapeRule = GetRandomEndShapeRule();
		result.Add(endShapeRule);
		List<VisitShapeRule> visitShapeRules = new List<VisitShapeRule>();
		visitShapeRules.Add(GenerateVisitingRule(startShapeRule, endShapeRule));
		int extraRuleRnd = Random.Range(0, 3);
		AvoidShapeRule extraAvoidShapeRule = null;
		if (extraRuleRnd == 0) visitShapeRules.Add(GenerateVisitingRule(startShapeRule, endShapeRule, visitShapeRules.First()));
		else if (extraRuleRnd == 1) extraAvoidShapeRule = GenerateAvoidingRule(startShapeRule, endShapeRule, visitShapeRules);
		result = result.Concat(visitShapeRules.Cast<IRule>()).ToList();
		result.Add(GenerateAvoidingRule(startShapeRule, endShapeRule, visitShapeRules, extraAvoidShapeRule));
		if (extraAvoidShapeRule != null) result.Add(extraAvoidShapeRule);
		return result;
	}

	public AvoidShapeRule GenerateAvoidingRule(
		StartShapeRule startShapeRule,
		EndShapeRule endShapeRule,
		List<VisitShapeRule> visitShapeRules,
		AvoidShapeRule avoidShapeRule = null
	) {
		HashSet<int> ignoreSets = new HashSet<int>();
		HashSet<ShapeComponent.Shape>[] visitingShapes = new[] { startShapeRule.shapes, endShapeRule.shapes }.Concat(visitShapeRules.Select(r => r.shapes)).ToArray();
		HashSet<ShapeComponent.Shape> allVisitingShapes = new HashSet<ShapeComponent.Shape>(visitingShapes.SelectMany(s => s));
		if (avoidShapeRule != null) allVisitingShapes = new HashSet<ShapeComponent.Shape>(allVisitingShapes.Concat(avoidShapeRule.shapes));
		if (allVisitingShapes.Any(s => ShapeComponent.IsTriangle(s))) ignoreSets.Add(0);
		if (allVisitingShapes.Any(s => ShapeComponent.HasFourVertices(s))) ignoreSets.Add(8);
		ShapeRule sr = GenerateRandomShapeRule(ignoreSets, allVisitingShapes);
		return new AvoidShapeRule(sr.shapes, "Avoid " + sr.description + "s");
	}

	// TODO: refactor this method
	public VisitShapeRule GenerateVisitingRule(StartShapeRule startShapeRule, EndShapeRule endShapeRule, VisitShapeRule anotherVisitingRule = null) {
		IRule.CollectionState state = IRule.ALL_COLLECTION_STATES.PickRandom();
		HashSet<int> ignoreSets = new HashSet<int>();
		HashSet<ShapeComponent.Shape> ignoreShapes = anotherVisitingRule != null
			? new HashSet<ShapeComponent.Shape>(anotherVisitingRule.shapes)
			: new HashSet<ShapeComponent.Shape>();
		string count;
		if (state == IRule.CollectionState.ANY) {
			count = "at least one";
			bool ignoreStartRuleShapes = false;
			bool ignoreEndRuleShapes = false;
			if (startShapeRule.shapes.Any(s => ShapeComponent.IsTriangle(s))) {
				ignoreStartRuleShapes = true;
				ignoreSets.Add(0);
			}
			if (endShapeRule.shapes.Any(s => ShapeComponent.IsTriangle(s))) {
				ignoreEndRuleShapes = true;
				ignoreSets.Add(0);
			}
			if (startShapeRule.shapes.Any(s => ShapeComponent.HasFourVertices(s))) {
				ignoreStartRuleShapes = true;
				ignoreSets.Add(8);
			}
			if (endShapeRule.shapes.Any(s => ShapeComponent.HasFourVertices(s))) {
				ignoreEndRuleShapes = true;
				ignoreSets.Add(8);
			}
			if (!ignoreStartRuleShapes) ignoreShapes = new HashSet<ShapeComponent.Shape>(ignoreShapes.Concat(startShapeRule.shapes));
			if (!ignoreEndRuleShapes) ignoreShapes = new HashSet<ShapeComponent.Shape>(ignoreShapes.Concat(endShapeRule.shapes));
		} else if (state == IRule.CollectionState.ONE) {
			count = "exactly one";
			if (startShapeRule.shapes.Any(s => ShapeComponent.IsTriangle(s)) && endShapeRule.shapes.Any(s => ShapeComponent.IsTriangle(s))) ignoreSets.Add(0);
			if (startShapeRule.shapes.Any(s => ShapeComponent.HasFourVertices(s)) && endShapeRule.shapes.Any(s => ShapeComponent.HasFourVertices(s))) ignoreSets.Add(8);
			foreach (ShapeComponent.Shape shape in ShapeComponent.ALL_SHAPES) {
				if (startShapeRule.shapes.Contains(shape) && endShapeRule.shapes.Contains(shape)) ignoreShapes.Add(shape);
			}
		} else if (state == IRule.CollectionState.ALL) count = "all";
		else throw new System.Exception("Unknown rule collection state");
		ShapeRule sr = GenerateRandomShapeRule(ignoreSets, ignoreShapes);
		string str = sr.shapes.Count == 1 && sr.shapes.Contains(ShapeComponent.Shape.NONE) ? "Visit {0} " + sr.description : "Collect {0} " + sr.description;
		if (state == IRule.CollectionState.ALL) str += "s";
		return new VisitShapeRule(sr.shapes, state, string.Format(str, count));
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

	private ShapeRule GenerateRandomShapeRule(HashSet<int> ignoreSets = null, HashSet<ShapeComponent.Shape> ignoreShapes = null) {
		if (ignoreSets == null) ignoreSets = new HashSet<int>();
		if (ignoreShapes == null) ignoreShapes = new HashSet<ShapeComponent.Shape>();
		if (ignoreShapes.Contains(ShapeComponent.Shape.SQUARE)) ignoreSets.Add(1);
		if (ignoreShapes.Contains(ShapeComponent.Shape.DIAMOND)) ignoreSets.Add(2);
		if (ignoreShapes.Contains(ShapeComponent.Shape.CIRCLE)) ignoreSets.Add(3);
		if (ignoreShapes.Contains(ShapeComponent.Shape.STAR)) ignoreSets.Add(4);
		if (ignoreShapes.Contains(ShapeComponent.Shape.HEART)) ignoreSets.Add(5);
		if (ignoreShapes.Contains(ShapeComponent.Shape.NONE)) ignoreSets.Add(6);
		if (ShapeComponent.FOUR_VERTICES_SHAPES.All(s => ignoreShapes.Contains(s))) ignoreSets.Add(8);
		if (ShapeComponent.TRIANGLE_SHAPES.All(s => ignoreShapes.Contains(s))) ignoreSets.Add(7);
		if (ignoreSets.Count == 9) throw new System.Exception("Can not generate random shape rule");
		int rnd = Enumerable.Range(0, 9).Where(i => !ignoreSets.Contains(i)).PickRandom();
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
					ShapeComponent.Shape resShape = ShapeComponent.TRIANGLE_SHAPES.Where(t => !ignoreShapes.Contains(t)).PickRandom();
					return new ShapeRule(new[] { resShape }, new Dictionary<ShapeComponent.Shape, string> {
						{ ShapeComponent.Shape.TRIANGLE_UP , "up-triangle" },
						{ ShapeComponent.Shape.TRIANGLE_RIGHT , "right-triangle" },
						{ ShapeComponent.Shape.TRIANGLE_DOWN , "down-triangle" },
						{ ShapeComponent.Shape.TRIANGLE_LEFT , "left-triangle" },
					}[resShape]);
				}
			default: return new ShapeRule(new[] { ShapeComponent.Shape.SQUARE, ShapeComponent.Shape.DIAMOND }, "4-vertices shape");
		}
	}

	public StartShapeRule GetRandomStartShapeRule() {
		ShapeRule rawRule = GenerateRandomShapeRule();
		return new StartShapeRule(rawRule.shapes, "Start on " + rawRule.description);
	}

	public EndShapeRule GetRandomEndShapeRule() {
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
