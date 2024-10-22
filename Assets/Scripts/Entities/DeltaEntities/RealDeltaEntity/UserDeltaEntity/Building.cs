using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class Building : UserDeltaEntity
{
    protected override int MoveSpeed() => 0;
    protected override bool CanPush => false;
}
