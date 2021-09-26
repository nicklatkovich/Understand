using UnityEngine;

public class CellComponent : MonoBehaviour {
	public Renderer Tile;
	public KMSelectable Selectable;
	public ShapeComponent Shape;
	public Vector2Int Coord;

	public bool active { get { return _active; } set { if (_active == value) return; _active = value; UpdateActivity(); } } private bool _active = false;
	public Color color { get { return _color; } set { if (_color == value) return; _color = value; Tile.material.color = value; } } private Color _color;

	private void Start() {
		UpdateActivity();
	}

	private void UpdateActivity() {
		Tile.gameObject.SetActive(_active);
	}
}
