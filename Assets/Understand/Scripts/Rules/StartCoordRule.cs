using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StartCoordRule : ICoordRule {
	public StartCoordRule(int size) {
		SetRandom(size);
		description = "Start in " + description;
	}

	public override void FillGrid(ShapeComponent.Shape[][] grid, RuleGeneratorHelper gen, List<Vector2Int> path) {
	}

	public override bool Valid(ShapeComponent.Shape[][] grid, List<Vector2Int> path) {
		return path.First() == Coord;
	}
}
