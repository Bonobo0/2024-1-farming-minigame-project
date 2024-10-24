using System.Collections.Generic;
using System.Linq;
using Fusion;
using Items;
using UnityEngine;

public enum CharacterClassEnum
{
    Knight,
    Archer,
    Tank,
}

public class ClassIcons
{
    static public List<Sprite> classIcons = new();
    static public Sprite GetIcon(CharacterClassEnum type)
    {
        if (classIcons.Count == 0)
        {
            classIcons = Resources.LoadAll<Sprite>("Icons/Classes").ToList();
        }

        return classIcons.Find(x => x.name == type.ToString());
    }
}

[System.Serializable]
public struct CharacterStatMultiplier : INetworkStruct
{
    public NetworkString<_16> name;
    public float value;
    public Buff buff;
}

[System.Serializable]
public struct SkillStruct : INetworkStruct
{
    public float coolDown;
    public CustomTickTimer coolDownTimer;
    public float duration;
}


public abstract class CharacterClass
{
    public float maxHealth;
    public float attackSpeed;
    public float moveSpeed;
    public Weapon weapon;
    public RuntimeAnimatorController characterAnimator;
    public Dictionary<NetworkString<_16>, SkillStruct> SkillList = new();
    public Dictionary<string, float> skillList = new(); // skillName, coolDown

    public static void ChangeClass(int characterClass, GameObject player)
    {
        switch (characterClass)
        {
            case (int)CharacterClassEnum.Knight:
                var Knight = new Knight();
                ChangeStats(Knight, player);
                break;
            case (int)CharacterClassEnum.Archer:
                var archer = new Archer();
                ChangeStats(archer, player);
                break;
            case (int)CharacterClassEnum.Tank:
                var tank = new Tank();
                ChangeStats(tank, player);
                break;
        }
    }

    static void ChangeStats(CharacterClass classObj, GameObject player)
    {
        var controller = player.GetComponent<PlayerControllerNetworked>();
        controller.speed = classObj.moveSpeed;
        controller.attackSpeed = classObj.attackSpeed;
        controller.MaxHealth = classObj.maxHealth;
        controller.weapon = classObj.weapon;
        controller.weapon.controller = controller;
        controller.weapon.rangeObject = controller.rangeObject;
        controller.weapon.skillObject = controller.SkillObject;
        controller.CurrentHealth = classObj.maxHealth;
        // Set skills
        controller.SkillList.Clear();
        foreach (var skill in classObj.SkillList)
        {
            controller.SkillList.Add(skill.Key, skill.Value);
        }
        player.GetComponent<Animator>().runtimeAnimatorController = classObj.characterAnimator;
    }

    // abstract public void Attack();

    // [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    // abstract public void RPC_CreateEffect();

    // [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    // abstract public void RPC_CreateProjectile();
}
