using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShapePriorityManager {
	public static ShapeComponent.Shape PickRandomFromSetWithoutPriority(HashSet<ShapeComponent.Shape> shapes) {
		ShapeComponent.Shape[] notTriangles = shapes.Where(s => !ShapeComponent.IsTriangle(s)).ToArray();
		if (notTriangles.Length == shapes.Count) return shapes.PickRandom();
		if (Random.Range(0, notTriangles.Length + 1) == 0) return shapes.Where(s => ShapeComponent.IsTriangle(s)).PickRandom();
		return notTriangles.PickRandom();
	}

	public enum Priority {
		MEDIUM = 0,
		HIGH = 1,
		LOW = -1,
		NEVER = -2,
	};

	private Dictionary<ShapeComponent.Shape, Priority> Priorities = new Dictionary<ShapeComponent.Shape, Priority>();

	public Priority GetPriorityOf(ShapeComponent.Shape shape) {
		return Priorities.ContainsKey(shape) ? Priorities[shape] : Priority.MEDIUM;
	}

	public void SetPriorityOf(ShapeComponent.Shape shape, Priority priority) {
		Priorities[shape] = priority;
	}

	public ShapeComponent.Shape PickRandomShape() {
		foreach (Priority priority in new[] { Priority.HIGH, Priority.MEDIUM, Priority.LOW }.Skip(Random.Range(0, 3))) {
			HashSet<ShapeComponent.Shape> possibleShapes = new HashSet<ShapeComponent.Shape>(ShapeComponent.ALL_SHAPES.Where(s => GetPriorityOf(s) >= priority));
			if (possibleShapes.Count == 0) continue;
			return PickRandomFromSetWithoutPriority(possibleShapes);
		}
		throw new System.Exception("Unable to pick random shape using priority");
	}

	public HashSet<ShapeComponent.Shape> GetNotForbidden() {
		return new HashSet<ShapeComponent.Shape>(ShapeComponent.ALL_SHAPES.Where(s => GetPriorityOf(s) != Priority.NEVER));
	}

	public HashSet<ShapeComponent.Shape> GetNotForbiddenExcluding(HashSet<ShapeComponent.Shape> excluded) {
		HashSet<ShapeComponent.Shape> result = GetNotForbidden();
		foreach (ShapeComponent.Shape s in excluded) result.Remove(s);
		return result;
	}
}
