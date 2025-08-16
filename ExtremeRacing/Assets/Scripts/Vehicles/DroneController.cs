using UnityEngine;
using ExtremeRacing.Infrastructure;

namespace ExtremeRacing.Vehicles
{
    [RequireComponent(typeof(Rigidbody))]
    public class DroneController : MonoBehaviour
    {
        [Header("Flight Settings")]
        [SerializeField] private float _maxSpeed = 30f;
        [SerializeField] private float _acceleration = 10f;
        [SerializeField] private float _rotationSpeed = 100f;
        [SerializeField] private float _hoverForce = 50f;
        [SerializeField] private float _maxAltitude = 200f;
        [SerializeField] private float _minAltitude = 1f;

        [Header("Drone Physics")]
        [SerializeField] private float _tiltAngle = 15f;
        [SerializeField] private float _bankAngle = 30f;
        [SerializeField] private float _stabilizationForce = 20f;
        [SerializeField] private float _windResistance = 0.1f;

        [Header("Racing Features")]
        [SerializeField] private bool _hasBoost = true;
        [SerializeField] private float _boostMultiplier = 2f;
        [SerializeField] private float _boostDuration = 3f;
        [SerializeField] private float _boostCooldown = 10f;

        [Header("Drone Type")]
        [SerializeField] private DroneType _droneType = DroneType.Racing;

        private Rigidbody _rigidbody;
        private InputManager _inputManager;
        
        // Flight state
        private bool _isHovering = true;
        private float _currentAltitude;
        private float _targetAltitude;
        private Vector3 _velocity;
        
        // Boost system
        private bool _boostActive = false;
        private float _boostTimer = 0f;
        private float _boostCooldownTimer = 0f;
        
        // Rotation helpers
        private float _pitchInput;
        private float _rollInput;
        private float _yawInput;
        private float _throttleInput;

        public enum DroneType
        {
            Racing,      // Szybki, agile
            Surveillance, // Wolny ale stabilny
            Freestyle,   // Akrobatyczny
            Cargo        // Wolny ale strong
        }

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _inputManager = InputManager.Instance;
            
            // Drone physics setup
            _rigidbody.drag = _windResistance;
            _rigidbody.angularDrag = 2f;
            _rigidbody.useGravity = true;
            
            ConfigureDroneByType();
        }

        private void Start()
        {
            _currentAltitude = transform.position.y;
            _targetAltitude = _currentAltitude;
        }

        private void Update()
        {
            HandleInput();
            UpdateBoost();
        }

        private void FixedUpdate()
        {
            HandleFlight();
            HandleRotation();
            HandleStabilization();
            ApplyMovement();
        }

        private void ConfigureDroneByType()
        {
            switch (_droneType)
            {
                case DroneType.Racing:
                    _maxSpeed = 40f;
                    _acceleration = 15f;
                    _rotationSpeed = 120f;
                    _tiltAngle = 25f;
                    break;
                    
                case DroneType.Surveillance:
                    _maxSpeed = 20f;
                    _acceleration = 8f;
                    _rotationSpeed = 60f;
                    _stabilizationForce = 30f;
                    break;
                    
                case DroneType.Freestyle:
                    _maxSpeed = 35f;
                    _acceleration = 12f;
                    _rotationSpeed = 150f;
                    _tiltAngle = 45f;
                    _bankAngle = 60f;
                    break;
                    
                case DroneType.Cargo:
                    _maxSpeed = 15f;
                    _acceleration = 5f;
                    _rotationSpeed = 40f;
                    _rigidbody.mass = 2f;
                    break;
            }
        }

        private void HandleInput()
        {
            if (_inputManager == null) return;

            // Flight controls
            _pitchInput = -_inputManager.Steer; // Lewy stick góra/dół = pitch
            _rollInput = _inputManager.Throttle - _inputManager.Brake; // Prawy stick lewo/prawo = roll
            _yawInput = _inputManager.Steer; // Rotation
            _throttleInput = _inputManager.Throttle; // Altitude

            // Boost
            if (_inputManager.Boost && _hasBoost && _boostCooldownTimer <= 0f && !_boostActive)
            {
                ActivateBoost();
            }

            // Hover toggle
            if (_inputManager.Handbrake)
            {
                _isHovering = !_isHovering;
            }
        }

