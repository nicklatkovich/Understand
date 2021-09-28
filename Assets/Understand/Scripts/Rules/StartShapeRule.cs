using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StartShapeRule : IRule {
	public HashSet<ShapeComponent.Shape> shapes;

	public StartShapeRule(HashSet<ShapeComponent.Shape> shapes, string description) {
		this.shapes = shapes;
		this.description = description;
		this.fillingOrder = 10;
	}

	public override void FillGrid(ShapeComponent.Shape[][] grid, bool[][] filledCell, List<Vector2Int> path) {
		Vector2Int start = path.First();
		grid[start.x][start.y] = shapes.PickRandom();
		filledCell[start.x][start.y] = true;
	}

	public override bool Valid(ShapeComponent.Shape[][] grid, List<Vector2Int> path) {
		Vector2Int firstCell = path.First();
		return this.shapes.Contains(grid[firstCell.x][firstCell.y]);
	}
}
