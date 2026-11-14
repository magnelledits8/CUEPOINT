using UnityEngine;

public class RowBuilder : MonoBehaviour
{
    public GameObject pillar;
    public int count = 5;
    public float spacing = 2f;

    void Start()
    {
        // Create a row of pillars using a for loop
        for (int i = 0; i < count; i++)
        {
            Vector3 position = new Vector3(i * spacing, 1.5f, 8);
            Instantiate(pillar, position, Quaternion.identity);
        }
    }
}
