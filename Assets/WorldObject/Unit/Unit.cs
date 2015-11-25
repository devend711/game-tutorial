using UnityEngine;
using System.Collections;
using RTS;

public class Unit : WorldObject {
	protected override void Awake () {
		base.Awake();
	}
	
	protected override void Start () {
		base.Start();
	}
	
	protected override void Update () {
		base.Update();
	}
	
	protected override void OnGUI () {
		base.OnGUI();
	}

	public override void SetHoverState(GameObject hoverObject) {
		base.SetHoverState(hoverObject);
		Debug.Log ("set hover state of unit");
		// indicate this as moveable if hovering over the ground
		if(player && player.human && currentlySelected) {
			Debug.Log("unit selected and hovering over the ground");
			if(hoverObject.name == "Ground") player.hud.SetCursorState(CursorState.Move);
		}
	}
}
