﻿using ProtoBuf;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sanguosha.Lobby.Core;
[Serializable]
[ProtoContract]
public class Account
{
    [ProtoMember(1)]
    [Key]
    public string UserName { get; set; }
    [NonSerialized]
    private string password;

    public string Password
    {
        get { return password; }
        set { password = value; }
    }
    [ProtoMember(2)]
    public int Credits { get; set; }
    [ProtoMember(3)]
    public int Wins { get; set; }
    [ProtoMember(4)]
    public int Losses { get; set; }
    [ProtoMember(5)]
    public int Quits { get; set; }
    [ProtoMember(6)]
    public int TotalGames { get; set; }
    [ProtoMember(7)]
    public string DisplayedName { get; set; }
    [ProtoMember(8)]
    public int Experience { get; set; }
    [ProtoMember(9)]
    public int Level { get; set; }
    
    // Game Related
    [NonSerialized]
    private bool isDead;
    [NotMapped]
    public bool IsDead
    {
        get { return isDead; }
        set { isDead = value; }
    }
    
    [NonSerialized]
    private LoginToken loginToken;
    [NotMapped]
    public LoginToken LoginToken
    {
        get { return loginToken; }
        set { loginToken = value; }
    }
    
}
