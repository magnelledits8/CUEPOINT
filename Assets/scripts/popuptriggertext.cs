using UnityEngine;
using TMPro; // Required if you are using TextMesh Pro

public class TextPopUpTrigger : MonoBehaviour
{
    // Drag your UI Text GameObject into this slot in the Unity Inspector
    [SerializeField] private GameObject textPopUp;

    // Occurs when another collider enters this object's trigger zone
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("trigger entered");
        
        // Make sure your player object has the tag "Player" assigned to it
        if (other.CompareTag("Player"))
        {
            textPopUp.SetActive(true); // Show the text
        }
    }

    // Occurs when the collider exits the trigger zone
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            textPopUp.SetActive(false); // Hide the text
        }
    }
}
