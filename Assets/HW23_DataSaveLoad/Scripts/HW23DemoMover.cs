using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public sealed class HW23DemoMover : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private Transform selectedTarget;

    private float nextInputLogTime;

    public Transform SelectedTarget => selectedTarget;

    public void SelectTarget(Transform target)
    {
        selectedTarget = target;
        Debug.Log("[HW23 Mover] Selected target: " + (target != null ? target.name : "None"));
    }

    private void Update()
    {
        if (selectedTarget == null)
        {
            return;
        }

        Vector3 move = Vector3.zero;
        float turn = 0f;
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) move += Vector3.forward;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) move += Vector3.back;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) move += Vector3.left;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) move += Vector3.right;
            if (keyboard.rKey.isPressed) move += Vector3.up;
            if (keyboard.fKey.isPressed) move += Vector3.down;
            if (keyboard.qKey.isPressed) turn -= 1f;
            if (keyboard.eKey.isPressed) turn += 1f;
        }
#else
        if (IsPressed(KeyCode.W) || IsPressed(KeyCode.UpArrow)) move += Vector3.forward;
        if (IsPressed(KeyCode.S) || IsPressed(KeyCode.DownArrow)) move += Vector3.back;
        if (IsPressed(KeyCode.A) || IsPressed(KeyCode.LeftArrow)) move += Vector3.left;
        if (IsPressed(KeyCode.D) || IsPressed(KeyCode.RightArrow)) move += Vector3.right;
        if (IsPressed(KeyCode.R)) move += Vector3.up;
        if (IsPressed(KeyCode.F)) move += Vector3.down;
        if (IsPressed(KeyCode.Q)) turn -= 1f;
        if (IsPressed(KeyCode.E)) turn += 1f;
#endif

        if (move.sqrMagnitude > 0f)
        {
            selectedTarget.Translate(move.normalized * moveSpeed * Time.deltaTime, Space.World);

            if (Time.unscaledTime >= nextInputLogTime)
            {
                Debug.Log("[HW23 Mover] Moving " + selectedTarget.name + " to " + selectedTarget.position);
                nextInputLogTime = Time.unscaledTime + 0.5f;
            }
        }

        if (!Mathf.Approximately(turn, 0f))
        {
            selectedTarget.Rotate(Vector3.up, turn * rotationSpeed * Time.deltaTime, Space.World);

            if (Time.unscaledTime >= nextInputLogTime)
            {
                Debug.Log("[HW23 Mover] Rotating " + selectedTarget.name + " to " + selectedTarget.eulerAngles);
                nextInputLogTime = Time.unscaledTime + 0.5f;
            }
        }
    }

    public void MoveSelectedForward()
    {
        MoveSelected(Vector3.forward);
    }

    public void MoveSelectedBack()
    {
        MoveSelected(Vector3.back);
    }

    public void MoveSelectedLeft()
    {
        MoveSelected(Vector3.left);
    }

    public void MoveSelectedRight()
    {
        MoveSelected(Vector3.right);
    }

    public void RotateSelectedLeft()
    {
        RotateSelected(-15f);
    }

    public void RotateSelectedRight()
    {
        RotateSelected(15f);
    }

    private void MoveSelected(Vector3 direction)
    {
        if (selectedTarget != null)
        {
            selectedTarget.Translate(direction, Space.World);
        }
    }

    private void RotateSelected(float degrees)
    {
        if (selectedTarget != null)
        {
            selectedTarget.Rotate(Vector3.up, degrees, Space.World);
        }
    }

    private bool IsPressed(KeyCode keyCode)
    {
        return Input.GetKey(keyCode);
    }
}
