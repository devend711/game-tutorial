using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;

public class Unit : WorldObject {
	protected bool moving, rotating;
	protected float moveSpeed, rotateSpeed;
	public float currentHealth, maxHealth;

	protected Dictionary<ResourceType, int> resourceCosts;
	
	private Vector3 destination;
	private Quaternion targetRotation;

	protected Animation animation;

	private const int DEFAULT_UNIT_MOVESPEED = 10;
	public static float UNIT_MOVEMENT_RANDOMIZER = 1.5f;
	private const int DEFAULT_UNIT_ROTATESPEED = 10;

	private const int DEFAULT_UNIT_MAXHEALTH = 1;

	protected override void Awake () {
		base.Awake();
	}
	
	protected override void Start () {
		base.Start();
		this.type = WorldObjectType.Unit;
		this.moveSpeed = DEFAULT_UNIT_MOVESPEED;
		this.rotateSpeed = DEFAULT_UNIT_ROTATESPEED;
		this.maxHealth = this.currentHealth = DEFAULT_UNIT_MAXHEALTH;
		this.animation = null;
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
		// add some randomness to the destination point
		Vector3 randomizedDestination = AddRandomnessToPoint (destination);
		// set our new destinatino point
		this.destination = randomizedDestination;
		this.targetRotation = Quaternion.LookRotation (randomizedDestination - transform.position);
		this.rotating = true;
		this.moving = false;
	}

	private void TurnToTarget () {
		Debug.Log (this.name + " TurnToTarget()");
		transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed);
		//sometimes it gets stuck exactly 180 degrees out in the calculation and does nothing, this check fixes that
		Quaternion inverseTargetRotation = new Quaternion(-targetRotation.x, -targetRotation.y, -targetRotation.z, -targetRotation.w);
		if(transform.rotation == targetRotation || transform.rotation == inverseTargetRotation) {
			rotating = false;
			moving = true;
		}
	}

	private void MakeMove () {
		Debug.Log (this.name + " MakeMove()");
		transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * moveSpeed);
		if(transform.position == destination) this.moving = false;
	}

	public static Vector3 AddRandomnessToPoint(Vector3 point) {
		Vector3 randomizedPoint = point;
		randomizedPoint.x += Random.Range (-UNIT_MOVEMENT_RANDOMIZER, UNIT_MOVEMENT_RANDOMIZER);
		randomizedPoint.z += Random.Range (-UNIT_MOVEMENT_RANDOMIZER, UNIT_MOVEMENT_RANDOMIZER);
		return randomizedPoint;
	}
}
