using System.Collections.Generic;
using UnityEngine;

public abstract class ICoordRule : IRule {
	public Vector2Int Coord;

	protected void SetRandom(int size) {
		if (Random.Range(0, 2) == 0) {
			Dictionary<Vector2Int, string> corners = new Dictionary<Vector2Int, string>{
				{ new Vector2Int(0, 0), "top-left" },
				{ new Vector2Int(size - 1, 0), "top-right" },
				{ new Vector2Int(0, size - 1), "bottom-left" },
				{ new Vector2Int(size - 1, size - 1), "bottom-right" },
			};
			KeyValuePair<Vector2Int, string> choose = corners.PickRandom();
			Coord = choose.Key;
			description = choose.Value + " corner";
		} else if (size % 2 == 1 && Random.Range(0, 2) == 0) {
			Coord = new Vector2Int((size - 1) / 2, (size - 1) / 2);
			description = "center";
		} else if (Random.Range(0, 2) == 0) {
			int midX = (size - 1) / 2;
			Dictionary<Vector2Int, string> middles = new Dictionary<Vector2Int, string>{
				{ new Vector2Int(midX, 0), "top-middle" },
				{ new Vector2Int(size - 1, midX), "middle-right" },
				{ new Vector2Int(midX, size - 1), "bottom-middle" },
				{ new Vector2Int(0, midX), "middle-left" },
			};
			KeyValuePair<Vector2Int, string> choose = middles.PickRandom();
			Coord = choose.Key;
			description = choose.Value + " cell";
		} else {
			Coord = new Vector2Int(Random.Range(0, size), Random.Range(0, size));
			description = RuleGeneratorHelper.CoordToString(Coord);
		}
	}
}
