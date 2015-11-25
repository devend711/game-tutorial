using UnityEngine;
using System.Collections;
using RTS;

public class Unit : WorldObject {
	protected bool moving, rotating;
	public float moveSpeed, rotateSpeed;
	
	private Vector3 destination;
	private Quaternion targetRotation;

	private const int DEFAULT_UNIT_MOVESPEED = 2;

	protected override void Awake () {
		base.Awake();
	}
	
	protected override void Start () {
		base.Start();
		this.moveSpeed = DEFAULT_UNIT_MOVESPEED;
	}
	
	protected override void Update () {
		base.Update();
		if(rotating) TurnToTarget();
		else if(moving) MakeMove();
		// update the selection area
		CalculateBounds();
	}
	
	protected override void OnGUI () {
		base.OnGUI();
	}

	public override void SetHoverState (GameObject hoverObject) {
		base.SetHoverState(hoverObject);
		// indicate this as moveable if hovering over the ground
		if(player && player.human && currentlySelected) {
			if(hoverObject.name == "Ground") player.hud.SetCursorState(CursorState.Move);
		}
	}

	// when something is clicked...
	public override void MouseClick (GameObject hitObject, Vector3 hitPoint, Player controller) {
		base.MouseClick(hitObject, hitPoint, controller);
		Debug.Log ("clicked on " + hitObject.name);
		//only handle input if owned by a human player and currently selected
		if(this.player && this.player.human && this.currentlySelected) {
			if(hitObject.name == "Ground" && hitPoint != ResourceManager.InvalidPosition) {
				Debug.Log ("moving...");
				float x = hitPoint.x;
				float z = hitPoint.z;
				// makes sure that the unit stays on top of the surface it is on
				float y = hitPoint.y + player.SelectedObject.transform.position.y;
				Vector3 destination = new Vector3(x, y, z);
				StartMove(destination);
			}
		}
	}

	public void StartMove (Vector3 destination) {
		Debug.Log (this.name + " starting to move");
		this.destination = destination;
		this.targetRotation = Quaternion.LookRotation (destination - transform.position);
		this.rotating = true;
		this.moving = false;
	}

	private void TurnToTarget () {
		transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed);
		//sometimes it gets stuck exactly 180 degrees out in the calculation and does nothing, this check fixes that
		Quaternion inverseTargetRotation = new Quaternion(-targetRotation.x, -targetRotation.y, -targetRotation.z, -targetRotation.w);
		if(transform.rotation == targetRotation || transform.rotation == inverseTargetRotation) {
			rotating = false;
			moving = true;
		}
	}

	private void MakeMove () {
		transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * moveSpeed);
		if(transform.position == destination) moving = false;
	}
}
