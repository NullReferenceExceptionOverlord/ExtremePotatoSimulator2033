namespace Santase.Tests.GameSimulations.GameSimulators
{
	using Logic.Players;
	using Logic.GameMechanics;

	public class GenericGameSimulator<TFirstPlayer, TSecondPlayer> : BaseGameSimulator where TFirstPlayer : IPlayer, new() where TSecondPlayer : IPlayer, new()
	{
		protected override ISantaseGame CreateGame()
		{
			IPlayer firstPlayer = new TFirstPlayer();
			IPlayer secondPlayer = new TSecondPlayer();
			ISantaseGame game = new SantaseGame(firstPlayer, secondPlayer);
			return game;
		}
	}
}