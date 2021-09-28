using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuleGeneratorHelper {
	public HashSet<ShapeComponent.Shape> OffPathShapes = new HashSet<ShapeComponent.Shape>(ShapeComponent.ALL_SHAPES);
	public HashSet<ShapeComponent.Shape> OnPathShapes = new HashSet<ShapeComponent.Shape>(ShapeComponent.ALL_SHAPES);
	public bool[][] filledCell;

	public RuleGeneratorHelper(int size) {
		filledCell = new bool[size][];
		for (int x = 0; x < size; x++) filledCell[x] = new bool[size];
	}
}
