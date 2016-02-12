using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

#if CURVEDUI_TMP 
using TMPro;
#endif 

//To use this class you have to add CURVEDUI_TMP to your define symbols. You can do it in project settings.
//To learn how to do it visit http://docs.unity3d.com/Manual/PlatformDependentCompilation.html and search for "Platform Custom Defines"

[ExecuteInEditMode]
public class CurvedUITMP : MonoBehaviour {

	#if CURVEDUI_TMP

	//internal
	CurvedUIVertexEffect crvdVE;
	TextMeshProUGUI tmp;
	Mesh savedMesh;
	VertexHelper vh;

	Vector2 savedSize;
	Vector3 savedRotation;
	Vector3 savedPos;


	public bool Dirty = false; // set this to true to force mesh update.

	void Start(){
		if(tmp == null && this.GetComponent<TextMeshProUGUI>() != null ){
			tmp =  this.gameObject.GetComponent<TextMeshProUGUI>();
			crvdVE = this.gameObject.GetComponent<CurvedUIVertexEffect>();
			transform.hasChanged = false;
		}

	}
		

	void Update(){
		//Edit Mesh on TextMeshPro component
		if(tmp != null){

			if(tmp.havePropertiesChanged){
				Dirty = true;
			}
				
			if(Dirty || savedPos != transform.position  || transform.hasChanged || savedSize == null || savedSize != (transform as RectTransform).rect.size || savedRotation == null || savedRotation != transform.eulerAngles)
			{
				Dirty = false;
				transform.hasChanged = false;
				savedSize = (transform as RectTransform).rect.size;
				savedRotation = transform.eulerAngles;
				savedPos = transform.position;

				tmp.ForceMeshUpdate();

				savedMesh = new Mesh();
				savedMesh.vertices = tmp.mesh.vertices;
				savedMesh.uv = tmp.mesh.uv;
				savedMesh.uv2 = tmp.mesh.uv2;
				savedMesh.bounds = tmp.mesh.bounds;
				savedMesh.colors = tmp.mesh.colors;
				savedMesh.triangles = tmp.mesh.triangles;
				savedMesh.normals = tmp.mesh.normals;
				savedMesh.tangents = tmp.mesh.tangents;

				crvdVE.TesselationRequired = true;

				vh = new VertexHelper(savedMesh);

				#if UNITY_5_1
				crvdVE.ModifyMesh(vh.GetUIVertexStream);
				#else 
				crvdVE.ModifyMesh(vh);
				#endif

				vh.FillMesh(savedMesh);
				vh.Dispose();
			}

			tmp.canvasRenderer.SetMesh(savedMesh);
		}
	}

	#endif
}



