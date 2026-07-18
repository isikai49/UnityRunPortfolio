using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region//インスペクターで設定する
    [Header("移動速度")] public float speed;
    [Header("重力")] public float gravity;
    [Header("ジャンプ速度")] public float jumpSpeed;
    [Header("ジャンプ制限時間")] public float jumpLimitTime;
    [Header("踏みつけ判定の高さの割合")] public float stepOnRate;
    [Header("設置判定")] public GroundCheck ground;
    [Header("ジャンプ高さ")] public float jumpHeight;
    [Header("頭ぶつけた判定")] public GroundCheck head;
    [Header("ダッシュの速さ表現")] public AnimationCurve dashCurve;
    [Header("ジャンプの速さ表現")] public AnimationCurve jumpCurve;
    #endregion

    #region//プライベート変数
    private Animator anim;
    private Rigidbody2D rb;
    private CapsuleCollider2D capcol;

    private bool isGround = false;
    private bool isRun = false;
    private bool isDown = false;
    private bool isOtherJump = false;
    private bool isHead = false;
    private bool isJump = false;
    private bool jumpPressed = false;
    private float jumpPos = 0.0f;
    private float otherJumpHeight = 0.0f;
    private float dashTime = 0.0f;
    private float jumpTime = 0.0f;
    private float beforeKey = 0.0f;
    private string enemyTag = "Enemy";
    #endregion

    private void Start()
    {
        //コンポーネントのインスタンスを捕まえる
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        capcol = GetComponent<CapsuleCollider2D>();
    }

    private void Update()
    {
        // スペースキーを押した瞬間だけtrue
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpPressed = true;
        }
    }

    private void FixedUpdate()
    {
        if (!isDown)
        {
            //接地判定を得る
            isGround = ground.IsGround();
            isHead = head.IsGround();

            //各種座標軸の速度を求める
            float xSpeed = GetXSpeed();
            float ySpeed = GetYSpeed();

            //アニメーションを適用
            SetAnimation();

            //移動速度を設定
            rb.linearVelocity = new Vector2(xSpeed, ySpeed);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, -gravity);
        }
    }

    /// <summary> 
    /// Y成分で必要な計算をし、速度を返す。 
    /// </summary> 
    /// <returns>Y軸の速さ</returns> 
    private float GetYSpeed()
    {
        float ySpeed = -gravity;

        if (isOtherJump)
        {
            // 現在の高さが飛べる高さより下か
            bool canHeight =jumpPos + otherJumpHeight > transform.position.y;
            // ジャンプ時間が長くなりすぎていないか
            bool canTime =jumpLimitTime > jumpTime;

            if (canHeight && canTime && !isHead)
            {
                ySpeed = jumpSpeed;
                jumpTime += Time.fixedDeltaTime;
            }
            else
            {
                isOtherJump = false;
                jumpTime = 0.0f;
            }
        }
        else if (isGround)
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
            bool canHeight =jumpPos + jumpHeight > transform.position.y;
            // ジャンプ時間が長くなりすぎていないか
            bool canTime =jumpLimitTime > jumpTime;

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

        if (isJump || isOtherJump)
        {
            ySpeed *= jumpCurve.Evaluate(jumpTime);
        }

        return ySpeed;
    }

    /// <summary> 
    /// X成分で必要な計算をし、速度を返す。 
    /// </summary> 
    /// <returns>X軸の速さ</returns> 
    private float GetXSpeed()
    {
        float horizontalKey = Input.GetAxis("Horizontal");
        float xSpeed = 0.0f;



        if (horizontalKey > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
            isRun = true;
            dashTime += Time.deltaTime;
            xSpeed = speed;
        }
        else if (horizontalKey < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            isRun = true;
            dashTime += Time.deltaTime;
            xSpeed = -speed;
        }
        else
        {
            isRun = false;
            xSpeed = 0.0f;
            dashTime = 0.0f;
        }

        //前回の入力からダッシュの反転を判断して速度を変える
        if (horizontalKey > 0 && beforeKey < 0)
        {
            dashTime = 0.0f;
        }
        else if (horizontalKey < 0 && beforeKey > 0)
        {
            dashTime = 0.0f;
        }

        beforeKey = horizontalKey;
        xSpeed *= dashCurve.Evaluate(dashTime);
        beforeKey = horizontalKey;

        return xSpeed;
    }

    /// <summary> 
    /// アニメーションを設定する 
    /// </summary> 
    private void SetAnimation()
    {
        anim.SetBool("jump", isJump || isOtherJump);
        anim.SetBool("ground", isGround);
        anim.SetBool("run", isRun);
    }

    /// <summary> 
    /// 敵との接触判定
    /// </summary> 
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == enemyTag)
        {
            //踏みつけ判定になる高さ
            float stepOnHeight = (capcol.size.y * (stepOnRate / 100f));

            //踏みつけ判定のワールド座標
            float judgePos = transform.position.y - (capcol.size.y / 2f) + stepOnHeight;

            foreach (ContactPoint2D p in collision.contacts)
            {
                if (p.point.y < judgePos)
                {
                    //もう一度跳ねる
                    ObjectCollision o = collision.gameObject.GetComponent<ObjectCollision>();

                    if (o != null)
                    {
                        otherJumpHeight = o.boundHeight;    //踏んづけたものから跳ねる高さを取得する
                        o.playerStepOn = true;        //踏んづけたものに対して踏んづけた事を通知する
                        jumpPos = transform.position.y; //ジャンプした位置を記録する
                        isOtherJump = true;
                        isJump = false;
                        jumpTime = 0.0f;
                    }
                    else
                    {
                        Debug.Log("ObjectCollisionが付いてないよ!");
                    }
                }
                else
                {
                    //ダウンする
                    anim.Play("player_down");
                    isDown = true;
                    Debug.Log("敵と接触した！");
                    break;
                }
                
            }
            
        }
    }
}