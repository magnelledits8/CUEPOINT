using UnityEngine;

public class WallCannon : MonoBehaviour
{
    [Header("Kanon Inställningar")]
    public GameObject wallobject;
    public int shotsFired = 0;
    public float fireRate = 1f;

    [Header("Inställningar för Väggen")]
    public Vector3 wallSize = new Vector3(3f, 2f, 0.5f);
    public Vector3 wallRotationBeforeLaunch = new Vector3(0f, 0f, 0f);

    [Header("Rörelse")]
    public float speed = 10f;

    [Header("Respawn Inställningar för Väggen")]
    public Vector3 wallRespawnPosition = new Vector3(0f, 5f, 0f); // Höjden spelaren spawnar på vid träff

    private float nextFireTime = 0f;

    void Update()
    {
        if (Time.time >= nextFireTime)
        {
            ShootWall();
            nextFireTime = Time.time + fireRate;
        }
    }

    void ShootWall()
    {
        if (wallobject != null)
        {
            Quaternion desiredRotation = Quaternion.Euler(wallRotationBeforeLaunch);
            GameObject spawnedWall = Instantiate(wallobject, transform.position, desiredRotation);

            spawnedWall.transform.localScale = wallSize;

            // Skicka med respawn-positionen till den skapade väggen
            ConstantMove mover = spawnedWall.AddComponent<ConstantMove>();
            mover.speed = speed;
            mover.moveDirection = transform.right;
            mover.respawnPosition = wallRespawnPosition; // Skickar vidare positionen

            shotsFired = shotsFired + 1;
            Debug.Log("Shots fired: " + shotsFired);
        }
    }
}

// Hastighet och kollision för väggen som rör sig
public class ConstantMove : MonoBehaviour
{
    public float speed;
    public Vector3 moveDirection;
    public Vector3 respawnPosition; // Tas emot från kanonen

    void Update()
    {
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Kolla om spelaren träffas
        if (other.CompareTag("Player"))
        {
            Debug.Log("Spelaren träffades av en vägg och eliminerades!");

            // 1. Hämta CharacterController (STÄNG AV den istället för att ta bort den!)
            CharacterController cc = other.GetComponent<CharacterController>();

            if (cc != null)
            {
                cc.enabled = false;
            }

            // 2. Flytta spelaren till din valda höjd
            other.transform.position = respawnPosition;

            // 3. Tvinga Unity att fatta att spelaren flyttat på sig direkt
            Physics.SyncTransforms();

            // 4. Sätt på CharacterController igen så spelaren kan springa vidare
            if (cc != null)
            {
                cc.enabled = true;
            }

            // 5. Förstör vägg-objektet så det inte fortsätter flyga för evigt efter krocken
            Destroy(gameObject);
        }
    }
}
