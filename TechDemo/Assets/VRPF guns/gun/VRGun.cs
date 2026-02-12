using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class VRGun : MonoBehaviour
{
    [Header("gun settings")]
    public bool fullAuto = true;
    public float fireRate = 0.1f;
    public float range = 100f;
    public float muzzleFlashTime = 0.05f;
    public int damage = 1;

    [Header("assign this stuff")]
    public Transform firePoint;
    public GameObject muzzleFlash;
    public AudioSource shootSound;
    public XRGrabInteractable grabInteractable;
    public InputActionReference triggerAction;

    [Header("events")]
    public UnityEvent OnFired;

    [Header("kickback settings")]
    public float recoilForce = 0.3f;
    public float recoilUpwardForce = 0.05f;
    public float recoilTorque = 0.1f;
    public float springStrength = 5000f;
    public float springDamping = 50f;
    public float linearLimit = 0.02f;

    private bool isHeld;
    private bool isShooting;
    private float nextFireTime;
    private Rigidbody rb;
    private ConfigurableJoint joint;
    private Transform attachPoint;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.mass = 1.5f;
        rb.linearDamping = 2f;
        rb.angularDamping = 4f;
    }

    private void OnEnable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnRelease);
        }
    }

    private void OnDisable()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrab);
            grabInteractable.selectExited.RemoveListener(OnRelease);
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        isHeld = true;
        triggerAction.action.Enable();

        attachPoint = args.interactorObject.transform;
        SetupJoint();
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        isHeld = false;
        triggerAction.action.Disable();

        if (joint != null)
            Destroy(joint);
    }

    private void Update()
    {
        if (!isHeld || triggerAction == null) return;

        bool triggerPressed = triggerAction.action.ReadValue<float>() > 0.1f;

        if (fullAuto)
        {
            if (triggerPressed && Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + fireRate;
                Shoot();
            }
        }
        else
        {
            if (triggerPressed && !isShooting)
            {
                isShooting = true;
                Shoot();
            }
            else if (!triggerPressed)
            {
                isShooting = false;
            }
        }
    }

    private void Shoot()
    {
        if (muzzleFlash != null)
        {
            StopAllCoroutines();
            StartCoroutine(MuzzleFlashRoutine());
        }

        if (shootSound != null)
            shootSound.Play();

        OnFired?.Invoke();

        Debug.DrawRay(firePoint.position, firePoint.forward * range, Color.red, 1f);

        if (Physics.Raycast(firePoint.position, firePoint.forward, out RaycastHit hit, range))
        {
            Debug.Log("ray hit: " + hit.collider.name);
            GameObject debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            debugSphere.transform.position = hit.point;
            debugSphere.transform.localScale = Vector3.one * 0.05f;
            Destroy(debugSphere, 0.5f);

            Damageable damageable = hit.collider.GetComponent<Damageable>();
            if (damageable != null)
                damageable.TakeDamage(damage);
        }
        else
        {
            Debug.Log("gun ray hit nothing");
        }

        ApplyRecoil();
    }

    private void ApplyRecoil()
    {
        if (rb == null) return;

        Vector3 recoilDir = -firePoint.forward * recoilForce + firePoint.up * recoilUpwardForce;
        rb.AddForce(recoilDir, ForceMode.Impulse);
        rb.AddTorque(firePoint.right * recoilTorque, ForceMode.Impulse);
    }

    private IEnumerator MuzzleFlashRoutine()
    {
        muzzleFlash.SetActive(true);
        yield return new WaitForSeconds(muzzleFlashTime);
        muzzleFlash.SetActive(false);
    }

    private void SetupJoint()
    {
        if (rb == null || attachPoint == null) return;

        Rigidbody attachRb = attachPoint.GetComponent<Rigidbody>();
        if (attachRb == null)
        {
            attachRb = attachPoint.gameObject.AddComponent<Rigidbody>();
            attachRb.isKinematic = true;
        }

        joint = gameObject.AddComponent<ConfigurableJoint>();
        joint.connectedBody = attachRb;

        joint.xMotion = ConfigurableJointMotion.Limited;
        joint.yMotion = ConfigurableJointMotion.Limited;
        joint.zMotion = ConfigurableJointMotion.Limited;
        joint.angularXMotion = ConfigurableJointMotion.Locked;
        joint.angularYMotion = ConfigurableJointMotion.Locked;
        joint.angularZMotion = ConfigurableJointMotion.Locked;

        JointDrive drive = new JointDrive
        {
            positionSpring = springStrength,
            positionDamper = springDamping,
            maximumForce = Mathf.Infinity
        };
        joint.xDrive = joint.yDrive = joint.zDrive = drive;

        SoftJointLimit limit = new SoftJointLimit { limit = linearLimit };
        joint.linearLimit = limit;
    }
}
