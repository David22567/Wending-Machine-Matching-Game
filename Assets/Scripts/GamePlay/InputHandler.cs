using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    private Camera cam;
    private InputSystem_Actions input;

    private void Awake()
    {
        cam = Camera.main;
        input = new InputSystem_Actions();
        input.Player.Attack.performed += OnAttack;
    }

    private void OnEnable()
    {
        input.Enable();
    }

    private void OnDisable()
    {
        input.Disable();
    }

    private void OnAttack(InputAction.CallbackContext ctx)
    {
        Vector2 screenPos = Pointer.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Item item = hit.collider.GetComponent<Item>();
            if (item != null)
                item.OnTapped();
        }
    }
}
