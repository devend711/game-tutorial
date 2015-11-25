using UnityEngine;
using System.Collections;
using RTS;

public class WorldObject : MonoBehaviour {

	public string objectName;
	public Texture2D buildImage;
	public int cost, sellValue, hitPoints, maxHitPoints;

	protected Player player;
	protected string[] actions = {};
	protected bool currentlySelected = false;

	protected Bounds selectionBounds;
	protected Rect playingArea = new Rect(0.0f, 0.0f, 0.0f, 0.0f);

	// this is called before Start ()
	protected virtual void Awake () {

	}

	protected virtual void Start () {
		this.player = transform.root.GetComponentInChildren<Player>();
		this.cost = this.sellValue = 0;
		this.hitPoints = this.maxHitPoints = 0;

		selectionBounds = ResourceManager.InvalidBounds;
		CalculateBounds();
	}
	
	protected virtual void Update () {
		
	}
	
	protected virtual void OnGUI() {
		if(this.currentlySelected) DrawSelection();
	}

	public void SetSelection (bool selected, Rect playingArea) {
		this.currentlySelected = selected;
		if(selected) this.playingArea = playingArea;
	}

	public string[] GetActions() {
		return this.actions;
	}
	
	public virtual void PerformAction (string actionToPerform) {
	}

	public bool BelongsToPlayer(Player otherPlayer) {
		return (this.player == otherPlayer);
	}

	// when something is clicked...
	public virtual void MouseClick (GameObject hitObject, Vector3 hitPoint, Player controller) {
		// if this is currently selected...
		if(this.currentlySelected && hitObject && hitObject.name != "Ground") {
			WorldObject worldObject = hitObject.transform.parent.GetComponent<WorldObject>();
			// select the other object
			if(worldObject) ChangeSelection(worldObject, controller);
		}
	}

	private void ChangeSelection(WorldObject worldObject, Player player) {
		//this should be called by the following line, but there is an outside chance it will not
		SetSelection(false, this.playingArea);
		if(player.SelectedObject) player.SelectedObject.SetSelection(false, this.playingArea);
		player.SelectedObject = worldObject;
		worldObject.SetSelection(true, this.playingArea);
	}

	public void CalculateBounds() {
		selectionBounds = new Bounds(transform.position, Vector3.zero);
		foreach(Renderer r in GetComponentsInChildren< Renderer >()) {
			selectionBounds.Encapsulate(r.bounds);
		}
	}

	protected virtual void DrawSelectionBox(Rect selectBox) {
		GUI.Box(selectBox, "");
	}
	
	private void DrawSelection () {
		GUI.skin = ResourceManager.SelectBoxSkin;
		Rect selectBox = WorkManager.CalculateSelectionBox(selectionBounds, playingArea);
		//Draw the selection box around the currently selected object, within the bounds of the playing area
		GUI.BeginGroup(this.playingArea);
		DrawSelectionBox(selectBox);
		GUI.EndGroup();
	}

	public virtual void SetHoverState(GameObject hoverObject) {
		// by default, show this object as selectable
		if(player && player.human && currentlySelected) {
			if(hoverObject.name != "Ground") player.hud.SetCursorState(CursorState.Select);
		}
	}
}
