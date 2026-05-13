using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelComplete : MonoBehaviour
{
    public float slideSpeed = 3f;
    public float delayBeforeLoad = 1.5f;

    private bool activated = false;
    private Transform player;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (activated) return;

        if (collision.CompareTag("Player"))
        {
            activated = true;
            player = collision.transform;

            // tắt điều khiển player
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;

            // cho player dính vào cổng
            player.SetParent(transform);
            // giữ player luôn nằm chính giữa cổng theo chiều X
            player.localPosition = new Vector3(0f, player.localPosition.y, player.localPosition.z);

            // bắt đầu coroutine
            StartCoroutine(FinishLevel());
        }
    }

    System.Collections.IEnumerator FinishLevel()
    {
        float timer = 0;

        while (timer < delayBeforeLoad)
        {
            // trượt xuống
            transform.position += Vector3.down * slideSpeed * Time.deltaTime;

            // nếu player đã gắn cổng thì ép về giữa theo X mỗi frame
            if (player != null)
                player.localPosition = new Vector3(0f, player.localPosition.y, player.localPosition.z);

            timer += Time.deltaTime;
            yield return null;
        }

        // load màn tiếp theo
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}