using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody))]
public class XRGrabNetworkInteractable : XRGrabInteractable
{
    private PhotonView photonView;
    private XRBaseInteractor currentInteractor;
    private Rigidbody rb;
    private int grabCount = 0;

    [Header("Custom Physics Settings")]
    public float customMass = 1.0f;
    public bool useCustomMass = false;

    void Start()
    {
        photonView = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();

        // Custom mass setup
        if (useCustomMass)
        {
            rb.mass = customMass;
        }
    }

    protected override void OnSelectEntered(XRBaseInteractor interactor)
    {
        grabCount++;

        if (grabCount == 1)
        {
            photonView.RequestOwnership();
            currentInteractor = interactor;
        }

        base.OnSelectEntered(interactor);
    }

    protected override void OnSelectExited(XRBaseInteractor interactor)
    {
        grabCount--;

        if (grabCount == 0)
        {
            currentInteractor = null;
        }

        base.OnSelectExited(interactor);
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        if (currentInteractor != null && !photonView.IsMine)
        {
            interactionManager.SelectExit(currentInteractor, this);
            currentInteractor = null;
        }

        base.ProcessInteractable(updatePhase);
    }

    // Update colliders to handle head and body
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Player"))
        {
            Physics.IgnoreCollision(other, GetComponent<Collider>(), true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Player"))
        {
            Physics.IgnoreCollision(other, GetComponent<Collider>(), false);
        }
    }
}
