using System.Collections.Generic;

public class RuleGeneratorHelper {
	public ShapePriorityManager OnPathShapes = new ShapePriorityManager();
	public ShapePriorityManager OffPathShapes = new ShapePriorityManager();
	public bool[][] filledCell;
	public HashSet<ShapeComponent.Shape> NotFillers = new HashSet<ShapeComponent.Shape>();

	public RuleGeneratorHelper(int size) {
		filledCell = new bool[size][];
		for (int x = 0; x < size; x++) filledCell[x] = new bool[size];
	}
}
