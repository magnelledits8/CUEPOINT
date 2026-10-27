using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]   // makes sure this applies before any other movement scripts
public class MovingPlatform : MonoBehaviour
{
    public Vector3 direction = Vector3.right;
    public float speed = 2f;
    public float distance = 5f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // 1. Remember where we were
        Vector3 oldPosition = transform.position;

        // 2. Move the platform (back and forth)
        float t = Mathf.PingPong(Time.time * speed, distance);
        transform.position = startPosition + direction.normalized * t;
    }


}
