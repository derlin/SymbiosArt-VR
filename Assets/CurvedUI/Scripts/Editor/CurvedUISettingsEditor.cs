using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.UI;

#if CURVEDUI_TMP
using TMPro;
#endif 


[CustomEditor(typeof(CurvedUISettings))]
public class CurvedUISettingsEditor : Editor {

	void Start(){
		AddCurvedUIComponents();
	}
		
	public override void OnInspectorGUI(){
		CurvedUISettings myTarget = (CurvedUISettings)target;

		GUI.changed = false;

		myTarget.Mapping = (CurvedUISettings.CurvedUIMapping)EditorGUILayout.EnumPopup("Mapping", myTarget.Mapping);


		switch(myTarget.Mapping){
			case CurvedUISettings.CurvedUIMapping.CYLLINDER:{
				myTarget.Angle =  EditorGUILayout.IntSlider("Cyllinder Angle", myTarget.Angle, -360, 360);
				myTarget.CanBeUsedFromBothSides =  EditorGUILayout.Toggle("Double Sided Collider", myTarget.CanBeUsedFromBothSides);

				break;
			}
			case CurvedUISettings.CurvedUIMapping.RING:{
				myTarget.RingExternalDiameter = Mathf.Clamp(EditorGUILayout.IntField("External Diameter", myTarget.RingExternalDiameter), 1, 100000);
				myTarget.Angle =  EditorGUILayout.IntSlider("Ring Segment Angle", myTarget.Angle, -360, 360);
				myTarget.RingFill = EditorGUILayout.Slider("Fill", myTarget.RingFill, 0.0f, 1.0f);
				myTarget.RingFlipVertical = EditorGUILayout.Toggle("Flip Canvas Vertically",myTarget.RingFlipVertical);
				break;
			}
//			case CurvedUISettings.CurvedUIMapping.SPHERE:{
//				myTarget.Angle =  EditorGUILayout.IntSlider("Horizontal Angle", myTarget.Angle, 0, 180);
//				myTarget.VerticalAngle =  -EditorGUILayout.IntSlider("Vertical Angle", -myTarget.VerticalAngle, 0, 360);
//				break;
//			}
		}

	
		GUILayout.Space(10);
		myTarget.CurveSegmentsMultiplier = Mathf.Clamp(EditorGUILayout.FloatField("Curve Quality", myTarget.CurveSegmentsMultiplier), 0.1f, 5.0f);

		GUILayout.Space(10);
		if(GUILayout.Button("Update Child Objects"))
		{
			AddCurvedUIComponents();
		}

		if(GUI.changed)
			EditorUtility.SetDirty(myTarget);

	}


	void OnEnable()
	{
		EditorApplication.hierarchyWindowChanged += AddCurvedUIComponents;
	}

	void OnDisable()
	{
		EditorApplication.hierarchyWindowChanged -= AddCurvedUIComponents;
	}

	//Travel the hierarchy and add CurvedUIVertexEffect to every gameobject that can be bent.
	private void AddCurvedUIComponents()
	{
		if(target == null)return;
		
		foreach(UnityEngine.UI.Graphic graph in ((CurvedUISettings)target).GetComponentsInChildren<UnityEngine.UI.Graphic>()){
			if(graph.GetComponent<CurvedUIVertexEffect>() == null){
				graph.gameObject.AddComponent<CurvedUIVertexEffect>();
				graph.SetAllDirty();
			}
		}

		//TextMeshPro experimental support. Go to CurvedUITMP.cs to learn how to enable it.
		#if CURVEDUI_TMP 
		foreach(TextMeshProUGUI tmp in ((CurvedUISettings)target).GetComponentsInChildren<TextMeshProUGUI>()){
			if(tmp.GetComponent<CurvedUITMP>() == null){
				tmp.gameObject.AddComponent<CurvedUITMP>();
				tmp.SetAllDirty();
			}
		}
		#endif

	}

}
