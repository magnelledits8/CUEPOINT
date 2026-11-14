
using UnityEngine;

public class CorridorSequenceBuilder : MonoBehaviour
{
    [Header("Corridor Prefabs")]
    [SerializeField] private GameObject[] corridorPrefabs;

    [Header("Generation Settings")]
    [SerializeField, Min(1)] private int repeatCount = 3;
    [SerializeField, Min(0.01f)] private float lengthMultiplier = 1f;

    [Header("Automatic Generation")]
    [Tooltip("Keep OFF when manually editing the level.")]
    [SerializeField] private bool buildOnPlay = false;

    private const string GeneratedName = "Generated Corridor";

    private void Start()
    {
        // Build the corridor when starting
        // Keep the old corridor if it already exists
        if (Application.isPlaying && buildOnPlay)
        {
            if (transform.Find(GeneratedName) == null)
            {
                BuildCorridor();
            }
            else
            {
                Debug.Log(
                    "Corridor already exists. Manual edits are preserved.",
                    this
                );
            }
        }
    }

    [ContextMenu("Build Corridor")]
    public void BuildCorridor()
    {
        if (!ValidatePrefabs())
            return;

        // Don't replace a corridor that already exists
        Transform existingCorridor =
            transform.Find(GeneratedName);

        if (existingCorridor != null)
        {
            Debug.LogWarning(
                "A Generated Corridor already exists. " +
                "It has NOT been rebuilt, so your manual " +
                "floor and hazard edits are preserved. " +
                "Use Clear Generated Corridor only if " +
                "you intentionally want to start over.",
                this
            );

            return;
        }

        GameObject generatedObject =
            new GameObject(GeneratedName);

        Transform generatedRoot =
            generatedObject.transform;

        generatedRoot.SetParent(transform, false);

        // Start building from this position
        Transform currentConnection = transform;

        // Repeat the corridor sections
        for (int repeat = 0; repeat < repeatCount; repeat++)
        {
            foreach (GameObject prefab in corridorPrefabs)
            {
                GameObject segment = Instantiate(
                    prefab,
                    generatedRoot
                );

                segment.name = prefab.name;

                // Change the length of each section
                Vector3 originalScale =
                    prefab.transform.localScale;

                segment.transform.localScale =
                    new Vector3(
                        originalScale.x,
                        originalScale.y,
                        originalScale.z * lengthMultiplier
                    );

                Transform entry =
                    segment.transform.Find("EntryPoint");

                Transform exit =
                    segment.transform.Find("ExitPoint");

                // Match the rotation with the previous section
                Quaternion rotationOffset =
                    currentConnection.rotation *
                    Quaternion.Inverse(entry.rotation);

                segment.transform.rotation =
                    rotationOffset *
                    segment.transform.rotation;

                // Connect the new section to the last one
                segment.transform.position +=
                    currentConnection.position -
                    entry.position;

                // Use this exit for the next section
                currentConnection = exit;
            }
        }

        Debug.Log(
            "Corridor generated successfully. " +
            "You can now edit floor sections manually.",
            this
        );
    }

    private bool ValidatePrefabs()
    {
        if (corridorPrefabs == null ||
            corridorPrefabs.Length == 0)
        {
            Debug.LogError(
                "No corridor prefabs assigned.",
                this
            );

            return false;
        }

        if (repeatCount < 1 ||
            lengthMultiplier <= 0f)
        {
            Debug.LogError(
                "Check Repeat Count and Length Multiplier.",
                this
            );

            return false;
        }

        foreach (GameObject prefab in corridorPrefabs)
        {
            if (prefab == null)
            {
                Debug.LogError(
                    "One corridor prefab is missing.",
                    this
                );

                return false;
            }

            if (prefab.transform.Find("EntryPoint") == null ||
                prefab.transform.Find("ExitPoint") == null)
            {
                Debug.LogError(
                    prefab.name +
                    " is missing EntryPoint or ExitPoint.",
                    prefab
                );

                return false;
            }
        }

        return true;
    }

    [ContextMenu("Clear Generated Corridor")]
    public void ClearGeneratedCorridor()
    {
        Transform existingCorridor =
            transform.Find(GeneratedName);

        if (existingCorridor == null)
        {
            Debug.Log(
                "No generated corridor to remove.",
                this
            );

            return;
        }

        if (Application.isPlaying)
        {
            Destroy(existingCorridor.gameObject);
        }
        else
        {
            DestroyImmediate(existingCorridor.gameObject);
        }

        Debug.LogWarning(
            "Generated Corridor removed. " +
            "Manual edits inside it have also been removed.",
            this
        );
    }
}
