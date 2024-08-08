using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 *### ���� ����
 *- 1������
 *  - ������ ���ƿö��ٰ� ���� ���
 *      - �� ���̴� ������ �̵� ��, ���� ���
 *      - ���� ��� ������ ���� �ǵ�� ��
 *      - ��ȸ ���: ������, ���� �ۿ� ����
 *  - ����
 *      - ������ ����
 *      - �ڷ� ����
 *      - ��/�ڷ� ����
 *      - ���鼭 �˱� ������
 *      - ��ȸ ���: ������, ���� �ۿ� ����, �и�
 *  - ����
 *      - ���� ���� ���� ĳ���Ϳ��� ����
 *      - ��ȸ ���: ����, �и�
 *          - �и� �� ĳ���Ͱ� �ణ �ڷ� �з���
 */
public class BossSkill : MonoBehaviour
{
    public Transform boss;
    public Transform target;
    public void performDiveAttack() { }
    public void slashForward() { }
    public void slashBackward() { }
    public void slashBothDir() { }
    public void slashWithEnerge() { }
    public void chargeAttack() { }


    private void OnGUI()
    {
        GUI.Box(new Rect(10, 10, 220, 220), "Boss Pattern");

        
        if (GUI.Button(new Rect(20, 40, 190, 20), "Fly_Attack"))
        {
            //Application.LoadLevel(1);
        }

        // �ι� ° ��ư �����
        if (GUI.Button(new Rect(20, 70, 190, 20), "DolJin"))
        {
            //Application.LoadLevel(2);
        }

        if (GUI.Button(new Rect(20, 100, 190, 20), "Cut_Forward"))
        {
            //Application.LoadLevel(2);
        }

        if (GUI.Button(new Rect(20, 130, 190, 20), "Cut_Backward"))
        {
            //Application.LoadLevel(2);
        }

        if (GUI.Button(new Rect(20, 160, 190, 20), "Cut_Forward_and_Backward"))
        {
            //Application.LoadLevel(2);
        }

        if (GUI.Button(new Rect(20, 190, 190, 20), "Cut_with_Gi"))
        {
            //Application.LoadLevel(2);
        }

    }
}
