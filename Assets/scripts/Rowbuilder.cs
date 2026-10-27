using UnityEngine;

public class RowBuilder : MonoBehaviour
{
    public GameObject pillar;
    public int count = 5;
    public float spacing = 2f;

    void Start()
    {
        // I'm making one pillar for each number from 0 up to count
        for (int i = 0; i < count; i++)
        {
            Vector3 position = new Vector3(i * spacing, 1.5f, 8);
            Instantiate(pillar, position, Quaternion.identity);
        }
    }
}
