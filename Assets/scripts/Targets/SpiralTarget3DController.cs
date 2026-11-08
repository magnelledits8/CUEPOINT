
using System.Collections;
using System.Collections.Generic;
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

    [Header("Movement in 3D space (manual mode only)")]
    [SerializeField] private Transform destination;
    [SerializeField, Min(0f)] private float moveDuration = 0.5f;
    [SerializeField] private bool beginOnStart = false;

    [Header("Animation timing")]
    [SerializeField, Min(0.05f)] private float chargeDuration = 0.9f;
    [SerializeField, Min(0.02f)] private float hitAnimationDuration = 0.15f;
    [SerializeField] private bool hideCoreAfterHit = true;
    [SerializeField, Min(0f)] private float readyHoldSeconds = 0f;
    [SerializeField, Min(0.01f)] private float missFadeDuration = 0.15f;

    [Header("3D outer ring")]
    [SerializeField] private Transform ringSegmentsRoot;
    [SerializeField] private GameObject wholeRingVisual;

    [Header("Fragment Explosion Settings")]
    [SerializeField, Min(0f)] private float fragmentExplosionForce = 4f;

    [Tooltip("Extra upward force. Use 0 to avoid pushing everything upward.")]
    [SerializeField, Min(0f)] private float fragmentUpwardForce = 0f;

    [Tooltip("How much the pieces rotate.")]
    [SerializeField, Min(0f)] private float fragmentSpinForce = 1.5f;

    [Tooltip("0 = mostly straight outward. 1 = very random directions.")]
    [SerializeField, Range(0f, 1f)] private float fragmentDirectionVariation = 0.85f;

    [Tooltip("Random difference in force between fragments.")]
    [SerializeField, Range(0f, 1f)] private float fragmentSpeedVariation = 0.3f;

    [Tooltip("Small random movement out of the ring's plane.")]
    [SerializeField, Range(0f, 1f)] private float fragmentDepthVariation = 0.2f;

    [SerializeField, Min(0.1f)] private float fragmentLifetime = 2f;
    [SerializeField] private bool fragmentsUseGravity = false;

    [Header("Hit detection")]
    [SerializeField] private Collider targetHitCollider;

    private enum TargetState
    {
        Waiting, Incoming, Charging, Ready, Hit, Missed, Finished
    }

    private TargetState state = TargetState.Waiting;
    private Coroutine sequenceRoutine;
    private Color originalCoreColor = Color.white;
    private bool startedFromCue;

    private void Awake()
    {
        if (coreRenderer == null)
            coreRenderer = GetComponentInChildren<SpriteRenderer>(true);

        if (targetHitCollider == null)
            targetHitCollider = GetComponent<Collider>();

        if (coreRenderer != null)
            originalCoreColor = coreRenderer.color;

        SetFrame(IncomingIndex);
    }

    private void Start()
    {
        if (beginOnStart && !startedFromCue)
            BeginSequence();
    }

    public void BeginScheduledSequence(
        Vector3 startPos,
        Quaternion startRot,
        Vector3 endPos,
        Quaternion endRot,
        double moveStartTime,
        float moveTime,
        float chargeTime,
        float fadeTime)
    {
        startedFromCue = true;

        if (sequenceRoutine != null)
            StopCoroutine(sequenceRoutine);

        transform.SetPositionAndRotation(startPos, startRot);

        if (Vector3.Distance(startPos, endPos) < 0.01f)
        {
            Debug.LogWarning(
                name + ": Spawn Point and Hit Point are in the same position!",
                this
            );
        }

        sequenceRoutine = StartCoroutine(
            ScheduledSequenceRoutine(
                startPos,
                startRot,
                endPos,
                endRot,
                moveStartTime,
                Mathf.Max(0f, moveTime),
                Mathf.Max(0f, chargeTime)
            )
        );
    }

    private IEnumerator ScheduledSequenceRoutine(
        Vector3 startPos,
        Quaternion startRot,
        Vector3 endPos,
        Quaternion endRot,
        double moveStartTime,
        float moveTime,
        float chargeTime)
    {
        state = TargetState.Incoming;
        SetFrame(IncomingIndex);
        SetHitColliderEnabled(true);

        while (AudioSettings.dspTime < moveStartTime)
            yield return null;

        double readyTime = moveStartTime + moveTime + chargeTime;

        while (AudioSettings.dspTime < readyTime)
        {
            double elapsed = AudioSettings.dspTime - moveStartTime;

            if (elapsed < moveTime && moveTime > 0f)
            {
                state = TargetState.Incoming;

                float progress = Mathf.Clamp01(
                    (float)(elapsed / moveTime)
                );

                float smooth = Mathf.SmoothStep(0f, 1f, progress);

                transform.position = Vector3.Lerp(
                    startPos, endPos, smooth
                );

                transform.rotation = Quaternion.Slerp(
                    startRot, endRot, smooth
                );

                SetFrame(IncomingIndex);
            }
            else
            {
                transform.SetPositionAndRotation(endPos, endRot);
                state = TargetState.Charging;

                double chargeElapsed = elapsed - moveTime;

                float progress = chargeTime <= 0f
                    ? 1f
                    : Mathf.Clamp01(
                        (float)(chargeElapsed / chargeTime)
                    );

                float eased = Mathf.Pow(progress, 1.35f);

                int index = FirstChargeIndex + Mathf.RoundToInt(
                    eased * (ReadyIndex - FirstChargeIndex)
                );

                SetFrame(Mathf.Clamp(
                    index, FirstChargeIndex, ReadyIndex
                ));
            }

            yield return null;
        }

        transform.SetPositionAndRotation(endPos, endRot);
        SetFrame(ReadyIndex);
        state = TargetState.Ready;

        if (readyHoldSeconds > 0f)
        {
            double endHoldTime = readyTime + readyHoldSeconds;

            while (state == TargetState.Ready &&
                   AudioSettings.dspTime < endHoldTime)
            {
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

                float t = Mathf.SmoothStep(
                    0f, 1f,
                    Mathf.Clamp01(elapsed / moveDuration)
                );

                transform.position = Vector3.Lerp(start, end, t);
                yield return null;
            }

            transform.position = end;
        }

        state = TargetState.Charging;
        float chargeElapsed = 0f;

        while (chargeElapsed < chargeDuration)
        {
            chargeElapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(
                chargeElapsed / chargeDuration
            );

            float eased = Mathf.Pow(progress, 1.35f);

            int index = FirstChargeIndex + Mathf.RoundToInt(
                eased * (ReadyIndex - FirstChargeIndex)
            );

            SetFrame(Mathf.Clamp(
                index, FirstChargeIndex, ReadyIndex
            ));

            yield return null;
        }

        SetFrame(ReadyIndex);
        state = TargetState.Ready;

        if (readyHoldSeconds > 0f)
        {
            yield return new WaitForSeconds(readyHoldSeconds);

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
        if (!gameObject.activeInHierarchy ||
            state != TargetState.Ready)
        {
            return false;
        }

        state = TargetState.Hit;
        SetHitColliderEnabled(false);

        if (sequenceRoutine != null)
            StopCoroutine(sequenceRoutine);

        SetFrame(FirstHitIndex);

        ExplodeOuterRing();

        sequenceRoutine = StartCoroutine(HitAnimationRoutine());

        if (ScoreHUD.Instance != null)
        {
            ScoreHUD.Instance.RegisterHit();
        }

        return true;
    }

    private IEnumerator HitAnimationRoutine()
    {
        int lastIndex = frames == null
            ? 0
            : Mathf.Min(frames.Length, 26);

        int totalHitFrames = lastIndex - FirstHitIndex;

        if (totalHitFrames <= 0)
        {
            state = TargetState.Finished;
            yield break;
        }

        float duration = Mathf.Max(0.02f, hitAnimationDuration);
        float startTime = Time.time;

        while (Time.time - startTime < duration)
        {
            float elapsed = Time.time - startTime;
            float progress = Mathf.Clamp01(elapsed / duration);

            int frameOffset = Mathf.Clamp(
                Mathf.FloorToInt(progress * totalHitFrames),
                0,
                totalHitFrames - 1
            );

            SetFrame(FirstHitIndex + frameOffset);
            yield return null;
        }

        SetFrame(lastIndex - 1);
        yield return null;

        if (hideCoreAfterHit && coreRenderer != null)
        {
            coreRenderer.enabled = false;
        }

        state = TargetState.Finished;
    }

    private void ExplodeOuterRing()
    {
        // Keep the 2D core outside the ring object.
        if (wholeRingVisual != null && coreRenderer != null)
        {
            if (coreRenderer.transform != transform &&
                coreRenderer.transform.IsChildOf(
                    wholeRingVisual.transform))
            {
                coreRenderer.transform.SetParent(transform, true);
            }
        }

        if (ringSegmentsRoot != null)
        {
            Vector3 ringCenter = ringSegmentsRoot.position;
            Vector3 ringNormal = ringSegmentsRoot.up.normalized;

            int segmentCount = ringSegmentsRoot.childCount;

            List<Collider> fragmentColliders = new List<Collider>();

            for (int i = segmentCount - 1; i >= 0; i--)
            {
                Transform segment = ringSegmentsRoot.GetChild(i);

                if (coreRenderer != null &&
                    segment == coreRenderer.transform)
                {
                    continue;
                }

                // Find the actual center of this ring piece.
                Vector3 meshCenter = segment.position;

                MeshFilter meshFilter = segment.GetComponent<MeshFilter>();

                if (meshFilter != null &&
                    meshFilter.sharedMesh != null)
                {
                    meshCenter = segment.TransformPoint(
                        meshFilter.sharedMesh.bounds.center
                    );
                }

                // Start with the piece's natural outward direction.
                Vector3 outward = Vector3.ProjectOnPlane(
                    meshCenter - ringCenter,
                    ringNormal
                );

                if (outward.sqrMagnitude < 0.0001f)
                {
                    float angle = 360f * i / Mathf.Max(1, segmentCount);

                    outward = Quaternion.AngleAxis(
                        angle, ringNormal
                    ) * ringSegmentsRoot.right;
                }

                outward.Normalize();

                // RANDOMNESS:
                // Rotate each piece's outward direction by a random angle.
                // Higher variation makes the explosion less predictable.
                float randomAngle = Random.Range(
                    -135f * fragmentDirectionVariation,
                     135f * fragmentDirectionVariation
                );

                Vector3 direction = Quaternion.AngleAxis(
                    randomAngle,
                    ringNormal
                ) * outward;

                // Add small random movement into or out of the ring plane.
                direction += ringNormal * Random.Range(
                    -fragmentDepthVariation,
                    fragmentDepthVariation
                );

                // Optional upward boost.
                direction += Vector3.up * fragmentUpwardForce;

                direction.Normalize();

                // Each fragment gets a different strength.
                float randomStrength = Random.Range(
                    Mathf.Max(0.1f, 1f - fragmentSpeedVariation),
                    1f + fragmentSpeedVariation
                );

                segment.SetParent(null, true);
                segment.gameObject.SetActive(true);

                Collider segmentCollider =
                    segment.GetComponent<Collider>();

                if (segmentCollider == null)
                {
                    segmentCollider =
                        segment.gameObject.AddComponent<BoxCollider>();
                }

                MeshCollider meshCollider =
                    segmentCollider as MeshCollider;

                if (meshCollider != null)
                    meshCollider.convex = true;

                // Prevent ring pieces from colliding with each other.
                foreach (Collider otherCollider in fragmentColliders)
                {
                    if (otherCollider != null)
                    {
                        Physics.IgnoreCollision(
                            segmentCollider,
                            otherCollider
                        );
                    }
                }

                fragmentColliders.Add(segmentCollider);

                Rigidbody body = segment.GetComponent<Rigidbody>();

                if (body == null)
                    body = segment.gameObject.AddComponent<Rigidbody>();

                body.isKinematic = false;
                body.useGravity = fragmentsUseGravity;
                body.mass = 0.2f;

                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;

                // Use the original impulse-based physics feel.
                body.AddForce(
                    direction * fragmentExplosionForce * randomStrength,
                    ForceMode.Impulse
                );

                // Random rotation for each piece.
                body.AddTorque(
                    Random.insideUnitSphere * fragmentSpinForce,
                    ForceMode.Impulse
                );

                Destroy(segment.gameObject, fragmentLifetime);
            }
        }

        // Hide the original ring without disabling the target.
        if (wholeRingVisual != null)
        {
            MeshRenderer[] ringRenderers =
                wholeRingVisual.GetComponentsInChildren<MeshRenderer>(true);

            foreach (MeshRenderer ringRenderer in ringRenderers)
            {
                ringRenderer.enabled = false;
            }

            Collider[] ringColliders =
                wholeRingVisual.GetComponentsInChildren<Collider>(true);

            foreach (Collider ringCollider in ringColliders)
            {
                ringCollider.enabled = false;
            }
        }
    }

    private IEnumerator FadeCoreAndFinish()
    {
        float elapsed = 0f;

        while (elapsed < missFadeDuration)
        {
            elapsed += Time.deltaTime;

            float alpha = 1f - Mathf.Clamp01(
                elapsed / missFadeDuration
            );

            if (coreRenderer != null)
            {
                coreRenderer.color = new Color(
                    originalCoreColor.r,
                    originalCoreColor.g,
                    originalCoreColor.b,
                    alpha * originalCoreColor.a
                );
            }

            yield return null;
        }

        state = TargetState.Finished;
        gameObject.SetActive(false);
    }

    private void SetFrame(int index)
    {
        if (coreRenderer == null ||
            frames == null ||
            index < 0 ||
            index >= frames.Length)
        {
            return;
        }

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
