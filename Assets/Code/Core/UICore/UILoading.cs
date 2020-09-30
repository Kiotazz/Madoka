using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UILoading : MonoBehaviour 
{
    public Image img;
    public float speed = 1f;
    float step;

	public void Init() 
	{
        //Effect.Create("fx_ui_loading", (effect) => {
        //    effect.transform.parent = transform;
        //    effect.transform.localPosition = Vector3.zero;
        //    //effect.transform.localScale = Vector3.one;
        //});
        step = -360 / 25f;
	}

    public void DoUpdate(float deltaTime)
    {
        int value = (int)(speed * Time.realtimeSinceStartup);
        Vector3 rot = img.transform.localEulerAngles;
        rot.z = value * step;
        img.transform.localEulerAngles = rot;
    }

}
