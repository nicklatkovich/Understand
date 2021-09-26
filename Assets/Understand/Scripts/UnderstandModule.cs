using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KeepCoding;
using Enum = System.Enum;

public class UnderstandModule : ModuleScript {
	public const float CELL_SIZE = .018f;
	public const float OUTLINE_WIDTH = .0012f;
	public static readonly Color32 INACTIVE_BUTTON_COLOR = new Color32(0x00, 0x00, 0x00, 0xff);
	public static readonly Color32 INACTIVE_BUTTON_OUTLINE_COLOR = new Color32(0x44, 0x44, 0x44, 0xff);
	public static readonly Color32 SELECTED_CELL_COLOR = new Color32(0x88, 0x88, 0x88, 0xff);
	public static readonly Color32 PATH_CELL_COLOR = new Color32(0x55, 0x55, 0x55, 0xff);
	public static readonly Color32 INACTIVE_SHAPE_COLOR = new Color32(0x33, 0x33, 0x33, 0xff);
	public static readonly Color32 INACTIVE_SHAPE_OUTLINE_COLOR = new Color32(0xaa, 0xaa, 0xaa, 0xff);

	public GameObject GridOutlinePrefab;
	public Transform GridContainer;
	public KMSelectable Selectable;
	public ShapeComponent ShapePrefab;
	public CellComponent CellPrefab;
	public GameObject PathPrefab;

	private ShapeComponent BackButton;
	private ShapeComponent NextButton;
	private CellComponent SelectedCell = null;
	private CellComponent[][] Cells = new CellComponent[UnderstandPuzzle.SIZE][];
	private List<GameObject> ActivePath = new List<GameObject>();
	private List<GameObject> InactivePath = new List<GameObject>();
	private List<Vector2Int> Path = new List<Vector2Int>();
	private bool DrawingPath = false;

	private void Start() {
		float median = (CELL_SIZE * UnderstandPuzzle.SIZE) / 2f;
		for (int i = 0; i <= UnderstandPuzzle.SIZE; i++) {
			GameObject xOutline = Instantiate(GridOutlinePrefab);
			xOutline.transform.parent = GridContainer;
			xOutline.transform.localPosition = new Vector3(median, .001f, i * CELL_SIZE);
			xOutline.transform.localScale = new Vector3(CELL_SIZE * UnderstandPuzzle.SIZE, 0, 0) + Vector3.one * OUTLINE_WIDTH;
			xOutline.transform.localRotation = Quaternion.identity;
			GameObject yOutline = Instantiate(GridOutlinePrefab);
			yOutline.transform.parent = GridContainer;
			yOutline.transform.localPosition = new Vector3(i * CELL_SIZE, .001f, median);
			yOutline.transform.localScale = new Vector3(0, 0, CELL_SIZE * UnderstandPuzzle.SIZE) + Vector3.one * OUTLINE_WIDTH;
			yOutline.transform.localRotation = Quaternion.identity;
		}
		BackButton = CreateButton(ShapeComponent.Shape.TRIANGLE_LEFT, new Vector3(-0.06f, 0.0151f, 0.065f));
		NextButton = CreateButton(ShapeComponent.Shape.TRIANGLE_RIGHT, new Vector3(0.02f, 0.0151f, 0.065f));
		for (int x = 0; x < UnderstandPuzzle.SIZE; x++) {
			Cells[x] = new CellComponent[UnderstandPuzzle.SIZE];
			for (int y = 0; y < UnderstandPuzzle.SIZE; y++) {
				CellComponent cell = Instantiate(CellPrefab);
				Cells[x][y] = cell;
				cell.transform.parent = GridContainer;
				cell.transform.localPosition = new Vector3((x + .5f) * CELL_SIZE, .001f, (y + .5f) * CELL_SIZE);
				cell.transform.localScale = Vector3.one;
				cell.transform.localRotation = Quaternion.identity;
				cell.Coord = new Vector2Int(x, y);
				cell.Shape.shape = GetRandomShape();
				cell.Shape.color = INACTIVE_SHAPE_COLOR;
				cell.Shape.outlineColor = INACTIVE_SHAPE_OUTLINE_COLOR;
				cell.Selectable.Parent = Selectable;
				cell.Selectable.OnHighlight += () => OnCellHover(cell);
				cell.Selectable.OnHighlightEnded += () => OnCellHoverOver(cell);
				cell.Selectable.OnInteract += () => { OnCellPressed(cell); return false; };
			}
		}
		Selectable.Children = Cells.SelectMany(row => row.Select(cell => cell.Selectable)).ToArray();
		Selectable.UpdateChildren();
	}

	private void OnCellHover(CellComponent cell) {
		if (SelectedCell != null) SelectedCell.active = false;
		SelectedCell = cell;
		SelectedCell.active = true;
		SelectedCell.color = SELECTED_CELL_COLOR;
		// TODO: check adjacement
		if (DrawingPath && !Path.Contains(cell.Coord)) Path.Add(cell.Coord);
	}

	private void OnCellHoverOver(CellComponent cell) {
		if (SelectedCell != cell) return;
		if (Path.Contains(cell.Coord)) SelectedCell.color = PATH_CELL_COLOR;
		else SelectedCell.active = false;
		SelectedCell = null;
	}

	private void OnCellPressed(CellComponent cell) {
		if (DrawingPath) {
			if (Path.Last() != cell.Coord) return;
			DrawingPath = false;
			// TODO: validate path
			return;
		}
		foreach (Vector2Int coord in Path) {
			CellComponent c = Cells[coord.x][coord.y];
			if (c == SelectedCell) continue;
			c.active = false;
		}
		DrawingPath = true;
		Path = new List<Vector2Int>();
		Path.Add(cell.Coord);
		cell.active = true;
		cell.color = PATH_CELL_COLOR;
	}

	private ShapeComponent CreateButton(ShapeComponent.Shape shape, Vector3 position) {
		ShapeComponent result = Instantiate(ShapePrefab);
		result.transform.parent = transform;
		result.transform.localPosition = position;
		result.transform.localRotation = Quaternion.identity;
		result.transform.localScale = Vector3.one;
		result.shape = shape;
		result.color = INACTIVE_BUTTON_COLOR;
		result.outlineColor = INACTIVE_BUTTON_OUTLINE_COLOR;
		return result;
	}

	private ShapeComponent.Shape GetRandomShape() {
		// if (Random.Range(0, 3) != 0) return ShapeComponent.Shape.NONE;
		int rnd = Random.Range(0, 7);
		switch (rnd) {
			case 0:
				return new[] {
					ShapeComponent.Shape.TRIANGLE_UP,
					ShapeComponent.Shape.TRIANGLE_RIGHT,
					ShapeComponent.Shape.TRIANGLE_DOWN,
					ShapeComponent.Shape.TRIANGLE_LEFT,
				}.PickRandom();
			case 1: return ShapeComponent.Shape.SQUARE;
			case 2: return ShapeComponent.Shape.DIAMOND;
			case 3: return ShapeComponent.Shape.CIRCLE;
			case 4: return ShapeComponent.Shape.STAR;
			case 5: return ShapeComponent.Shape.HEART;
			default: return ShapeComponent.Shape.NONE;
		}
	}
}
