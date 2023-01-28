using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    [SerializeField] private Vector3 _runTrigger = new Vector3(3, 3);
    [SerializeField] private Transform _player;
    private Vector3 _playerPosition;
    private Rigidbody2D _rb;

    [SerializeField] private float _speed = 4f;
    [SerializeField] private int _maxHealth = 5;
    private int _currentHealth;

    private float _dazedTime;
    [SerializeField] float _startDazedTime;

    private SpriteRenderer _sprite;
    private bool isGrounded = false;
    public static Vector3 _start_position;
    private Animator _animator;

   
    public float deltaAlpha = 1f;
    private void FixedUpdate()
    {
        CheckGround();
    }
    private void Start()
    {
        _currentHealth = _maxHealth;
        _start_position = transform.position;
        _playerPosition = _player.position;
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponentInChildren<Animator>();
        GetComponent<BoxCollider2D>().enabled = true;

        _rb.bodyType = RigidbodyType2D.Dynamic;
        _animator.SetBool(Names.IsLive, true);

        _sprite = GetComponentInChildren<SpriteRenderer>();
        Color color = _sprite.material.color;
        color.a = 1f;
        _sprite.material.color = color;
    }
    protected virtual void Update()
    {
        if (transform.position.x - _playerPosition.x >= _runTrigger.x)
        {
            Run();
        }
        else
        {
            if (isGrounded) _animator.SetBool(Names.Run, false);
        }

        if (_currentHealth > 0)
        {    
            if (_dazedTime <= 0)
                _speed = 2f;
            else 
            {
                _speed = 0;
                _dazedTime -= Time.deltaTime;
            }
        }
    }
    protected virtual void Run()
    {
        if (isGrounded) _animator.SetBool(Names.Run, true);

        Vector3 _dir = _player.transform.position;
        transform.position = Vector3.MoveTowards(transform.position, _dir, _speed * Time.deltaTime);
        _sprite.flipX = _dir.x - transform.position.x < 0.0f;


    }
    protected virtual void CheckGround()
    {
        Collider2D[] collider = Physics2D.OverlapCircleAll(transform.position, 0.3f);
        isGrounded = collider.Length > 1;
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == Names.Player)
        {
            _animator.SetTrigger(Names.Attack);
        }
    }

    public void TakeDamageEnemy(int _damage)
    {
        _animator.SetTrigger(Names.Damage);
        _currentHealth -= _damage;
        _dazedTime = _startDazedTime;

        if (_currentHealth <= 0)
        {
            StartCoroutine(Die());
        }

    }


    IEnumerator Die()
    {
        _speed = 0;
        _rb.bodyType = RigidbodyType2D.Kinematic;
        GetComponent<BoxCollider2D>().enabled = false;
        _animator.SetBool(Names.IsLive, false);
        yield return new WaitForSeconds(3f);
        StartCoroutine("InvisibleSprite");
        
    }
    IEnumerator InvisibleSprite()
    {
        for (float f = 1f; f >= -0.05f; f -= 0.05f)
        {
            Color color = _sprite.material.color;
            color.a = f;
            _sprite.material.color = color;
            yield return new WaitForSeconds(0.05f);
        }
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        //Gizmos.DrawWireMesh();
    }

    protected virtual void PlayerApplyDamage()
    {
        FindObjectOfType<Hero>().ApplyDamage();
    }

}
