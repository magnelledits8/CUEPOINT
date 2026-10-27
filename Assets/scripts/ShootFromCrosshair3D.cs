using UnityEngine;

/// <summary>
/// Casts a ray from the centre of the player camera when left mouse is clicked.
/// Put this on the camera. The 3D target needs a collider and SpiralTarget3DController.
/// </summary>
public class ShootFromCrosshair3D : MonoBehaviour
{
    [SerializeField] private Camera aimCamera;
    [SerializeField, Min(1f)] private float maxDistance = 100f;
    [SerializeField] private LayerMask targetLayers = ~0;

    private void Awake()
    {
        if (aimCamera == null)
            aimCamera = GetComponent<Camera>();
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0) || aimCamera == null)
            return;

        Ray ray = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, targetLayers, QueryTriggerInteraction.Ignore))
        {
            SpiralTarget3DController target = hit.collider.GetComponentInParent<SpiralTarget3DController>();
            if (target != null && !target.TryShoot())
                Debug.Log("Target hit, but it is not at Frame 13 / ready timing yet.", target);
        }
    }
}
