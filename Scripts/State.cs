using UnityEngine;

public class State
{
    public int player_id { set; get; }
    public ulong mes_id { set; get; }
    public Vector2 mousePosition { set; get; }
    public Vector2 movementVector { set; get; }
    public int hp { set; get; }
    public bool shoot { set; get; }
}