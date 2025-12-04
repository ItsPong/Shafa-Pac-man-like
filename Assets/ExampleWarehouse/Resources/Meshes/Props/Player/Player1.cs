using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player1 : MonoBehaviour
{
    public Action OnPowerUpStart;
    public Action OnPowerUpStop;

    [SerializeField]
    private float _speed;
    [SerializeField]
    private Camera _camera;
    [SerializeField]
    private float _powerupDuration;


    private Rigidbody _rigidbody;
    private Coroutine _powerupCoroutine;
    private bool _isPowerUpActive = false;

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
        _isPowerUpActive = true;
        if (OnPowerUpStart != null)
        {
            OnPowerUpStart();
        }
        yield return new WaitForSeconds(_powerupDuration);
        _isPowerUpActive = false;
        if (OnPowerUpStop != null)
        {
            OnPowerUpStop();
        }
    }


    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        HideAndLockCursor();
    }

    private void HideAndLockCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 horizontalDirection = horizontal * _camera.transform.right;
        Vector3 verticalDirection = vertical * _camera.transform.forward;
        verticalDirection.y = 0;
        horizontalDirection.y = 0;


        Vector3 movementDirection = horizontalDirection + verticalDirection;

        _rigidbody.velocity = movementDirection * _speed * Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_isPowerUpActive)
        {
            if (collision.gameObject.CompareTag("Enemy"))
            {
                // collision.gameObject.GetComponent<Enemy>().Dead();
            }
        }
    }

}
