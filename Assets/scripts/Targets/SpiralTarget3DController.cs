using System.Collections;
using UnityEngine;

public class SpiralTarget3DController : MonoBehaviour
{
    private const int IncomingIndex = 0;
    private const int FirstChargeIndex = 1;
    private const int ReadyIndex = 12;
    private const int FirstHitIndex = 13;

    [Header("2D animated core (child object)")]
    [SerializeField] private SpriteRenderer coreRenderer;
    [SerializeField] private Sprite[] frames = new Sprite[26];

    [Header("Movement in 3D space")]
    [SerializeField] private Transform destination;
    [SerializeField, Min(0f)] private float moveDuration = 0.5f;
    [SerializeField] private bool beginOnStart = true;

    [Header("Animation timing")]
    [SerializeField, Min(0.05f)] private float chargeDuration = 0.9f;
    [SerializeField, Min(1f)] private float hitFramesPerSecond = 24f;
    [Tooltip("0 = hold Frame 13 until hit. Set a small value later if misses should time out.")]
    [SerializeField, Min(0f)] private float readyHoldSeconds = 0f;
    [SerializeField, Min(0.01f)] private float missFadeDuration = 0.15f;

    [Header("3D outer ring")]
    [Tooltip("Optional parent whose direct children are separate 3D ring segments.")]
    [SerializeField] private Transform ringSegmentsRoot;
    [Tooltip("Optional whole-ring object to hide when the ring segments explode.")]
    [SerializeField] private GameObject wholeRingVisual;
    [SerializeField, Min(0.1f)] private float fragmentExplosionForce = 3.5f;
    [SerializeField, Min(0.1f)] private float fragmentUpwardForce = 0.5f;
    [SerializeField, Min(0.1f)] private float fragmentLifetime = 2.0f;
    [SerializeField] private bool fragmentsUseGravity = false;

    [Header("Hit detection")]
    [Tooltip("A collider on the target's root/core used to register hits.")]
    [SerializeField] private Collider targetHitCollider;

    private enum TargetState { Waiting, Incoming, Charging, Ready, Hit, Missed, Finished }
    private TargetState state = TargetState.Waiting;
    private Coroutine sequenceRoutine;
    private Color originalCoreColor = Color.white;

    private void Awake()
    {
        if (coreRenderer == null)
            coreRenderer = GetComponentInChildren<SpriteRenderer>(true);

        if (targetHitCollider == null)
            targetHitCollider = GetComponent<Collider>();

        if (coreRenderer != null)
            originalCoreColor = coreRenderer.color;

        if (frames == null || frames.Length < 26)
            Debug.LogWarning($"{name}: Assign all 26 frames in numerical order (Frame_01 to Frame_26).", this);

        SetFrame(IncomingIndex);
    }

    private void Start()
    {
        if (beginOnStart)
            BeginSequence();
    }

    public void BeginSequence()
    {
        if (sequenceRoutine != null)
            StopCoroutine(sequenceRoutine);
        sequenceRoutine = StartCoroutine(ArrivalAndChargeRoutine());
    }

