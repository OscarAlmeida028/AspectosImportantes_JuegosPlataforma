using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementDobleJump : MonoBehaviour
{
    private float horizontal; // Variable para almacenar la entrada horizontal del jugador
    [SerializeField] private float speed = 8f; // Velocidad de movimiento horizontal del jugador
    [SerializeField] private float jumpingPower = 16f; // Potencia del salto del jugador
    private bool isFacingRight = true; // Indicador de si el jugador está mirando hacia la derecha

    private bool doubleJump; // Indicador para permitir el doble salto

    [SerializeField] private Rigidbody2D rb; // Referencia al componente Rigidbody2D del jugador
    [SerializeField] private Transform groundCheck; // Punto para verificar si el jugador está en el suelo
    [SerializeField] private LayerMask groundLayer; // Capa que define qué se considera suelo

    private void Update()
    {
        // Llamar a la función para voltear al jugador si es necesario
        Flip();
        
        // Obtener la entrada horizontal del jugador (valor entre -1 y 1)
        horizontal = Input.GetAxisRaw("Horizontal");

        // Verificar si el jugador está en el suelo y no está presionando el botón de salto
        if (IsGrounded() && !Input.GetButton("Jump"))
        {
            doubleJump = false; // Restablecer la capacidad de doble salto
        }

        // Verificar si se presiona el botón de salto
        if (Input.GetButtonDown("Jump"))
        {
            if (IsGrounded() || doubleJump)
            {
                // Aplicar una velocidad vertical para hacer que el jugador salte
                rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
                doubleJump = !doubleJump; // Alternar el estado del doble salto
            }
        }

        // Reducir la velocidad vertical si se suelta el botón de salto
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
        {
            // Reducir la velocidad vertical a la mitad
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }
    }

    private void FixedUpdate()
    {
        // Aplicar la velocidad horizontal al Rigidbody2D del jugador
        rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
    }

    private bool IsGrounded()
    {
        // Crear un círculo pequeño en la posición de groundCheck y verificar si está en contacto con la capa del suelo
        return Physics2D.OverlapCircle(groundCheck.position, 0.3f, groundLayer);
    }

    private void Flip()
    {
        // Verificar si el jugador debe voltear
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            // Cambiar la dirección en la que está mirando el jugador
            isFacingRight = !isFacingRight;
            // Obtener la escala local del jugador y cambiar el signo de la escala X
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            // Aplicar la nueva escala al transform del jugador
            transform.localScale = localScale;
        }
    }
}
