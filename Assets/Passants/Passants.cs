using UnityEngine;

public class Passants : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float speed = 1;
    [SerializeField] public Vector3 direction;
    [SerializeField] public int type = 1; // 1 = normal, 2 = bourre
    [SerializeField] int damage = 1;
    [SerializeField] float changeDirInterval = 2f;
    [Header("Type probabilities (sum should be 1)")]
    [SerializeField] float[] probaType = { 0.9f, 0.1f };

    [Header("Visuals")]
    [Tooltip("Assign your FBX/model prefabs here. They will be instantiated as child.")]
    public GameObject[] modelVariants;
    [Tooltip("Optional accessories for drunk people (bottle, bag...).")]
    public GameObject[] drunkAccessories;
    [SerializeField] Material materialBourre;
    [SerializeField] float delaiAvantBourre = 6f;
    [SerializeField] Vector3 centreScene = Vector3.zero;

    [Header("Drunk visual params")]
    public float drunkBobbingAmplitude = 0.08f;
    public float drunkBobbingSpeed = 2.5f;
    public float drunkYawAmplitude = 10f; // degrees
    public float drunkYawSpeed = 1.7f;

    // runtime
    float timer = 0f;
    float changeTimer = 0f;
    GameObject visualRoot; // instantiated model
    GameObject drunkAccessoryInstance;
    float bobbingPhase = 0f;
    float yawPhase = 0f;
    int randomSeed;
    public AudioClip ouch;

    void Reset()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        randomSeed = Random.Range(1000, 9999);
        bobbingPhase = Random.value * Mathf.PI * 2f;
        yawPhase = Random.value * Mathf.PI * 2f;

        // choose a random model variant if any
        if (modelVariants != null && modelVariants.Length > 0)
        {
            GameObject variant = modelVariants[Random.Range(0, modelVariants.Length)];
            visualRoot = Instantiate(variant, transform);
            visualRoot.transform.localPosition = Vector3.zero;
            visualRoot.transform.localRotation = Quaternion.identity;
            visualRoot.transform.localScale = Vector3.one;
        }

        // initial direction toward center with a small angle variance (same as before)
        Vector3 dir = (centreScene - transform.position).normalized;
        float maxAngle = 45f;
        float angle = Random.Range(-maxAngle, maxAngle);
        direction = Quaternion.Euler(0, angle, 0) * dir;
        direction.y = 0f;

        // determine type by probabilities
        float alea = Random.Range(0f, 1f);
        float cumul = 0f;
        for (int i = 0; i < probaType.Length; i++)
        {
            cumul += probaType[i];
            if (alea <= cumul)
            {
                type = i + 1;
                break;
            }
        }

        // if drunk, apply quick visual differentiation
        if (type == 2)
        {
            ApplyDrunkVisuals();
        }
    }

    void ApplyDrunkVisuals()
    {
        // tint the renderer(s)
        if (materialBourre != null && visualRoot != null)
        {
            // try to find renderers on the visual root and assign the drunk material
            Renderer[] rends = visualRoot.GetComponentsInChildren<Renderer>();
            foreach (var r in rends)
            {
                // clone material instance for this renderer to avoid global override
                r.material = materialBourre;
            }
        }
        // spawn accessory if provided
        if (drunkAccessories != null && drunkAccessories.Length > 0)
        {
            var acc = drunkAccessories[Random.Range(0, drunkAccessories.Length)];
            drunkAccessoryInstance = Instantiate(acc, visualRoot != null ? visualRoot.transform : transform);
            // position roughly at hand or root; better to use a small offset
            drunkAccessoryInstance.transform.localPosition = new Vector3(0.15f, 0.0f, 0.0f);
            drunkAccessoryInstance.transform.localRotation = Quaternion.identity;
            drunkAccessoryInstance.transform.localScale = Vector3.one * 0.6f;
        }

        // slight scale / tint variations (adds variety)
        float s = Random.Range(0.9f, 1.08f);
        if (visualRoot != null) visualRoot.transform.localScale *= s;
    }

    void Update()
    {
        if (visualRoot != null)
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            if (flatVel.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(flatVel.normalized);
                visualRoot.transform.rotation =
                    Quaternion.Slerp(visualRoot.transform.rotation, targetRot, Time.deltaTime * 10f);
            }
        }
        // Movement behavior
        if (type == 1)
        {
            rb.linearVelocity = direction * speed;
        }
        else if (type == 2)
        {
            timer += Time.deltaTime;
            changeTimer += Time.deltaTime;
            if (changeTimer >= delaiAvantBourre)
            {
                // change direction randomly a little (drunk wandering)
                NewDirection();
                changeTimer = 0f;
            }
            rb.linearVelocity = direction * speed;

            // visual swaying + bobbing
            if (visualRoot != null)
            {
                bobbingPhase += Time.deltaTime * drunkBobbingSpeed;
                yawPhase += Time.deltaTime * drunkYawSpeed;
                float bob = Mathf.Sin(bobbingPhase + randomSeed) * drunkBobbingAmplitude;
                float yaw = Mathf.Sin(yawPhase + randomSeed) * drunkYawAmplitude;
                visualRoot.transform.localPosition = new Vector3(0f, bob, 0f);
                visualRoot.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            }
        }
        else
        {
            // fallback
            rb.linearVelocity = direction * speed;
        }

        // bounds check
        if (transform.position.x < GameManager.minX-1 || transform.position.x > GameManager.maxX+1 ||
            transform.position.z > GameManager.maxZ+1 || transform.position.z < GameManager.minZ-1)
        {
            GameManager.Instance.RemovePassant(this);
            Destroy(gameObject, 0f);
            return;
        }
    }

    void NewDirection()
    {
        direction = new Vector3(
            Random.Range(-1f, 1f),
            0f,
            Random.Range(-1f, 1f)).normalized;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<Player>(out var _))
        {
            // hit player
            GameManager.Instance.TakeDamage(damage);
            SoundManager.Instance.PlaySFX(ouch, volume: 20f, pitch: Random.Range(0.9f, 1.1f));
            SoundManager.Instance.SetSFXVolume(1000);
        }
    }
}
