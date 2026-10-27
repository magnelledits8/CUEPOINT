using UnityEngine;

[DefaultExecutionOrder(-50)] // this applies after MovingPlatform, before ThirdPersonController
[RequireComponent(typeof(CharacterController))]
public class PlatformRider : MonoBehaviour
{
    private CharacterController controller;
    private Transform currentGround;
    private Vector3 lastGroundPosition;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // 1. What am I standing on?
        Transform ground = null;
        float radius = controller.radius * 0.9f;
        Vector3 origin = transform.position + Vector3.up * (radius + 0.1f);

        if (Physics.SphereCast(origin, radius, Vector3.down, out RaycastHit hit, 0.3f,
                               ~0, QueryTriggerInteraction.Ignore))
        {
            ground = hit.transform;
        }

        // 2. Same ground as last frame, and it moved? Move with it.
        if (ground != null && ground == currentGround)
        {
            Vector3 moved = ground.position - lastGroundPosition;
            if (moved != Vector3.zero)
            {
                transform.position += moved;   // shift the player directly
                Physics.SyncTransforms();      // tell the physics system we moved it
            }
        }

        // 3. Remember for next frame
        currentGround = ground;
        if (ground != null) lastGroundPosition = ground.position;
    }
}