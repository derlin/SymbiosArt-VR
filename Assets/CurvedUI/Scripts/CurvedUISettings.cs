using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent (typeof(Canvas))]
public class CurvedUISettings : MonoBehaviour {

	public enum CurvedUIMapping{
		CYLLINDER = 0,
		RING = 1,
		//SPHERE = 2
	}
			
	//Global settings
	[SerializeField] CurvedUIMapping mapping;
	[SerializeField] float curveSegmentsMultiplier = 1f;

	//Cyllinder
	[SerializeField] int angle = 90;
	[SerializeField] bool canBeUsedFromBothSides = true;

	//Sphere settings
	[SerializeField] int vertAngle = 90;

	//ring settings
	[SerializeField] float ringFill = 0.5f;
	[SerializeField] int ringExternalDiamater = 1000;
	[SerializeField] bool ringFlipVertical = false;

	//internal system settings
	int baseCircleSegments = 48;


	#region LIFECYCLE
	void Start(){

		//lets get rid of any raycasters and add our custom one
		GraphicRaycaster castie = GetComponent<GraphicRaycaster>();

		if(castie != null){
			if(!(castie is CurvedUIRaycaster)){
				Destroy(castie);
				this.gameObject.AddComponent<CurvedUIRaycaster>();
			}
		} else {
			this.gameObject.AddComponent<CurvedUIRaycaster>();
		}
			
	}
	
	#endregion 


	#region PRIVATE

	void SetUIAngle(int newAngle){

		angle =  newAngle;

		foreach(CurvedUIVertexEffect ve in GetComponentsInChildren<CurvedUIVertexEffect>())
			ve.TesselationRequired = true;

		foreach(Graphic graph in GetComponentsInChildren<Graphic>())
			graph.SetVerticesDirty();

		if(Application.isPlaying && GetComponent<CurvedUIRaycaster>() != null)
			//tell raycaster to update its collider now that angle has changed.
			GetComponent<CurvedUIRaycaster>().RebuildCollider();
	}
	#endregion 


	#region PUBLIC

	/// <summary>
	/// Returns the radius of curved canvas cyllinder, expressed in Cavas's local space units.
	/// </summary>
	public float GetCyllinderRadiusInCanvasSpace(){
		return angle == 0 ? 0 :  ((transform as RectTransform).rect.size.x * 0.5f) / Mathf.Sin( Mathf.Clamp(angle,-180.0f, 180.0f) * 0.5f * Mathf.Deg2Rad);
	}

	/// <summary>
	/// Returns the mid point of Canvas cyllinder, in Canvas's local space.
	/// </summary>
	/// <returns>The cyllinder middle point in canvas space.</returns>
	public Vector3 GetCyllinderMidPointInCanvasSpace(){
		return new Vector3(0,0, -GetCyllinderRadiusInCanvasSpace());
	}

	/// <summary>
	/// Tells you how big UI quads can get before they should be tesselate to look good on current canvas settings.
	/// Used by CurvedUIVertexEffect
	/// </summary>
	/// <returns>The tesslation size.</returns>
	public float GetTesslationSize(){

		if(Mapping == CurvedUIMapping.CYLLINDER){

			return angle == 0 ? 10000 : GetComponent<RectTransform>().rect.width / (Mathf.Abs(angle).Remap(0.0f, 360.0f) * baseCircleSegments * Mathf.Clamp(CurveSegmentsMultiplier, 0.001f, 1000.0f));
		
		}else if (Mapping == CurvedUIMapping.RING){

			return GetComponent<RectTransform>().rect.width / baseCircleSegments;
				
		} else {
			return 50.0f;

		}
		
	}


	/// <summary>
	/// Tells you how many segmetens should the entire 360 deg. cyllinder consist of.
	/// Used by CurvedUIVertexEffect
	/// </summary>
	/// <value>The base circle segments.</value>
	public int BaseCircleSegments {
		get { return baseCircleSegments;
		}
	}

	/// <summary>
	/// The measure of the arc of the Canvas.
	/// </summary>
	/// <value>The angle.</value>
	public int Angle{
		get {return angle;}
		set { if(angle != value)
				SetUIAngle(value);}

	}


	/// <summary>
	/// Is the canvas clickable from both the front and the back?
	/// </summary>
	/// <value><c>true</c> if this instance can be used from both sides; otherwise, <c>false</c>.</value>
	public bool CanBeUsedFromBothSides{
		get {return canBeUsedFromBothSides;}
		set { if(canBeUsedFromBothSides != value){ 	
				canBeUsedFromBothSides = value;
				SetUIAngle(angle);
			}
		}
	}

	public float CurveSegmentsMultiplier{
		get {return curveSegmentsMultiplier;}
		set { curveSegmentsMultiplier = value;
		}
	}

	public CurvedUIMapping Mapping {
		get { return mapping; }
		set { 
			if(mapping != value){
				mapping = value; 
				SetUIAngle(angle);
			}
		}
	}


	public int VerticalAngle {
		get { return vertAngle; }
		set { 
			if(vertAngle != value){
				vertAngle = value;
				SetUIAngle(angle);
			}
		}
	}

	public float RingFill {
		get { return ringFill; }
		set { 
			if(ringFill != value){
				ringFill = value;
				SetUIAngle(angle);
			}
		}
	}

	public int RingExternalDiameter {
		get { return ringExternalDiamater; }
		set {if(ringExternalDiamater != value){ 
				ringExternalDiamater = value;
				SetUIAngle(angle);
			}
		}
	}

	public bool RingFlipVertical {
		get { return ringFlipVertical; }
		set { if(ringFlipVertical != value){ 
				ringFlipVertical = value;
				SetUIAngle(angle);
			}
		}
	}
	#endregion
}
