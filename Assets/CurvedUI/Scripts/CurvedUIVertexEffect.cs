using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;




#if UNITY_5_1
///Pre 5.2 Unity uses BaseVertexEffect class, works on quads.
public partial class CurvedUIVertexEffect : BaseVertexEffect {

	public override void ModifyVertices(List<UIVertex> verts) { 
	if (!this.IsActive() || mySettings == null)
	return;

	ModifyVerts(verts);
}
#else
///Post 5.2 Unity uses BaseMeshEffect class, which works on triangles. 
///We need to convert those to quads to be used with tesselation routine.
///TODO: Write a separate subdivision based on triangles to skip the convertion to quads.
public partial class CurvedUIVertexEffect : BaseMeshEffect {

	public override void ModifyMesh (VertexHelper vh)
	{
		if (!this.IsActive() || mySettings == null)
			return;

		//Get vertices from the vertex stream. These come as triangles.
		List<UIVertex> list = new List<UIVertex>();
		vh.GetUIVertexStream(list);

		// calls the old ModifyVertices which was used on pre 5.2. 
		ModifyVerts(list); 

		vh.Clear();

		//Add our vertices back to vertex stream. They can come as quads or as triangles.
		if(list.Count % 4 == 0){
			for(int i =0; i < list.Count; i+=4){
				vh.AddUIVertexQuad(new UIVertex[]{
					list[i + 0], list[i + 1], list[i + 2], list[i + 3],
				});
			}
		} else {
			vh.AddUIVertexTriangleStream(list);
		}

	}
#endif

	//public variables
	bool tesselationRequired = true;

	//saved variables and references
	float tessellatedSize = 50.0f;
	float angle = 90;
	bool parentTransformMisaligned = false;
	Canvas myCanvas;
	CurvedUISettings mySettings;
	List<UIVertex> savedVertices;
	Vector2 lastTesselationRectSize;
	Vector3 lastTesselationRotation;

		
	#region LIFECYCLE

	protected override void OnEnable(){
		//find the settings object and its canvas.
		if(mySettings == null){ 
			mySettings = GetComponentInParent<CurvedUISettings>();

			if(mySettings == null)return;

			myCanvas = mySettings.GetComponent<Canvas>();
			angle = mySettings.Angle;
		}

		//If there is an update to the graphic, we cant reuse old vertices, so new tesselation will be required
		//TODO: Try tesselating only the vertex color data for Images.
		if(GetComponent<Graphic>())
			GetComponent<Graphic>().RegisterDirtyMaterialCallback(TesselationRequiredCallback);

		if(GetComponent<Text>())
			GetComponent<Text>().RegisterDirtyVerticesCallback(TesselationRequiredCallback);

		this.SetVerticesDirty();
	}
		
	protected override void OnDisable(){
		//If there is an update to the graphic, we cant reuse old vertices, so new tesselation will be required
		if(GetComponent<Graphic>())
			GetComponent<Graphic>().UnregisterDirtyMaterialCallback(TesselationRequiredCallback);

		if(GetComponent<Text>())
			GetComponent<Text>().UnregisterDirtyVerticesCallback(TesselationRequiredCallback);

		this.SetVerticesDirty();
	}
		
	void TesselationRequiredCallback(){
		tesselationRequired = true;
	}


	void Update(){



		//recalculate vertices' positions if there was a change in transform.
		if(transform.hasChanged){
			SetVerticesDirty();
			transform.hasChanged = false;

			//Find if the change in transform requires us to retesselate the UI
			if(!tesselationRequired){ // do not perform tesselation required check if we already know it is, god damnit!

				if((transform as RectTransform).rect.size != lastTesselationRectSize){ 
					//the size of this RectTransform has changed, we have to tesselate again! =(
					tesselationRequired = true;
				}

				if(((lastTesselationRotation == Vector3.zero || transform.localEulerAngles == Vector3.zero) && transform.localEulerAngles != lastTesselationRotation)){ // the rotation of this transform changed from zero to any other angle. We need to tesselate x and y directions now!
					
					// This transform has changed rotation and is no longer aligned with local x axis. We need to tesselate in y axis as well.
					// Tell children that they need to retesselate as well.
					foreach(CurvedUIVertexEffect eff in GetComponentsInChildren<CurvedUIVertexEffect>()){
						eff.TesselationRequired = true;
					}
					tesselationRequired = true;
				}	
			}
		}
	}

