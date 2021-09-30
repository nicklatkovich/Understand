using System.Collections.Generic;
using UnityEngine;

public class RuleGeneratorHelper {
	public ShapePriorityManager OnPathShapes = new ShapePriorityManager();
	public ShapePriorityManager OffPathShapes = new ShapePriorityManager();
	public bool[][] filledCell;
	public HashSet<ShapeComponent.Shape> NotFillers = new HashSet<ShapeComponent.Shape>();

	public RuleGeneratorHelper(int size) {
		filledCell = new bool[size][];
		for (int x = 0; x < size; x++) filledCell[x] = new bool[size];
	}

	public static string CoordToString(Vector2Int coord) {
		return (char)(coord.x + 'A') + (coord.y + 1).ToString();
	}
}
