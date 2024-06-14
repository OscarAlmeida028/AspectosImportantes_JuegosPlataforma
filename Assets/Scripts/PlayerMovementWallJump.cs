using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementWallJump : MonoBehaviour
{
    private float horizontal; // Almacena la entrada horizontal del jugador
    [SerializeField] private float speed = 8f; // Velocidad de movimiento horizontal del jugador
    [SerializeField] private float jumpingPower = 16f; // Potencia del salto del jugador
    private bool isFacingRight = true; // Indica si el jugador está mirando hacia la derecha

    private bool isWallSliding; // Indica si el jugador está deslizándose por una pared
    private float wallSlidingSpeed = 2f; // Velocidad de deslizamiento por la pared

    private bool isWallJumping; // Indica si el jugador está realizando un salto en la pared
    private float wallJumpingDirection; // Dirección del salto en la pared
    private float wallJumpingTime = 0.2f; // Tiempo durante el cual se puede ejecutar el salto en la pared
    private float wallJumpingCounter; // Contador para el tiempo de salto en la pared
    private float wallJumpingDuration = 0.4f; // Duración del salto en la pared
    private Vector2 wallJumpingPower = new Vector2(8f, 16f); // Potencia del salto en la pared

    [SerializeField] private Rigidbody2D rb; // Referencia al componente Rigidbody2D del jugador
    [SerializeField] private Transform groundCheck; // Punto para verificar si el jugador está en el suelo
    [SerializeField] private LayerMask groundLayer; // Capa que define qué se considera suelo
    [SerializeField] private Transform wallCheck; // Punto para verificar si el jugador está junto a una pared
    [SerializeField] private LayerMask wallLayer; // Capa que define qué se considera una pared

    private void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal"); // Captura la entrada horizontal del jugador

        // Si el jugador está en el suelo y presiona el botón de salto, realiza un salto normal
        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
        }

        // Si el jugador suelta el botón de salto y aún está subiendo, reduce la velocidad vertical
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }

        // Verifica si el jugador está deslizándose por una pared
        WallSlide();

        // Verifica si el jugador está realizando un salto en la pared
        WallJump();

        // Si no se está realizando un salto en la pared, voltea al jugador según la dirección del movimiento horizontal
        if (!isWallJumping)
        {
            Flip();
        }
    }

    private void FixedUpdate()
    {
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
}
