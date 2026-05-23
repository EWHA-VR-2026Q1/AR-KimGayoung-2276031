using UnityEngine;
using UnityEngine.UI;

public sealed class CanvasModeComparisonController : MonoBehaviour
{
    public Text overlayExplanation;
    public Text cameraExplanation;
    public Text worldObservation;
    public Text overlayButtonLabel;
    public Text cameraButtonLabel;
    public Text worldButtonLabel;
    public Text selectionStatus;

    private const string OverlayDefault = "Fixed to display / ignores AR camera";
    private const string CameraDefault = "ARCamera renders this view-following panel";
    private const string WorldDefault = "Move closer: my apparent size changes";

    private void Awake()
    {
        ShowIntro();
    }

    public void SelectOverlay()
    {
        ResetText();
        overlayExplanation.text = "SELECTED: fixed pixels, unchanged by movement";
        overlayButtonLabel.text = "SELECTED";
        selectionStatus.text = "OVERLAY: this information remains attached to the phone screen.";
    }

    public void SelectCamera()
    {
        ResetText();
        cameraExplanation.text = "SELECTED: rendered through the AR Camera";
        cameraButtonLabel.text = "SELECTED";
        selectionStatus.text = "CAMERA: this panel follows the camera view through its render plane.";
    }

    public void SelectWorld()
    {
        ResetText();
        worldObservation.text = "SELECTED: anchored in marker space";
        worldButtonLabel.text = "SELECTED";
        selectionStatus.text = "WORLD: move the phone and observe position, scale, and angle changes.";
    }

    private void ShowIntro()
    {
        ResetText();
        selectionStatus.text = "Tap a mode to compare how its UI belongs to screen, camera, or reality.";
    }

    private void ResetText()
    {
        overlayExplanation.text = OverlayDefault;
        cameraExplanation.text = CameraDefault;
        worldObservation.text = WorldDefault;
        overlayButtonLabel.text = "CHECK HUD";
        cameraButtonLabel.text = "FOCUS VIEW";
        worldButtonLabel.text = "INSPECT IN SPACE";
    }
}