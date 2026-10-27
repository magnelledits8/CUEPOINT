using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerMove : MonoBehaviour
{
    public float sprintSpeed = 7f;

    // I'm only watching this one. The code sets it every frame.
    public float currentSpeed;

    public float moveSpeed = 0.05f;

    void Update()
    {
        // At the bottom of Update, after the movement lines:

        // I walked off the far edge, so I'm going back to the start
        if (transform.position.z > 20)
        {
            transform.position = new Vector3(0, 1, 0);
        }



        // First, I'm deciding how fast to go this frame
        if (Keyboard.current.leftShiftKey.isPressed)
        {
            currentSpeed = sprintSpeed;
        }
        else
        {
            currentSpeed = moveSpeed;
        }


        // Then I'm working out how far to move this frame
        float step = currentSpeed * Time.deltaTime;

        if (Keyboard.current.wKey.isPressed) { transform.Translate(0, 0, step); }
        if (Keyboard.current.sKey.isPressed) { transform.Translate(0, 0, -step); }
        if (Keyboard.current.aKey.isPressed) { transform.Translate(-step, 0, 0); }
        if (Keyboard.current.dKey.isPressed) { transform.Translate(step, 0, 0); }
    }

}



