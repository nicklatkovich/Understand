using System.Collections.Generic;
using UnityEngine;

public class ShapeComponent : MonoBehaviour {
	public enum Shape { NONE, TRIANGLE_UP, TRIANGLE_RIGHT, TRIANGLE_DOWN, TRIANGLE_LEFT, SQUARE, DIAMOND, CIRCLE, STAR, HEART }

	private static Dictionary<Shape, float> Rotations = new Dictionary<Shape, float> {
		{ Shape.TRIANGLE_RIGHT, 90f },
		{ Shape.TRIANGLE_DOWN, 180f },
		{ Shape.TRIANGLE_LEFT, 270f },
	};

	public Renderer SelfRenderer;
	public Texture TriangleTexture;
	public Texture SquareTexture;
	public Texture DiamondTexture;
	public Texture CircleTexture;
	public Texture StarTexture;
	public Texture HeartTexture;

	public Shape shape { get { return _shape; } set { if (_shape == value) return; _shape = value; UpdateShape(); } } private Shape _shape;
	public Color color { get { return _color; } set { if (_color == value) return; _color = value; SelfRenderer.material.color = value; } } private Color _color;

	private Color _outlineColor;
	public Color outlineColor {
		get { return _outlineColor; }
		set { if (_outlineColor == value) return; _outlineColor = value; SelfRenderer.material.SetColor("_OutlineColor", value); }
	}

	private void Start() {
		UpdateShape();
	}

	public Texture GetTexture(Shape shape) {
		switch (shape) {
			case Shape.NONE: return null;
			case Shape.TRIANGLE_UP:
			case Shape.TRIANGLE_RIGHT:
			case Shape.TRIANGLE_DOWN:
			case Shape.TRIANGLE_LEFT: return TriangleTexture;
			case Shape.SQUARE: return SquareTexture;
			case Shape.DIAMOND: return DiamondTexture;
			case Shape.CIRCLE: return CircleTexture;
			case Shape.STAR: return StarTexture;
			case Shape.HEART: return HeartTexture;
			default: throw new System.Exception("Unknown shape");
		}
	}

	private void UpdateShape() {
		Texture texture = GetTexture(_shape);
		if (texture == null) gameObject.SetActive(false);
		else {
			gameObject.SetActive(true);
			SelfRenderer.material.SetTexture("_MainTex", texture);
			transform.localRotation = Quaternion.Euler(0f, Rotations.ContainsKey(_shape) ? Rotations[_shape] : 0f, 0f);
		}
	}
}
