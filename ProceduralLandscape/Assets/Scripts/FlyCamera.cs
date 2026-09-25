using UnityEngine;

// WASD/QE to move, mouse to look, Shift to accelerate
public class FlyCamera : MonoBehaviour
{
    public float moveSpeed = 50f;
    public float fastMultiplier = 3f;
    public float mouseSensitivity = 2f;

    private float pitch;
    private float yaw;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, -89f, 89f);
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? fastMultiplier : 1f);
        Vector3 move = Vector3.zero;
        move += transform.forward * Input.GetAxisRaw("Vertical");
        move += transform.right * Input.GetAxisRaw("Horizontal");
        move += Vector3.up * (Input.GetKey(KeyCode.E) ? 1f : Input.GetKey(KeyCode.Q) ? -1f : 0f);
        transform.position += move.normalized * speed * Time.deltaTime;
    }
}
