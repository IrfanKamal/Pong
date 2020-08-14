using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerMovement : MonoBehaviour
{
    // Variabel
    public GameManager gameManager;
    public Rigidbody2D rb2D;

    // Ketika tabrakan dengan player
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerControl target = collision.gameObject.GetComponent<PlayerControl>();
            gameManager.PowerCollide(gameObject, target);
            ResetBall();
        }
    }

    // Mengulang kondisi power
    public void ResetBall()
    {
        rb2D.velocity = Vector3.zero;
        gameObject.SetActive(false);
    }
}
