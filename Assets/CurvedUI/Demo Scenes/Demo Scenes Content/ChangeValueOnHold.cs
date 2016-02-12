using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChangeValueOnHold : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, ISubmitHandler  {

	bool pressed = false;

	[SerializeField] Image bg;
	[SerializeField] Color SelectedColor;
	[SerializeField] Color NormalColor ;

	[SerializeField] CanvasGroup IntroCG;
	[SerializeField] CanvasGroup MenuCG;

	// Update is called once per frame
	void Update () {
		ChangeVal();
	}

	void ChangeVal(){

		if(this.GetComponent<Slider>().normalizedValue == 1){
			IntroCG.alpha -=  Time.deltaTime;
			MenuCG.alpha +=  Time.deltaTime;
		} else {
			this.GetComponent<Slider>().normalizedValue += pressed ?  Time.deltaTime :  -Time.deltaTime;
		}
	}

	public void OnPointerDown(PointerEventData data){
		pressed = true;
	}

	public void OnPointerUp(PointerEventData data){
		pressed = false;
	}

	public void OnPointerEnter(PointerEventData data){
		bg.color = SelectedColor;
		bg.GetComponent<CurvedUIVertexEffect>().TesselationRequired = true;
	}

	public void OnPointerExit(PointerEventData data){
		bg.color = NormalColor;
		bg.GetComponent<CurvedUIVertexEffect>().TesselationRequired = true;
	}

	public void OnSubmit(BaseEventData data){
		pressed = true;
	}
}
