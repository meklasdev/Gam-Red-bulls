using UnityEngine;
using ExtremeRacing.Infrastructure;

namespace ExtremeRacing.Vehicles
{
    [RequireComponent(typeof(Rigidbody))]
    public class GokartController : MonoBehaviour
    {
        [Header("Gokart Settings")]
        [SerializeField] private VehicleSpec _spec;
        [SerializeField] private float _maxSpeed = 25f;
        [SerializeField] private float _acceleration = 12f;
        [SerializeField] private float _steering = 35f;
        [SerializeField] private float _brakeForce = 30f;

        [Header("Gokart Physics")]
        [SerializeField] private float _downforce = 10f;
        [SerializeField] private float _wheelGrip = 1.2f;
        [SerializeField] private float _centerOfMassOffset = -0.3f;
        [SerializeField] private float _stabilityFactor = 0.8f;

        [Header("Drift System")]
        [SerializeField] private float _driftThreshold = 0.3f;
        [SerializeField] private float _driftForce = 15f;
        [SerializeField] private float _driftStabilization = 0.7f;
        [SerializeField] private bool _allowDrifting = true;

        [Header("Turbo System")]
        [SerializeField] private bool _hasTurbo = true;
        [SerializeField] private float _turboMultiplier = 1.8f;
        [SerializeField] private float _turboDuration = 2.5f;
        [SerializeField] private float _turboCooldown = 8f;

        [Header("Gokart Type")]
        [SerializeField] private GokartType _gokartType = GokartType.Sport;

        private Rigidbody _rigidbody;
        private InputManager _inputManager;

        // Physics state
        private float _motor;
        private float _steering_input;
        private float _brake;
        private bool _isDrifting = false;
        private float _currentSpeed;
        private Vector3 _velocity;

        // Turbo system
        private bool _turboActive = false;
        private float _turboTimer = 0f;
        private float _turboCooldownTimer = 0f;

        // Gokart specific
        private float _rpmSimulation = 0f;
        private bool _isOnGround = true;
        private float _airTime = 0f;

        public enum GokartType
        {
            Recreational, // Wolny, stabilny
            Sport,        // Balanced
            Racing,       // Szybki, trudny
            Electric,     // Instant torque
            Offroad       // Wyższa clearance
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _inputManager = InputManager.Instance;
            
            SetupGokartPhysics();
            ConfigureGokartByType();
        }

        private void Start()
        {
            ApplyVehicleSpec();
        }

        private void Update()
        {
            HandleInput();
            UpdateTurbo();
            CheckGroundContact();
            UpdateRPMSimulation();
        }

        private void FixedUpdate()
        {
            HandleMotor();
            HandleSteering();
            HandleBraking();
            HandleDrift();
            ApplyDownforce();
            UpdateVelocity();
        }

