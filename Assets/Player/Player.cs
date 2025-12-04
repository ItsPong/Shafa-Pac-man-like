using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public Action OnPowerUpStart;
    public Action OnPowerUpStop;

    [Header("Movement")]
    [SerializeField] private float _speed;
    [SerializeField] private float _rotationTime = 0.1f;

    [Header("Camera")]
    [SerializeField] private Transform _camera;

    [Header("Combat & PowerUp")]
    [SerializeField] private float _powerupDuration;

    [Header("Health")]
    [SerializeField] private int _health;
    [SerializeField] private TMP_Text _healthText;

    [Header("Respawn")]
    [SerializeField] private List<Transform> _respawnPoints; // multiple respawn
    [SerializeField] private float _enemyCheckRadius = 3f;   // radius aman

    [Header("Animation")]
    [SerializeField] private Animator _animator;

    private Rigidbody _rigidBody;
    private Coroutine _powerupCoroutine;
    private bool _isPowerUpActive = false;
    private float _rotationVelocity;


    // -------------------------------------------------------
    //                  DEAD & RESPAWN LOGIC
    // -------------------------------------------------------
    public void Dead()
    {
        _health -= 1;

        if (_health > 0)
        {
            Transform chosenRespawn = ChooseSafeRespawnPoint();

            transform.position = chosenRespawn.position;
        }
        else
        {
            _health = 0;
            SceneManager.LoadScene("LoseScreen");
        }

        UpdateUI();
    }

    // -------------------------------------------------------
    //   MENENTUKAN RESPAWN POINT MENGGUNAKAN LOGIKA SAFE
    // -------------------------------------------------------
    private Transform ChooseSafeRespawnPoint()
    {
        List<Transform> safeList = new List<Transform>();
        List<(Transform point, float minEnemyDistance)> dangerList = new List<(Transform, float)>();

        Enemy[] enemies = FindObjectsOfType<Enemy>();

        foreach (Transform respawn in _respawnPoints)
        {
            bool unsafePoint = false;
            float closestEnemy = Mathf.Infinity;

            foreach (Enemy enemy in enemies)
            {
                float dist = Vector3.Distance(enemy.transform.position, respawn.position);

                if (dist < closestEnemy)
                    closestEnemy = dist;

                // enemy terlalu dekat → respawn dianggap bahaya
                if (dist <= _enemyCheckRadius)
                {
                    unsafePoint = true;
                }
            }

            if (!unsafePoint)
            {
                // aman → masuk list aman
                safeList.Add(respawn);
            }
            else
            {
                // tidak aman → masukkan ke list bahaya untuk fallback
                dangerList.Add((respawn, closestEnemy));
            }
        }

        // Jika ada respawn point aman → pilih salah satu random
        if (safeList.Count > 0)
        {
            return safeList[UnityEngine.Random.Range(0, safeList.Count)];
        }

        // Semua respawn point bahaya → pilih yang musuhnya PALING JAUH
        float maxDist = -Mathf.Infinity;
        Transform safest = null;

        foreach (var item in dangerList)
        {
            if (item.minEnemyDistance > maxDist)
            {
                maxDist = item.minEnemyDistance;
                safest = item.point;
            }
        }

        return safest;
    }


    // -------------------------------------------------------
    //               POWERUP & MOVEMENT SYSTEM
    // -------------------------------------------------------

    public void PickPowerUp()
    {
        if (_powerupCoroutine != null)
            StopCoroutine(_powerupCoroutine);

        _powerupCoroutine = StartCoroutine(StartPowerUp());
    }


    private IEnumerator StartPowerUp()
    {
        _isPowerUpActive = true;
        OnPowerUpStart?.Invoke();

        yield return new WaitForSeconds(_powerupDuration);

        _isPowerUpActive = false;
        OnPowerUpStop?.Invoke();
    }


    private void Awake()
    {
        UpdateUI();
        _rigidBody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movementDirection = new Vector3(horizontal, 0, vertical);

        if (movementDirection.magnitude >= 0.1f)
        {
            float rotationAngle = Mathf.Atan2(movementDirection.x, movementDirection.z) * Mathf.Rad2Deg + _camera.eulerAngles.y;
            float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationAngle, ref _rotationVelocity, _rotationTime);
            transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
            movementDirection = Quaternion.Euler(0f, rotationAngle, 0f) * Vector3.forward;
        }

        _rigidBody.velocity = movementDirection * _speed * Time.deltaTime;

        _animator?.SetFloat("Velocity", _rigidBody.velocity.magnitude);
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (_isPowerUpActive && collision.gameObject.CompareTag("Enemy"))
        {
            collision.gameObject.GetComponent<Enemy>().Dead();
        }
    }

    private void UpdateUI()
    {
        _healthText.text = "Health: " + _health;
    }
}
