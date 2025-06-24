using UnityEngine;

public class DragControl : BaseNoteControl
{
    public float[] FingerPositionX; // 0x50

    public override bool Judge()
    {
        return false;
    }
}