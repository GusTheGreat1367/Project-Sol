using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    public PlayerInput PlayerInput;
    private InputAction movement;
    private InputAction scroll;
    public Rigidbody2D rb;
    public float cameraMoveForce = 1f;
    public float cameraRBMaxSpeed = 5f;
    public float stoppingForce = 2.0f;
    public Camera cam;
    public ParrallaxTest parrallax;
    // Start is called once before the first execution of Update after the MonoBehaviour is created\
    private void Awake()
    {
        PlayerInput = new PlayerInput();
        movement = PlayerInput.Main.Movement;
        scroll = PlayerInput.Main.Scroll;
        rb = GetComponent<Rigidbody2D>();
    }
    void Start()
    {


    }

    public void OnEnable()
    {
        movement.Enable();
        scroll.Enable();
    }

    public void OnDisable()
    {
        movement.Disable();
        scroll.Disable();
    }
    // Update is called once per frame
    void Update()
    {
        Vector2 moveDir = movement.ReadValue<Vector2>();
        float scrollDir = scroll.ReadValue<float>();
        if (scrollDir != 0)
        {
            if (scrollDir < 0 && cam.orthographicSize < 1000)
            {
                cam.orthographicSize -= scrollDir;
            }
            else if (scrollDir > 0 && cam.orthographicSize > 2)
            {
                cam.orthographicSize -= scrollDir;
            }
        }
        if (rb.linearVelocity != Vector2.zero && parrallax != null)
        {
            parrallax.Move(rb.linearVelocity);
        }
        if (moveDir == Vector2.zero)
        {
            rb.linearDamping = stoppingForce;
        }
        else
        {
            rb.linearDamping = 0;
        }
        rb.AddForce(moveDir * cameraMoveForce);
    }
}
