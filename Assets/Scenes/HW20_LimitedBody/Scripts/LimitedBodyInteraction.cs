using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Vuforia;

public sealed class LimitedBodyInteraction : MonoBehaviour
{
    [Header("Spatial References")]
    public Camera interactionCamera;
    public Transform energyCore;
    public Transform snapSocket;
    public Transform platformRoot;
    public Transform markerStart;
    public LineRenderer guideLine;
    public PlaneFinderBehaviour planeFinder;

    [Header("Feedback")]
    public Text statusText;
    public Text distanceText;
    public Text questionText;
    public Renderer coreRenderer;
    public Renderer socketRenderer;
    public Material idleMaterial;
    public Material selectedMaterial;
    public Material nearbyMaterial;
    public Material lockedMaterial;

    [Header("Interaction Tuning")]
    public float snapDistance = 0.14f;
    public float dragHeight = 0.075f;
    public float followSpeed = 14f;
    public float snapSpeed = 7f;
    public float platformRevealSpeed = 6f;

    private Transform initialParent;
    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;
    private Vector3 dragGoal;
    private bool platformPlaced;
    private bool dragging;
    private bool snapped;

    private void Awake()
    {
        if (!interactionCamera)
            interactionCamera = Camera.main;
        if (!planeFinder)
            planeFinder = FindObjectOfType<PlaneFinderBehaviour>();

        initialParent = energyCore.parent;
        initialLocalPosition = energyCore.localPosition;
        initialLocalRotation = energyCore.localRotation;
        dragGoal = energyCore.position;

        if (platformRoot)
        {
            platformRoot.localScale = Vector3.zero;
            platformRoot.gameObject.SetActive(false);
        }

        if (questionText)
            questionText.text = "Can a single touch still feel spatial?";

        SetCoreMaterial(idleMaterial);
        SetStatus("Find the marker, then tap a real surface to place the dock.");
        UpdateFeedback();
    }

    public void NotifyPlatformPlaced(GameObject placedStage)
    {
        platformPlaced = true;
        if (platformRoot)
        {
            platformRoot.gameObject.SetActive(true);
            platformRoot.localScale = Vector3.zero;
        }

        SetStatus("TAP the glowing core, then DRAG it onto the dock socket.");
    }

    private void Update()
    {
        if (platformPlaced && platformRoot && platformRoot.localScale != Vector3.one)
            platformRoot.localScale = Vector3.Lerp(platformRoot.localScale, Vector3.one, Time.deltaTime * platformRevealSpeed);

        ReadPointerInput();

        if (dragging && !snapped)
            energyCore.position = Vector3.Lerp(energyCore.position, dragGoal, Time.deltaTime * followSpeed);

        if (snapped)
        {
            energyCore.position = Vector3.Lerp(energyCore.position, snapSocket.position, Time.deltaTime * snapSpeed);
            energyCore.rotation = Quaternion.Slerp(energyCore.rotation, snapSocket.rotation, Time.deltaTime * snapSpeed);
        }

        UpdateFeedback();
    }

    private void ReadPointerInput()
    {
        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;
            Vector2 touchPosition = touch.position.ReadValue();
            if (touch.press.wasPressedThisFrame)
                PressPointer(touchPosition);
            if (touch.press.isPressed)
                MovePointer(touchPosition);
            if (touch.press.wasReleasedThisFrame)
                EndPointer();
            if (touch.press.isPressed || touch.press.wasPressedThisFrame || touch.press.wasReleasedThisFrame)
                return;
        }

