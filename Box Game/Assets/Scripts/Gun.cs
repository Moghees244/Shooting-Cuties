using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public enum fireMode
    {
        Auto,
        Burst,
        Single
    };

    public fireMode FireMode;
    bool triggerReleaseSinceLastShot;
    public Transform muzzle;
    public Projectile proj;
    public float msBetweenShots = 100f;
    public float muzzleSpeed = 35f;
    float nextShotTime;
    public int burstCount;
    int shotsRemainingInBurst;
    public AudioClip shootAudio;

    private void Start()
    {
        shotsRemainingInBurst = burstCount;
    }

    public void Shot()
    {
        if (Time.time > nextShotTime)
        {
            if (FireMode == fireMode.Burst)
            {
                if (shotsRemainingInBurst == 0)
                    return;

                shotsRemainingInBurst--;
            }
            else if (FireMode == fireMode.Single)
            {
                if (!triggerReleaseSinceLastShot)
                    return;
            }

            nextShotTime = Time.time + msBetweenShots / 1000;

            Projectile newProj = Instantiate(proj, muzzle.position, muzzle.rotation) as Projectile;
            newProj.setSpeed(muzzleSpeed);
            AudioManager.instance.playSound(shootAudio,transform.position);
        }
    }

    public void OnTriggerHold()
    {
        Shot();
        triggerReleaseSinceLastShot = false;
    }

    public void OnTriggerRelease()
    {
        triggerReleaseSinceLastShot = true;
        shotsRemainingInBurst = burstCount;
    }
}