        private void SetupGokartPhysics()
        {
            // Center of mass nisko dla stabilności
            _rigidbody.centerOfMass = new Vector3(0, _centerOfMassOffset, 0);
            
            // Gokart physics properties
            _rigidbody.mass = 0.8f; // Lekki
            _rigidbody.drag = 0.3f;
            _rigidbody.angularDrag = 3f;
            
            // Wysokie momentum dla smooth ride
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void ConfigureGokartByType()
        {
            switch (_gokartType)
            {
                case GokartType.Recreational:
                    _maxSpeed = 18f;
                    _acceleration = 8f;
                    _steering = 25f;
                    _stabilityFactor = 1.2f;
                    _allowDrifting = false;
                    break;

                case GokartType.Sport:
                    _maxSpeed = 25f;
                    _acceleration = 12f;
                    _steering = 35f;
                    _stabilityFactor = 0.8f;
                    break;

                case GokartType.Racing:
                    _maxSpeed = 35f;
                    _acceleration = 18f;
                    _steering = 45f;
                    _stabilityFactor = 0.5f;
                    _downforce = 15f;
                    break;

                case GokartType.Electric:
                    _maxSpeed = 28f;
                    _acceleration = 20f; // Instant torque
                    _steering = 30f;
                    _hasTurbo = false; // Electric boost instead
                    break;

                case GokartType.Offroad:
                    _maxSpeed = 22f;
                    _acceleration = 10f;
                    _steering = 28f;
                    _wheelGrip = 1.5f; // Better traction
                    transform.position += Vector3.up * 0.1f; // Higher clearance
                    break;
            }
        }

        private void ApplyVehicleSpec()
        {
            if (_spec != null)
            {
                _maxSpeed = _spec.maxSpeedKmh / 3.6f; // Convert to m/s
                _acceleration = _spec.acceleration;
                
                // Override some values with gokart-specific limits
                _maxSpeed = Mathf.Clamp(_maxSpeed, 15f, 40f);
                _acceleration = Mathf.Clamp(_acceleration, 8f, 25f);
            }
        }

        private void HandleInput()
        {
            if (_inputManager == null) return;

            _motor = _inputManager.Throttle;
            _steering_input = _inputManager.Steer;
            _brake = _inputManager.Brake;

            // Turbo/Boost
            if (_inputManager.Boost && _hasTurbo && _turboCooldownTimer <= 0f && !_turboActive)
            {
                ActivateTurbo();
            }

            // Handbrake for sharper turns
            if (_inputManager.Handbrake)
            {
                _brake = Mathf.Max(_brake, 0.5f);
                _steering_input *= 1.3f; // Sharper steering z handbrake
            }
        }

        private void HandleMotor()
        {
            float speedMultiplier = _turboActive ? _turboMultiplier : 1f;
            float effectiveAcceleration = _acceleration * speedMultiplier;
            
            // Electric instant torque
            if (_gokartType == GokartType.Electric)
            {
                effectiveAcceleration *= (1f + (1f - _currentSpeed / _maxSpeed) * 0.5f);
            }

            Vector3 forwardForce = transform.forward * (_motor * effectiveAcceleration);
            
            // Apply only if on ground
            if (_isOnGround)
            {
                _rigidbody.AddForce(forwardForce, ForceMode.Acceleration);
            }

            // Speed limiting
            _currentSpeed = Vector3.Dot(_rigidbody.velocity, transform.forward);
            if (_currentSpeed > _maxSpeed * speedMultiplier)
            {
                Vector3 limitedVelocity = _rigidbody.velocity;
                limitedVelocity = Vector3.ClampMagnitude(limitedVelocity, _maxSpeed * speedMultiplier);
                _rigidbody.velocity = limitedVelocity;
            }
        }

        private void HandleSteering()
        {
            if (!_isOnGround) return;

            float steerInput = _steering_input * _steering;
            
            // Speed-dependent steering (slower at high speed)
            float speedFactor = 1f - (_currentSpeed / _maxSpeed) * 0.3f;
            steerInput *= speedFactor;

            // Apply steering torque
            _rigidbody.AddTorque(Vector3.up * steerInput * _stabilityFactor, ForceMode.Acceleration);
            
            // Front wheel steering effect (slide sideways)
            Vector3 steerForce = transform.right * (steerInput * 0.5f);
            _rigidbody.AddForceAtPosition(steerForce, transform.position + transform.forward * 0.5f, ForceMode.Acceleration);
        }

        private void HandleBraking()
        {
            if (_brake > 0f && _isOnGround)
            {
                Vector3 brakeForceVector = -_rigidbody.velocity * (_brake * _brakeForce);
                _rigidbody.AddForce(brakeForceVector, ForceMode.Acceleration);
                
                // Brake sound/particles można dodać tutaj
            }
        }

        private void HandleDrift()
        {
            if (!_allowDrifting || !_isOnGround) return;

            // Calculate drift angle
            Vector3 velocityDirection = _rigidbody.velocity.normalized;
            float driftAngle = Vector3.Angle(transform.forward, velocityDirection);
            
            _isDrifting = driftAngle > _driftThreshold * 90f && _currentSpeed > 5f;

            if (_isDrifting)
            {
                // Drift physics - reduce side friction
                Vector3 sidewaysVelocity = Vector3.Project(_rigidbody.velocity, transform.right);
                Vector3 forwardVelocity = Vector3.Project(_rigidbody.velocity, transform.forward);
                
                // Maintain forward momentum, reduce sideways
                Vector3 driftVelocity = forwardVelocity + (sidewaysVelocity * _driftStabilization);
                _rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, driftVelocity, Time.fixedDeltaTime * _driftForce);
                
                // Drift particles/effects można dodać tutaj
            }
            else
            {
                // Normal grip behavior
                Vector3 sidewaysVelocity = Vector3.Project(_rigidbody.velocity, transform.right);
                Vector3 counterForce = -sidewaysVelocity * _wheelGrip;
                _rigidbody.AddForce(counterForce, ForceMode.Acceleration);
            }
        }

