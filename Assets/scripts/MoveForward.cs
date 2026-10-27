using UnityEngine;
using UnityEngine.Rendering;

public class MoveForward : MonoBehaviour
{
    public float speed = 0.1f;
    public float size = 1f;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //I'm setting the size once, when the ball appears
        transform.localScale = new Vector3(size, size, size);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(0, 0, speed);

    }
}
