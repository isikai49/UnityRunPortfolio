using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    //インスペクターで設定する変数
    public float speed;//速度
    public float gravity;//重力
    public float jumpSpeed;//ジャンプ速度
    public float jumpLimitTime;//ジャンプ制限時間
    public GroundCheck ground;//設置判定
    public float jumpHeight;//ジャンプ高さ
    public GroundCheck head;//頭ぶつけた判定

    //プライベート変数
    private Animator anim;
    private Rigidbody2D rb;
    private bool isGround = false;
    private bool isHead = false;
    private bool isJump = false;
    private bool jumpPressed = false;
    private float jumpPos = 0.0f;
    private float jumpTime = 0.0f;

    private void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // ↑キーを押した瞬間だけtrue
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpPressed = true;
        }
    }

    private void FixedUpdate()
    {
        //接地判定を得る
        isGround = ground.IsGround();
        isHead = head.IsGround();

        //キー入力されたら行動する
        float horizontalKey = Input.GetAxis("Horizontal");

        float xSpeed = 0.0f;
        float ySpeed = -gravity;

        if (isGround)
        {
            // 押しっぱなしではなく、押した瞬間だけジャンプ開始
            if (jumpPressed)
            {
                ySpeed = jumpSpeed;
                jumpPos = transform.position.y;//ジャンプした位置を記録する
                isJump = true;
                jumpTime = 0.0f;
            }
            else
            {
                isJump = false;
            }

            // 入力を1回だけ消費
            jumpPressed = false;
        }
        else if (isJump)
        {
            // スペースキーを押し続けているか
            bool pushJumpKey = Input.GetKey(KeyCode.Space);

            // 現在の高さが飛べる高さより下か
            bool canHeight =
                jumpPos + jumpHeight > transform.position.y;

            // ジャンプ時間が長くなりすぎていないか
            bool canTime =
                jumpLimitTime > jumpTime;

            if (pushJumpKey && canHeight && canTime && !isHead)
            {
                ySpeed = jumpSpeed;
                jumpTime += Time.fixedDeltaTime;
            }
            else
            {
                isJump = false;
                jumpTime = 0.0f;
            }
        }

        // 空中で押した↑キーを着地後まで残さない
        if (!isGround && !isJump)
        {
            jumpPressed = false;
        }

        if (horizontalKey > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
            anim.SetBool("run", true);
            xSpeed = speed;
        }
        else if (horizontalKey < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            anim.SetBool("run", true);
            xSpeed = -speed;
        }
        else
        {
            anim.SetBool("run", false);
            xSpeed = 0.0f;
        }

        anim.SetBool("jump", isJump);
        anim.SetBool("ground", isGround);

        rb.linearVelocity = new Vector2(xSpeed, ySpeed);
    }
}
