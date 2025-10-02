using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    public PlayerInput PlayerInput;
    private InputAction movement;
    public Rigidbody2D rb;
    public float cameraMoveForce = 1f;
    public float cameraRBMaxSpeed = 5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created\
    private void Awake()
    {
        PlayerInput = new PlayerInput();
        movement = PlayerInput.Main.Movement;
        rb = GetComponent<Rigidbody2D>();
    }
    void Start()
    {


    }

    public void OnEnable()
    {
        movement.Enable();
    }

    public void OnDisable()
    {
        movement.Disable();
    }
    // Update is called once per frame
    void Update()
    {
        Vector2 moveDir = movement.ReadValue<Vector2>();
        rb.AddForce(moveDir * cameraMoveForce);
    }
}
