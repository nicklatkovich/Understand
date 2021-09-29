using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AvoidShapeRule : IRule {
	public HashSet<ShapeComponent.Shape> shapes;

	public AvoidShapeRule(HashSet<ShapeComponent.Shape> shapes, string description) {
		this.shapes = shapes;
		this.description = description;
	}

	public override void FillGrid(ShapeComponent.Shape[][] grid, RuleGeneratorHelper gen, List<Vector2Int> path) {
		gen.OnPathShapes.RemoveWhere(s => shapes.Contains(s));
	}

	public override bool Valid(ShapeComponent.Shape[][] grid, List<Vector2Int> path) {
		return path.All(cell => !shapes.Contains(grid[cell.x][cell.y]));
	}
}
