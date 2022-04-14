using System;
using System.Collections;
using System.Collections.Generic;


public enum AttributeKey
{
    maxHp = 0,
    hp,
    atk,
    def
}

//加入序列化特性，才能通过U3D暴露出该结构
[System.Serializable]
public class Attribute 
{
  

    public uint maxHp; // 血
    public uint atk; // 攻击
    public uint def; // 防御
    public uint hp = 0;
    public uint striking_Range_Min = 1;
    public uint striking_Range_Max =1 ;


    public uint this[int index]
    {
        get
        {

            if (index == 0) return this.maxHp;
            else if (index == 1) return this.hp;
            else if (index == 2) return this.atk;
            else if (index == 3) return this.def;
            else
            {
                throw new System.IndexOutOfRangeException();
            }
        }
        set
        {
            if (index == 0) this.maxHp = value;

            else if (index == 1) this.hp = value;
            else if (index == 2) this.atk = value;
            else if (index == 3) this.def = value;
            else
            {
                throw new System.IndexOutOfRangeException();
            }
        }
    }

    internal Attribute Clone()
    {
        var p = new Attribute();
        p[0] = this[0];
        p[1] = this[1];
        p[2] = this[2];
        p[3] = this[3];

        return p;
    }


    public void addValue(AttributeKey name,int addvalue)
    {
        var value = this[(int)name];

     
        if (addvalue < 0)
        {
            //数值相减
            //防止数值溢出
            if (value <=  Math.Abs(addvalue))
            {
                value = 0;
            }
            else
            {
                value -= (uint)Math.Abs(addvalue);
               
            }
        }else
        {
            value += (uint)addvalue;
        }

        this[(int)name] = value;

    }

   
}



