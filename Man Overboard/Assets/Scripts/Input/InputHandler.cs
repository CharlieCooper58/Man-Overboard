using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    PlayerActions inputActions;

    public Vector2 lookVector { get; private set; }
    public Vector2 moveVector { get; private set; }
    private void OnEnable()
    {
        if(inputActions == null)
        {
            inputActions = new PlayerActions();
            SetBindings();
        }
        inputActions.Enable();
        
    }
    private void OnDisable()
    {
        if(inputActions != null)
        {
            inputActions.Disable();
        }
    }
    public void DeactivateInput()
    {
        if(inputActions != null)
        {
            inputActions.Dispose();
            enabled = false;
        }
    }

    void SetBindings()
    {
        inputActions.Movement.Look.performed += x => lookVector = x.ReadValue<Vector2>();
    }

}
