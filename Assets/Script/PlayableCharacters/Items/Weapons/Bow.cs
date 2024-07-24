using System.Collections;
using UnityEngine;
using Items;
using Unity.VisualScripting;

public class Bow : Weapon
{
    [System.Serializable]
    private class MultiShotArrowProperties
    {
        public Vector3 position;
        public Vector3 rotation;
    }

    private bool isAttackCooldown = false;
    private MultiShotArrowProperties[] multiShotArrows = new MultiShotArrowProperties[3];
    // initialize bow properties
    public Bow(float attackSpeed)
    {
        this.attackSpeed = attackSpeed;
        projectileSpeed = 30.0f;
        range = 10.0f;
        damage = 50;
    }
    // initialize multi shot arrow properties
    private void InitMultiShotArrows()
    {
        multiShotArrows[0] = new MultiShotArrowProperties
        {
            position = new Vector3(1.4f, 0.57f, 0),
            rotation = new Vector3(0, 0, -10.0f)
        };

        multiShotArrows[1] = new MultiShotArrowProperties
        {
            position = new Vector3(1.3f, 0.1f, 0),
            rotation = new Vector3(0, 0, -20.0f)
        };

        multiShotArrows[2] = new MultiShotArrowProperties
        {
            position = new Vector3(0.9f, -0.25f, 0),
            rotation = new Vector3(0, 0, -26.0f)
        };
    }
    public override IEnumerator Attack(Animator anim, Transform character)
    {
        if (multiShotArrows[0] == null)
        {
            InitMultiShotArrows();
        }
        if (!isAttackCooldown)
        {
            // 0.5f = animation length
            // 0.3f = combo delay

            if (attackState == 3)
            {
                attackState = 0;
            }
            attackState++;
            // 애니메이션 배속
            anim.SetFloat("AttackAnimSpeed", 0.5f * attackSpeed);
            anim.SetInteger("AttackState", attackState);
            prevAttack = Time.time;
            anim.SetFloat("PrevAttack", prevAttack);
            anim.SetTrigger("Attack");
            anim.SetBool("Combo", true);
            yield return new WaitForSeconds(0.1f);
            isAttackCooldown = true;
            yield return new WaitForSeconds(1.0f / attackSpeed - 0.1f);
            isAttackCooldown = false;
            yield return new WaitForSeconds(0.3f);
            if (Time.time - prevAttack > 1.0f / attackSpeed + 0.3f)
            {
                anim.SetInteger("AttackState", 0);
                anim.SetBool("Combo", false);
                attackState = 0;
            }
        }
        else
        {
            yield return null;
        }
    }

    public override IEnumerator FireProjectile(Animator anim, Transform character)
    {
        if (anim.GetInteger("AttackState") == 2)
        {
            yield return MultiShot(character);
        }
        else
        {
            GameObject projectile = ObjectPoolManager.Instance.GetGo("ArcherProjectile");
            Base arrow = projectile.GetComponent<Base>();
            GameObject projectileObject = arrow.projectile;
            Vector3 scale = character.localScale;
            // character's scale must be 1 or -1
            projectile.transform.position = character.position + new Vector3(scale.x * 1.5f, 0, 0);
            projectile.transform.localScale = scale;
            projectileObject.transform.localPosition = new Vector3(0, 0, 0);
            projectileObject.transform.rotation = Quaternion.Euler(0, 0, 0);
            arrow.isReady = true;
            arrow.shotType = 0;
            arrow.projectileSpeed = projectileSpeed;
            arrow.damage = damage;
            arrow.range = range;
            yield return null;
        }
    }

    public IEnumerator MultiShot(Transform character)
    {
        if (multiShotArrows[0] == null)
        {
            InitMultiShotArrows();
        }
        for (int i = 0; i < 3; i++)
        {
            GameObject projectile = ObjectPoolManager.Instance.GetGo("ArcherProjectile");
            Base arrow = projectile.GetComponent<Base>();
            GameObject projectileObject = arrow.projectile;
            Vector3 scale = character.localScale;
            Vector3 newRotationVector = multiShotArrows[i].rotation;
            newRotationVector.z *= scale.x;
            Quaternion newRotation = Quaternion.Euler(newRotationVector);
            projectile.transform.position = character.position + new Vector3(scale.x * multiShotArrows[i].position.x, multiShotArrows[i].position.y, 0);
            projectile.transform.localScale = scale;
            projectileObject.transform.localPosition = new Vector3(0, 0, 0);
            projectileObject.transform.rotation = newRotation;
            arrow.isReady = true;
            if (i == 0)
            {
                arrow.shotType = 1;
            }
            else
            {
                arrow.shotType = -1;
            }
            arrow.projectileSpeed = projectileSpeed;
            arrow.damage = damage;
            arrow.range = range;
        }
        yield return null;
    }

}
