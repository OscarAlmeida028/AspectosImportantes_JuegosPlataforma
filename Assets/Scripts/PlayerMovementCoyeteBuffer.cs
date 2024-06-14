using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementCoyeteBuffer : MonoBehaviour
{
    private float horizontal; // Variable para almacenar la entrada horizontal del jugador
    [SerializeField] private float speed = 8f; // Velocidad de movimiento horizontal del jugador
    [SerializeField] private float jumpingPower = 16f; // Potencia del salto del jugador
    private bool isFacingRight = true; // Indicador de si el jugador está mirando hacia la derecha

    private bool isJumping; // Indicador de si el jugador está saltando

    private float coyoteTime = 0.2f; // Tiempo máximo de coyote (período después de dejar el borde en el que aún se puede saltar)
    private float coyoteTimeCounter; // Contador de coyote time

    private float jumpBufferTime = 0.2f; // Tiempo de buffer para el salto (período en el que se guarda la entrada del salto)
    private float jumpBufferCounter; // Contador de jump buffer

    [SerializeField] private Rigidbody2D rb; // Referencia al componente Rigidbody2D del jugador
    [SerializeField] private Transform groundCheck; // Punto para verificar si el jugador está en el suelo
    [SerializeField] private LayerMask groundLayer; // Capa que define qué se considera suelo

    private void Update()
    {
        // Obtener la entrada horizontal del jugador (valor entre -1 y 1)
        horizontal = Input.GetAxisRaw("Horizontal");

        // Coyote Jump: permite saltar incluso después de salir del borde de una plataforma
        if (IsGrounded())
        {
            coyoteTimeCounter = coyoteTime; // Restablece el contador de coyote time si está en el suelo
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
        if (coyoteTimeCounter > 0f && jumpBufferCounter > 0f && !isJumping)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower); // Aplicar una velocidad vertical para hacer que el jugador salte

            jumpBufferCounter = 0f; // Restablece el contador de jump buffer

            StartCoroutine(JumpCooldown()); // Inicia la corrutina para evitar saltos consecutivos
        }

        // Reducir la velocidad vertical si se suelta el botón de salto
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f); // Reduce la velocidad vertical

            coyoteTimeCounter = 0f; // Restablece el contador de coyote time
        }

        // Llamar a la función para voltear al jugador si es necesario
        Flip();
    }

    // FixedUpdate se llama a intervalos fijos y se usa para las actualizaciones de física
    private void FixedUpdate()
    {
        // Aplicar la velocidad horizontal al Rigidbody2D del jugador
        rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
    }

    // Función para verificar si el jugador está en el suelo
    private bool IsGrounded()
    {
        // Crear un círculo pequeño en la posición de groundCheck y verificar si está en contacto con la capa del suelo
        return Physics2D.OverlapCircle(groundCheck.position, 0.3f, groundLayer);
    }

    // Función para voltear al jugador si cambia de dirección
    private void Flip()
    {
        // Verificar si el jugador debe voltear
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            Vector3 localScale = transform.localScale; // Obtener la escala local del jugador
            isFacingRight = !isFacingRight; // Cambiar la dirección en la que está mirando el jugador
            localScale.x *= -1f; // Cambiar el signo de la escala X
            transform.localScale = localScale; // Aplicar la nueva escala al transform del jugador
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