        private void HandleFlight()
        {
            // Podstawowa siła nośna (przeciw grawitacji)
            Vector3 liftForce = Vector3.up * _hoverForce;
            
            if (_isHovering)
            {
                // Hover mode - maintain altitude
                float altitudeDiff = _targetAltitude - transform.position.y;
                liftForce += Vector3.up * (altitudeDiff * _stabilizationForce);
            }
            else
            {
                // Free flight mode
                liftForce += Vector3.up * (_throttleInput * _hoverForce * 0.5f);
            }

            // Forward/backward movement
            Vector3 forwardForce = transform.forward * (_pitchInput * _acceleration);
            
            // Side movement  
            Vector3 sideForce = transform.right * (_rollInput * _acceleration * 0.7f);

            // Apply boost
            float speedMultiplier = _boostActive ? _boostMultiplier : 1f;
            
            Vector3 totalForce = (liftForce + forwardForce + sideForce) * speedMultiplier;
            _rigidbody.AddForce(totalForce, ForceMode.Force);

            // Speed limiting
            if (_rigidbody.velocity.magnitude > _maxSpeed * speedMultiplier)
            {
                _rigidbody.velocity = _rigidbody.velocity.normalized * _maxSpeed * speedMultiplier;
            }

            // Altitude limiting
            if (transform.position.y > _maxAltitude)
            {
                _rigidbody.AddForce(Vector3.down * _hoverForce * 2f, ForceMode.Force);
            }
            else if (transform.position.y < _minAltitude)
            {
                _rigidbody.AddForce(Vector3.up * _hoverForce * 2f, ForceMode.Force);
            }
        }

        private void HandleRotation()
        {
            // Yaw (obrót wokół osi Y)
            float yawTorque = _yawInput * _rotationSpeed;
            _rigidbody.AddTorque(Vector3.up * yawTorque, ForceMode.Force);

            // Pitch (przechylenie do przodu/tyłu)
            float pitchTorque = _pitchInput * _tiltAngle;
            _rigidbody.AddTorque(transform.right * pitchTorque, ForceMode.Force);

            // Roll (przechylenie na boki)
            float rollTorque = _rollInput * _bankAngle;
            _rigidbody.AddTorque(transform.forward * rollTorque, ForceMode.Force);
        }

        private void HandleStabilization()
        {
            if (_isHovering)
            {
                // Auto-stabilization w hover mode
                Vector3 stability = Vector3.zero;
                
                // Wyrównaj pitch i roll
                float currentPitch = Vector3.Dot(transform.forward, Vector3.down);
                float currentRoll = Vector3.Dot(transform.right, Vector3.down);
                
                stability += transform.right * (-currentPitch * _stabilizationForce);
                stability += transform.forward * (-currentRoll * _stabilizationForce);
                
                _rigidbody.AddTorque(stability, ForceMode.Force);
            }
        }

        private void ApplyMovement()
        {
            _currentAltitude = transform.position.y;
            _velocity = _rigidbody.velocity;

            // Wind effect simulation
            if (_droneType != DroneType.Cargo)
            {
                Vector3 windEffect = new Vector3(
                    Mathf.Sin(Time.time * 0.5f) * 0.5f,
                    Mathf.Sin(Time.time * 0.3f) * 0.3f,
                    Mathf.Cos(Time.time * 0.4f) * 0.4f
                );
                _rigidbody.AddForce(windEffect, ForceMode.Force);
            }
        }

        private void ActivateBoost()
        {
            _boostActive = true;
            _boostTimer = _boostDuration;
            _boostCooldownTimer = _boostCooldown;
            
            // Visual/audio effect można dodać tutaj
            Debug.Log("[Drone] Boost activated!");
        }

        private void UpdateBoost()
        {
            if (_boostActive)
            {
                _boostTimer -= Time.deltaTime;
                if (_boostTimer <= 0f)
                {
                    _boostActive = false;
                    Debug.Log("[Drone] Boost deactivated!");
                }
            }

            if (_boostCooldownTimer > 0f)
            {
                _boostCooldownTimer -= Time.deltaTime;
            }
        }

        // Public API
        public bool IsFlying => transform.position.y > _minAltitude + 1f;
        public bool IsHovering => _isHovering;
        public float CurrentSpeed => _rigidbody.velocity.magnitude;
        public float Altitude => _currentAltitude;
        public bool CanBoost => _hasBoost && _boostCooldownTimer <= 0f && !_boostActive;
        public DroneType Type => _droneType;

        public void SetHover(bool hover)
        {
            _isHovering = hover;
            if (hover)
            {
                _targetAltitude = transform.position.y;
            }
        }

        public void SetTargetAltitude(float altitude)
        {
            _targetAltitude = Mathf.Clamp(altitude, _minAltitude, _maxAltitude);
            _isHovering = true;
        }

        // Drone stunts dla freestyle
        public void PerformBarrelRoll()
        {
            if (_droneType == DroneType.Freestyle)
            {
                _rigidbody.AddTorque(transform.forward * 1000f, ForceMode.Impulse);
            }
        }

        public void PerformLoop()
        {
            if (_droneType == DroneType.Freestyle)
            {
                _rigidbody.AddTorque(transform.right * 800f, ForceMode.Impulse);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Debug visualization
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 2f);
            
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * 5f);
            
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 3f);
            
            // Altitude limits
            Gizmos.color = Color.blue;
            Vector3 pos = transform.position;
            Gizmos.DrawLine(new Vector3(pos.x - 5, _maxAltitude, pos.z), new Vector3(pos.x + 5, _maxAltitude, pos.z));
            Gizmos.DrawLine(new Vector3(pos.x - 5, _minAltitude, pos.z), new Vector3(pos.x + 5, _minAltitude, pos.z));
        }
    }
}