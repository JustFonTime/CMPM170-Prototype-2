using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_2019_3_OR_NEWER
using UnityEngine.VFX; // allows VFX Graph prefabs too
#endif

public class HorizontalCountdown : MonoBehaviour
{
    [Header("Tilt detection (2.5D)")]
    [Tooltip("Degrees of roll around Z that count as 'horizontal'. 70–80 is a good start.")]
    public float horizontalDegrees = 75f;

    [Header("Timer")]
    public float secondsHorizontalToExplode = 5f;

    [Header("Explosion")]
    [Tooltip("Prefab with a ParticleSystem or a VisualEffect component.")]
    public GameObject explosionPrefab;
    public bool reloadSceneOnExplode = true;
    public float restartDelay = 1f;

    [Header("Enemy contact (instant explode)")]
    public bool useEnemyTag = true;
    public string enemyTag = "Enemy";
    public LayerMask enemyLayers; // used if useEnemyTag is false

    [Header("Optional cleanup")]
    [Tooltip("Components to disable when exploding (drag from Player inspector).")]
    public Behaviour[] disableOnExplode;
    public bool disableAllColliders = true;
    public bool hideAllRenderers = true;

    [Header("Debug")]
    public bool logDetails = false;

    float timer;
    float zeroRollZ;   // upright reference captured at Start
    bool exploded = false;

    void Start()
    {
        // Calibrate current rotation as upright baseline.
        zeroRollZ = transform.eulerAngles.z;
    }

    void Update()
    {
        if (exploded) return;

        // Roll around Z (0..180)
        float zTilt = Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z, zeroRollZ));
        bool isHorizontal = zTilt >= horizontalDegrees;

        if (isHorizontal)
        {
            timer += Time.deltaTime;

            if (logDetails)
            {
                float left = Mathf.Max(0f, secondsHorizontalToExplode - timer);
                Debug.Log($"Tilt={zTilt:0.0}°  |  Fallen {timer:0.0}s  |  {left:0.0}s to explode");
            }

            if (timer >= secondsHorizontalToExplode)
            {
                Explode();
            }
        }
        else
        {
            if (timer > 0f && logDetails) Debug.Log("Recovered (tilt under threshold).");
            timer = 0f;
        }
    }

    // --- instant explode on enemy contact ---
    void OnCollisionEnter(Collision other)
    {
        if (exploded) return;
        if (ShouldExplodeFrom(other.gameObject))
            Explode();
    }

    void OnTriggerEnter(Collider other)
    {
        if (exploded) return;
        if (ShouldExplodeFrom(other.gameObject))
            Explode();
    }

    bool ShouldExplodeFrom(GameObject go)
    {
        if (useEnemyTag) return go.CompareTag(enemyTag);
        // layer mode:
        return (enemyLayers.value & (1 << go.layer)) != 0;
    }

    // --- boom ---
    void Explode()
    {
        if (exploded) return;
        exploded = true;

        // FX (ParticleSystem or VFX Graph)
        if (explosionPrefab)
        {
            var fx = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            var ps = fx.GetComponentInChildren<ParticleSystem>(); if (ps) ps.Play();
#if UNITY_2019_3_OR_NEWER
            var vfx = fx.GetComponentInChildren<VisualEffect>(); if (vfx) vfx.Play();
#endif
        }

        // Disable gameplay components you dragged in
        if (disableOnExplode != null)
            foreach (var b in disableOnExplode) if (b) b.enabled = false;

        if (disableAllColliders)
            foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false;

        if (hideAllRenderers)
            foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = false;

        // Halt physics & freeze
        var rb = GetComponent<Rigidbody>();
        if (rb)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
#else
            rb.velocity        = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
#endif
            rb.isKinematic = true;
        }

        if (reloadSceneOnExplode)
            Invoke(nameof(Reload), restartDelay);
    }

    void Reload() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
}
