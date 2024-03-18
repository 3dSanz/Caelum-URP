using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private CharacterController _controller;
    private Animator _anim;
    private Transform _camera;

    //Movimiento
    private float _horizontal;
    private float _vertical;
    [SerializeField] private float _vel = 9;
    [SerializeField] private float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;

    //Salto
    [SerializeField] private float _alturaSalto = 1;
    private float _gravedad = -35f;
    private Vector3 _jugadorGravedad;
    [SerializeField] private Transform _posicionSensor;
    [SerializeField] private float _radioSensor = 0.2f;
    [SerializeField] private LayerMask _layerSuelo;
    public bool _isGrounded;

    //Ataque
    //Dano ataque normal
    public float _damage = 1f;
    //Dano ataque parry
    public float _damageStrong = 3f;
    public LayerMask enemyLayer;
    public float attackRadius = 1.5f;
    [SerializeField] private bool _lookUp;
    [SerializeField] private bool _lookDown;
    [SerializeField] private float downwardAttackForce = 15;
    [SerializeField] private Transform _attackForward;

    //Dash
    public float dashDistance = 10f;
    public float dashDuration = 0.5f;
    public float dashSpeed = 10f;
    private Vector3 dashStartPos;
    private bool isDashing;
    [SerializeField] private bool _lookDash;

    //Parry
    public float parryCooldown = 1f;
    public float meleeRange = 2f;
    public LayerMask meleeLayer;
    private bool canParry = true;

    //Parry Disparo
    //public LayerMask bulletLayer;

    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _anim = GetComponentInChildren<Animator>();
        _camera = Camera.main.transform;
    }

    void Update()
    {
        _horizontal = Input.GetAxisRaw("Horizontal");
        //_vertical = Input.GetAxisRaw("Vertical");

        //Movimiento
        Movimiento();
        
        //Salto
        Salto();
        _anim.SetBool("isJumping",!_isGrounded);


        //Ataque
        if (_isGrounded == true && Input.GetButtonDown("Fire1") && _lookUp == false)
        {
            PerformAttack(_damage);
            _anim.SetBool("isAttacking",true);
            Debug.Log("Ataque Normal");
        }else{
            _anim.SetBool("isAttacking",false);
        }

        //Ataque hacia arriba
        if (Input.GetKeyDown(KeyCode.W))
        {
            _lookUp = true;
        }else if(Input.GetKeyUp(KeyCode.W))
        {
            _lookUp = false;
        }
        /*//Ataque hacia arriba Suelo
        if (_isGrounded == true && _lookUp == true && Input.GetButtonDown("Fire1"))
        {
            PerformAttack(_damage);
            //_anim.SetBool("upAttack", true);
        }else
        {
            //_anim.SetBool("upAttack", false);
        }
        //Ataque hacia arriba Aire
        if (_isGrounded == false && _lookUp == true && Input.GetButtonDown("Fire1"))
        {
            PerformAttack(_damage);
            //_anim.SetBool("upAttack", true);
        }else
        {
            //_anim.SetBool("upAttack", false);
        }*/
        if (_lookUp == true && Input.GetButtonDown("Fire1"))
        {
            PerformAttack(_damage);
            //_anim.SetBool("upAttack", true);
            Debug.Log("Ataque hacia arriba");
        }else
        {
            //_anim.SetBool("upAttack", false);
        }


        //Ataque hacia abajo aire
        if (Input.GetKeyDown(KeyCode.S))
        {
            _lookDown = true;
        }else if(Input.GetKeyUp(KeyCode.S))
        {
            _lookDown = false;
        }
        if (_isGrounded == false && _lookDown == true && Input.GetButtonDown("Fire1"))
        {
            PerformDownwardAttack();
            //_anim.SetBool("downAttack", true);
            Debug.Log("Ataque Hacia Abajo en salto");
        }else
        {
            //_anim.SetBool("upAttack", false);
        }

        //Dash
        //Mirar Dash
        if (Input.GetButtonDown("Horizontal"))
        {
            _lookDash = true;
        }else if(Input.GetButtonUp("Horizontal"))
        {
            _lookDash = false;
        }

        if (Input.GetKeyDown(KeyCode.Z) && _lookDash == true)
        {
            PerformDash();
            
        }
        if (isDashing)
        {
            DashMovement();
            _anim.SetBool("isDash",true);
            
        }else
        {
            _anim.SetBool("isDash",false);
        }
        
        //Parry
        if (Input.GetButtonDown("Fire2") && canParry)
        {
            Parry();
        }else{
            _anim.SetBool("isParry", false);
        }
    
    }

    void Movimiento()
    {
        Vector3 _direccion = new Vector3 (_horizontal, 0, 0);

        _anim.SetFloat("VelX",0);
        _anim.SetFloat("VelZ", _direccion.magnitude);

        /*if(_direccion != Vector3.zero)
        {
            float _targetAngle = Mathf.Atan2(_direccion.x, _direccion.z) * Mathf.Rad2Deg + _camera.eulerAngles.y;
            float _smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0,_smoothAngle,0);
            Vector3 _moveDirection = Quaternion.Euler(0, _targetAngle, 0) * Vector3.forward;
            _controller.Move(_moveDirection.normalized * _vel * Time.deltaTime);
        }*/

        if (_direccion != Vector3.zero)
        {
        // Calcula el angulo objetivo basado en la direccion de entrada
        float _targetAngle = Mathf.Atan2(_direccion.x, _direccion.z) * Mathf.Rad2Deg + _camera.eulerAngles.y;

        // Suaviza el angulo de rotacion
        float _smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetAngle, ref turnSmoothVelocity, turnSmoothTime);

        // Aplica la rotacion solo en el eje Y
        transform.rotation = Quaternion.Euler(0, _smoothAngle, 0);

        // Calcula la direccion de movimiento en el plano XZ (sin componente Z)
        Vector3 _moveDirection = Quaternion.Euler(0, _targetAngle, 0) * Vector3.forward;
        _moveDirection.y = 0; // Bloquea el movimiento en el eje Z

        // Normaliza y aplica la velocidad de movimiento
        _controller.Move(_moveDirection.normalized * _vel * Time.deltaTime);
        }


        /*Vector3 movimiento = -transform.right * _horizontal * _vel * Time.deltaTime;

        _controller.Move(movimiento);*/
    }


    void Salto()
    {
        _isGrounded = Physics.CheckSphere(_posicionSensor.position, _radioSensor, _layerSuelo);

        if(_isGrounded && _jugadorGravedad.y < 0)
        {
            _jugadorGravedad.y = -2;
        }

        if(_isGrounded && Input.GetButtonDown("Jump"))
        {
            _jugadorGravedad.y = Mathf.Sqrt(_alturaSalto * -2 * _gravedad);
            _anim.SetBool("isJumping",true);
        }
        _jugadorGravedad.y += _gravedad * Time.deltaTime;
        _controller.Move(_jugadorGravedad * Time.deltaTime);
    }

   void PerformAttack(float _inputDamage)
    {

       Collider[] enemies = Physics.OverlapSphere(_attackForward.position, attackRadius, enemyLayer);

        foreach (Collider enemy in enemies)
        {
            //enemy.GetComponent<Enemy>().TakeDamage(_inputDamage);
        }

        /*Collider[] projectiles = Physics.OverlapSphere(transform.position, attackRadius, bulletLayer);
        foreach (Collider projectile in projectiles)
        {
            projectile = GetComponent<Projectile>();
            ReturnShoot();
        }*/
    }

    void PerformDownwardAttack()
    {
        //animator.SetTrigger("DownwardAttack"); // Activa la animacion de el ataque hacia abajo
        Collider[] enemies = Physics.OverlapSphere(transform.position, attackRadius, enemyLayer);

        foreach (Collider enemy in enemies)
        {
            //enemy.GetComponent<Enemy>().TakeDamage(_damage);
        }
        _controller.Move(Vector3.up * downwardAttackForce * Time.deltaTime);
    }

    void PerformDash()
    {
        if (!isDashing)
        {
            // Guarda la posicion inicial del dash
            dashStartPos = transform.position;

            // Inicia la flag para poder dashear
            isDashing = true;
        }
    }

    /*void ReturnShoot()
    {
        projectile._enemyThatShoot.position;
    }*/

    void DashMovement()
    {
       /* // Calcular la posicion final del dash
        Vector3 finalPosition = dashStartPos + transform.forward * dashDistance;

        // Movimiento hacia la posicion final del dash
        _controller.Move(transform.forward * dashSpeed * Time.deltaTime);

        // Comprueba si se ha realizado el dash
        if (Vector3.Distance(transform.position, dashStartPos) >= dashDistance)
        {
            // Resetea el flag y para el dash
            isDashing = false;
        }*/

        // Calcular la posicion final del dash
        Vector3 finalPosition = dashStartPos + transform.forward * dashDistance;

        // Movimiento hacia la posicion final del dash (solo en el plano XZ)
        Vector3 dashMove = transform.forward * dashSpeed * Time.deltaTime;
        dashMove.y = 0; // Bloquear el movimiento en el eje Z

        _controller.Move(dashMove);

        // Comprueba si se ha realizado el dash
        if (Vector3.Distance(transform.position, dashStartPos) >= dashDistance)
        {
            // Resetea el flag y para el dash
            isDashing = false;
        }

    }

    //Parry
    IEnumerator ParryCooldown()
    {
        canParry = false;
        yield return new WaitForSeconds(parryCooldown);
        canParry = true;
    }

    void Parry()
    {
        _anim.SetBool("isParry", true);
        Debug.Log("Parrendo");
        StartCoroutine(ParryCooldown());

        // Verificar si hay un objetivo cercano para el parry melee
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, meleeRange, meleeLayer);
        if (hitColliders.Length > 0)
        {
            PerformMeleeParry(hitColliders[0]);
        }
    }

    void PerformMeleeParry(Collider target)
    {
        //Anadir boleana al enemigo que indique si el enemigo esta atacando, si el enemigo se encuentra en el estado de ataque que se ejecute el parry
        Enemy enemy = target.GetComponent<Enemy>();
        if (enemy != null) //enemy.isAttacking == true
        {  
            PerformAttack(_damageStrong);
            _anim.SetTrigger("Parry_Attack");
            Debug.Log("Parry acertado!");
        }else
        {
            _anim.SetBool("isParry", false);
            Debug.Log("Parry fallado!");
        }


    }

    //Gizmos
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_attackForward.position, attackRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(_posicionSensor.position, _radioSensor);
    }
    
}