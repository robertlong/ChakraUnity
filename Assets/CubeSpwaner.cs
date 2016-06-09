using UnityEngine;
using System;

public class CubeSpwaner : MonoBehaviour {

	private GameObject cube;
	private Color color = new Color(1, 0, 0, 0.1f);
	private SteamVR_TrackedController controller;

	// Use this for initialization
	void Start () {
		controller = GetComponent<SteamVR_TrackedController>();

        if (controller == null) {
			controller = gameObject.AddComponent<SteamVR_TrackedController>();
        }

		controller.TriggerClicked += new ClickedEventHandler(OnTriggerClicked);
		controller.PadTouched += new ClickedEventHandler(OnPadTouched);

		CreateCube();
	}

	void OnTriggerClicked(object sender, ClickedEventArgs e) {
		CreateCube();
	}

	void OnPadTouched(object sender, ClickedEventArgs e) {
		SetCubeColor((e.padX + 1) / 2, (e.padY + 1) / 2, 0);
	}

	void CreateCube() {
		cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		cube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
	}

	void SetCubeColor(float r, float g, float b) {
		color = new Color(r, g, b, 0.1f);
	}
	
	// Update is called once per frame
	void Update () {
		if (controller != null) {
			Vector3 controllerPos = controller.transform.position + (controller.transform.forward * 0.2f);

    		cube.transform.position = new Vector3((float) Math.Round(controllerPos.x, 1), (float) Math.Round(controllerPos.y, 1), (float) Math.Round(controllerPos.z, 1));

			var material = cube.GetComponent<Renderer>().material;

			if (!material.color.Equals(color)) {
				material.color = color;
			}
    		
		}
	}
}
