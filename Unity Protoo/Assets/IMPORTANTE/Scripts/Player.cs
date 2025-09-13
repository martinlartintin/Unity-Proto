using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public float speed = 5f;
    private Vector2 moveInput;

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void Update()
    {
        Vector3 dir = new Vector3(moveInput.x, 0f, moveInput.y);
        transform.Translate(dir * speed * Time.deltaTime, Space.World);
    }
}
