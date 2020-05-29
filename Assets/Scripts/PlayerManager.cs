using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private UIManger _uiManger;
    [SerializeField] private float _velocity = 1;
    [SerializeField] private AudioSource _audioSource;
    private Rigidbody2D _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _rb.velocity = Vector2.up * _velocity;
            _audioSource.Play();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        _uiManger.GameOver();
        _rb.gameObject.SetActive(false);
    }

}
