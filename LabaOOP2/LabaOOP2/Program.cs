using System;
using System.Collections.Generic;

// Enumeration for different game modes
enum GameMode
{
    Standard,
    Practice
}

// Base class for game
abstract class BaseGame
{
    public abstract int CalculateRating(int player1Rating, int player2Rating, string outcome);
}

// Standard game class
class StandardGame : BaseGame
{
    public override int CalculateRating(int player1Rating, int player2Rating, string outcome)
    {
        int kFactor = 32; // Adjust as needed
        double expectedOutcome = 1 / (1 + Math.Pow(10, (player2Rating - player1Rating) / 400.0));

        if (outcome.ToLower() == "win")
        {
            return (int)(player1Rating + kFactor * (1 - expectedOutcome));
        }
        else if (outcome.ToLower() == "loss")
        {
            return (int)(player1Rating + kFactor * (0 - expectedOutcome));
        }
        else
        {
            return player1Rating; // Draw or other outcomes
        }
    }
}

// Practice game class
class PracticeGame : BaseGame
{
    public override int CalculateRating(int player1Rating, int player2Rating, string outcome)
    {
        return player1Rating; // Practice game does not affect rating
    }
}

// Factory class for creating games
class GameFactory
{
    public static BaseGame CreateStandardGame()
    {
        return new StandardGame();
    }

    public static BaseGame CreatePracticeGame()
    {
        return new PracticeGame();
    }
}

// Base class for game account
abstract class BaseGameAccount
{
    protected string UserName;
    protected int CurrentRating;
    protected int GamesCount;
    protected List<GameResult> GameHistory;
    protected Random random;
    protected BaseGame Game;
    protected GameMode Mode;

    public BaseGameAccount(string userName, int initialRating, GameMode mode)
    {
        UserName = userName;
        CurrentRating = initialRating;
        GamesCount = 0;
        GameHistory = new List<GameResult>();
        random = new Random();
        Mode = mode;
        SetGameMode(mode);
    }

    // Set the game mode for the player
    public void SetGameMode(GameMode mode)
    {
        Mode = mode;

        switch (mode)
        {
            case GameMode.Standard:
                Game = GameFactory.CreateStandardGame();
                break;
            case GameMode.Practice:
                Game = GameFactory.CreatePracticeGame();
                break;
                // Add more game modes if needed
                // ...
        }
    }

    public abstract int CalculatePoints(string outcome, int rating);

    public void RecordGameResult(string opponentName, string outcome, int rating)
    {
        GamesCount++;
        int pointsChange = CalculatePoints(outcome, rating);
        CurrentRating += pointsChange;

        GameHistory.Add(new GameResult(opponentName, outcome, rating, pointsChange, GamesCount - 1));
    }

    public void GetStats()
    {
        Console.WriteLine($"Game history for {UserName} ({Mode} mode):");
        foreach (var result in GameHistory)
        {
            Console.WriteLine($"Game {result.GameIndex + 1}: Against {result.OpponentName}, {result.Outcome} with rating {result.Rating}. Points Change: {result.PointsChange}");
        }
        Console.WriteLine($"Total games played: {GamesCount}, Current Rating: {CurrentRating}");
    }

    public void PlayGames(int numberOfGames)
    {
        for (int i = 0; i < numberOfGames; i++)
        {
            Console.Write($"Enter opponent name for game {i + 1}: ");
            string opponentName = Console.ReadLine();

            Console.Write($"Enter rating for game {i + 1}: ");
            if (int.TryParse(Console.ReadLine(), out int rating))
            {
                // Randomly determine the outcome
                string outcome = (random.Next(2) == 0) ? "Win" : "Loss";

                Console.WriteLine($"Game result for game {i + 1}: {outcome}");
                RecordGameResult(opponentName, outcome, rating);
            }
            else
            {
                Console.WriteLine($"Invalid rating. Game {i + 1} not recorded.");
            }
        }
    }

    // Method to simulate winning a game
    public void WinGame(string opponentName)
    {
        GamesCount++;
        int rating = GetOpponentRating(opponentName);
        int pointsChange = Game.CalculateRating(CurrentRating, rating, "Win");
        CurrentRating += pointsChange;

        GameHistory.Add(new GameResult(opponentName, "Win", rating, pointsChange, GamesCount - 1));
    }

    // Method to simulate losing a game
    public void LoseGame(string opponentName)
    {
        GamesCount++;
        int rating = GetOpponentRating(opponentName);
        int pointsChange = Game.CalculateRating(CurrentRating, rating, "Loss");
        CurrentRating += pointsChange;

        GameHistory.Add(new GameResult(opponentName, "Loss", rating, pointsChange, GamesCount - 1));
    }

