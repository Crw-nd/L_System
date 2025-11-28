using UnityEngine;

public class CameraZoomPresets : MonoBehaviour
{

    [Header("Preset Positions")]
    public Vector3 presetPosition1 = new Vector3(0,72,128);
    public Vector3 presetPosition2 = new Vector3(0, 21, 45);
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();

        if (cam == null)
        {
            Debug.LogError("CameraZoomPresets must be attached to a Camera!");
        }
    }

    void Update()
    {

        // PRESET 1
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            transform.position = presetPosition1;
            Debug.Log("Camera moved to Preset 1");
        }

        // PRESET 2
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            transform.position = presetPosition2;
            Debug.Log("Camera moved to Preset 2");
        }
    }
}
