using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuleIndicatorComponent : MonoBehaviour {
	public enum State { OFF, PASS, STRIKE }

	public Renderer SelfRenderer;

	public State state { get { return _state; } set { if (_state == value) return; _state = value; UpdateColor(); } } private State _state = State.OFF;

	private void Start() {
		UpdateColor();
	}

	private void UpdateColor() {
		SelfRenderer.material.color = GetColor();
	}

	private Color GetColor() {
		switch (state) {
			case State.OFF: return Color.black;
			case State.PASS: return Color.green;
			case State.STRIKE: return Color.red;
			default: throw new System.Exception("Unknown rule indicator state");
		}
	}
}
