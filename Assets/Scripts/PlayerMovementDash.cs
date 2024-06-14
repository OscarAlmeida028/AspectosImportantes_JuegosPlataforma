using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementDash : MonoBehaviour
{
    private float horizontal; // Almacena la entrada horizontal del jugador
    private float speed = 8f; // Velocidad de movimiento horizontal del jugador
    private float jumpingPower = 16f; // Potencia del salto del jugador
    private bool isFacingRight = true; // Indica si el jugador está mirando hacia la derecha

    private bool canDash = true; // Indica si el jugador puede realizar un dash
    private bool isDashing; // Indica si el jugador está actualmente en estado de dash
    private float dashingPower = 24f; // Potencia del dash (velocidad)
    private float dashingTime = 0.2f; // Duración del dash
    private float dashingCooldown = 1f; // Tiempo de enfriamiento entre dashes

    [SerializeField] private Rigidbody2D rb; // Referencia al componente Rigidbody2D del jugador
    [SerializeField] private Transform groundCheck; // Punto para verificar si el jugador está en el suelo
    [SerializeField] private LayerMask groundLayer; // Capa que define qué se considera suelo
    [SerializeField] private TrailRenderer tr; // Referencia al componente TrailRenderer utilizado para visualizar el dash

    private void Update()
    {
        if (isDashing) // Si el jugador está actualmente en estado de dash, no se procesa la entrada
        {
            return;
        }

        horizontal = Input.GetAxisRaw("Horizontal"); // Captura la entrada horizontal del jugador

        if (Input.GetButtonDown("Jump") && IsGrounded()) // Si se presiona el botón de salto y el jugador está en el suelo
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower); // Aplica una velocidad vertical para realizar el salto
        }

        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f) // Si se suelta el botón de salto y el jugador está subiendo
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f); // Reduce la velocidad vertical
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash) // Si se presiona la tecla de dash y el jugador puede realizarlo
        {
            StartCoroutine(Dash()); // Inicia la corrutina para realizar el dash
        }

        Flip(); // Voltea al jugador según la dirección de su movimiento horizontal
    }

    private void FixedUpdate()
    {
        if (isDashing) // Si el jugador está en estado de dash, no se aplica movimiento horizontal
        {
            return;
        }

        rb.velocity = new Vector2(horizontal * speed, rb.velocity.y); // Aplica la velocidad horizontal al Rigidbody2D del jugador
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.3f, groundLayer); // Verifica si el jugador está en el suelo usando un OverlapCircle
    }

    private void Flip()
    {
        // Voltea al jugador si su dirección horizontal cambia
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight = !isFacingRight; // Invierte el estado de la variable que indica si el jugador está mirando hacia la derecha
            Vector3 localScale = transform.localScale; // Obtiene la escala local del jugador
            localScale.x *= -1f; // Invierte la escala en el eje X para voltear al jugador
            transform.localScale = localScale; // Aplica la nueva escala al transform del jugador
        }
    }

    private IEnumerator Dash()
    {
        canDash = false; // El jugador no puede realizar un dash mientras está en proceso
        isDashing = true; // El jugador está actualmente en estado de dash

        // Guarda la gravedad original del jugador y la establece en cero durante el dash
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        // Aplica una velocidad al jugador para realizar el dash
        rb.velocity = new Vector2(transform.localScale.x * dashingPower, 0f);

        // Activa el componente TrailRenderer para visualizar el dash
        tr.emitting = true;

        yield return new WaitForSeconds(dashingTime); // Espera durante la duración del dash

        // Desactiva el componente TrailRenderer al finalizar el dash
        tr.emitting = false;

        // Restaura la gravedad original del jugador
        rb.gravityScale = originalGravity;

        isDashing = false; // El jugador ha completado el dash
        yield return new WaitForSeconds(dashingCooldown); // Espera el tiempo de enfriamiento entre dashes
        canDash = true; // El jugador puede realizar otro dash después del tiempo de enfriamiento
    }
}
