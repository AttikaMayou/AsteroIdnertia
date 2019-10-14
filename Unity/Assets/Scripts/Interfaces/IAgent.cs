public interface IAgent
{
    IAgent Act(GameState gs, ActionsTypes[] availablesActions);
}