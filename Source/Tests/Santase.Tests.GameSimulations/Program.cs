namespace Santase.Tests.GameSimulations
{
    using System;

    using Santase.AI.SmartPlayer;
    using Santase.AI.ProPlayer;

    using Santase.Tests.GameSimulations.GameSimulators;

    public static class Program
    {
		public const int DefaultSimulationsCount = 100000;

		public static void Main()
        {
            // For easier debugging start a single game:
            //// new SantaseGame(new SmartPlayer(), new SmartPlayerOld()).Start();

            SimulateGames(new GenericGameSimulator<ProPlayer, SmartPlayer>());

            Console.WriteLine("Closed games: {0}", GlobalStats.GamesClosedByPlayer);

            //SimulateGames(new SmartAndDummyPlayerChangingTrumpSimulator());

            //SimulateGames(new SmartAndDummyPlayersSimulator());
        }

        private static void SimulateGames(IGameSimulator gameSimulator)
        {
			int simulationsCount = 100; // DefaultSimulationsCount

			Console.WriteLine($"Running {gameSimulator.GetType().Name}...");

            var simulationResult = gameSimulator.Simulate(simulationsCount);

            Console.WriteLine(simulationResult.SimulationDuration);
            Console.WriteLine($"Total games: {simulationResult.FirstPlayerWins:0,0} - {simulationResult.SecondPlayerWins:0,0}");
            Console.WriteLine($"Rounds played: {simulationResult.RoundsPlayed:0,0}");
            Console.WriteLine(
                $"Total round points (Our player): {simulationResult.FirstPlayerTotalRoundPoints:0,0} - {simulationResult.SecondPlayerTotalRoundPoints:0,0}");
            Console.WriteLine(new string('=', 75));

            Console.ReadKey();
        }
    }
}
