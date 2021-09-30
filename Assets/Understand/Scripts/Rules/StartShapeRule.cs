using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StartShapeRule : IRule {
	public HashSet<ShapeComponent.Shape> shapes;

	public StartShapeRule(HashSet<ShapeComponent.Shape> shapes, string description) {
		this.shapes = shapes;
		this.description = description;
		this.priority = 10;
	}

	public override void FillGrid(ShapeComponent.Shape[][] grid, RuleGeneratorHelper gen, List<Vector2Int> path) {
		Vector2Int start = path.First();
		grid[start.x][start.y] = shapes.PickRandom();
		gen.filledCell[start.x][start.y] = true;
		foreach (ShapeComponent.Shape s in shapes) {
			gen.OnPathShapes.SetPriorityOf(s, ShapePriorityManager.Priority.LOW);
			gen.OffPathShapes.SetPriorityOf(s, ShapePriorityManager.Priority.LOW);
		}
	}

	public override bool Valid(ShapeComponent.Shape[][] grid, List<Vector2Int> path) {
		Vector2Int firstCell = path.First();
		return this.shapes.Contains(grid[firstCell.x][firstCell.y]);
	}
}