	#endregion

	void ModifyVerts(List<UIVertex> verts) {

		if(verts == null || verts.Count == 0)return; 

		//tesselate the vertices if needed and save them to a list,
		//so we don't have to retesselate if RectTransform's size has not changed.
		if(tesselationRequired || !Application.isPlaying){
			tessellatedSize = mySettings.GetTesslationSize();

			parentTransformMisaligned = false;
			Transform trans = transform.parent;
			while(trans != null && trans != myCanvas.transform){
				if(trans.localEulerAngles != Vector3.zero){
					parentTransformMisaligned = true;
					break;
				} else 
 					trans = trans.parent;
			}


#if !UNITY_5_1 /// Convert the list from triangles to quads to be used by the tesselation

			List<UIVertex> vertsInQuads = new List<UIVertex>();
			int vertsInTrisCount = verts.Count;
			for(int i = 0; i < verts.Count; i+=6){
				// Get four corners from two triangles. Basic UI always comes in quads anyway.
				vertsInQuads.Add(verts[i + 0]);
				vertsInQuads.Add(verts[i + 1]);
				vertsInQuads.Add(verts[i + 2]);
				vertsInQuads.Add(verts[i + 4]);
			}
			//add quads to the list and remove the triangles
			verts.AddRange(vertsInQuads);
			verts.RemoveRange(0, vertsInTrisCount);
#endif

			// Tesselate quads and apply transformation
			int startingVertexCount = verts.Count;
			for (int i = 0; i < startingVertexCount; i += 4)
				ModifyQuad(verts, i);
			
			// Remove old quads
			verts.RemoveRange(0, startingVertexCount);

			// Save the tesselated vertices, so if the size does not change,
			// we can use them when redrawing vertices.
			savedVertices = new List<UIVertex>();
			savedVertices.AddRange(verts);

			//save the transform properties we last tesselated for.
			lastTesselationRotation = transform.localEulerAngles;
			lastTesselationRectSize = (transform as RectTransform).rect.size;

			tesselationRequired = false;
		}


		// Lets map the vertices to an arc! Magic happens here!
		if(mySettings.Angle != 0){

			angle = mySettings.Angle;
			int initialCount = verts.Count;
			
			if(savedVertices != null){ // use saved verts if we have those

				for(int i = 0; i <savedVertices.Count; i++){
						verts.Add(CurveVertex(savedVertices[i], angle, mySettings.GetCyllinderRadiusInCanvasSpace(), myCanvas.GetComponent<RectTransform>().rect.size));
				}
				verts.RemoveRange(0, initialCount);

			} else { // or just the mesh's vertices if we do not

				for(int i = 0; i < initialCount; i++){
					verts.Add(CurveVertex(verts[i], angle, mySettings.GetCyllinderRadiusInCanvasSpace(), myCanvas.GetComponent<RectTransform>().rect.size));
				}
				verts.RemoveRange(0, initialCount);

			}
		}		
	}


