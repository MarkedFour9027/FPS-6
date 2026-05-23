using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


[System.Serializable]
public class GunProfile : MonoBehaviour
{
    [SerializeField] private float firingRange = 100f;
    [SerializeField] private float firerate = 5f;
    [SerializeField] private float reloadTime = 5f;
    [SerializeField] private float hitForce = 100f;
    [SerializeField] private int maxBullet = 30;
    [SerializeField] private int currentBullet = 30;
    [SerializeField] private int currentAmmo;
    [SerializeField] private int ammoThreshold = 300;
    [SerializeField] private int usedBullet;
    [SerializeField] private Transform playerVision;
    public float RecoilX;
    public float RecoilY;
    public float RecoilZ;
    public float adsRecoilX;
    public float adsRecoilY;
    public float adsRecoilZ;
    public bool isAim;
    public RecoilScript recoil;

    float nextTimeToFire;
    bool reloading;

    void OnEnable()
    {
        
    }

    void Start()
    {
        currentBullet = maxBullet;
        currentAmmo = ammoThreshold - maxBullet;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentAmmo > currentBullet + currentAmmo)
        {
            currentAmmo = ammoThreshold;
        }
        if(reloading) return;
        if(Mouse.current.leftButton.isPressed && Time.time >= nextTimeToFire)
        {
            Fire();
        }
    }

    void Fire()
    {
        if(currentBullet > 0)
        {
            nextTimeToFire = Time.time + 1f / firerate;
            currentBullet--;
            usedBullet++;
            
            recoil.RecFire();

            Debug.Log("Current Bullet is " + currentBullet);
            RaycastHit hit;

            if(Physics.Raycast(playerVision.position, playerVision.TransformDirection(Vector3.forward), out hit, firingRange))
            {
                Debug.DrawRay(playerVision.position, playerVision.TransformDirection(Vector3.forward) * hit.distance, Color.green);
                Debug.Log("Hit");

                if(hit.rigidbody != null)
                {
                    hit.rigidbody.AddForce (-hit.normal *  hitForce);
                }
            }
        }
        else if(currentBullet == 0)
        {
            StartCoroutine(Reload());
        }
    }

    IEnumerator Reload()
    {
        yield return new WaitForSeconds(.1f);
        reloading = true;
        yield return new WaitForSeconds(reloadTime - .25f);
        currentBullet = maxBullet;
        currentAmmo = currentAmmo - usedBullet;
        reloading = false;
    }
}
