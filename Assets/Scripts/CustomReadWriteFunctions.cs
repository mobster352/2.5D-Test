using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public static class CustomReadWriteFunctions
{
    public static void WriteDoor(this NetworkWriter writer, Door door){
        writer.WriteBoolean(door.canOpen);
        writer.WriteBoolean(door.doorOpening);
    }

    public static Door ReadDoor(this NetworkReader reader){
        return new Door(reader.ReadBoolean(), reader.ReadBoolean());
    }
}
