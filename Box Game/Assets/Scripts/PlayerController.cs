using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(GunController))]
public class PlayerController : LivingEntity
{
    public float speed = 3.0f;
    float heightThreshold;
    Camera viewCam;

    GunController gunController;

    private void Awake()
    {
        heightThreshold = Time.time;
        heightThreshold += 20;
    }

    public override void Start()
    {
        base.Start();
        speed *= Time.fixedDeltaTime;
        gunController = GetComponent<GunController>();
        viewCam = Camera.main;
    }

    void Update()
    {
        //Movements using keyboard

        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            this.transform.Translate(new Vector3(0, 0, speed));

        //rotations using mouse

        /*my code
        Ray rae = viewCam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(rae, out hit))
        {
            //correcting y axis as head must be at same level
            Vector3 correctedHitPoint = new Vector3(hit.point.x, transform.position.y, hit.point.z);

            this.transform.LookAt(correctedHitPoint);
        }*/

        /*  Sebastian's code*/
        Ray rae = viewCam.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayDistance;

        if (groundPlane.Raycast(rae, out rayDistance))
        {
            Vector3 point = rae.GetPoint(rayDistance);
            Vector3 correctedPoint = new Vector3(point.x, transform.position.y, point.z);

            this.transform.LookAt(correctedPoint);
        }

        if (Input.GetMouseButton(0))
            gunController.onTriggerHold();

        if (Input.GetMouseButtonUp(0))
            gunController.onTriggerRelease();

        if (this.transform.position.y <= -3)
            Die();

        if (this.transform.position.y > 2)
        {
            if (Time.time > heightThreshold)
            {
                heightDamage();
                heightThreshold += 1;
            }
        }
        else if (this.transform.position.y < 2)
        {
            heightThreshold = Time.time;
            heightThreshold += 3;
        }
    }

    public override void Die()
    {
        AudioManager.instance.playSound("PlayerDeath", transform.position);
        base.Die();
    }
}
