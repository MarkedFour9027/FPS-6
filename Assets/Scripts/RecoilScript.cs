using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecoilScript : MonoBehaviour
{
    public GunProfile gunProfile;

    private Vector3 currentRot;
    private Vector3 targetRot;

    [SerializeField] private float snap = 10f;
    [SerializeField] private float returnSpeed = 6f;

    void Awake()
    {
        if (gunProfile == null)
            gunProfile = GetComponentInChildren<GunProfile>();
    }

    void Update()
    {
        // Return to center
        targetRot = Vector3.Lerp(targetRot, Vector3.zero, returnSpeed * Time.deltaTime);

        // Smooth movement
        currentRot = Vector3.Lerp(currentRot, targetRot, snap * Time.deltaTime);

        transform.localRotation = Quaternion.Euler(currentRot);
    }

    public void RecFire()
    {
        if (gunProfile == null) return;

        bool aiming = gunProfile.isAim;

        float x = aiming ? gunProfile.adsRecoilX : gunProfile.RecoilX;
        float y = aiming ? gunProfile.adsRecoilY : gunProfile.RecoilY;
        float z = aiming ? gunProfile.adsRecoilZ : gunProfile.RecoilZ;

        targetRot += new Vector3(
            Random.Range(0, x),
            Random.Range(-y, y),
            Random.Range(-z, z)
        );
    }
}