    private IEnumerator ArrivalAndChargeRoutine()
    {
        state = TargetState.Incoming;
        SetFrame(IncomingIndex);
        SetHitColliderEnabled(true);

        if (destination != null && moveDuration > 0f)
        {
            Vector3 start = transform.position;
            Vector3 end = destination.position;
            float elapsed = 0f;
            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / moveDuration));
                transform.position = Vector3.LerpUnclamped(start, end, t);
                yield return null;
            }
            transform.position = end;
        }

        state = TargetState.Charging;
        float chargeElapsed = 0f;
        SetFrame(FirstChargeIndex);
        while (chargeElapsed < chargeDuration)
        {
            chargeElapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(chargeElapsed / chargeDuration);
            float eased = Mathf.Pow(progress, 1.35f);
            int index = FirstChargeIndex + Mathf.RoundToInt(eased * (ReadyIndex - FirstChargeIndex));
            SetFrame(Mathf.Clamp(index, FirstChargeIndex, ReadyIndex));
            yield return null;
        }

        SetFrame(ReadyIndex);
        state = TargetState.Ready;

        if (readyHoldSeconds > 0f)
        {
            float waited = 0f;
            while (state == TargetState.Ready && waited < readyHoldSeconds)
            {
                waited += Time.deltaTime;
                yield return null;
            }
            if (state == TargetState.Ready)
            {
                state = TargetState.Missed;
                SetHitColliderEnabled(false);
                yield return FadeCoreAndFinish();
            }
        }
    }

    public bool TryShoot()
    {
        if (state != TargetState.Ready)
            return false;

        state = TargetState.Hit;
        SetHitColliderEnabled(false);
        if (sequenceRoutine != null)
            StopCoroutine(sequenceRoutine);

        ExplodeOuterRing();
        sequenceRoutine = StartCoroutine(HitAnimationRoutine());
        return true;
    }

    private IEnumerator HitAnimationRoutine()
    {
        float delay = 1f / Mathf.Max(1f, hitFramesPerSecond);
        int lastIndex = frames == null ? 0 : Mathf.Min(frames.Length, 26);

        for (int i = FirstHitIndex; i < lastIndex; i++)
        {
            SetFrame(i);
            yield return new WaitForSeconds(delay); // Helt tidsbaserad frame-växling, stabil i alla FPS
        }

        state = TargetState.Finished;
    }

    private void ExplodeOuterRing()
    {
        if (wholeRingVisual != null)
        {
            if (coreRenderer != null && coreRenderer.transform.IsChildOf(wholeRingVisual.transform))
            {
                coreRenderer.transform.SetParent(transform, true);
            }
            wholeRingVisual.SetActive(false);
        }

        if (ringSegmentsRoot == null)
            return;

        for (int i = ringSegmentsRoot.childCount - 1; i >= 0; i--)
        {
            Transform segment = ringSegmentsRoot.GetChild(i);

            if (coreRenderer != null && segment == coreRenderer.transform)
                continue;

            segment.SetParent(null, true);

            Collider segmentCollider = segment.GetComponent<Collider>();
            if (segmentCollider == null)
                segmentCollider = segment.gameObject.AddComponent<BoxCollider>();

            MeshCollider meshCollider = segmentCollider as MeshCollider;
            if (meshCollider != null)
                meshCollider.convex = true;

            Rigidbody body = segment.GetComponent<Rigidbody>();
            if (body == null)
                body = segment.gameObject.AddComponent<Rigidbody>();

            body.useGravity = fragmentsUseGravity;
            body.mass = 0.2f;
            Vector3 direction = (segment.position - transform.position).normalized;
            if (direction.sqrMagnitude < 0.01f)
                direction = Random.onUnitSphere;
            direction += Vector3.up * fragmentUpwardForce;
            body.AddForce(direction.normalized * fragmentExplosionForce, ForceMode.Impulse);
            body.AddTorque(Random.insideUnitSphere * fragmentExplosionForce, ForceMode.Impulse);
            Destroy(segment.gameObject, fragmentLifetime);
        }
    }

    private IEnumerator FadeCoreAndFinish()
    {
        float elapsed = 0f;
        while (elapsed < missFadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(elapsed / missFadeDuration);
            if (coreRenderer != null)
                coreRenderer.color = new Color(originalCoreColor.r, originalCoreColor.g, originalCoreColor.b, alpha);
            yield return null;
        }
        state = TargetState.Finished;
        gameObject.SetActive(false);
    }

    private void SetFrame(int index)
    {
        if (coreRenderer == null || frames == null || index < 0 || index >= frames.Length)
            return;
        if (frames[index] != null)
        {
            coreRenderer.gameObject.SetActive(true);
            coreRenderer.sprite = frames[index];
        }
    }

    private void SetHitColliderEnabled(bool enabledState)
    {
        if (targetHitCollider != null)
            targetHitCollider.enabled = enabledState;
    }
}
