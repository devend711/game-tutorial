using UnityEngine;
using System.Collections;
using RTS;

public class HUD : MonoBehaviour {

	public GUISkin resourceSkin, ordersSkin, selectBoxSkin, mouseCursorSkin;

	public Texture2D activeCursor;
	public Texture2D selectCursor, leftCursor, rightCursor, upCursor, downCursor;
	public Texture2D[] moveCursors, attackCursors, harvestCursors;

	private CursorState activeCursorState;
	private int currentFrame = 0;

	private const int ORDERS_BAR_WIDTH = 150, RESOURCE_BAR_HEIGHT = 40;
	private const int LABEL_HEIGHT = 24;
	private const int PADDING = 5;

	private Player player;

	// Use this for initialization
	void Start () {
		player = transform.root.GetComponent< Player >();
		ResourceManager.StoreSelectBoxItems(this.selectBoxSkin);
		SetCursorState(CursorState.Select);
	}
	
	// Update is called once per frame
	void OnGUI () {
		if(player && player.human) {
			DrawOrdersBar();
			DrawResourceBar();
			DrawMouseCursor();
		}
	}

	public bool MouseInBounds() {
		//Screen coordinates start in the lower-left corner of the screen
		//not the top-left of the screen like the drawing coordinates do
		Vector3 mousePos = Input.mousePosition;
		bool insideWidth = mousePos.x >= 0 && mousePos.x <= Screen.width - ORDERS_BAR_WIDTH;
		bool insideHeight = mousePos.y >= 0 && mousePos.y <= Screen.height - RESOURCE_BAR_HEIGHT;
		return insideWidth && insideHeight;
	}

	private Rect GetCursorDrawPosition() {
		//set base position for custom cursor image
		float leftPos = Input.mousePosition.x;
		float topPos = Screen.height - Input.mousePosition.y; //screen draw coordinates are inverted
		//adjust position base on the type of cursor being shown
		if(activeCursorState == CursorState.PanRight) leftPos = Screen.width - activeCursor.width;
		else if(activeCursorState == CursorState.PanDown) topPos = Screen.height - activeCursor.height;
		else if(activeCursorState == CursorState.Move || activeCursorState == CursorState.Select || activeCursorState == CursorState.Harvest) {
			topPos -= activeCursor.height / 2;
			leftPos -= activeCursor.width / 2;
		}
		return new Rect(leftPos, topPos, activeCursor.width, activeCursor.height);
	}

	public void SetCursorState(CursorState newState) {
		activeCursorState = newState;
		switch(newState) {
		case CursorState.Select:
			activeCursor = selectCursor;
			break;
		case CursorState.Attack:
			currentFrame = (int)Time.time % attackCursors.Length;
			activeCursor = attackCursors[currentFrame];
			break;
		case CursorState.Harvest:
			currentFrame = (int)Time.time % harvestCursors.Length;
			activeCursor = harvestCursors[currentFrame];
			break;
		case CursorState.Move:
			currentFrame = (int)Time.time % moveCursors.Length;
			activeCursor = moveCursors[currentFrame];
			break;
		case CursorState.PanLeft:
			activeCursor = leftCursor;
			break;
		case CursorState.PanRight:
			activeCursor = rightCursor;
			break;
		case CursorState.PanUp:
			activeCursor = upCursor;
			break;
		case CursorState.PanDown:
			activeCursor = downCursor;
			break;
		default: break;
		}
	}

	private void DrawMouseCursor() {
		bool mouseOverHud = !MouseInBounds() && activeCursorState != CursorState.PanRight && activeCursorState != CursorState.PanUp;

		if(mouseOverHud) {
			//show system cursor
			Cursor.visible = true;
		} else {
			Cursor.visible = false;
			GUI.skin = this.mouseCursorSkin;
			GUI.BeginGroup(new Rect(0,0,Screen.width,Screen.height));
			UpdateCursorAnimation();
			Rect cursorPosition = GetCursorDrawPosition();
			GUI.Label(cursorPosition, this.activeCursor);
			GUI.EndGroup();
		}
	}

	private void UpdateCursorAnimation() {
		//sequence animation for cursor (based on more than one image for the cursor)
		//change once per second, loops through array of images
		if(this.activeCursorState == CursorState.Move) {
			this.currentFrame = (int)Time.time % moveCursors.Length;
			activeCursor = moveCursors[this.currentFrame];
		} else if(activeCursorState == CursorState.Attack) {
			this.currentFrame = (int)Time.time % attackCursors.Length;
			activeCursor = attackCursors[this.currentFrame];
		} else if(activeCursorState == CursorState.Harvest) {
			this.currentFrame = (int)Time.time % harvestCursors.Length;
			activeCursor = harvestCursors[this.currentFrame];
		}
	}

	private void DrawOrdersBar() {
		GUI.skin = ordersSkin;
		GUI.BeginGroup(new Rect(Screen.width-ORDERS_BAR_WIDTH,RESOURCE_BAR_HEIGHT,ORDERS_BAR_WIDTH,Screen.height-RESOURCE_BAR_HEIGHT));
		GUI.Box(new Rect(0,0,ORDERS_BAR_WIDTH,Screen.height-RESOURCE_BAR_HEIGHT),"");

		// write the name of the currently selected object
		if(player.SelectedObject) {
			string selectionName = player.SelectedObject.objectName;
			if(selectionName != "") GUI.Label(new Rect(PADDING,PADDING,ORDERS_BAR_WIDTH - 2*PADDING,LABEL_HEIGHT), selectionName);
			if(player.SelectedObject.BelongsToPlayer(this.player)) GUI.Label(new Rect(PADDING,PADDING + LABEL_HEIGHT,ORDERS_BAR_WIDTH - 2*PADDING,LABEL_HEIGHT), "You own this");
		}

		GUI.EndGroup();
	}

	private void DrawResourceBar() {
		GUI.skin = resourceSkin;
		GUI.BeginGroup(new Rect(0,0,Screen.width,RESOURCE_BAR_HEIGHT));
		GUI.Box(new Rect(0,0,Screen.width,RESOURCE_BAR_HEIGHT),"");
		GUI.EndGroup();
	}

	public Rect GetPlayingArea() {
		return new Rect(0, RESOURCE_BAR_HEIGHT, Screen.width - ORDERS_BAR_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT);
	}
}
