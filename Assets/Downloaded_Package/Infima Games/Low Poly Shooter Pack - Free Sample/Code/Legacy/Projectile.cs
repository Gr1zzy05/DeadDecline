using System;
using UnityEngine;
using System.Collections;
using InfimaGames.LowPolyShooterPack;
using Random = UnityEngine.Random;

public class Projectile : MonoBehaviour {

	[Header("Damage Settings")]
    [Tooltip("How much damage this bullet deals to shootable objects.")]
    public float damage = 25f;

    [Header("Destruction Settings")]
    [Range(5, 100)]
    [Tooltip("After how long time should the bullet prefab be destroyed?")]
    public float destroyAfter;
    [Tooltip("If enabled the bullet destroys on impact")]
    public bool destroyOnImpact = false;
    [Tooltip("Minimum time after impact that the bullet is destroyed")]
    public float minDestroyTime;
    [Tooltip("Maximum time after impact that the bullet is destroyed")]
    public float maxDestroyTime;

    [Header("Impact Effect Prefabs")]
    public Transform [] bloodImpactPrefabs;
    public Transform [] metalImpactPrefabs;
    public Transform [] dirtImpactPrefabs;
    public Transform [] concreteImpactPrefabs;

    private void Start ()
    {
        //Grab the game mode service, we need it to access the player character!
        var gameModeService = ServiceLocator.Current.Get<IGameModeService>();
        //Ignore the main player character's collision.
        Physics.IgnoreCollision(gameModeService.GetPlayerCharacter().GetComponent<Collider>(), GetComponent<Collider>());
        
        //Start destroy timer
        StartCoroutine (DestroyAfter ());
    }

    //If the bullet collides with anything
    private void OnCollisionEnter (Collision collision)
    {
        //Ignore collisions with other projectiles.
        if (collision.gameObject.GetComponent<Projectile>() != null)
            return;

        // ==========================================
        // 💥 DAMAGE SHOOTABLE OBJECTS
        // ==========================================
        // Check if the object we hit (or its parent) has the ShootableObject component
        ShootableObject shootable = collision.gameObject.GetComponentInParent<ShootableObject>();
        if (shootable != null)
        {
            shootable.TakeDamage(damage);
            Destroy(gameObject);
            return; // Exit so other collision logic doesn't conflict
        }

        //If destroy on impact is false, start coroutine with random destroy timer
        if (!destroyOnImpact) 
        {
            StartCoroutine (DestroyTimer ());
        }
        else 
        {
            Destroy (gameObject);
        }

        //If bullet collides with "Blood" tag
        if (collision.transform.CompareTag("Blood")) 
        {
            Instantiate (bloodImpactPrefabs [Random.Range (0, bloodImpactPrefabs.Length)], 
                transform.position, Quaternion.LookRotation (collision.contacts [0].normal));
            Destroy(gameObject);
        }

        //If bullet collides with "Metal" tag
        if (collision.transform.CompareTag("Metal")) 
        {
            Instantiate (metalImpactPrefabs [Random.Range (0, metalImpactPrefabs.Length)], 
                transform.position, Quaternion.LookRotation (collision.contacts [0].normal));
            Destroy(gameObject);
        }

        //If bullet collides with "Dirt" tag
        if (collision.transform.CompareTag("Dirt")) 
        {
            Instantiate (dirtImpactPrefabs [Random.Range (0, dirtImpactPrefabs.Length)], 
                transform.position, Quaternion.LookRotation (collision.contacts [0].normal));
            Destroy(gameObject);
        }

        //If bullet collides with "Concrete" tag
        if (collision.transform.CompareTag("Concrete")) 
        {
            Instantiate (concreteImpactPrefabs [Random.Range (0, concreteImpactPrefabs.Length)], 
                transform.position, Quaternion.LookRotation (collision.contacts [0].normal));
            Destroy(gameObject);
        }

        //If bullet collides with "Target" tag
        if (collision.transform.CompareTag("Target")) 
        {
            collision.transform.gameObject.GetComponent<TargetScript>().isHit = true;
            Destroy(gameObject);
        }
            
        //If bullet collides with "ExplosiveBarrel" tag
        if (collision.transform.CompareTag("ExplosiveBarrel")) 
        {
            collision.transform.gameObject.GetComponent<ExplosiveBarrelScript>().explode = true;
            Destroy(gameObject);
        }

        //If bullet collides with "GasTank" tag
        if (collision.transform.CompareTag("GasTank")) 
        {
            collision.transform.gameObject.GetComponent<GasTankScript> ().isHit = true;
            Destroy(gameObject);
        }
    }

    private IEnumerator DestroyTimer () 
    {
        yield return new WaitForSeconds (Random.Range(minDestroyTime, maxDestroyTime));
        Destroy(gameObject);
    }

    private IEnumerator DestroyAfter () 
    {
        yield return new WaitForSeconds (destroyAfter);
        Destroy (gameObject);
    }
}