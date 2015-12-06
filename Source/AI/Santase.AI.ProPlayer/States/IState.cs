namespace Santase.AI.ProPlayer.States
{
	using Logic.Players;

	public interface IState
	{
		PlayerAction ChooseCard(PlayerTurnContext context);
	}
}