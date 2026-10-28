using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLaneMovement : MonoBehaviour
{
    [Header("Laneinställningar")]
    public float laneDistance = 3f;
    public float laneChangeSpeed = 10f;
    public int currentLane = 0;

    [Header("Gång- & Springinställningar")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 7f;
    public float currentSpeed;

    [HideInInspector] public bool hasStartedRunning = false;
    [HideInInspector] public bool isInStartSequence = false;
    [HideInInspector] public float targetPositionZ;

    // FIX: Denna variabel talar om ifall hastighetsrampen fortfarande pågår
    [HideInInspector] public bool isRampingSpeed = false;

    private float targetPositionX;
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;

    void Start()
    {
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
        targetPositionX = transform.position.x;
        targetPositionZ = transform.position.z;
        currentSpeed = 0f;
        isRampingSpeed = false;
    }

    void Update()
    {
        if (!hasStartedRunning)
        {
            Vector3 startPos = transform.position;
            startPos.x = Mathf.Lerp(startPos.x, targetPositionX, laneChangeSpeed * Time.deltaTime);

            if (!isInStartSequence)
            {
                startPos.z = targetPositionZ;
                transform.position = startPos;
            }
            else
            {
                transform.position = new Vector3(startPos.x, transform.position.y, transform.position.z);
            }
            return;
        }

        // --- ORDINARIE SPELKONTROLLER (A & D) ---
        if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            if (currentLane > -1) currentLane--;
        }
        if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            if (currentLane < 1) currentLane++;
        }

        targetPositionX = spawnPosition.x + (currentLane * laneDistance);

        // FIX: Om introskriptet håller på att rampa upp farten mjukt, rör INTE currentSpeed härifrån!
        if (!isRampingSpeed)
        {
            float maxSpeed = Keyboard.current.leftShiftKey.isPressed ? sprintSpeed : moveSpeed;
            currentSpeed = maxSpeed;
        }

        // Flytta spelaren framåt baserat på den tillåtna hastigheten
        targetPositionZ += currentSpeed * Time.deltaTime;

        Vector3 newPos = transform.position;
        newPos.x = Mathf.Lerp(newPos.x, targetPositionX, laneChangeSpeed * Time.deltaTime);
        newPos.z = targetPositionZ;
        transform.position = newPos;
    }

    public void ResetMovement()
    {
        currentLane = 0;
        targetPositionX = spawnPosition.x;
        targetPositionZ = spawnPosition.z;
        transform.position = spawnPosition;
        transform.rotation = spawnRotation;
        currentSpeed = 0f;
        hasStartedRunning = false;
        isInStartSequence = false;
        isRampingSpeed = false;
    }
}
