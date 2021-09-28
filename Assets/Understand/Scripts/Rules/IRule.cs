using System.Collections.Generic;
using UnityEngine;

public abstract class IRule {
	public int fillingOrder;
	public string description;
	public abstract void FillGrid(ShapeComponent.Shape[][] grid, bool[][] filledCell, List<Vector2Int> path);
	public abstract bool Valid(ShapeComponent.Shape[][] grid, List<Vector2Int> path);
}
