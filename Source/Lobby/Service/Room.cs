namespace Sanguosha.Lobby.Core;

public partial class Room
{
    public bool IsEmpty => !Seats.Any(state => state.State != SeatState.Empty && state.State != SeatState.Closed);

}
