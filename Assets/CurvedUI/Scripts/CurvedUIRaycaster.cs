using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;


public class CurvedUIRaycaster : GraphicRaycaster{

	[SerializeField] bool UseVRGazeRaycaster = false;
	[SerializeField] bool showDebug = false;
//	[SerializeField] float reticleDistance = 500;
	//internal
	Canvas myCanvas;
	CurvedUISettings mySettings;
	Vector3 cyllinderMidPoint;


	#region LIFECYCLE
	protected override void Awake() {
		base.Awake();
		myCanvas = GetComponent<Canvas>();
		mySettings = GetComponent<CurvedUISettings>();

		cyllinderMidPoint = mySettings.GetCyllinderMidPointInCanvasSpace();

		//the canvas needs an event camera set up to process events correctly. Try to use main camera if no one is provided.
		if(myCanvas.worldCamera == null && Camera.main != null)
			myCanvas.worldCamera = Camera.main;
	}

	protected override void Start(){
		CreateCollider();
	}
	#endregion 


	#region RAYCASTING
	public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList) {
		//check if we have a world camera to process events by
		if(myCanvas.worldCamera == null)
			Debug.LogWarning("CurvedUIRaycaster requires Canvas to have a world camera reference to process events!", myCanvas.gameObject);


		Camera worldCamera = myCanvas.worldCamera;
		Ray ray3D;
		if(UseVRGazeRaycaster){
			//get a ray from the center of world camera. used for gaze input
			ray3D = new Ray(worldCamera.transform.position, worldCamera.transform.forward);
			foreach(GameObject go in eventData.hovered){
				if(go.GetComponent<Selectable>() != null){
					eventData.selectedObject = go;
					break;
				}
			}

		} else {
			// Get a ray from the camera through the point on the screen - used for mouse input
			ray3D = worldCamera.ScreenPointToRay(eventData.position);
		}

		Vector2 remappedPosition = eventData.position;

		if(mySettings.Mapping == CurvedUISettings.CurvedUIMapping.CYLLINDER){
			
			if (!RaycastToCurvedCanvas(ray3D, out remappedPosition))return;

		} else if(mySettings.Mapping == CurvedUISettings.CurvedUIMapping.RING){

			if (!RaycastToRingCanvas(ray3D, out remappedPosition))return;
		}
			
		// Update event data
		eventData.position = remappedPosition;

		// Use base class raycast method to finish the raycast
		base.Raycast(eventData, resultAppendList);
	}




	public virtual bool RaycastToCurvedCanvas(Ray ray3D, out Vector2 o_canvasPos) {

		if(showDebug){
			Debug.DrawLine(ray3D.origin, ray3D.GetPoint(1000), Color.red);
		}

		RaycastHit hit = new RaycastHit();
		if(Physics.Raycast(ray3D, out hit)){

			//direction from the cyllinder center to the hit point
			Vector3 localHitPoint = myCanvas.transform.worldToLocalMatrix.MultiplyPoint3x4(hit.point);
			Vector3 directionFromCyllinderCenter = (localHitPoint - cyllinderMidPoint).normalized;
			
			//angle between middle of the projected canvas and hit point direction
			float angle = -AngleSigned(directionFromCyllinderCenter.ModifyY(0) , mySettings.Angle < 0 ? Vector3.back : Vector3.forward, Vector3.up);
			
			//convert angle to canvas coordinates
			Vector2 canvasSize = myCanvas.GetComponent<RectTransform>().rect.size;
			
			//map the intersection point to 2d point in canvas space
			Vector2 pointOnCanvas = new Vector3(0,0,0);
			pointOnCanvas.x = angle.Remap(-mySettings.Angle / 2.0f, mySettings.Angle / 2.0f, -canvasSize.x / 2.0f,  canvasSize.x / 2.0f);
			pointOnCanvas.y = localHitPoint.y;

			//convert the result to screen point in camera. This will be later used by raycaster and world camera to determine what we're pointing at
			o_canvasPos = myCanvas.worldCamera.WorldToScreenPoint(myCanvas.transform.localToWorldMatrix.MultiplyPoint3x4(pointOnCanvas));

			if(showDebug){
				//Debug.DrawLine(canvasWorldPoint, canvasWorldPoint.ModifyY(canvasWorldPoint.y + 10), Color.blue);
				Debug.DrawLine(hit.point, hit.point.ModifyY(hit.point.y + 10), Color.green);
				Debug.DrawLine(hit.point, myCanvas.transform.localToWorldMatrix.MultiplyPoint3x4(cyllinderMidPoint), Color.yellow);
			}

			return true;
		}

		o_canvasPos = Vector2.zero;
		return false;
	}



	public virtual bool RaycastToRingCanvas(Ray ray3D, out Vector2 o_canvasPos) {

		RaycastHit hit = new RaycastHit();
		if(Physics.Raycast(ray3D, out hit)){

			//local hit point on canvas and a direction from center
			Vector3 localHitPoint = myCanvas.transform.worldToLocalMatrix.MultiplyPoint3x4(hit.point);
			Vector3 directionFromRingCenter = localHitPoint.ModifyZ(0).normalized;

			Vector2 canvasSize = myCanvas.GetComponent<RectTransform>().rect.size;

			//angle between middle of the projected canvas and hit point direction from center
			float angle = -AngleSigned(directionFromRingCenter.ModifyZ(0) , Vector3.up, Vector3.back);

			//map the intersection point to 2d point in canvas space
			Vector2 pointOnCanvas = new Vector2(0,0);

			//map x coordinate based on angle between vector up and direction to hitpoint
			if(angle < 0){
				pointOnCanvas.x = angle.Remap(0, -mySettings.Angle, -canvasSize.x / 2.0f,  canvasSize.x / 2.0f);
			} else {
				pointOnCanvas.x = angle.Remap(0, mySettings.Angle, canvasSize.x / 2.0f,  -canvasSize.x / 2.0f);
			}
				
			//map y coordinate based on hitpoint distance from the center and external diameter
			pointOnCanvas.y = localHitPoint.magnitude.Remap(mySettings.RingExternalDiameter * 0.5f * (1 - mySettings.RingFill) , mySettings.RingExternalDiameter * 0.5f,
				-canvasSize.y * 0.5f * (mySettings.RingFlipVertical ? -1 : 1),  canvasSize.y * 0.5f * (mySettings.RingFlipVertical ? -1 : 1));

			//convert the result to screen point in camera. This will be later used by raycaster and world camera to determine what we're pointing at
			o_canvasPos = myCanvas.worldCamera.WorldToScreenPoint(myCanvas.transform.localToWorldMatrix.MultiplyPoint3x4(pointOnCanvas));
			return true;
		}

		o_canvasPos = Vector2.zero;
		return false;
	}
	#endregion 



	#region COLLIDER MANAGEMENT
	/// <summary>
	/// Creates a mesh collider for curved canvas based on current angle and curve segments.
	/// </summary>
	/// <returns>The collider.</returns>
	protected Collider CreateCollider(){

		//remove all colliders on this object
		List<Collider>Cols = new List<Collider>();
		Cols.AddRange(this.GetComponents<Collider>());
		for(int i =0; i < Cols.Count; i++){
			Destroy(Cols[i]);
		}
			
		//create a collider based on mapping type
		switch(mySettings.Mapping){
				
			case CurvedUISettings.CurvedUIMapping.CYLLINDER:{
				//create a meshfilter if this object does not have one yet.
				if(GetComponent<MeshFilter>() == null){
					this.gameObject.AddComponent<MeshFilter>();
				}
				MeshFilter mf = GetComponent<MeshFilter>();

				MeshCollider mc = this.gameObject.AddComponent<MeshCollider>();

				Mesh meshie = new Mesh();
				mf.mesh = meshie;

				Vector3[] Vertices = new Vector3[4];
				(myCanvas.transform as RectTransform).GetWorldCorners(Vertices);
				meshie.vertices = Vertices;

				//rearrenge them to be in an easy to interpolate order and convert to canvas local spce
				Vertices[0] = myCanvas.transform.worldToLocalMatrix.MultiplyPoint3x4(meshie.vertices[1]);
				Vertices[1] = myCanvas.transform.worldToLocalMatrix.MultiplyPoint3x4(meshie.vertices[0]);
				Vertices[2] = myCanvas.transform.worldToLocalMatrix.MultiplyPoint3x4(meshie.vertices[2]);
				Vertices[3] = myCanvas.transform.worldToLocalMatrix.MultiplyPoint3x4(meshie.vertices[3]);
			
				//add some bounds to the collider so we can get interactions with buttons just outside the canvas.
				//If you really need to click stuff way outside the canvas just change it to 2.0f
				for(int i =0; i < Vertices.GetLength(0); i++){
					Vertices[i] *= 1.2f;
				}

				meshie.vertices = Vertices;

				//create a new array of vertices, subdivided as needed
				List<Vector3> verts = new List<Vector3>();
				int vertsCount = Mathf.Max(2, Mathf.RoundToInt(mySettings.BaseCircleSegments * Mathf.Abs(mySettings.Angle) / 360.0f * 0.5f));
				for(int i =0; i < vertsCount; i++){
					verts.Add (Vector3.Lerp(meshie.vertices[0], meshie.vertices[2], (i * 1.0f) /  (vertsCount - 1) ));
					verts.Add ( Vector3.Lerp(meshie.vertices[1], meshie.vertices[3], (i * 1.0f) /  (vertsCount - 1) ));
				}
					
				//curve the verts in canvas local space
				if(mySettings.Angle != 0){
					Rect canvasRect = myCanvas.GetComponent<RectTransform>().rect;
					float radius =  GetComponent<CurvedUISettings>().GetCyllinderRadiusInCanvasSpace();

					for(int i = 0; i < verts.Count; i++){

						Vector3 newpos = verts[i];		
						float theta = (verts[i].x / canvasRect.size.x) * mySettings.Angle * Mathf.Deg2Rad;
						newpos.x = Mathf.Sin(theta) * radius;
						newpos.z += Mathf.Cos(theta) * radius - radius;
						verts[i] = newpos;
					}
				}

				meshie.vertices = verts.ToArray();

				//create triangles drom verts
				List<int> tris = new List<int>();
				for(int i =0; i < verts.Count / 2 - 1; i++){

					//forward tris
					tris.Add( i * 2 + 2);
					tris.Add( i * 2 + 1);
					tris.Add( i * 2 + 0);

					tris.Add( i * 2 + 2);
					tris.Add( i * 2 + 3);
					tris.Add( i * 2 + 1);

		 			//backwards tris - create them if you want a double sided collider
					if(mySettings.CanBeUsedFromBothSides){
						tris.Add( i * 2 + 0);
						tris.Add( i * 2 + 1);
						tris.Add( i * 2 + 2);
						
						tris.Add( i * 2 + 1);
						tris.Add( i * 2 + 3);
						tris.Add( i * 2 + 2);
					}

				}
				meshie.triangles = tris.ToArray();
				meshie.RecalculateNormals();
				mc.sharedMesh = meshie;

				return mc as Collider;
			}
			case CurvedUISettings.CurvedUIMapping.RING :{
				BoxCollider col = this.gameObject.AddComponent<BoxCollider>();
				col.size = new Vector3(mySettings.RingExternalDiameter, mySettings.RingExternalDiameter, 1.0f);
				return col as Collider;
			} 
			default: {
				return null;	//no collider for other cases.
			}
		}

	}
	#endregion 


	#region SUPPORT FUNCTIONS
	/// <summary>
	/// Determine the signed angle between two vectors, with normal 'n'
	/// as the rotation axis.
	/// </summary>
	float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n) 	{
		return Mathf.Atan2(
			Vector3.Dot(n, Vector3.Cross(v1, v2)),
			Vector3.Dot(v1, v2)) * Mathf.Rad2Deg;
	}
	#endregion



	#region PUBLIC

	public void RebuildCollider(){
		cyllinderMidPoint = mySettings.GetCyllinderMidPointInCanvasSpace();
		CreateCollider();
	}

	#endregion 

}
