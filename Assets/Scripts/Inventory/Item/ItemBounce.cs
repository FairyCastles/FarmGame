using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.Inventory
{
    public class ItemBounce : MonoBehaviour
    {
        private Transform spriteTransform;
        private BoxCollider2D coll;

        public float gravity = -3.5f;
        private bool isGround;
        private float distance;
        private Vector2 direction;
        private Vector3 targetPos;

        #region Life Function 

        private void Awake()
        {
            spriteTransform = transform.GetChild(0);
            // 飞行时关闭碰撞体
            coll = GetComponent<BoxCollider2D>();
            coll.enabled = false;
        }

        private void Update()
        {
            Bounce();
        }

        #endregion

        public void InitBounceItem(Vector3 target, Vector2 dir)
        {
            coll.enabled = false;
            direction = dir;
            targetPos = target;
            distance = Vector3.Distance(target, transform.position);

            // 玩家头顶距离玩家 1.5，从头顶生成物体
            // 影子的坐标就是地面坐标
            spriteTransform.position += Vector3.up * 1.5f;
        }

        private void Bounce()
        {
            // 用物体和影子距离来判断是否落地
            isGround = spriteTransform.position.y <= transform.position.y;

            // 未到达鼠标点击的位置，进行位移
            // TODO: 在进行远距离丢东西时，会出现先到坐标点再下落的情况，可以调整一下数值
            if (Vector3.Distance(transform.position, targetPos) > 0.1f)
            {
                transform.position += (Vector3)direction * distance * -gravity * Time.deltaTime;
            }

            // 未到达地面，模拟重力下落物体
            if (!isGround)
            {
                spriteTransform.position += Vector3.up * gravity * Time.deltaTime;
            }
            // 落到地面上了
            else
            {
                spriteTransform.position = transform.position;
                coll.enabled = true;
            }
        }
    }
}