	void ModifyQuad(List<UIVertex> verts, int vertexIndex) {

		// Read the existing quad vertices
		List<UIVertex> quad = new List<UIVertex>();
		for(int i = 0; i < 4; i++)
			quad.Add(verts[vertexIndex + i]);

		// horizotal and vertical directions of a quad. We're going to tesselate parallel to these.
		Vector3 horizontalDir = quad[2].position - quad[1].position;
		Vector3 verticalDir = quad[1].position - quad[0].position;

		// Find how many quads we need to create
		float tessSize = 1.0f / Mathf.Max(1.0f, tessellatedSize);
		int horizontalQuads = Mathf.CeilToInt(horizontalDir.magnitude * tessSize);
		int verticalQuads = 1;

		// Tesselate vertically only if the recttransform (or parent) is rotated
		// This cuts down the time needed to tesselate by 90% in some cases.
		if(transform.localEulerAngles != Vector3.zero || parentTransformMisaligned /*|| mySettings.Mapping == CurvedUISettings.CurvedUIMapping.SPHERE*/)
			verticalQuads = Mathf.CeilToInt(verticalDir.magnitude * tessSize);

		// Create the quads!
		float yStart = 0.0f;
		for (int y = 0; y < verticalQuads; ++y) {

			float yEnd = (y + 1.0f) / verticalQuads;
			float xStart = 0.0f;

			for (int x = 0; x < horizontalQuads; ++x) {
				float xEnd = (x + 1.0f) / horizontalQuads;

				//Add new quads to list
				verts.Add(TesselateQuad(quad, xStart, yStart));
				verts.Add(TesselateQuad(quad, xStart, yEnd));
				verts.Add(TesselateQuad(quad, xEnd, yEnd));
				verts.Add(TesselateQuad(quad, xEnd, yStart));

				//begin the next quad where we ened this one
				xStart = xEnd;
			}
			//begin the next row where we ended this one
			yStart = yEnd;
		}
	}

	

	#region TESSELATION

	UIVertex TesselateQuad(List<UIVertex> quad, float x, float y){

			UIVertex ret;
			
			//1. calculate weighting factors
			List<float> weights = new List<float>(){
				(1-x) * (1-y),
				(1-x) * y,
				x * y,
				x * (1-y),
			};
			
			//2. interpolate all the vertex properties using weighting factors
			Vector2 uv0 = Vector2.zero, uv1 = Vector2.zero;
			Vector3 pos = Vector3.zero;

			for(int i =0; i < 4; i++){
				uv0 += quad[i].uv0 * weights[i];
				uv1 += quad[i].uv1 * weights[i];
				pos += quad[i].position * weights[i];
				//normal += quad[i].normal * weights[i]; // normals should be recalculated to take the curve into account;
				//tan += quad[i].tangent * weights[i]; // tangents should be recalculated to take the curve into account;
			}

			//4. return output
			ret.position = pos;
			ret.color = quad[0].color;
			ret.uv0 = uv0;
			ret.uv1 = uv1;
			ret.normal = quad[0].normal;
			ret.tangent  = quad[0].tangent;
			
			return ret;
	}
	#endregion

