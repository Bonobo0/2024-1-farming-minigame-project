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
    private void OnGUI()
    {
        GUI.Box(new Rect(10, 10, 100, 200), "Boss Pattern");

        
        if (GUI.Button(new Rect(20, 40, 80, 20), "Fly Attack"))
        {
            //Application.LoadLevel(1);
        }

        // �ι� ° ��ư �����
        if (GUI.Button(new Rect(20, 70, 80, 20), "DolJin"))
        {
            //Application.LoadLevel(2);
        }

        if (GUI.Button(new Rect(20, 90, 80, 20), "Cut Forward"))
        {
            //Application.LoadLevel(2);
        }

        if (GUI.Button(new Rect(20, 110, 80, 20), "Cut Backward"))
        {
            //Application.LoadLevel(2);
        }

        if (GUI.Button(new Rect(20, 130, 80, 20), "Cut Forward and Backward"))
        {
            //Application.LoadLevel(2);
        }

        if (GUI.Button(new Rect(20, 150, 80, 20), "Cut with Gi"))
        {
            //Application.LoadLevel(2);
        }

    }
}
