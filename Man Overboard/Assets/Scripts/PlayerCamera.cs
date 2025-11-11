using UnityEngine;
using Unity.Cinemachine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] float cameraSensitivityHorizontal;
    [SerializeField] float cameraHorizontalRotationClamp;

    // Camera's y-axis rotation when the player is seated
    float targetCameraYAxisRotation;

    // Rotation that the player's transform should attempt; used for turning the player as the camera looks around while not rowing
    float targetPlayerRotation;

    [SerializeField] float cameraSensitivityVertical;
    [SerializeField] float cameraUpperVerticalClamp;
    [SerializeField] float cameraLowerVerticalClamp;
    float targetCameraXAxisRotation;

    float playerRotationAngularVelocity;
    float cameraYAngularVelocity;
    float cameraXAngularVelocity;

    Vector3 playerEulers;
    Vector3 cameraEulers;

    [SerializeField] float cameraSmoothingHorizontal;
    [SerializeField] float cameraSmoothingVertical;

    [SerializeField] Transform cameraPivot;
    [SerializeField] CinemachineCamera _camera;
    public void Initialize()
    {
        targetPlayerRotation = transform.localEulerAngles.y;
        playerEulers = transform.eulerAngles;
    }
    public void DeactivateCamera() 
    {
        Destroy(_camera.gameObject);
        this.enabled = false;
    }

    public void HandleCameraMovement(Vector2 look, bool isRowing)
    {
        float lookX = (cameraSensitivityHorizontal * 100) * look.x * Time.deltaTime;
        float lookY = (cameraSensitivityVertical * 100) * look.y * Time.deltaTime;
        // This is a bit messed up, but upward rotation is negative and downward rotation is positive, so I'm keeping this clamp order
        targetCameraXAxisRotation = Mathf.Clamp(targetCameraXAxisRotation - lookY, cameraUpperVerticalClamp, cameraLowerVerticalClamp);

        if (isRowing)
        {
            targetCameraYAxisRotation = Mathf.Clamp(targetCameraYAxisRotation + lookX, -cameraHorizontalRotationClamp, cameraHorizontalRotationClamp);
            targetPlayerRotation = playerEulers.y;
        }
        else
        {
            targetCameraYAxisRotation = 0;
            targetPlayerRotation += lookX;
            playerEulers.y = Mathf.SmoothDampAngle(playerEulers.y, targetPlayerRotation, ref playerRotationAngularVelocity, cameraSmoothingHorizontal);
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, playerEulers.y, transform.eulerAngles.z);
        }
        
        cameraEulers.x = Mathf.SmoothDampAngle(cameraEulers.x, targetCameraXAxisRotation, ref cameraXAngularVelocity, cameraSmoothingVertical);
        cameraEulers.y = Mathf.SmoothDampAngle(cameraEulers.y, targetCameraYAxisRotation, ref cameraYAngularVelocity, cameraSmoothingHorizontal);
        cameraPivot.localRotation = Quaternion.Euler(cameraEulers.x, cameraEulers.y, 0);
    }
}
