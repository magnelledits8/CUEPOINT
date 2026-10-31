using UnityEngine;
using UnityEngine.InputSystem;

public class MouseLook : MonoBehaviour
{
    [Header("Settings (Aimlabs Raw Input)")]
    [Tooltip("Justera känsligheten här. Testa ett lågt värde, t.ex. mellan 0.1 och 0.8.")]
    public float sensitivity = 0.4f;
    public Transform playerBody;

    private float xRotation = 0f;
    private float yRotation = 0f;

    void Start()
    {
        // Lås muspekaren till mitten av skärmen och gör den osynlig
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerBody == null && transform.parent != null)
        {
            playerBody = transform.parent;
        }
    }

    void Update()
    {
        // Läser av musrörelsen direkt från det nya Input-systemet (FPS-oberoende Raw Input)
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        float mouseX = mouseDelta.x * sensitivity;
        float mouseY = mouseDelta.y * sensitivity;

        // 1. STENHÅRD VERTIKAL SPÄRR (Aimlabs): Går aldrig över kanten till upp-och-ner
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -89f, 89f);

        // 2. Horisontell rotation (Titta höger/vänster)
        // Läggs direkt på kamerans transform istället för kroppen för att förhindra att spelaren springer snett!
        yRotation += mouseX;

        // 3. Applicera hela rotationen stabilt direkt på kamerans transform
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }
}