	#region CURVING
	/// <summary>
	/// Map position of a vertex to a section of a circle. calculated in canvas's local space
	/// </summary>
	UIVertex CurveVertex(UIVertex input, float cylinder_angle, float radius, Vector2 canvasSize){

		Vector3 pos = input.position;

		//calculated in canvas local space version:
		pos = myCanvas.transform.worldToLocalMatrix.MultiplyPoint3x4(transform.localToWorldMatrix.MultiplyPoint3x4(pos));	

		if(mySettings.Mapping == CurvedUISettings.CurvedUIMapping.CYLLINDER){
			
			float theta = (pos.x / canvasSize.x) * cylinder_angle * Mathf.Deg2Rad;
			radius += pos.z; // change the radius depending on how far the element is moved in z direction from canvas plane
			pos.x = Mathf.Sin(theta) * radius;
			pos.z += Mathf.Cos(theta) * radius - radius;

		} else if (mySettings.Mapping == CurvedUISettings.CurvedUIMapping.RING){
			
			float angleOffset = 0;
			float r = pos.y.Remap(canvasSize.y * 0.5f * (mySettings.RingFlipVertical ? 1 : -1), -canvasSize.y * 0.5f * (mySettings.RingFlipVertical ? 1 : -1), mySettings.RingExternalDiameter * (1 - mySettings.RingFill) * 0.5f, mySettings.RingExternalDiameter * 0.5f);
			float theta = (pos.x / canvasSize.x).Remap(-0.5f,0.5f, Mathf.PI / 2.0f, cylinder_angle * Mathf.Deg2Rad + Mathf.PI / 2.0f) - angleOffset;
			pos.x = r * Mathf.Cos(theta);
			pos.y = r * Mathf.Sin(theta);

		} 
//		else if(mySettings.Mapping == CurvedUISettings.CurvedUIMapping.SPHERE){
//
//			float theta = (pos.x / canvasSize.x).Remap(-0.5f,0.5f, (180 - cylinder_angle) / 2.0f , 180 - (180 - cylinder_angle) / 2.0f);
//			theta *= Mathf.Deg2Rad;
//			float gamma = (pos.y / canvasSize.y).Remap(-0.5f,0.5f, 0, mySettings.VerticalAngle * Mathf.Deg2Rad);
//			radius = canvasSize.x + pos.z;
//			pos.x = Mathf.Sin(theta) * Mathf.Cos(gamma) * radius;
//			pos.z = radius * Mathf.Cos(theta);
//			pos.y = Mathf.Sin(theta) * Mathf.Sin(gamma) * - radius;
//
//			//Debug.Log("sin theta: "+Mathf.Sin(theta) + " Cos gamma:" + Mathf.Cos(gamma) + " rad:"+radius);
//		}
		//4. write output
		input.position = transform.worldToLocalMatrix.MultiplyPoint3x4(myCanvas.transform.localToWorldMatrix.MultiplyPoint3x4(pos));
		
		return input;
	}
	#endregion

	protected void SetVerticesDirty() 	{
		foreach (Graphic gph in this.GetComponentsInChildren<Graphic>())
	 		gph.SetVerticesDirty();
	
	}

	public bool TesselationRequired{
		get { return tesselationRequired;}
		set { tesselationRequired = value; } 
	}
	
}


#region EXTENSION METHODS
public static class CalculationMethods {

	public static float Remap (this float value, float from1, float to1, float from2, float to2) {
		return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
	}
	
	public static float Remap (this float value, float from1, float to1) {
		return Remap(value, from1, to1, 0, 1);
	}
	
	public static float Remap (this int value, float from1, float to1, float from2, float to2) {
		return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
	}
	
	public static double Remap (this double value, double from1, double to1, double from2, double to2) {
		return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
	}
	
	public static float Clamp(this float value, float min, float max){
		return Mathf.Clamp(value, min, max);
	}
	
	public static float Clamp(this int value, int min, int max){
		return Mathf.Clamp(value, min, max);
	}
	
	
	/// <summary>
	/// Returns value rounded to nearest integer (both up and down).
	/// </summary>
	/// <returns>The int.</returns>
	/// <param name="value">Value.</param>
	public static int ToInt(this float value){
		return Mathf.RoundToInt(value);
	}
	
	public static int FloorToInt(this float value){
		return Mathf.FloorToInt(value);
	}
	
	public static int CeilToInt(this float value){
		return Mathf.FloorToInt(value);
	}

	public static Vector3 ModifyX(this Vector3 trans, float newVal){
		trans = new Vector3(newVal, trans.y, trans.z);
		return trans;
	}
	
	public static Vector3 ModifyY(this Vector3 trans, float newVal){
		trans = new Vector3(trans.x, newVal, trans.z);
		return trans;
	}
	
	public static Vector3 ModifyZ(this Vector3 trans, float newVal){
		trans = new Vector3(trans.x, trans.y, newVal);
		return trans;
	}

	
	/// <summary>
	/// Resets transform's local position, rotation and scale
	/// </summary>
	/// <param name="trans">Trans.</param>
	public static void ResetTransform(this Transform trans){
		trans.localPosition = Vector3.zero;
		trans.localRotation  = new Quaternion(0,0,0,0);
		trans.localScale = Vector3.one;
	}
	
	#endregion 
}	
