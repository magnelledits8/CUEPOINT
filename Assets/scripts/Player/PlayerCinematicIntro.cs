using System.Collections;
using UnityEngine;

public class PlayerCinematicIntro : MonoBehaviour
{
    [Header("Filmisk Startsekvens (INTRON)")]
    public Camera fpsCam;

    [Tooltip("Vid vilken bildruta (Frame Index) i din countdown ska spelaren börja ta sina första steg?")]
    public int startStepsAtFrame = 40;

    [Tooltip("Vid vilken bildruta (Frame Index) i din countdown ska spelaren göra själva starthoppet?")]
    public int triggerJumpAtFrame = 100;

    [Header("Fas 1: Steg & Acceleration (Ramp Up)")]
    [Tooltip("Hur snabbt accelererar spelaren under de första stegen innan hoppet? Högre = snabbare start.")]
    public float introAcceleration = 5f;

    [Header("Fas 2: Det fysiska starthoppet")]
    public float introJumpUpwardForce = 12f;
    public float introJumpForwardBoost = 4f;
    public float introGravityValue = 22f;

    [Header("Fas 3: Landning & Slutacceleration")]
    public float kneelDuration = 0.4f;
    public float kneelCameraDip = 0.5f;

    [Tooltip("Hur många sekunder (i tid) ska det ta för farten att öka från 0 upp till din fulla spelfart efter att karaktären har rest sig från knäböjen? Högre värde = långsammare och mjukare ramp up!")]
    public float speedRampDurationAfterKneel = 3.0f; // ÄNDRA DENNA I INSPECTOR!

    private Rigidbody rb;
    private PlayerLaneMovement laneMove;
    private PlayerJumpAndCrouch jumpCrouch;
    private Vector3 originalCamLocalPos;
    private bool isGrounded;
    private bool introStarted = false;
    private bool jumpTriggered = false;
    private float currentIntroSpeed = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        laneMove = GetComponent<PlayerLaneMovement>();
        jumpCrouch = GetComponent<PlayerJumpAndCrouch>();
    }

    void Start()
    {
        if (fpsCam != null) originalCamLocalPos = fpsCam.transform.localPosition;
        introStarted = false;
        jumpTriggered = false;
        currentIntroSpeed = 0f;
    }

    public void CheckFrameTrigger(int currentFrameIndex)
    {
        if (laneMove == null) return;

        if (!introStarted && currentFrameIndex >= startStepsAtFrame)
        {
            introStarted = true;
            laneMove.isInStartSequence = true;
            if (rb != null) rb.isKinematic = true;
            StartCoroutine(IntroMovementRoutine());
        }

        if (!jumpTriggered && currentFrameIndex >= triggerJumpAtFrame)
        {
            jumpTriggered = true;
        }
    }

    public void TriggerStartSequence()
    {
        if (laneMove != null && !laneMove.hasStartedRunning && !laneMove.isInStartSequence)
        {
            introStarted = true;
            jumpTriggered = true;
            laneMove.isInStartSequence = true;
            StartCoroutine(IntroMovementRoutine());
        }
    }

    IEnumerator IntroMovementRoutine()
    {
        // --- DEL 1: TA STEG ---
        while (!jumpTriggered)
        {
            currentIntroSpeed += introAcceleration * Time.deltaTime;
            laneMove.targetPositionZ += currentIntroSpeed * Time.deltaTime;

            Vector3 stepPos = transform.position;
            stepPos.z = laneMove.targetPositionZ;
            transform.position = stepPos;

            yield return null;
        }

        // --- DEL 2: DET FYSISKA STARTHOPPET ---
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = false;
            float totalForwardVelocity = currentIntroSpeed + introJumpForwardBoost;
            rb.linearVelocity = new Vector3(0f, introJumpUpwardForce, totalForwardVelocity);
        }

        yield return new WaitForSeconds(0.1f);
        isGrounded = false;

        while (!isGrounded)
        {
            if (rb != null) rb.AddForce(Vector3.down * introGravityValue, ForceMode.Acceleration);
            laneMove.targetPositionZ = transform.position.z;
            isGrounded = Physics.Raycast(transform.position, Vector3.down, (transform.localScale.y * 1.1f));
            yield return null;
        }

        if (rb != null) rb.linearVelocity = Vector3.zero;
        laneMove.targetPositionZ = transform.position.z;

        // --- DEL 3: LANDA PÅ KNÄ ---
        if (fpsCam != null)
        {
            Vector3 targetCamPos = originalCamLocalPos - new Vector3(0, kneelCameraDip, 0);
            float t = 0f;
            while (t < 0.1f)
            {
                t += Time.deltaTime;
                fpsCam.transform.localPosition = Vector3.Lerp(originalCamLocalPos, targetCamPos, t / 0.1f);
                yield return null;
            }

            yield return new WaitForSeconds(kneelDuration);

            t = 0f;
            while (t < 0.15f)
            {
                t += Time.deltaTime;
                fpsCam.transform.localPosition = Vector3.Lerp(targetCamPos, originalCamLocalPos, t / 0.15f);
                yield return null;
            }
            fpsCam.transform.localPosition = originalCamLocalPos;
        }
        else
        {
            yield return new WaitForSeconds(kneelDuration);
        }

        // --- DEL 4: MJUK OCH KONTROLLERAD RAMP UP TILL DIN MOVE SPEED ---
        laneMove.isInStartSequence = false;
        laneMove.hasStartedRunning = true;
        laneMove.isRampingSpeed = true; // FIX: Låser vanliga rörelseskriptets hastighet!

        float rampTimer = 0f;
        float targetMaxSpeed = laneMove.moveSpeed; // Hämtar din spelfart (32)

        while (rampTimer < speedRampDurationAfterKneel)
        {
            rampTimer += Time.deltaTime;
            // Interpolerar helt steglöst baserat på sekunder istället för rå acceleration
            laneMove.currentSpeed = Mathf.Lerp(0f, targetMaxSpeed, rampTimer / speedRampDurationAfterKneel);
            yield return null;
        }

        laneMove.currentSpeed = laneMove.moveSpeed;
        laneMove.isRampingSpeed = false; // FIX: Släpper kontrollen tillbaka till vanliga skriptet!
    }

    public void ResetCameraPos()
    {
        if (fpsCam != null) fpsCam.transform.localPosition = originalCamLocalPos;
        if (rb != null) rb.isKinematic = false;
        if (laneMove != null) laneMove.isRampingSpeed = false; // FIX: Säkerställer återställning vid respawn
        introStarted = false;
        jumpTriggered = false;
        currentIntroSpeed = 0f;
    }
}
