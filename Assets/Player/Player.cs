using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Player : MonoBehaviour
{
    public Action OnPowerUpStart;
    public Action OnPowerUpStop;

    [SerializeField]
    private float _speed;
    [SerializeField]
    private Transform _camera;
    [SerializeField]
    private float _powerupDuration;

    private Rigidbody _rigidBody;
    private Coroutine _powerupCoroutine;

    public void PickPowerUp()
    {
        if (_powerupCoroutine != null)

        {

            StopCoroutine(_powerupCoroutine);

        }

        _powerupCoroutine = StartCoroutine(StartPowerUp());

    }

    private IEnumerator StartPowerUp()
    {
        if(OnPowerUpStart != null)
        {
            OnPowerUpStart();
        }
        yield return new WaitForSeconds(_powerupDuration);
        if (OnPowerUpStop != null)

        {

            OnPowerUpStop();

        }

    }


    private void Awake()
    {
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


        Vector3 horizontalDirection = horizontal * _camera.right;

        Vector3 verticalDirection = vertical * _camera.forward;

        verticalDirection.y = 0;

        horizontalDirection.y = 0;


        Vector3 movementDirection = horizontalDirection + verticalDirection;

        _rigidBody.velocity = movementDirection * _speed * Time.fixedDeltaTime;
    }

}