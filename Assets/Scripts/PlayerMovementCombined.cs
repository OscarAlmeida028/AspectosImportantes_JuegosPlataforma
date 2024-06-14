using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementCombined : MonoBehaviour
{
    private float horizontal; // Almacena la entrada horizontal del jugador
    [SerializeField] private float speed = 8f; // Velocidad de movimiento horizontal del jugador
    [SerializeField] private float jumpingPower = 16f; // Potencia del salto del jugador
    private bool isFacingRight = true; // Indica si el jugador está mirando hacia la derecha

    private bool isJumping; // Indica si el jugador está saltando

    private float coyoteTime = 0.2f; // Tiempo máximo de coyote (período después de dejar el borde en el que aún se puede saltar)
    private float coyoteTimeCounter; // Contador de coyote time

    private float jumpBufferTime = 0.2f; // Tiempo de buffer para el salto (período en el que se guarda la entrada del salto)
    private float jumpBufferCounter; // Contador de jump buffer

    private bool doubleJump; // Indicador para permitir el doble salto

    private bool isWallSliding; // Indica si el jugador está deslizándose por una pared
    private float wallSlidingSpeed = 2f; // Velocidad de deslizamiento por la pared

    private bool isWallJumping; // Indica si el jugador está realizando un salto en la pared
    private float wallJumpingDirection; // Dirección del salto en la pared
    private float wallJumpingTime = 0.2f; // Tiempo durante el cual se puede ejecutar el salto en la pared
    private float wallJumpingCounter; // Contador para el tiempo de salto en la pared
    private float wallJumpingDuration = 0.4f; // Duración del salto en la pared
    private Vector2 wallJumpingPower = new Vector2(8f, 16f); // Potencia del salto en la pared

    private bool canDash = true;
    private bool isDashing;
    private float dashingPower = 24f;
    private float dashingTime = 0.2f;
    private float dashingCooldown = 1f;

    [SerializeField] private Rigidbody2D rb; // Referencia al componente Rigidbody2D del jugador
    [SerializeField] private Transform groundCheck; // Punto para verificar si el jugador está en el suelo
    [SerializeField] private LayerMask groundLayer; // Capa que define qué se considera suelo
    [SerializeField] private Transform wallCheck; // Punto para verificar si el jugador está junto a una pared
    [SerializeField] private LayerMask wallLayer; // Capa que define qué se considera una pared
    [SerializeField] private TrailRenderer dashTrail; // Referencia al TrailRenderer para el dash

    [SerializeField] private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (isDashing)
        {
            return;
        }
        
        if(horizontal != 0f )
        {
            animator.SetBool("isRunning", true);
        }
        else
        {
            animator.SetBool("isRunning", false);
        }

        horizontal = Input.GetAxisRaw("Horizontal"); // Captura la entrada horizontal del jugador

        // Coyote Jump: permite saltar incluso después de salir del borde de una plataforma
        if (IsGrounded())
        {
            coyoteTimeCounter = coyoteTime; // Restablece el contador de coyote time si está en el suelo
            doubleJump = true; // Habilita el doble salto cuando el jugador está en el suelo
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime; // Decrementa el contador de coyote time
        }

        // Time Buffering: permite que las entradas de salto se ejecuten dentro de un período de tiempo corto
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime; // Restablece el contador de jump buffer si se presiona el botón de salto
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime; // Decrementa el contador de jump buffer
        }

        // Realizar el salto si se presionó el botón de salto durante el coyote time y el buffer time
        if ((IsGrounded() || coyoteTimeCounter > 0f || doubleJump) && jumpBufferCounter > 0f && !isJumping)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower); // Aplicar una velocidad vertical para hacer que el jugador salte

            jumpBufferCounter = 0f; // Restablece el contador de jump buffer

        if (coyoteTimeCounter <= 0f && !IsGrounded() && doubleJump)
        {
            doubleJump = false; // Restablecer la capacidad de doble salto solo si estaba disponible y se realizó un salto en el aire
        }

            StartCoroutine(JumpCooldown()); // Inicia la corrutina para evitar saltos consecutivos
        }

        // Si el jugador suelta el botón de salto y aún está subiendo, reduce la velocidad vertical
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }

        // Verifica si el jugador está deslizándose por una pared y ajusta su velocidad vertical
        WallSlide();

        // Verifica si el jugador está realizando un salto en la pared
        WallJump();

        // Dash
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(Dash());
        }

        // Si no se está realizando un salto en la pared, voltea al jugador según la dirección del movimiento horizontal
        if (!isWallJumping && !isDashing)
        {
            Flip();
        }
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            return;
        }

        // Aplica la velocidad horizontal al Rigidbody2D del jugador, solo si no está realizando un salto en la pared
        if (!isWallJumping)
        {
            rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
        }
    }

    private bool IsGrounded()
    {
        // Verifica si el jugador está en el suelo usando un OverlapCircle en groundCheck
        return Physics2D.OverlapCircle(groundCheck.position, 0.3f, groundLayer);
    }

    private bool IsWalled()
    {
        // Verifica si el jugador está junto a una pared usando un OverlapCircle en wallCheck
        return Physics2D.OverlapCircle(wallCheck.position, 0.3f, wallLayer);
    }

    private void WallSlide()
    {
        // Verifica si el jugador está deslizándose por una pared y ajusta su velocidad vertical
        if (IsWalled() && !IsGrounded() && horizontal != 0f)
        {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void WallJump()
    {
        // Si el jugador está deslizándose por una pared, configura el salto en la pared
        if (isWallSliding)
        {
            isWallJumping = false;
            wallJumpingDirection = -transform.localScale.x; // Determina la dirección del salto en la pared
            wallJumpingCounter = wallJumpingTime; // Inicializa el contador de tiempo para el salto en la pared

            // Cancela cualquier invocación pendiente para detener el salto en la pared
            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            // Si el jugador no está deslizándose por una pared, reduce el contador de tiempo para el salto en la pared
            wallJumpingCounter -= Time.deltaTime;
        }

        // Si el jugador presiona el botón de salto y está dentro del tiempo permitido para el salto en la pared
        if (Input.GetButtonDown("Jump") && wallJumpingCounter > 0f)
        {
            isWallJumping = true; // Indica que el jugador está realizando un salto en la pared
            // Aplica una velocidad al jugador para realizar el salto en la pared
            rb.velocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);
            wallJumpingCounter = 0f; // Reinicia el contador de tiempo para el salto en la pared

            // Si el jugador cambió de dirección durante el salto en la pared, voltea su sprite
            if (transform.localScale.x != wallJumpingDirection)
            {
                isFacingRight = !isFacingRight;
                Vector3 localScale = transform.localScale;
                localScale.x *= -1f;
                transform.localScale = localScale;
            }

            // Programa la detención del salto en la pared después de un cierto tiempo
            Invoke(nameof(StopWallJumping), wallJumpingDuration);
        }
    }

    private void StopWallJumping()
    {
        // Detiene el salto en la pared
        isWallJumping = false;
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(transform.localScale.x * dashingPower, 0f);
        dashTrail.emitting = true;
        yield return new WaitForSeconds(dashingTime);
        dashTrail.emitting = false;
        rb.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }

    private void Flip()
    {
        // Voltea al jugador si su dirección horizontal cambia
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    // Corrutina para evitar saltos consecutivos
    private IEnumerator JumpCooldown()
    {
        isJumping = true; // Indicador de que el jugador está saltando
        yield return new WaitForSeconds(0.4f); // Tiempo de espera después del salto
        isJumping = false; // Indicador de que el jugador ya no está saltando
    }
}
