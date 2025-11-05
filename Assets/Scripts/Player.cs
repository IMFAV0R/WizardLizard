using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float walkSpeed = 2f;
    public float runningSpeed = 5f;

    public PowerManager powerManager;

    private CharacterController controller;
    private Vector3 moveDirection;
    public VoiceManager voiceManager;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        moveDirection = new Vector3(moveX, 0, moveZ);

        if (moveDirection.magnitude > 1)
            moveDirection.Normalize();

        if (Input.GetKey(KeyCode.LeftShift))
        {
            moveSpeed = runningSpeed;
            voiceManager.MakeVoice(transform.position);
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            moveSpeed = walkSpeed;
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            powerManager.Hide();
        }

        if (Input.GetKeyUp(KeyCode.P))
        {
            powerManager.Unhide();
        }

        controller.Move(moveDirection * moveSpeed * Time.deltaTime);
    }
}
