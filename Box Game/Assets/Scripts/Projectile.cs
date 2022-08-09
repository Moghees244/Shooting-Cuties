using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class Projectile : MonoBehaviour
{
    float skinWidth = .1f;
    float speed = 10f;
    float damage = 1f;
    public LayerMask collisionMask;

    private void Start()
    {
        Destroy(gameObject, 3f);

        Collider[] initialCollisions = Physics.OverlapSphere(
            transform.position,
            .1f,
            collisionMask
        );

        if (initialCollisions.Length > 0)
            onHitEnemy(initialCollisions[0],transform.position);
    }

    public void setSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    void Update()
    {
        float moveDistance = speed * Time.deltaTime;
        checkCollision(moveDistance);
        this.transform.Translate(Vector3.forward * moveDistance);
    }

    void checkCollision(float dis)
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (
            Physics.Raycast(
                ray,
                out hit,
                dis + skinWidth,
                collisionMask,
                QueryTriggerInteraction.Collide
            )
        )
            onHitEnemy(hit.collider,hit.point);
    }

    void onHitEnemy(Collider c,Vector3 hitPoint)
    {
        IDamagable damagableObject = c.GetComponent<IDamagable>();

        if (damagableObject != null)
            damagableObject.takeHit(damage,hitPoint,transform.forward);
    }
}
