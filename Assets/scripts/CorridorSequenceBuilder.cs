using UnityEngine;

[ExecuteAlways] // Allows the script to run in the Editor without pressing Play
public class CorridorSequenceBuilder : MonoBehaviour
{
    [Header("Corridor Prefabs")]
    [SerializeField] private GameObject[] corridorPrefabs;

    [Header("Generation Settings")]
    [SerializeField, Min(1)] private int repeatCount = 3;
    [SerializeField, Min(0.01f)] private float lengthMultiplier = 1f;
    [SerializeField] private bool buildOnPlay = true;

    private const string GeneratedName = "Generated Corridor";

    private void Start()
    {
        // Only build on start if we are actually playing the game and buildOnPlay is true
        if (Application.isPlaying && buildOnPlay)
        {
            BuildCorridor();
        }
    }

    // This built-in Unity function runs automatically whenever you change a value in the Inspector
    private void OnValidate()
    {
        // Guard clause to prevent infinite loops or errors during prefab editing
        if (gameObject.scene.rootCount == 0) return;

        // Automatically rebuild the corridor in the editor as you change settings
        if (!Application.isPlaying)
        {
            // Delay the call slightly to ensure Unity's hierarchy is ready to handle Instantiate/Destroy
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall -= ExecuteEditorBuild;
            UnityEditor.EditorApplication.delayCall += ExecuteEditorBuild;
#endif
        }
    }

    private void ExecuteEditorBuild()
    {
        if (this == null) return;
        BuildCorridor();
    }

    [ContextMenu("Build Corridor")]
    public void BuildCorridor()
    {
        if (!ValidatePrefabs())
            return;

        ClearGeneratedCorridor();

        GameObject generatedObject = new GameObject(GeneratedName);
        Transform generatedRoot = generatedObject.transform;

        generatedRoot.SetParent(transform, false);

        // The builder's transform is the first connection point.
        Transform currentConnection = transform;

        // Repeat the entire prefab sequence.
        for (int repeat = 0; repeat < repeatCount; repeat++)
        {
            // Place each prefab in the assigned order.
            foreach (GameObject prefab in corridorPrefabs)
            {
                GameObject segment = Instantiate(
                    prefab,
                    generatedRoot
                );

                segment.name = prefab.name;

                // Stretch the segment along its local Z axis.
                Vector3 originalScale = prefab.transform.localScale;

                segment.transform.localScale = new Vector3(
                    originalScale.x,
                    originalScale.y,
                    originalScale.z * lengthMultiplier
                );

                Transform entry = segment.transform.Find("EntryPoint");
                Transform exit = segment.transform.Find("ExitPoint");

                // Rotate the segment to match the previous exit.
                Quaternion rotationOffset =
                    currentConnection.rotation *
                    Quaternion.Inverse(entry.rotation);

                segment.transform.rotation =
                    rotationOffset * segment.transform.rotation;

                // Move the segment so the entry points connect.
                segment.transform.position +=
                    currentConnection.position - entry.position;

                // The next segment connects to this segment's exit.
                currentConnection = exit;
            }
        }
    }

    private bool ValidatePrefabs()
    {
        if (corridorPrefabs == null || corridorPrefabs.Length == 0)
        {
            // Reduced log severity for editor comfort, prevents console spamming when array is empty
            if (Application.isPlaying) Debug.LogError("No corridor prefabs assigned.", this);
            return false;
        }

        if (repeatCount < 1 || lengthMultiplier <= 0f)
        {
            if (Application.isPlaying) Debug.LogError("Check repeat count and length multiplier.", this);
            return false;
        }

        foreach (GameObject prefab in corridorPrefabs)
        {
            if (prefab == null)
            {
                return false;
            }

            if (prefab.transform.Find("EntryPoint") == null ||
                prefab.transform.Find("ExitPoint") == null)
            {
                if (Application.isPlaying)
                    Debug.LogError(prefab.name + " is missing EntryPoint or ExitPoint.", prefab);
                return false;
            }
        }

        return true;
    }

    [ContextMenu("Clear Generated Corridor")]
    public void ClearGeneratedCorridor()
    {
        Transform oldGenerated = transform.Find(GeneratedName);

        if (oldGenerated != null)
        {
            if (Application.isPlaying)
                Destroy(oldGenerated.gameObject);
            else
                DestroyImmediate(oldGenerated.gameObject);
        }
    }
}
