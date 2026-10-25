using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class ServerMessage
{
    public string type;
    public ObjectUpdatePayload payload;
}

[Serializable]
public class ObjectUpdatePayload
{
    public string objectId;
    public string state;
}

public class ServerConnection : MonoBehaviour
{

}
