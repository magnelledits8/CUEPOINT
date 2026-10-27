using UnityEngine;

public class GraphicSpawnerLoop : MonoBehaviour
{
    [Header("1. Välj modell och antal:")]
    public GameObject objectPrefab;       // Model i want to duplicate
    public int totalAmount = 20;          // How many object I want the loop to create
    [Header("2. Spridning (Density):")]
    public float spawnRadius = 10f;       // Radious of objects

    [Header("3. Slumpmässig storlek (Randomness):")]
    public float minScale = 0.5f;         // Smallest size
    public float maxScale = 2.0f;         // Biggest size

    void Start()
    {
        // Safty check
        if (objectPrefab == null)
        {
            Debug.LogWarning("Du har glömt att lägga till en modell i objectPrefab-rutan!");
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
