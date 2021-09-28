using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public abstract class IRule {
	public enum CollectionState { ANY, ONE, ALL }
	public static readonly ReadOnlyCollection<CollectionState> ALL_COLLECTION_STATES = System.Array.AsReadOnly(new[] {
		CollectionState.ANY,
		CollectionState.ONE,
		CollectionState.ALL,
	});

	public int priority = 0;
	public string description;
	public abstract void FillGrid(ShapeComponent.Shape[][] grid, RuleGeneratorHelper gen, List<Vector2Int> path);
	public abstract bool Valid(ShapeComponent.Shape[][] grid, List<Vector2Int> path);
}
