using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KeepCoding;

public class UnderstandModule : ModuleScript {
	public const float CELL_SIZE = .018f;
	public const float OUTLINE_WIDTH = .0012f;
	public const float RULE_INDICATORS_OFFSET = .014f;
	public static readonly Vector3 RULE_INDICATORS_CENTER = new Vector3(.07f, .015f, -.019f);
	public static readonly Color32 INACTIVE_BUTTON_COLOR = new Color32(0x00, 0x00, 0x00, 0xff);
	public static readonly Color32 INACTIVE_BUTTON_OUTLINE_COLOR = new Color32(0x44, 0x44, 0x44, 0xff);
	public static readonly Color32 ACTIVE_BUTTON_COLOR = new Color32(0x00, 0x00, 0x00, 0xff);
	public static readonly Color32 ACTIVE_BUTTON_OUTLINE_COLOR = new Color32(0x88, 0x88, 0x88, 0xff);
	public static readonly Color32 ACTIVE_SELECTED_BUTTON_COLOR = new Color32(0x44, 0x44, 0x44, 0xff);
	public static readonly Color32 SELECTED_CELL_COLOR = new Color32(0x88, 0x88, 0x88, 0xff);
	public static readonly Color32 PATH_CELL_COLOR = new Color32(0x55, 0x55, 0x55, 0xff);
	public static readonly Color32 START_PATH_CELL_COLOR = new Color32(0x77, 0x55, 0x55, 0xff);
	public static readonly Color32 START_SELECTED_PATH_CELL_COLOR = new Color32(0x99, 0x55, 0x55, 0xff);
	public static readonly Color32 FINISH_PATH_CELL_COLOR = new Color32(0x55, 0x55, 0x77, 0xff);
	public static readonly Color32 FINISH_SELECTED_PATH_CELL_COLOR = new Color32(0x55, 0x55, 0x99, 0xff);
	public static readonly Color32 INACTIVE_SHAPE_COLOR = new Color32(0x33, 0x33, 0x33, 0xff);
	public static readonly Color32 INACTIVE_SHAPE_OUTLINE_COLOR = new Color32(0xaa, 0xaa, 0xaa, 0xff);

	public GameObject GridOutlinePrefab;
	public Transform GridContainer;
	public TextMesh StageText;
	public KMSelectable Selectable;
	public ShapeComponent ShapePrefab;
	public CellComponent CellPrefab;
	public GameObject PathPrefab;
	public RuleIndicatorComponent RuleIndicatorPrefab;
	public ButtonComponent ButtonPrefab;