        private void ApplyDownforce()
        {
            if (_isOnGround && _currentSpeed > 5f)
            {
                // Downforce increases with speed
                float speedFactor = (_currentSpeed / _maxSpeed);
                Vector3 downforceVector = -transform.up * (_downforce * speedFactor * speedFactor);
                _rigidbody.AddForce(downforceVector, ForceMode.Acceleration);
            }
        }

        private void UpdateVelocity()
        {
            _velocity = _rigidbody.velocity;
            _currentSpeed = _velocity.magnitude;
        }

        private void CheckGroundContact()
        {
            // Simple ground check
            RaycastHit hit;
            Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
            
            _isOnGround = Physics.Raycast(rayOrigin, Vector3.down, out hit, 0.5f);
            
            if (!_isOnGround)
            {
                _airTime += Time.deltaTime;
            }
            else
            {
                if (_airTime > 0.5f)
                {
                    // Landing effect można dodać tutaj
                    Debug.Log($"[Gokart] Landed after {_airTime:F1}s airtime!");
                }
                _airTime = 0f;
            }
        }

        private void UpdateRPMSimulation()
        {
            // Simulate engine RPM dla audio
            if (_gokartType != GokartType.Electric)
            {
                float targetRPM = 1000f + (_motor * 2000f) + (_currentSpeed / _maxSpeed * 1500f);
                _rpmSimulation = Mathf.Lerp(_rpmSimulation, targetRPM, Time.deltaTime * 5f);
            }
            else
            {
                _rpmSimulation = 0f; // Electric - no RPM
            }
        }

        private void ActivateTurbo()
        {
            _turboActive = true;
            _turboTimer = _turboDuration;
            _turboCooldownTimer = _turboCooldown;
            
            // Turbo effect można dodać tutaj
            Debug.Log("[Gokart] Turbo activated!");
        }

        private void UpdateTurbo()
        {
            if (_turboActive)
            {
                _turboTimer -= Time.deltaTime;
                if (_turboTimer <= 0f)
                {
                    _turboActive = false;
                    Debug.Log("[Gokart] Turbo deactivated!");
                }
            }

            if (_turboCooldownTimer > 0f)
            {
                _turboCooldownTimer -= Time.deltaTime;
            }
        }

        // Public API
        public float CurrentSpeed => _currentSpeed * 3.6f; // Convert to km/h
        public float CurrentSpeedKmh => CurrentSpeed;
        public float MaxSpeedKmh => _maxSpeed * 3.6f;
        public bool IsDrifting => _isDrifting;
        public bool IsOnGround => _isOnGround;
        public bool CanTurbo => _hasTurbo && _turboCooldownTimer <= 0f && !_turboActive;
        public float RPM => _rpmSimulation;
        public GokartType Type => _gokartType;
        public float AirTime => _airTime;

        public void SetInput(float motor, float steering, float brake, bool turbo = false)
        {
            _motor = Mathf.Clamp01(motor);
            _steering_input = Mathf.Clamp(steering, -1f, 1f);
            _brake = Mathf.Clamp01(brake);
            
            if (turbo && CanTurbo)
            {
                ActivateTurbo();
            }
        }

        // Gokart specific actions
        public void ForceJump(float jumpForce = 300f)
        {
            if (_isOnGround)
            {
                _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }

        public void Spin180()
        {
            _rigidbody.AddTorque(Vector3.up * 500f, ForceMode.Impulse);
        }

        private void OnDrawGizmosSelected()
        {
            // Debug visualization
            Gizmos.color = _isOnGround ? Color.green : Color.red;
            Gizmos.DrawWireCube(transform.position, new Vector3(1.2f, 0.5f, 2.2f));
            
            // Velocity arrow
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, _rigidbody ? _rigidbody.velocity : Vector3.zero);
            
            // Steering visualization
            Gizmos.color = Color.yellow;
            float steerAngle = _steering_input * _steering;
            Vector3 steerDirection = Quaternion.Euler(0, steerAngle, 0) * transform.forward;
            Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, steerDirection * 3f);
            
            // Ground check ray
            Gizmos.color = Color.white;
            Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
            Gizmos.DrawRay(rayOrigin, Vector3.down * 0.5f);
        }
    }
}