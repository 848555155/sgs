using System.Runtime.Serialization;

namespace Sanguosha.Lobby.Core;

[DataContract(Name = "RoomType")]
public enum RoomType
{
    [EnumMember]
    Role,
}

[DataContract(Name = "RoomType")]
public enum RoomState
{
    [EnumMember]
    Waiting,
    [EnumMember]
    Gaming
}

public partial class Room
{
    public int Id { get; set; }

    public string Name { get; set; }

    public RoomType Type { get; set; }

    public RoomState State { get; set; }

    public bool SpectatorDisabled { get; set; }

    public bool ChatDisabled { get; set; }

    public int OwnerId { get; set; }

    public List<Seat> Seats { get; set; } = [];

    public string IpAddress { get; set; }

    public int IpPort { get; set; }

    public bool IsEmpty => !Seats.Any(state => state.State != SeatState.Empty && state.State != SeatState.Closed);

    public RoomSettings Settings { get; set; }
}
