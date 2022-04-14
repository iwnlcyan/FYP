using SampleBehaviorTree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wait : ActionBehavior
{
    public PlayerController playerC;
    public override IEnumerator Execute()
    {

        GameCtrl.instance.Wait_AI(playerC);
        return base.Execute();
    }
}