        if (Mouse.current == null)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        if (Mouse.current.leftButton.wasPressedThisFrame)
            PressPointer(mousePosition);
        if (Mouse.current.leftButton.isPressed)
            MovePointer(mousePosition);
        if (Mouse.current.leftButton.wasReleasedThisFrame)
            EndPointer();
    }

    private void PressPointer(Vector2 screenPosition)
    {
        if (!platformPlaced)
        {
            if (planeFinder)
            {
                planeFinder.PerformHitTest(screenPosition);
                SetStatus("Placing the dock on the detected surface...");
            }
            return;
        }

        BeginPointer(screenPosition);
    }

    private void BeginPointer(Vector2 screenPosition)
    {
        if (!interactionCamera || !energyCore)
            return;

        Ray ray = interactionCamera.ScreenPointToRay(screenPosition);
        if (snapped)
        {
            foreach (RaycastHit resetHit in Physics.RaycastAll(ray, 50f))
            {
                if (resetHit.transform == energyCore || resetHit.transform == snapSocket)
                {
                    ResetCore();
                    return;
                }
            }
            return;
        }

        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit, 50f) || hit.transform != energyCore)
            return;

        if (!platformPlaced)
        {
            SetStatus("Place the dock on a detected surface first.");
            return;
        }

        dragging = true;
        energyCore.SetParent(null, true);
        dragGoal = energyCore.position;
        SetCoreMaterial(selectedMaterial);
        SetStatus("DRAG: Move the core across the detected real surface.");
    }

    private void MovePointer(Vector2 screenPosition)
    {
        if (!dragging || !platformRoot || !interactionCamera)
            return;

        Plane movementPlane = new Plane(platformRoot.up, platformRoot.position + platformRoot.up * dragHeight);
        Ray ray = interactionCamera.ScreenPointToRay(screenPosition);
        float distance;
        if (movementPlane.Raycast(ray, out distance))
            dragGoal = ray.GetPoint(distance);
    }

    private void EndPointer()
    {
        if (!dragging)
            return;

        dragging = false;
        float remaining = Vector3.Distance(energyCore.position, snapSocket.position);
        if (remaining <= snapDistance)
        {
            snapped = true;
            SetCoreMaterial(lockedMaterial);
            if (socketRenderer && lockedMaterial)
                socketRenderer.material = lockedMaterial;
            SetStatus("SNAP + LERP: Alignment confirmed. Tap the core to reset.");
        }
        else
        {
            SetCoreMaterial(idleMaterial);
            SetStatus("Move closer: use the line and distance to find the socket.");
        }
    }

    private void ResetCore()
    {
        snapped = false;
        dragging = false;
        energyCore.SetParent(initialParent, false);
        energyCore.localPosition = initialLocalPosition;
        energyCore.localRotation = initialLocalRotation;
        dragGoal = energyCore.position;
        SetCoreMaterial(idleMaterial);
        if (socketRenderer && nearbyMaterial)
            socketRenderer.material = nearbyMaterial;
        SetStatus("Reset complete. TAP and DRAG the core again.");
    }

    private void UpdateFeedback()
    {
        if (!energyCore || !snapSocket)
            return;

        float distance = Vector3.Distance(energyCore.position, snapSocket.position);
        if (guideLine)
        {
            guideLine.enabled = platformPlaced && !snapped;
            guideLine.SetPosition(0, energyCore.position);
            guideLine.SetPosition(1, snapSocket.position);
            Color lineColor = distance <= snapDistance ? new Color(0.2f, 1f, 0.55f) : new Color(0.1f, 0.75f, 1f);
            guideLine.startColor = lineColor;
            guideLine.endColor = lineColor;
        }

        if (distanceText)
        {
            string orientation = Vector3.Dot(energyCore.forward, snapSocket.forward) > 0.75f ? "aligned" : "turning on snap";
            distanceText.text = platformPlaced ? "Distance: " + (distance * 100f).ToString("F1") + " cm  |  Direction: " + orientation : "Distance: waiting for surface placement";
        }

        if (dragging && distance <= snapDistance)
            SetCoreMaterial(nearbyMaterial);
    }

    private void SetCoreMaterial(Material material)
    {
        if (coreRenderer && material && coreRenderer.sharedMaterial != material)
            coreRenderer.material = material;
    }

    private void SetStatus(string message)
    {
        if (statusText)
            statusText.text = message;
    }
}