    private int GetOpponentRating(string opponentName)
    {
        // In a real application, you might retrieve the opponent's rating from a database or another source.
        // For simplicity, I'm using a random rating here.
        return random.Next(1000, 2000);
    }
}

// Derived class with standard point calculation
class StandardGameAccount : BaseGameAccount
{
    public StandardGameAccount(string userName, int initialRating) : base(userName, initialRating, GameMode.Standard)
    {
    }

    public override int CalculatePoints(string outcome, int rating)
    {
        if (outcome.ToLower() == "win")
        {
            return rating;
        }
        else if (outcome.ToLower() == "loss")
        {
            return -rating;
        }
        return 0; // Draw or other outcomes
    }
}

// Derived class with half points deducted on loss
class HalfPointsDeductedAccount : BaseGameAccount
{
    public HalfPointsDeductedAccount(string userName, int initialRating) : base(userName, initialRating, GameMode.Standard)
    {
    }

    public override int CalculatePoints(string outcome, int rating)
    {
        if (outcome.ToLower() == "win")
        {
            return rating;
        }
        else if (outcome.ToLower() == "loss")
        {
            return -rating / 2;
        }
        return 0; // Draw or other outcomes
    }
}

// Derived class with bonus points for a series of victories
class VictorySeriesBonusAccount : BaseGameAccount
{
    private int consecutiveWins;

    public VictorySeriesBonusAccount(string userName, int initialRating) : base(userName, initialRating, GameMode.Standard)
    {
        consecutiveWins = 0;
    }

    public override int CalculatePoints(string outcome, int rating)
    {
        if (outcome.ToLower() == "win")
        {
            consecutiveWins++;
            return rating + (consecutiveWins >= 3 ? 10 : 0); // Bonus points for a series of 3 wins
        }
        else if (outcome.ToLower() == "loss")
        {
            consecutiveWins = 0; // Reset consecutive wins on loss
            return -rating;
        }
        return 0; // Draw or other outcomes
    }
}

// Game result class
class GameResult
{
    public string OpponentName;
    public string Outcome;
    public int Rating;
    public int PointsChange;
    public int GameIndex;

    public GameResult(string opponentName, string outcome, int rating, int pointsChange, int gameIndex)
    {
        OpponentName = opponentName;
        Outcome = outcome;
        Rating = rating;
        PointsChange = pointsChange;
        GameIndex = gameIndex;
    }
}

class Program
{
    static void Main()
    {
        Console.WriteLine("Select a game account type:");
        Console.WriteLine("1. Standard Game Account");
        Console.WriteLine("2. Half Points Deducted Game Account");
        Console.WriteLine("3. Victory Series Bonus Game Account");

        int accountTypeChoice;
        if (int.TryParse(Console.ReadLine(), out accountTypeChoice) && accountTypeChoice >= 1 && accountTypeChoice <= 3)
        {
            BaseGameAccount player1, player2;

            Console.Write("Enter player name 1: ");
            string playerName1 = Console.ReadLine();

            Console.Write("Enter initial rating for player 1: ");
            if (int.TryParse(Console.ReadLine(), out int initialRating1))
            {
                Console.Write("Enter player name 2: ");
                string playerName2 = Console.ReadLine();

                Console.Write("Enter initial rating for player 2: ");
                if (int.TryParse(Console.ReadLine(), out int initialRating2))
                {
                    switch (accountTypeChoice)
                    {
                        case 1:
                            player1 = new StandardGameAccount(playerName1, initialRating1);
                            player2 = new StandardGameAccount(playerName2, initialRating2);
                            break;
                        case 2:
                            player1 = new HalfPointsDeductedAccount(playerName1, initialRating1);
                            player2 = new HalfPointsDeductedAccount(playerName2, initialRating2);
                            break;
                        case 3:
                            player1 = new VictorySeriesBonusAccount(playerName1, initialRating1);
                            player2 = new VictorySeriesBonusAccount(playerName2, initialRating2);
                            break;
                        default:
                            Console.WriteLine("Invalid account type. Exiting.");
                            return;
                    }

                    // Demonstrate working with class objects
                    player1.PlayGames(3);
                    player1.GetStats();

                    player2.PlayGames(3);
                    player2.GetStats();
                }
                else
                {
                    Console.WriteLine("Invalid initial rating for player 2. Exiting.");
                }
            }
            else
            {
                Console.WriteLine("Invalid initial rating for player 1. Exiting.");
            }
        }
        else
        {
            Console.WriteLine("Invalid choice. Exiting.");
        }
    }
}
