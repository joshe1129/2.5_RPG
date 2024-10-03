using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class MemberFollowAI : MonoBehaviour
{
    [SerializeField] private Transform followTarget;
    [SerializeField] private int speed;

    private float followDist;
    private Animator followerAnim;
    private SpriteRenderer spriteRenderer;
    
    private const string IS_WALKING = "isWalking";

    void Start()
    {
        followerAnim = gameObject.GetComponent<Animator>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

        followTarget = GameObject.FindFirstObjectByType<PlayerController>().transform;
    }

    private void FixedUpdate() {
        if (Vector3.Distance(transform.position, followTarget.position) > followDist)
        {
            followerAnim.SetBool(IS_WALKING, true);
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, followTarget.position, step);
            if(followTarget.position.x - transform.position.x < 0)
            {
                spriteRenderer.flipX = true;
            }
            else
            {
                spriteRenderer.flipX = false;
            }
        }
        else
        {
            followerAnim.SetBool(IS_WALKING, false);

        }
    }

    public void SetFollowDistance(float followDistance)
    {
        followDist = followDistance;
    }
}