	private int currentStageIndex = 0;
	private int passedStagesCount = 1;
	private UnderstandPuzzle Puzzle;
	private ButtonComponent BackButton;
	private ButtonComponent NextButton;
	private CellComponent SelectedCell = null;
	private CellComponent[][] Cells = new CellComponent[UnderstandPuzzle.SIZE][];
	private List<Vector2Int>[] Submittions;
	private HashSet<GameObject> ActivePath = new HashSet<GameObject>();
	private HashSet<GameObject> InactivePath = new HashSet<GameObject>();
	private List<Vector2Int> Path = new List<Vector2Int>();
	private bool DrawingPath = false;
	private RuleIndicatorComponent[] ruleIndicators;

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
		Submittions = new List<Vector2Int>[UnderstandPuzzle.LEVELS_COUNT];
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
				cell.Shape.shape = ShapeComponent.Shape.NONE;
				cell.Shape.color = INACTIVE_SHAPE_COLOR;
				cell.Shape.outlineColor = INACTIVE_SHAPE_OUTLINE_COLOR;
				cell.Selectable.Parent = Selectable;
				cell.Selectable.OnHighlight += () => OnCellHover(cell);
				cell.Selectable.OnHighlightEnded += () => OnCellHoverOver(cell);
				cell.Selectable.OnInteract += () => { OnCellPressed(cell); return false; };
			}
		}
		Selectable.Children = Cells.SelectMany(row => row.Select(cell => cell.Selectable)).Concat(new[] { BackButton.Selectable, NextButton.Selectable }).ToArray();
		Selectable.UpdateChildren();
	}

	public override void OnActivate() {
		base.OnActivate();
		Puzzle = new UnderstandPuzzle();
		for (int i = 0; i < Puzzle.rules.Length; i++) Log("Rule #{0}: {1}", i + 1, Puzzle.rules[i].description);
		Path = new List<Vector2Int>(Puzzle.pathes[0]);
		ruleIndicators = new RuleIndicatorComponent[Puzzle.rules.Length];
		float firstRuleIndicatorPos = -RULE_INDICATORS_OFFSET * (Puzzle.rules.Length - 1) / 2f;
		for (int i = 0; i < Puzzle.rules.Length; i++) {
			RuleIndicatorComponent ruleIndicator = Instantiate(RuleIndicatorPrefab);
			ruleIndicator.transform.parent = transform;
			ruleIndicator.transform.localScale = Vector3.one;
			ruleIndicator.transform.localRotation = Quaternion.identity;
			ruleIndicator.transform.localPosition = RULE_INDICATORS_CENTER + new Vector3(0, 0, firstRuleIndicatorPos + i * RULE_INDICATORS_OFFSET);
			ruleIndicator.state = RuleIndicatorComponent.State.PASS;
			ruleIndicators[i] = ruleIndicator;
		}
		NextButton.active = true;
		BackButton.Selectable.OnInteract += () => { OnPrevButtonPressed(); return false; };
		NextButton.Selectable.OnInteract += () => { OnNextButtonPressed(); return false; };
		RenderStage();
		StageText.text = string.Format("{0}/{1}", currentStageIndex + 1, UnderstandPuzzle.LEVELS_COUNT);
	}

	private void OnPrevButtonPressed() {
		if (DrawingPath || IsSolved) return;
		if (currentStageIndex == 0) return;
		Submittions[currentStageIndex] = Path;
		currentStageIndex -= 1;
		Path = Submittions[currentStageIndex];
		RenderStage();
		NextButton.active = true;
		BackButton.active = currentStageIndex > 0;
		StageText.text = string.Format("{0}/{1}", currentStageIndex + 1, UnderstandPuzzle.LEVELS_COUNT);
	}

	private void OnNextButtonPressed() {
		if (DrawingPath || IsSolved || currentStageIndex == passedStagesCount) return;
		Submittions[currentStageIndex] = Path;
		currentStageIndex += 1;
		Path = Submittions[currentStageIndex] ?? new List<Vector2Int>();
		RenderStage();
		NextButton.active = currentStageIndex < passedStagesCount && currentStageIndex + 1 != UnderstandPuzzle.LEVELS_COUNT;
		BackButton.active = true;
		StageText.text = string.Format("{0}/{1}", currentStageIndex + 1, UnderstandPuzzle.LEVELS_COUNT);
	}

	private void RenderStage() {
		for (int x = 0; x < UnderstandPuzzle.SIZE; x++) {
			for (int y = 0; y < UnderstandPuzzle.SIZE; y++) {
				CellComponent cell = Cells[x][y];
				cell.Shape.shape = Puzzle.maps[currentStageIndex][x][y];
				UpdateCellColor(cell);
			}
		}
		UpdateRulesValidities();
		UpdatePath();
	}

	private void OnCellHover(CellComponent cell) {
		if (SelectedCell != null) {
			if (cell == SelectedCell) return;
			UpdateCellColor(SelectedCell);
		}
		SelectedCell = cell;
		UpdateCellColor(cell);
		if (!DrawingPath) return;
		Vector2Int lastCoord = Path.Last();
		CellComponent lastCell = Cells[lastCoord.x][lastCoord.y];
		if (Path.Count > 1 && Path.SkipLast(1).Last() == cell.Coord) {
			Path = Path.SkipLast(1).ToList();
			UpdatePath();
			UpdateCellColor(lastCell);
			UpdateCellColor(cell);
			return;
		}
		if (Path.Contains(cell.Coord)) return;
		Vector2Int dd = cell.Coord - lastCoord;
		int dir = Maze.DD.IndexOf(dd);
		if (dir < 0) return;
		Path.Add(cell.Coord);
		UpdatePath();
		UpdateCellColor(cell);
	}

	private void OnCellHoverOver(CellComponent cell) {
		if (SelectedCell != cell) return;
		SelectedCell = null;
		UpdateCellColor(cell);
	}

	private void OnCellPressed(CellComponent cell) {
		if (!IsActive || IsSolved) return;
		if (DrawingPath) {
			if (Path.Last() != cell.Coord) return;
			DrawingPath = false;
			UpdateCellColor(cell);
			UpdateRulesValidities();
			if (ruleIndicators.All(ind => ind.state == RuleIndicatorComponent.State.PASS)) OnStagePassed(currentStageIndex);
			else if (currentStageIndex == UnderstandPuzzle.LEVELS_COUNT - 1) {
				foreach (RuleIndicatorComponent ruleIndicator in ruleIndicators) ruleIndicator.state = RuleIndicatorComponent.State.OFF;
			}
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
		foreach (RuleIndicatorComponent ruleIndicator in ruleIndicators) ruleIndicator.state = RuleIndicatorComponent.State.OFF;
		UpdatePath();
		UpdateCellColor(cell);
	}

	private void UpdateRulesValidities() {
		for (int i = 0; i < Puzzle.rules.Length; i++) {
			if (Path.Count == 0) ruleIndicators[i].state = RuleIndicatorComponent.State.OFF;
			else if (Puzzle.RuleValid(currentStageIndex, i, Path)) ruleIndicators[i].state = RuleIndicatorComponent.State.PASS;
			else ruleIndicators[i].state = RuleIndicatorComponent.State.STRIKE;
		}
	}

	private void OnStagePassed(int stageIndex) {
		if (passedStagesCount > stageIndex) return;
		passedStagesCount = stageIndex + 1;
		if (passedStagesCount == UnderstandPuzzle.LEVELS_COUNT) {
			Solve();
			StageText.text = "GG";
		} else NextButton.active = true;
	}

	private void UpdatePath() {
		foreach (GameObject path in ActivePath) {
			path.SetActive(false);
			InactivePath.Add(path);
		}
		ActivePath = new HashSet<GameObject>();
		if (Path.Count == 0) return;
		Vector2Int prevCoord = Path.First();
		foreach (Vector2Int coord in Path.Skip(1)) {
			int dir = Enumerable.Range(0, 4).First(d => coord + Maze.DD[d] == prevCoord);
			GameObject path;
			if (InactivePath.Count > 0) {
				path = InactivePath.First(_ => true);
				InactivePath.Remove(path);
				path.SetActive(true);
			} else {
				path = Instantiate(PathPrefab);
				path.transform.parent = GridContainer;
				path.transform.localScale = Vector3.one;
			}
			path.transform.localPosition = new Vector3((coord.x + .5f) * CELL_SIZE, .0014f, (coord.y + .5f) * CELL_SIZE);
			path.transform.localRotation = Quaternion.Euler(0, 90f * dir, 0);
			ActivePath.Add(path);
			prevCoord = coord;
		}
	}

	private void UpdateCellColor(CellComponent cell) {
		if (Path.Count > 0 && Path.Contains(cell.Coord)) {
			cell.active = true;
			if (!DrawingPath && Path.Last() == cell.Coord) cell.color = SelectedCell == cell ? FINISH_SELECTED_PATH_CELL_COLOR : FINISH_PATH_CELL_COLOR;
			else if (Path.First() == cell.Coord) cell.color = SelectedCell == cell ? START_SELECTED_PATH_CELL_COLOR : START_PATH_CELL_COLOR;
			else cell.color = SelectedCell == cell ? SELECTED_CELL_COLOR : PATH_CELL_COLOR;
			return;
		}
		if (SelectedCell == cell) {
			cell.active = true;
			cell.color = SELECTED_CELL_COLOR;
		} else cell.active = false;
	}

	private ButtonComponent CreateButton(ShapeComponent.Shape shape, Vector3 position) {
		ButtonComponent result = Instantiate(ButtonPrefab);
		result.transform.parent = transform;
		result.transform.localPosition = position;
		result.transform.localRotation = Quaternion.identity;
		result.transform.localScale = Vector3.one;
		result.shape = shape;
		return result;
	}
}
