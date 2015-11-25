using UnityEngine;
using System.Collections;
using RTS;

public class UserInput : MonoBehaviour {

	private Player player;

	// Use this for initialization
	void Start () {
		this.player = transform.root.GetComponent<Player>();
	}
	
	// Update is called once per frame
	void Update () {
		if(player.human) {
			MoveCamera();
			RotateCamera();
			MonitorMouseActivity();
		}
	}

	private void MonitorMouseActivity () {
		if(Input.GetMouseButtonDown(0)) LeftMouseClick();
		else if(Input.GetMouseButtonDown(1)) RightMouseClick();
		else MouseHover();
	}

	private void LeftMouseClick() {
		if(player.hud.MouseInBounds()) {
			GameObject hitObject = FindHitObject();
			Vector3 hitPoint = FindHitPoint();
			if(hitObject && hitPoint != ResourceManager.InvalidPosition) {
				Debug.Log ("hit " + hitObject.name);
				if(player.SelectedObject) {
					player.SelectedObject.MouseClick(hitObject, hitPoint, player);
				} else {
					WorldObject worldObject = hitObject.transform.parent.GetComponent<WorldObject>();
					Debug.Log(worldObject.name);
					if(worldObject) {
						// we already know the player has no selected object
						player.SelectedObject = worldObject;
						worldObject.SetSelection(true, player.hud.GetPlayingArea());
					}
				}
			}
		}
	}

	private void RightMouseClick () {
		if(player.hud.MouseInBounds() && player.SelectedObject) {
			player.SelectedObject.SetSelection(false, player.hud.GetPlayingArea());
			player.SelectedObject = null;
		}
	}

	private void MouseHover() {
		if(player.hud.MouseInBounds()) {
			GameObject hoverObject = FindHitObject();
			if(hoverObject) {
				if(player.SelectedObject) player.SelectedObject.SetHoverState(hoverObject);
				// if we don't have anything selected already...
				else if(hoverObject.name != "Ground") {
					// check if the hovered object is owned by the player
					if(hoverObject.transform.parent == null) return;
					Player owner = hoverObject.transform.parent.GetComponent<Player>();
					if(owner) {
						// look for a unit or building that we are hovering over
						Unit unit = hoverObject.transform.parent.GetComponent<Unit>();
						Building building = hoverObject.transform.parent.GetComponent<Building>();
						// indicate that it is selectable
						if(owner.username == player.username && (unit || building)) player.hud.SetCursorState(CursorState.Select);
					}
				}
			}
		}
	}

	private GameObject FindHitObject () {
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		// RayCast will store the first collided object in `hit`
		// or return false if no object is found
		if(Physics.Raycast(ray, out hit)) return hit.collider.gameObject;
		return null;
	}

	private Vector3 FindHitPoint () {
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if(Physics.Raycast(ray, out hit)) return hit.point;
		return ResourceManager.InvalidPosition;
	}

	private void MoveCamera () {
		float xpos = Input.mousePosition.x;
		float ypos = Input.mousePosition.y;
		Vector3 movement = new Vector3(0,0,0);

		bool mouseScroll = false;

		//horizontal camera movement
		if(xpos >= 0 && xpos < ResourceManager.ScrollWidth) {
			movement.x -= ResourceManager.ScrollSpeed;
			player.hud.SetCursorState(CursorState.PanLeft);
			mouseScroll = true;
		} else if(xpos <= Screen.width && xpos > Screen.width - ResourceManager.ScrollWidth) {
			movement.x += ResourceManager.ScrollSpeed;
			player.hud.SetCursorState(CursorState.PanRight);
			mouseScroll = true;
		}
		
		//vertical camera movement
		if(ypos >= 0 && ypos < ResourceManager.ScrollWidth) {
			movement.z -= ResourceManager.ScrollSpeed;
			player.hud.SetCursorState(CursorState.PanDown);
			mouseScroll = true;
		} else if(ypos <= Screen.height && ypos > Screen.height - ResourceManager.ScrollWidth) {
			movement.z += ResourceManager.ScrollSpeed;
			player.hud.SetCursorState(CursorState.PanUp);
			mouseScroll = true;
		}

		if(!mouseScroll) {
			player.hud.SetCursorState(CursorState.Select);
		}

		//make sure movement is in the direction the camera is pointing
		//but ignore the vertical tilt of the camera to get sensible scrolling
		movement = Camera.main.transform.TransformDirection(movement);
		movement.y = 0;

		//away from ground movement
		movement.y -= ResourceManager.ScrollSpeed * Input.GetAxis("Mouse ScrollWheel");

		//calculate desired camera position based on received input
		Vector3 origin = Camera.main.transform.position;
		Vector3 destination = origin;
		destination.x += movement.x;
		destination.y += movement.y;
		destination.z += movement.z;

		//limit away from ground movement to be between a minimum and maximum distance
		if(destination.y > ResourceManager.MaxCameraHeight) {
			destination.y = ResourceManager.MaxCameraHeight;
		} else if(destination.y < ResourceManager.MinCameraHeight) {
			destination.y = ResourceManager.MinCameraHeight;
		}

		//if a change in position is detected perform the necessary update
		if(destination != origin) {
			Camera.main.transform.position = Vector3.MoveTowards(origin, destination, Time.deltaTime * ResourceManager.ScrollSpeed);
		}
	}
	
	private void RotateCamera () {
		Vector3 origin = Camera.main.transform.eulerAngles;
		Vector3 destination = origin;
		
		//detect rotation amount if ALT is being held
		if((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))) {
			destination.x -= Input.GetAxis("Mouse Y") * ResourceManager.RotateAmount;
			destination.y += Input.GetAxis("Mouse X") * ResourceManager.RotateAmount;
		}
		
		//if a change in position is detected perform the necessary update
		if(destination != origin) {
			Camera.main.transform.eulerAngles = Vector3.MoveTowards(origin, destination, Time.deltaTime * ResourceManager.RotateSpeed);
		}
	}
}
