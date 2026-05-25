using System.Collections;
using UnityEngine;

public class GrenadeFunc : MonoBehaviour
{
    public float radius = 5f;
    public float power = 10f;
    public float upwardsModifier = 3f;
    public ForceMode forceMode = ForceMode.Force;
    public GameObject Root;

    private MeshRenderer mesh;

    [Header("Optional Settings")]
    public LayerMask affectedLayers = ~0;

    void Start()
    {
        mesh = gameObject.GetComponent<MeshRenderer>();
    }

    void OnEnable()
    {
        StartCoroutine(Ticker());
    }

    void OnDisable()
    {
        mesh.enabled = !mesh.enabled;
    }

    IEnumerator Ticker()
    {
        yield return new WaitForSeconds(3f);

        ApplyExplosionForce();

        yield return new WaitForSeconds(.1f);
        gameObject.transform.parent = Root.transform;
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localRotation = Quaternion.identity;

        Rigidbody rb = GetComponent<Rigidbody>();

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.Sleep();
        gameObject.SetActive(false);
    }

    public void ApplyExplosionForce()
    {
        Vector3 explosionPosition = transform.position;

        Collider[] colliders =
            Physics.OverlapSphere(
                explosionPosition,
                radius,
                affectedLayers
            );

        foreach (Collider collider in colliders)
        {
            if(collider.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.AddExplosionForce(
                    power,
                    explosionPosition,
                    radius,
                    upwardsModifier,
                    forceMode
                );
            }
        }
        mesh.enabled = !mesh.enabled;
    }

    void OnDrawGizmos()
    {
        Gizmos.color =
            new Color(1f,0.5f,0f,0.5f);

        Gizmos.DrawWireSphere(
            transform.position,
            radius
        );
    }
}