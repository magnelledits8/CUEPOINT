using UnityEngine;

public class GraphicSpawnerLoop : MonoBehaviour
{
    [Header("1. Select model and amount:")]
    public GameObject objectPrefab;       // Model i want to duplicate
    public int totalAmount = 20;          // How many object I want the loop to create
    [Header("2. Spread (Density):")]
    public float spawnRadius = 10f;       // Radious of objects

    [Header("3. Random size (Randomness):")]
    public float minScale = 0.5f;         // Smallest size
    public float maxScale = 2.0f;         // Biggest size

    void Start()
    {
        // Safety check
        if (objectPrefab == null)
        {
            Debug.LogWarning("You forgot to add a model to the objectPrefab slot!");
            return;
        }

        //For-loop multiply object
        for (int i = 0; i < totalAmount; i++)
        {
            // random posistion of object
            float randomX = Random.Range(-spawnRadius, spawnRadius);
            float randomZ = Random.Range(-spawnRadius, spawnRadius);

            Vector3 spawnPosition = new Vector3(randomX, 0f, randomZ) + transform.position;

            // duplicate object within the random position
            GameObject newObject = Instantiate(objectPrefab, spawnPosition, Quaternion.identity);

            // Make the script object parent to keep hierarchy clean
            newObject.transform.parent = transform;

            // Random size
            float randomSize = Random.Range(minScale, maxScale);

            // Applies new size to object
            newObject.transform.localScale = new Vector3(randomSize, randomSize, randomSize);

            // Random rotation of objects
            float randomRotationY = Random.Range(0f, 360f);
            newObject.transform.Rotate(0f, randomRotationY, 0f);
        }
    }
}