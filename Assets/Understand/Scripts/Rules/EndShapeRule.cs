using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EndShapeRule : IRule {
	public HashSet<ShapeComponent.Shape> shapes;

	public EndShapeRule(HashSet<ShapeComponent.Shape> shapes, string description) {
		this.shapes = shapes;
		this.description = description;
		this.priority = 10;
	}

	public override void FillGrid(ShapeComponent.Shape[][] grid, RuleGeneratorHelper gen, List<Vector2Int> path) {
		Vector2Int end = path.Last();
		grid[end.x][end.y] = shapes.PickRandom();
		gen.filledCell[end.x][end.y] = true;
	}

	public override bool Valid(ShapeComponent.Shape[][] grid, List<Vector2Int> path) {
		Vector2Int lastCell = path.Last();
		return this.shapes.Contains(grid[lastCell.x][lastCell.y]);
	}
}
