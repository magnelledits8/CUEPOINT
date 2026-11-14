
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiralTarget3DController : MonoBehaviour
{
    private const int IncomingIndex = 0;
    private const int FirstChargeIndex = 1;
    private const int ReadyIndex = 12;
    private const int FirstHitIndex = 13;

    [Header("2D animated core")]
    [SerializeField] private SpriteRenderer coreRenderer;
    [SerializeField] private Sprite[] frames = new Sprite[26];

    [Header("Movement - Manual Mode")]
    [SerializeField] private Transform destination;
    [SerializeField, Min(0f)] private float moveDuration = 0.5f;
    [SerializeField] private bool beginOnStart = false;

    [Header("Animation Timing")]
    [SerializeField, Min(0.05f)] private float chargeDuration = 0.9f;
    [SerializeField, Min(0.02f)] private float hitAnimationDuration = 0.15f;
    [SerializeField] private bool hideCoreAfterHit = true;

    [Tooltip("0 = use the hit timing windows. A positive value can shorten the late window.")]
    [SerializeField, Min(0f)] private float readyHoldSeconds = 0f;
    [SerializeField, Min(0.01f)] private float missFadeDuration = 0.15f;

    [Header("Hit Timing Windows - Seconds")]
    [Tooltip("Earliest accepted hit before Ready Time.")]
    [SerializeField, Min(0.01f)] private float earlyWindowSeconds = 0.20f;

    [Tooltip("Perfect window before and after Ready Time.")]
    [SerializeField, Min(0.001f)] private float perfectWindowSeconds = 0.05f;

    [Tooltip("Latest accepted hit after Ready Time.")]
    [SerializeField, Min(0.01f)] private float lateWindowSeconds = 0.15f;

    [Header("Hit Points")]
    [SerializeField, Min(0)] private int earlyPoints = 100;
    [SerializeField, Min(0)] private int perfectPoints = 300;
    [SerializeField, Min(0)] private int latePoints = 100;

    [Header("3D Outer Ring")]
    [SerializeField] private Transform ringSegmentsRoot;
    [SerializeField] private GameObject wholeRingVisual;

    [Header("Fragment Explosion Settings")]
    [SerializeField, Min(0f)] private float fragmentExplosionForce = 4f;
    [SerializeField, Min(0f)] private float fragmentUpwardForce = 0f;
    [SerializeField, Min(0f)] private float fragmentSpinForce = 1.5f;
    [SerializeField, Range(0f, 1f)] private float fragmentDirectionVariation = 0.85f;
    [SerializeField, Range(0f, 1f)] private float fragmentSpeedVariation = 0.30f;
    [SerializeField, Range(0f, 1f)] private float fragmentDepthVariation = 0.20f;
    [SerializeField, Min(0.1f)] private float fragmentLifetime = 1.2f;
    [SerializeField] private bool fragmentsUseGravity = false;

    [Header("Hit Detection")]
    [SerializeField] private Collider targetHitCollider;

    private enum TargetState
    {
        Waiting,
        Incoming,
        Charging,
        Ready,
        Hit,
        Missed,
        Finished
    }

    private TargetState state = TargetState.Waiting;
    private Coroutine sequenceRoutine;
    private Color originalCoreColor = Color.white;
    private bool startedFromCue;

    private double expectedReadyTime;
    private bool useDspClock;
    private bool timingConfigured;

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

    // Start the target using the song timing with extra timing settings
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
        useDspClock = true;
        timingConfigured = true;

        moveTime = Mathf.Max(0f, moveTime);
        chargeTime = Mathf.Max(0f, chargeTime);

        expectedReadyTime = moveStartTime + moveTime + chargeTime;

        if (sequenceRoutine != null)
            StopCoroutine(sequenceRoutine);

        transform.SetPositionAndRotation(startPos, startRot);

        if (Vector3.Distance(startPos, endPos) < 0.01f)
        {
            Debug.LogWarning(
                name + ": Spawn Point and Hit Point are identical.",
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
                moveTime,
                chargeTime
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
        SetHitColliderEnabled(false);

        while (AudioSettings.dspTime < moveStartTime)
            yield return null;

        SetHitColliderEnabled(true);

        while (AudioSettings.dspTime < expectedReadyTime)
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
                    startPos,
                    endPos,
                    smooth
                );

                transform.rotation = Quaternion.Slerp(
                    startRot,
                    endRot,
                    smooth
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
                    index,
                    FirstChargeIndex,
                    ReadyIndex
                ));
            }

            yield return null;
        }

        transform.SetPositionAndRotation(endPos, endRot);
        SetFrame(ReadyIndex);
        state = TargetState.Ready;

        yield return WaitForMissDeadline();
    }

    // Start the target without the song timing
    public void BeginSequence()
    {
        startedFromCue = false;
        useDspClock = false;
        timingConfigured = true;

        float actualMoveDuration =
            destination != null ? moveDuration : 0f;

        expectedReadyTime =
            Time.timeAsDouble +
            actualMoveDuration +
            chargeDuration;

        if (sequenceRoutine != null)
            StopCoroutine(sequenceRoutine);

        sequenceRoutine = StartCoroutine(
            ArrivalAndChargeRoutine()
        );
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

                float progress = Mathf.Clamp01(
                    elapsed / moveDuration
                );

                float smooth = Mathf.SmoothStep(0f, 1f, progress);

                transform.position = Vector3.Lerp(
                    start,
                    end,
                    smooth
                );

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
                index,
                FirstChargeIndex,
                ReadyIndex
            ));

            yield return null;
        }

        SetFrame(ReadyIndex);
        state = TargetState.Ready;

        yield return WaitForMissDeadline();
    }

    private double GetCurrentClock()
    {
        return useDspClock
            ? AudioSettings.dspTime
            : Time.timeAsDouble;
    }

    private float GetEarlyLimit()
    {
        return Mathf.Max(
            earlyWindowSeconds,
            perfectWindowSeconds
        );
    }

    private float GetLateLimit()
    {
        float allowedLate = Mathf.Max(
            lateWindowSeconds,
            perfectWindowSeconds
        );

        if (readyHoldSeconds > 0f)
        {
            allowedLate = Mathf.Min(
                allowedLate,
                readyHoldSeconds
            );
        }

        return allowedLate;
    }

    private IEnumerator WaitForMissDeadline()
    {
        double deadline = expectedReadyTime + GetLateLimit();

        while (state == TargetState.Ready &&
               GetCurrentClock() <= deadline)
        {
            yield return null;
        }

        if (state != TargetState.Ready)
            yield break;

        state = TargetState.Missed;
        SetHitColliderEnabled(false);

        if (ScoreHUD.Instance != null)
        {
            ScoreHUD.Instance.RegisterMiss();
        }

        // NEW: Show MISS on the player's HUD.
        if (HitJudgementHUD.Instance != null)
        {
            HitJudgementHUD.Instance.ShowMiss();
        }

        Debug.Log(
            name + ": MISS - target expired.",
            this
        );

        yield return FadeCoreAndFinish();
    }

    // EARLY / PERFECT / LATE SHOT JUDGEMENT
    public bool TryShoot()
    {
        if (!gameObject.activeInHierarchy ||
            !timingConfigured)
        {
            return false;
        }

        if (state != TargetState.Incoming &&
            state != TargetState.Charging &&
            state != TargetState.Ready)
        {
            return false;
        }

        double difference =
            GetCurrentClock() - expectedReadyTime;

        if (difference < -GetEarlyLimit())
            return false;

        if (difference > GetLateLimit())
            return false;

        int awardedPoints;
        string judgement;

        if (System.Math.Abs(difference) <=
            perfectWindowSeconds)
        {
            judgement = "PERFECT";
            awardedPoints = perfectPoints;
        }
        else if (difference < 0.0)
        {
            judgement = "EARLY";
            awardedPoints = earlyPoints;
        }
        else
        {
            judgement = "LATE";
            awardedPoints = latePoints;
        }

        if (awardedPoints <= 0)
            return false;

        state = TargetState.Hit;
        SetHitColliderEnabled(false);

        if (sequenceRoutine != null)
            StopCoroutine(sequenceRoutine);

        SetFrame(FirstHitIndex);
        ExplodeOuterRing();

        sequenceRoutine = StartCoroutine(
            HitAnimationRoutine()
        );

        // Add points to the score
        if (ScoreHUD.Instance != null)
        {
            ScoreHUD.Instance.RegisterHit(awardedPoints);
        }

        // NEW: Send the result to HitJudgementHUD.
        if (HitJudgementHUD.Instance != null)
        {
            switch (judgement)
            {
                case "EARLY":
                    HitJudgementHUD.Instance.ShowEarly();
                    break;

                case "PERFECT":
                    HitJudgementHUD.Instance.ShowPerfect();
                    break;

                case "LATE":
                    HitJudgementHUD.Instance.ShowLate();
                    break;
            }
        }

        Debug.Log(
            name + ": " + judgement +
            " | " + awardedPoints + " points" +
            " | Offset: " +
            (difference * 1000.0).ToString("F1") +
            " ms",
            this
        );

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

        float duration = Mathf.Max(
            0.02f,
            hitAnimationDuration
        );

        float startTime = Time.time;

        while (Time.time - startTime < duration)
        {
            float elapsed = Time.time - startTime;
            float progress = Mathf.Clamp01(
                elapsed / duration
            );

            int frameOffset = Mathf.Clamp(
                Mathf.FloorToInt(
                    progress * totalHitFrames
                ),
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

    // Break the outer ring into pieces when hit
    private void ExplodeOuterRing()
    {
        if (wholeRingVisual != null &&
            coreRenderer != null)
        {
            if (coreRenderer.transform != transform &&
                coreRenderer.transform.IsChildOf(
                    wholeRingVisual.transform
                ))
            {
                coreRenderer.transform.SetParent(
                    transform,
                    true
                );
            }
        }

        if (ringSegmentsRoot != null)
        {
            Vector3 ringCenter = ringSegmentsRoot.position;
            Vector3 ringNormal = ringSegmentsRoot.up.normalized;
            int segmentCount = ringSegmentsRoot.childCount;

            List<Collider> fragmentColliders =
                new List<Collider>();

            for (int i = segmentCount - 1; i >= 0; i--)
            {
                Transform segment = ringSegmentsRoot.GetChild(i);

                if (coreRenderer != null &&
                    segment == coreRenderer.transform)
                {
                    continue;
                }

                Vector3 meshCenter = segment.position;

                MeshFilter meshFilter =
                    segment.GetComponent<MeshFilter>();

                if (meshFilter != null &&
                    meshFilter.sharedMesh != null)
                {
                    meshCenter = segment.TransformPoint(
                        meshFilter.sharedMesh.bounds.center
                    );
                }

                Vector3 outward = Vector3.ProjectOnPlane(
                    meshCenter - ringCenter,
                    ringNormal
                );

                if (outward.sqrMagnitude < 0.0001f)
                {
                    float angle =
                        360f * i /
                        Mathf.Max(1, segmentCount);

                    outward = Quaternion.AngleAxis(
                        angle,
                        ringNormal
                    ) * ringSegmentsRoot.right;
                }

                outward.Normalize();

                float randomAngle = Random.Range(
                    -135f * fragmentDirectionVariation,
                    135f * fragmentDirectionVariation
                );

                Vector3 direction = Quaternion.AngleAxis(
                    randomAngle,
                    ringNormal
                ) * outward;

                direction += ringNormal * Random.Range(
                    -fragmentDepthVariation,
                    fragmentDepthVariation
                );

                direction += Vector3.up * fragmentUpwardForce;
                direction.Normalize();

                float randomStrength = Random.Range(
                    Mathf.Max(
                        0.1f,
                        1f - fragmentSpeedVariation
                    ),
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
                {
                    meshCollider.convex = true;
                }

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
                {
                    body =
                        segment.gameObject.AddComponent<Rigidbody>();
                }

                body.isKinematic = false;
                body.useGravity = fragmentsUseGravity;
                body.mass = 0.2f;

                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;

                body.AddForce(
                    direction *
                    fragmentExplosionForce *
                    randomStrength,
                    ForceMode.Impulse
                );

                body.AddTorque(
                    Random.insideUnitSphere * fragmentSpinForce,
                    ForceMode.Impulse
                );

                Destroy(
                    segment.gameObject,
                    fragmentLifetime
                );
            }
        }

        if (wholeRingVisual != null)
        {
            MeshRenderer[] ringRenderers =
                wholeRingVisual
                    .GetComponentsInChildren<MeshRenderer>(true);

            foreach (MeshRenderer renderer in ringRenderers)
            {
                renderer.enabled = false;
            }

            Collider[] ringColliders =
                wholeRingVisual
                    .GetComponentsInChildren<Collider>(true);

            foreach (Collider collider in ringColliders)
            {
                collider.enabled = false;
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
        {
            targetHitCollider.enabled = enabledState;
        }
    }
}
