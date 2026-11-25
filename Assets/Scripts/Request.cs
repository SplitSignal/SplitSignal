using System;
using System.Collections.Generic;
using System.Text;


[System.Serializable]
internal class UpdateRequest
{
    public string SessionCode;
    public string Object;
    public string Status;
    public string Notify;
}

[System.Serializable]
internal class CheckRequest
{
    public string SessionCode;
    public string Object;
}

public class UpdateClient
{
    public string Object { get; set; }

    public string Status { get; set; }
}

