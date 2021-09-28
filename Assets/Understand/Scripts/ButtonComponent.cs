using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonComponent : MonoBehaviour {
	public ShapeComponent ShapePrefab;
	public KMSelectable Selectable;

	public bool active { get { return _active; } set { if (_active == value) return; _active = value; UpdateColors(); } } private bool _active;
	public bool hover { get { return _hover; } set { if (_hover == value) return; _hover = value; UpdateColors(); } } private bool _hover = false;
	public ShapeComponent.Shape shape { get { return _shape; } set { if (_shape == value) return; _shape = value; UpdateShape(); } } private ShapeComponent.Shape _shape;

	public ShapeComponent Shape { get; private set; }

	private void Start() {
		Shape = Instantiate(ShapePrefab);
		Shape.transform.parent = transform;
		Shape.transform.localPosition = Vector3.zero;
		Shape.transform.localScale = Vector3.one;
		Shape.transform.localRotation = Quaternion.identity;
		Shape.shape = ShapeComponent.Shape.TRIANGLE_RIGHT;
		Selectable.OnHighlight = () => hover = true;
		Selectable.OnHighlightEnded = () => hover = false;
		UpdateColors();
		UpdateShape();
	}

	private void UpdateShape() {
		if (Shape == null) return;
		Shape.shape = shape;
	}

	private void UpdateColors() {
		if (!active) {
			Shape.color = new Color32(0x00, 0x00, 0x00, 0xff);
			Shape.outlineColor = new Color32(0x44, 0x44, 0x44, 0xff);
			return;
		}
		Shape.color = hover ? new Color32(0x44, 0x44, 0x44, 0xff) : new Color32(0x00, 0x00, 0x00, 0xff);
		Shape.outlineColor = new Color32(0x88, 0x88, 0x88, 0xff);
	}
}
