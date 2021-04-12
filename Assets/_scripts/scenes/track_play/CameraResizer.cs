using UnityEngine;

public class CameraResizer : MonoBehaviour {

    public Camera mainCamera;

    public Vector2 referenceSize;
    public int cameraSize;
    
    void Start() {

        var widthRatio = referenceSize.x / Screen.width;
        var newHeight = Screen.height * widthRatio;

        mainCamera.orthographicSize = cameraSize * newHeight / referenceSize.y;

    }
    
}
