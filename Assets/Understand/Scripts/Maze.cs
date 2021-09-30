using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Maze {
	public static readonly Vector2Int[] DD = new[] { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(-1, 0), new Vector2Int(0, -1) };

	private class GenerationCell {
		public bool processed;
		public bool queued;
	}

	public readonly int Size;
	private int[][] _data;

	public Maze(int Size) {
		this.Size = Size;
		_data = new int[Size][];
		GenerationCell[][] cells = new GenerationCell[Size][];
		for (int x = 0; x < Size; x++) {
			_data[x] = new int[Size];
			cells[x] = Enumerable.Range(0, Size).Select(_ => new GenerationCell()).ToArray();
		}
		Vector2Int startPos = new Vector2Int(Random.Range(0, Size), Random.Range(0, Size));
		GenerationCell startingCell = cells[startPos.x][startPos.y];
		startingCell.processed = true;
		startingCell.queued = true;
		HashSet<Vector2Int> q = new HashSet<Vector2Int>();
		foreach (Vector2Int dd in DD) {
			Vector2Int newPos = startPos + dd;
			if (!IsValidCoord(newPos)) continue;
			cells[newPos.x][newPos.y].queued = true;
			q.Add(newPos);
		}
		RunGenerator(q, cells);
	}

	public List<Vector2Int> GenerateRandomPath(int minLength) {
		return GenerateRandomPath(minLength, new Vector2Int(Random.Range(0, Size), Random.Range(0, Size)));
	}

	public List<Vector2Int> GenerateRandomPath(int minLength, Vector2Int visit) {
		List<Vector2Int> result = GenerateRandomPath(visit);
		if (result.Count >= minLength) return result;
		throw new System.Exception("Unable to create random path with required length");
	}

	private List<Vector2Int> GenerateRandomPath(Vector2Int from) {
		Queue<List<Vector2Int>> q = new Queue<List<Vector2Int>>();
		int potentialResultsCount = 0;
		List<Vector2Int> result = new List<Vector2Int>();
		bool[][] cellProcessed = new bool[Size][];
		for (int x = 0; x < Size; x++) cellProcessed[x] = new bool[Size];
		q.Enqueue(new List<Vector2Int>(new[] { from }));
		cellProcessed[from.x][from.y] = true;
		while (q.Count > 0) {
			List<Vector2Int> path = q.Dequeue();
			bool deadend = true;
			Vector2Int lastCell = path.Last();
			foreach (Vector2Int dd in Enumerable.Range(0, 4).Where(dir => (_data[lastCell.x][lastCell.y] & (1 << dir)) > 0).Select(dir => DD[dir])) {
				Vector2Int newCell = lastCell + dd;
				if (!IsValidCoord(newCell) || cellProcessed[newCell.x][newCell.y]) continue;
				deadend = false;
				cellProcessed[newCell.x][newCell.y] = true;
				q.Enqueue(path.Concat(new[] { newCell }).ToList());
			}
			if (!deadend) continue;
			if (result.Count > path.Count) continue;
			if (result.Count < path.Count) {
				result = path;
				potentialResultsCount = 1;
				continue;
			}
			potentialResultsCount += 1;
			if (Random.Range(0, potentialResultsCount) == 0) result = path;
		}
		return result;
	}

	private void RunGenerator(HashSet<Vector2Int> q, GenerationCell[][] cells) {
		while (q.Count > 0) {
			Vector2Int rndPos = q.PickRandom();
			q.Remove(rndPos);
			List<Vector2Int> availableDirs = new List<Vector2Int>();
			foreach (Vector2Int dd in DD) {
				Vector2Int newPos = rndPos + dd;
				if (!IsValidCoord(newPos)) continue;
				GenerationCell newCell = cells[newPos.x][newPos.y];
				if (newCell.processed) availableDirs.Add(dd);
				if (!newCell.queued) {
					q.Add(newPos);
					newCell.queued = true;
				}
			}
			Vector2Int dir = availableDirs.PickRandom();
			int dirIndex = System.Array.IndexOf(DD, dir);
			_data[rndPos.x][rndPos.y] |= 1 << dirIndex;
			Vector2Int prevPos = rndPos + dir;
			_data[prevPos.x][prevPos.y] |= 1 << ((dirIndex + 2) % 4);
			cells[rndPos.x][rndPos.y].processed = true;
		}
	}

	private bool IsValidCoord(Vector2Int a) {
		return a.x >= 0 && a.x < Size && a.y >= 0 && a.y < Size;
	}
}
