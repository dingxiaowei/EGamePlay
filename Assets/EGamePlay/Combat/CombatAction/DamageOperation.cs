﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EGamePlay;
using System;

namespace EGamePlay.Combat
{
    /// <summary>
    /// 伤害行动
    /// </summary>
    public class DamageOperation : CombatOperation
    {
        //伤害类型
        public DamageType DamageType { get; set; }
        //伤害来源
        public DamageSource DamageSource { get; set; }
        //伤害公式
        public string Expression { get; set; }
        //伤害数值
        public int DamageValue { get; set; }
        //是否是暴击
        public bool IsCritical { get; set; }
        //是否能暴击
        public bool CanCritical { get; set; }


        private void BeforeDamage()
        {
            if (DamageSource == DamageSource.Attack)
            {
                IsCritical = (RandomHelper.RandomRate() / 100f) < Creator.NumericBox.CriticalProb_F.Value;
                DamageValue = Mathf.Max(1, Creator.NumericBox.PhysicAttack_I.Value - Target.NumericBox.PhysicDefense_I.Value);
                if (IsCritical) DamageValue = (int)(DamageValue * 1.5f);
            }
            if (DamageSource == DamageSource.Skill)
            {
                DamageValue = int.Parse(Expression);
            }
        }

        public void ApplyDamage()
        {
            BeforeDamage();
            Target.ReceiveDamage(this);
            AfterDamage();
        }

        private void AfterDamage()
        {
            Creator.TriggerAction(CombatActionType.CauseDamage, this);
            Target.TriggerAction(CombatActionType.ReceiveDamage, this);
        }
    }

    public enum DamageSource
    {
        Attack = 1,//普攻
        Skill = 2,//技能
        Buff = 3,//Buff
    }
}