using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Pemain 1
    public PlayerControl player1; // skrip
    private Rigidbody2D player1Rigidbody;
    private float oldPlayer1Boundary;

    // Pemain 2
    public PlayerControl player2; // skrip
    private Rigidbody2D player2Rigidbody;
    private float oldPlayer2Boundary;

    // Bola
    public BallControl ball; // skrip
    private Rigidbody2D ballRigidbody;
    private CircleCollider2D ballCollider;

    // Skor maksimal
    public int maxScore;

    // Apakah debug window ditampilkan?
    private bool isDebugWindowShown = false;

    // Objek untuk menggambar prediksi lintasan bola
    public Trajectory trajectory;
    public GameObject ballAtCollision;

    public PowerMovement buff;
    public PowerMovement debuff;
    public float buffSize, buffTime, buffCycleTime, debuffCycleTime, buffSpeed, debuffSpeed;
    [Range(0f, 100f)]
    public float buffChance, debuffChance;
    //Coroutine buffCyclee, debuffCycle, getPower;

    // Start is called before the first frame update
    void Start()
    {
        player1Rigidbody = player1.GetComponent<Rigidbody2D>();
        player2Rigidbody = player2.GetComponent<Rigidbody2D>();
        ballRigidbody = ball.GetComponent<Rigidbody2D>();
        ballCollider = ball.GetComponent<CircleCollider2D>();
        oldPlayer1Boundary = player1.yBoundary;
        oldPlayer2Boundary = player2.yBoundary;
        StartCoroutine(BuffCycle());
        StartCoroutine(DebuffCycle());
    }


    // Untuk menampilkan GUI
    void OnGUI()
    {

        // Tampilkan skor pemain 1 di kiri atas dan pemain 2 di kanan atas
        GUI.Label(new Rect(Screen.width / 2 - 150 - 12, 20, 100, 100), "" + player1.Score);
        GUI.Label(new Rect(Screen.width / 2 + 150 + 12, 20, 100, 100), "" + player2.Score);

        // Tombol restart untuk memulai game dari awal
        if (GUI.Button(new Rect(Screen.width / 2 - 60, 35, 120, 53), "RESTART"))
        {
            // Ketika tombol restart ditekan, reset skor kedua pemain...
            player1.ResetScore();
            player2.ResetScore();

            // ...dan restart game.
            ball.SendMessage("RestartGame", 0.5f, SendMessageOptions.RequireReceiver);
            StopAllCoroutines();
            buff.ResetBall();
            debuff.ResetBall();
            StartCoroutine(BuffCycle());
            StartCoroutine(DebuffCycle());
            player1.transform.localScale = Vector3.one;
            player2.transform.localScale = Vector3.one;
            player1.yBoundary = oldPlayer1Boundary;
            player2.yBoundary = oldPlayer2Boundary;
        }

        // Jika pemain 1 menang (mencapai skor maksimal), ...
        if (player1.Score == maxScore)
        {
            // ...tampilkan teks "PLAYER ONE WINS" di bagian kiri layar...
            GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height / 2 - 10, 2000, 1000), "PLAYER ONE WINS");

            // ...dan kembalikan bola ke tengah.
            ball.SendMessage("ResetBall", null, SendMessageOptions.RequireReceiver);
            StopAllCoroutines();
        }
        // Sebaliknya, jika pemain 2 menang (mencapai skor maksimal), ...
        else if (player2.Score == maxScore)
        {
            // ...tampilkan teks "PLAYER TWO WINS" di bagian kanan layar... 
            GUI.Label(new Rect(Screen.width / 2 + 30, Screen.height / 2 - 10, 2000, 1000), "PLAYER TWO WINS");

            // ...dan kembalikan bola ke tengah.
            ball.SendMessage("ResetBall", null, SendMessageOptions.RequireReceiver);
            StopAllCoroutines();
        }

        // Jika isDebugWindowShown == true, tampilkan text area untuk debug window.
        if (isDebugWindowShown)
        {
            // Simpan nilai warna lama GUI
            Color oldColor = GUI.backgroundColor;

            // Beri warna baru
            GUI.backgroundColor = Color.red;

            // Simpan variabel-variabel fisika yang akan ditampilkan. 
            float ballMass = ballRigidbody.mass;
            Vector2 ballVelocity = ballRigidbody.velocity;
            float ballSpeed = ballRigidbody.velocity.magnitude;
            Vector2 ballMomentum = ballMass * ballVelocity;
            float ballFriction = ballCollider.friction;

            float impulsePlayer1X = player1.LastContactPoint.normalImpulse;
            float impulsePlayer1Y = player1.LastContactPoint.tangentImpulse;
            float impulsePlayer2X = player2.LastContactPoint.normalImpulse;
            float impulsePlayer2Y = player2.LastContactPoint.tangentImpulse;

            // Tentukan debug text-nya
            string debugText =
                "Ball mass = " + ballMass + "\n" +
                "Ball velocity = " + ballVelocity + "\n" +
                "Ball speed = " + ballSpeed + "\n" +
                "Ball momentum = " + ballMomentum + "\n" +
                "Ball friction = " + ballFriction + "\n" +
                "Last impulse from player 1 = (" + impulsePlayer1X + ", " + impulsePlayer1Y + ")\n" +
                "Last impulse from player 2 = (" + impulsePlayer2X + ", " + impulsePlayer2Y + ")\n";

            // Tampilkan debug window
            GUIStyle guiStyle = new GUIStyle(GUI.skin.textArea);
            guiStyle.alignment = TextAnchor.UpperCenter;
            GUI.TextArea(new Rect(Screen.width / 2 - 200, Screen.height - 200, 400, 110), debugText, guiStyle);

            // Kembalikan warna lama GUI
            GUI.backgroundColor = oldColor;
        }

        // Toggle nilai debug window ketika pemain mengeklik tombol ini.
        if (GUI.Button(new Rect(Screen.width / 2 - 60, Screen.height - 73, 120, 53), "TOGGLE\nDEBUG INFO"))
        {
            trajectory.enabled = !trajectory.enabled;
            isDebugWindowShown = !isDebugWindowShown;
            if (trajectory.enabled == false)
                ballAtCollision.SetActive(false);
        }

    }

    // Membuat player dapat buff
    IEnumerator PowerUp(PlayerControl target)
    {
        float oldYBoundary = target.yBoundary;
        target.transform.localScale = new Vector3(1, buffSize);
        target.yBoundary = target.yBoundary - (buffSize - 1);
        yield return new WaitForSeconds(buffTime);
        target.transform.localScale = new Vector3(1, 1);
        target.yBoundary = oldYBoundary;
        StartCoroutine(BuffCycle());
    }

    // Membuat player mendapat debuff
    void PowerDown(PlayerControl target)
    {
        switch (target.gameObject.name)
        {
            case "Player1":
                // Tambahkan skor ke pemain
                player2.IncrementScore();

                // Jika skor pemain belum mencapai skor maksimal...
                if (player2.Score < maxScore)
                {
                    // ...restart game setelah bola mengenai dinding.
                    ball.SendMessage("RestartGame", 2.0f, SendMessageOptions.RequireReceiver);
                }
                break;
            case "Player2":
                // Tambahkan skor ke pemain
                player1.IncrementScore();

                // Jika skor pemain belum mencapai skor maksimal...
                if (player1.Score < maxScore)
                {
                    // ...restart game setelah bola mengenai dinding.
                    ball.SendMessage("RestartGame", 2.0f, SendMessageOptions.RequireReceiver);
                }
                break;
        }
    }

    // Ketika terjadi tubrukan antara power dengan player
    public void PowerCollide(GameObject power, PlayerControl target)
    {
        if (power.name == "Buff")
        {
            StartCoroutine(PowerUp(target));
        }
        else
        {
            PowerDown(target);
        }
    }

    // Courotine spawn buff
    public IEnumerator BuffCycle()
    {
        while (true)
        {
            float chance = Random.Range(0, 100);
            yield return new WaitForSeconds(buffCycleTime);
            if (chance <= buffChance)
            {
                SpawnPower(buff, buffSpeed);
                break;
            }
        }
    }

    // Courotine spawn debuff
    public IEnumerator DebuffCycle()
    {
        while (true)
        {
            float chance = Random.Range(0, 100);
            yield return new WaitForSeconds(debuffCycleTime);
            if (chance <= debuffChance)
            {
                SpawnPower(debuff, debuffSpeed);
                break;
            }
        }
    }

    // Method me-spawn power
    void SpawnPower(PowerMovement target, float speed)
    {
        float yForce = Random.Range(-0.8f, 0.8f);
        float xForce = Mathf.Sqrt(1 - yForce * yForce);
        float leftOrRight = Random.Range(0f, 2f);
        if (leftOrRight > 1)
        {
            xForce *= -1;
        }
        target.gameObject.SetActive(true);
        target.gameObject.transform.position = Vector3.zero;
        target.rb2D.AddForce(new Vector2(xForce, yForce) * speed);
    }
}
