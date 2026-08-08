using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    #region//インスペクターで設定する
    [Header("歩き状態")] public bool walkingState;
    [Header("移動速度")] public float speed;
    [Header("重力")] public float gravity;
    [Header("画面外でも行動する")] public bool nonVisibleAct;
    [Header("接触判定")] public EnemyCollisionCheck checkCollision;
    #endregion

    #region//プライベート変数
    private Rigidbody2D rb = null;
    private SpriteRenderer sr = null;
    private Animator anim = null;
    private ObjectCollision oc = null;
    private BoxCollider2D col = null;
    private bool rightTleftF = false;
    private bool isDead = false;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        oc = GetComponent<ObjectCollision>();
        col = GetComponent<BoxCollider2D>();
    }

    void FixedUpdate()
    {
        if (!oc.playerStepOn)
        {
            if (walkingState)
            {
                anim.Play("walk");
                if (sr.isVisible || nonVisibleAct)
                {
                    if (checkCollision.isOn)
                    {
                        rightTleftF = !rightTleftF;
                    }
                    int xVector = -1;
                    if (rightTleftF)
                    {
                        xVector = 1;
                        transform.localScale = new Vector3(-1, 1, 1);
                    }
                    else
                    {
                        transform.localScale = new Vector3(1, 1, 1);
                    }
                    rb.linearVelocity = new Vector2(xVector * speed, -gravity);
                }
                else
                {
                    rb.Sleep();
                }
            }
        }
        else
        {
            if (!isDead)
            {
                anim.Play("dead");
                rb.linearVelocity = new Vector2(0, -gravity);
                isDead = true;
                col.enabled = false;
                Destroy(gameObject, 3f);
            }
            else
            {
                transform.Rotate(new Vector3(0, 0, 5));
            }
        }
    }
}