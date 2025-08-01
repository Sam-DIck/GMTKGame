using UnityEngine;
using UnityEngine.Serialization;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
	[FormerlySerializedAs("MoveSpeed")]
	[Header("Player")]
	[Tooltip("Move speed of the character in m/s")]
	public float moveSpeed = 4.0f;
	[FormerlySerializedAs("SprintSpeed")] [Tooltip("Sprint speed of the character in m/s")]
	public float sprintSpeed = 6.0f;
	[FormerlySerializedAs("MaxForce")] [Tooltip("Maximum force for accelerating and decelerating")]
	public float maxForce = 6.0f;
	[FormerlySerializedAs("RotationSpeed")] [Tooltip("Rotation speed of the character")]
	public float rotationSpeed = 1.0f;
	

	[FormerlySerializedAs("JumpHeight")]
	[Space(10)]
	[Tooltip("The height the player can jump")]
	public float jumpHeight = 1.2f;

	[FormerlySerializedAs("JumpTimeout")]
	[Space(10)]
	[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
	public float jumpTimeout = 0.1f;
	[FormerlySerializedAs("FallTimeout")] [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
	public float fallTimeout = 0.15f;

	[FormerlySerializedAs("Grounded")]
	[Header("Player Grounded")]
	[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
	public bool grounded = true;
	[FormerlySerializedAs("GroundedOffset")] [Tooltip("Useful for rough ground")]
	public float groundedOffset = -0.14f;
	[FormerlySerializedAs("GroundedRadius")] [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
	public float groundedRadius = 0.5f;
	[FormerlySerializedAs("GroundLayers")] [Tooltip("What layers the character uses as ground")]
	public LayerMask groundLayers;

	[FormerlySerializedAs("CinemachineCameraTarget")]
	[Header("Cinemachine")]
	[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
	public GameObject cinemachineCameraTarget;
	[FormerlySerializedAs("TopClamp")] [Tooltip("How far in degrees can you move the camera up")]
	public float topClamp = 90.0f;
	[FormerlySerializedAs("BottomClamp")] [Tooltip("How far in degrees can you move the camera down")]
	public float bottomClamp = -90.0f;

	// cinemachine
	private float _cinemachineTargetPitch;
	
	// timeout delta time
	private float _jumpTimeoutDelta;
	private float _fallTimeoutDelta;

	
	private PlayerInput _playerInput;
	private StarterAssets.StarterAssetsInputs _input;
	private GameObject _mainCamera;
	private Rigidbody _rigidbody;

	private const float _threshold = 0.01f;
	private float _rotationVelocity;

	private bool IsCurrentDeviceMouse
	{
		get
		{
			#if ENABLE_INPUT_SYSTEM
			return _playerInput.currentControlScheme == "KeyboardMouse";
			#else
			return false;
			#endif
		}
	}

	private void Awake()
	{
		// get a reference to our main camera
		if (_mainCamera == null)
		{
			_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
		}
	}

	private void Start()
	{
		_input = GetComponent<StarterAssets.StarterAssetsInputs>();
		_playerInput = GetComponent<PlayerInput>();
		_rigidbody = GetComponent<Rigidbody>();

		// reset our timeouts on start
		_jumpTimeoutDelta = jumpTimeout;
		_fallTimeoutDelta = fallTimeout;
	}

	private void FixedUpdate()
	{
		Jump();
		GroundedCheck();
		Move();
	}

	private void LateUpdate()
	{
		CameraRotation();
	}

	private void GroundedCheck()
	{
		// set sphere position, with offset
		Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z);
		grounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);
	}

	private void CameraRotation()
	{
		// if there is an input
		if (_input.look.sqrMagnitude >= _threshold)
		{
			//Don't multiply mouse input by Time.deltaTime
			float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
			
			_cinemachineTargetPitch += _input.look.y * rotationSpeed * deltaTimeMultiplier;
			_rotationVelocity = _input.look.x * rotationSpeed * deltaTimeMultiplier;

			// clamp our pitch rotation
			_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, bottomClamp, topClamp);

			// Update Cinemachine camera target pitch
			cinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

			// rotate the player left and right
			transform.Rotate(Vector3.up * _rotationVelocity);
		}
	}

	private void Move()
	{
		// set target speed based on move speed, sprint speed and if sprint is pressed
		float targetSpeed = _input.sprint ? sprintSpeed : moveSpeed;
		if (_input.move == Vector2.zero) targetSpeed = 0.0f;
		
		Vector3 right = Vector3.ProjectOnPlane( _mainCamera.transform.right,Vector3.up).normalized;
		Vector3 forward = Vector3.ProjectOnPlane(_mainCamera.transform.forward, Vector3.up).normalized;
		Vector3 targetVelocity = right * _input.move.x + forward * _input.move.y;
		targetVelocity *= targetSpeed;

		Vector3 current = Vector3.ProjectOnPlane(_rigidbody.linearVelocity, Vector3.up);
		Vector3 delta = targetVelocity - current;

		Vector3 force = Vector3.ClampMagnitude(delta, 1) * maxForce;
		
		_rigidbody.AddForce(force);
	}

	private void Jump()
	{
		if (grounded)
		{
			// reset the fall timeout timer
			_fallTimeoutDelta = fallTimeout;
			// Jump
			if (_input.jump && _jumpTimeoutDelta <= 0.0f)
			{
				// the square root of H * -2 * G = how much velocity needed to reach desired height
				float velocity = Mathf.Sqrt(2 * Physics.gravity.magnitude * jumpHeight);
				Vector3 impulse = velocity * Vector3.up;
				_rigidbody.AddForce(impulse,ForceMode.VelocityChange);
				_jumpTimeoutDelta = jumpTimeout;
			}

			// jump timeout
			if (_jumpTimeoutDelta >= 0.0f)
			{
				_jumpTimeoutDelta -= Time.deltaTime;
			}
		}
		else
		{
			// reset the jump timeout timer
			_jumpTimeoutDelta = jumpTimeout;

			// fall timeout
			if (_fallTimeoutDelta >= 0.0f)
			{
				_fallTimeoutDelta -= Time.deltaTime;
			}

			// if we are not grounded, do not jump
			_input.jump = false;
		}
	}

	private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
	{
		if (lfAngle < -360f) lfAngle += 360f;
		if (lfAngle > 360f) lfAngle -= 360f;
		return Mathf.Clamp(lfAngle, lfMin, lfMax);
	}

	private void OnDrawGizmosSelected()
	{
		Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
		Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);
		
		Gizmos.color = grounded?  transparentGreen : transparentRed;

		// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
		Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z), groundedRadius);
	}
}
