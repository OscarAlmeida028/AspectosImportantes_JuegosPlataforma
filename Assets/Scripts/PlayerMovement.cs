using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float horizontal; // Variable para almacenar la entrada horizontal del jugador
    [SerializeField] private float speed = 8f; // Velocidad de movimiento horizontal del jugador
    [SerializeField] private float jumpingPower = 16f; // Potencia del salto del jugador
    private bool isFacingRight = true; // Indicador de si el jugador está mirando hacia la derecha

    [SerializeField] private Rigidbody2D rb; // Referencia al componente Rigidbody2D del jugador
    [SerializeField] private Transform groundCheck; // Punto para verificar si el jugador está en el suelo
    [SerializeField] private LayerMask groundLayer; // Capa que define qué se considera suelo

    void Update()
    {
        // Obtener la entrada horizontal del jugador (valor entre -1 y 1)
        horizontal = Input.GetAxisRaw("Horizontal");

        // Verificar si el botón de salto ha sido presionado y si el jugador está en el suelo
        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            // Aplicar una velocidad vertical para hacer que el jugador salte
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
        }

        // Verificar si el botón de salto ha sido soltado y si el jugador aún está ascendiendo
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
        {
            // Reducir la velocidad vertical para permitir saltos de altura variable
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
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
