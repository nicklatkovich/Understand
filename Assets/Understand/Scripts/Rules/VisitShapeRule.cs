using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VisitShapeRule : IRule {
	public HashSet<ShapeComponent.Shape> shapes;
	public CollectionState state;

	public VisitShapeRule(HashSet<ShapeComponent.Shape> shapes, CollectionState state, string description) {
		this.shapes = shapes;
		this.state = state;
		this.description = description;
	}

	public override void FillGrid(ShapeComponent.Shape[][] grid, RuleGeneratorHelper gen, List<Vector2Int> path) {
		if (path.All(cell => !gen.filledCell[cell.x][cell.y] || !shapes.Contains(grid[cell.x][cell.y]))) {
			Vector2Int cell = path.Where(c => !gen.filledCell[c.x][c.y]).PickRandom();
			grid[cell.x][cell.y] = shapes.PickRandom();
			gen.filledCell[cell.x][cell.y] = true;
		}
		switch (state) {
			case CollectionState.ANY:
				foreach (ShapeComponent.Shape s in shapes) gen.OffPathShapes.SetPriorityOf(s, ShapePriorityManager.Priority.LOW);
				break;
			case CollectionState.ONE:
				foreach (ShapeComponent.Shape s in shapes) {
					gen.OffPathShapes.SetPriorityOf(s, ShapePriorityManager.Priority.HIGH);
					gen.OnPathShapes.SetPriorityOf(s, ShapePriorityManager.Priority.NEVER);
				}
				break;
			case CollectionState.ALL:
				foreach (ShapeComponent.Shape s in shapes) {
					gen.OffPathShapes.SetPriorityOf(s, ShapePriorityManager.Priority.NEVER);
					gen.OnPathShapes.SetPriorityOf(s, ShapePriorityManager.Priority.HIGH);
					gen.NotFillers.Add(s);
				}
				break;
		}
	}

	public override bool Valid(ShapeComponent.Shape[][] grid, List<Vector2Int> path) {
		switch (state) {
			case CollectionState.ANY: return path.Any(c => shapes.Contains(grid[c.x][c.y]));
			case CollectionState.ONE: return path.Count(c => shapes.Contains(grid[c.x][c.y])) == 1;
			case CollectionState.ALL: return path.Count(c => shapes.Contains(grid[c.x][c.y])) == grid.SelectMany(r => r).Count(c => shapes.Contains(c));
			default: throw new System.Exception("Unknown rule collection state");
		}
	}
}
