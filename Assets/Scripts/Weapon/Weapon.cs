using UnityEngine;
using System.Collections;
using DigitalRuby.Pooling; // This namespace is used to help with caching previously created objects so that we don't constantly instantiate and destroy them.
using InControl; // This namespace is used to help with input management.

public class Weapon : MonoBehaviour
{
    private Animator weaponAnim;
    private AudioSource weaponAudioSource;

    public string tracer;
    public string bulletImpact;

    public ParticleSystem muzzleFlash;
    public AudioClip weaponShootClip;
    public AudioClip weaponReloadClip;

    // These two variables control the spread of the cone.
    public float scaleLimit;
    public float spreadZDirection;

    public float tracerSpeed;

    public Transform bulletSpawnPoint;
    public int bulletRange;
    public float bulletSpeed;

    private Vector3 originalPosition;

    public Vector3 aimPosition;
    public float aimDownSightSpeed = 8.0f;

    public float maxRecoilZPosition;
    public float minRecoilZPosition;
    public float recoil;
    public float recoilSpeed;

    public enum ShootMode {Auto, Semi};
    public ShootMode shootingMode;

    public int bulletsPerMag = 30;
    public int bulletsLeft = 200;

    public float damage = 5.0f;

    public int currentBullets;

    private bool isReloading;
    private bool shootInput;

    public float fireDelay = 0.1f; // The delay between each shoot.

    private float fireTimer; // Time counter for the delay.

    void Awake()
    {
        originalPosition = transform.localPosition;
    }

    // Use this for initialization
    void Start ()
    {
        weaponAnim = GetComponent<Animator>();
        weaponAudioSource = GetComponent<AudioSource>();
        currentBullets = bulletsPerMag;
    }
	
	// Update is called once per frame
	void Update ()
    {
        InputDevice device = InputManager.ActiveDevice;

        switch(shootingMode)
        {
            case ShootMode.Auto:
                shootInput = device.RightBumper;
                break;
            case ShootMode.Semi:
                shootInput = device.RightBumper;
                break;
        }

        if (device.Action3)
        {
            if (currentBullets < bulletsPerMag && thereAreBulletsLeft())
                playReloadAnimation();
        }

        if (fireTimer < fireDelay)
            fireTimer += Time.deltaTime;

        aimDownSights(device);
    }

    private bool canFire()
    {
        return currentBullets > 0 && !isReloading;
    }

    private bool thereAreBulletsLeft()
    {
        return bulletsLeft > 0;
    }

    public bool isShooting()
    {
        return shootInput;
    }

    void OnEnable()
    {
        transform.localPosition = originalPosition;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    void FixedUpdate()
    {
        AnimatorStateInfo info = weaponAnim.GetCurrentAnimatorStateInfo(0);

        isReloading = info.IsName("Reload");

        if (shootInput)
        {
            if (canFire())
            {
                if (fireTimer > fireDelay)
                {
                    fire();
                    pushBackWeapon();
                }
            }
            else if (thereAreBulletsLeft())
                playReloadAnimation();
        }
    }

    private void aimDownSights(InputDevice device)
    {
        if(device.LeftBumper && !isReloading)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, aimPosition, Time.deltaTime * aimDownSightSpeed);
        }
        else
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, Time.deltaTime * aimDownSightSpeed);
        }
    }

    private void fire()
    {
        StartCoroutine(checkIfHit());

        muzzleFlash.Play();
        weaponAudioSource.PlayOneShot(weaponShootClip);

        currentBullets--;
        fireTimer = 0.0f;
    }

    private IEnumerator checkIfHit()
    {
        Vector3 randomDirection = getRandomBulletDirection();
        RaycastHit hit;

        // Cast an exploratory raycast first from a randomly generated direction.
        if(Physics.Raycast(bulletSpawnPoint.position, randomDirection, out hit, bulletRange))
        {
            // calculate how long it would take the bullet to get to its target.
            float delay = hit.distance / bulletSpeed;
            // calculate the bullet drop at the target.
            Vector3 hitPoint = hit.point;
            hitPoint.y -= delay * 9.8f;
            // calculate the new vector's direction
            Vector3 dir = (hitPoint - bulletSpawnPoint.position).normalized;
            yield return new WaitForSeconds(delay);

            // spawn a bullet tracer from the direction(normalized) that was lowered to simulate a bullet drop.
            StartCoroutine(spawnBulletTracer(dir));

            // Now perform the actual shooting.
            if (Physics.Raycast(bulletSpawnPoint.position, dir, out hit, bulletRange))
            {
                GameObject bulletHole = SpawningPool.CreateFromCache(bulletImpact, hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal));
                yield return new WaitForSeconds(3.0f);
                SpawningPool.ReturnToCache(bulletHole);
            }
        }
    }

    private Vector3 getRandomBulletDirection()
    {
        // Generate a random XY point inside a circle.
        Vector3 randomDirection = Random.insideUnitCircle * scaleLimit;
        randomDirection.z = spreadZDirection;
        randomDirection = transform.TransformDirection(randomDirection.normalized);
        return randomDirection;
    }

    private IEnumerator spawnBulletTracer(Vector3 direction)
    {
        // Retrieve from the spawn pool a cached bullet tracer object.
        GameObject bulletTracer = SpawningPool.CreateFromCache(tracer, bulletSpawnPoint.transform.position, bulletSpawnPoint.transform.rotation);

        Rigidbody rigidbodyTracer = bulletTracer.GetComponent<Rigidbody>();

        rigidbodyTracer.velocity = direction * tracerSpeed;

        yield return new WaitForSeconds(1.5f);
        SpawningPool.ReturnToCache(bulletTracer, "Tracer");
        // Send it back to the cache so that it may be used later.
    }


    private void pushBackWeapon()
    {
       Vector3 weaponObjectLocalPosition = transform.localPosition;
       weaponObjectLocalPosition.z = Mathf.Clamp(weaponObjectLocalPosition.z - recoil, minRecoilZPosition, maxRecoilZPosition);
       transform.localPosition = Vector3.Lerp(transform.localPosition, weaponObjectLocalPosition, Time.deltaTime * recoilSpeed);
    }

    public void playReloadAudioClip()
    {
        weaponAudioSource.PlayOneShot(weaponReloadClip);
    }

    public void reloadBullets()
    {
        if (bulletsLeft <= 0) return;

        int bulletsToLoad = bulletsPerMag - currentBullets;
        int bulletsToDeduct = (bulletsLeft >= bulletsToLoad) ? bulletsToLoad : bulletsLeft;

        bulletsLeft -= bulletsToDeduct;
        currentBullets += bulletsToDeduct;
    }

    private void playReloadAnimation()
    {
        if (isReloading) return;

        weaponAnim.CrossFadeInFixedTime("Reload", 0.01f);
    }
}
