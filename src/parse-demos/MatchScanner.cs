/*
    Creates a DemoParser of the provided demoFilePath and
    iterates the rounds in search of bomb plants.

    Because we have to parse the demo as a series of ticks
    sequentially and get so much information from events,
    we won't know the end result of a round when we
    process the ticks that have time series data. To work
    around this, the MatchScanner must cache partial results
    privately until a round result is known at which point a
    chunk of TrainingResults are flushed to the enumerator.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DemoInfo;

public class MatchScanner : IDisposable
{
    private DemoParser _demoParser;
    private Stream _parserStream;

    private readonly double secondsPerTick;

    private int currentTick => _demoParser.CurrentTick;

    /*******************
    * Round  variables *
    *******************/
    private int roundNumber = 0;
    private bool isBombPlanted = false;
    private bool isRoundOver = false;
    private int bombPlantedTick = 0;

    // Set to true when our tick parser determines
    // a training result should be created
    private bool snapshotResultThisTick = false;

    // Used to track number of ticks since
    // a bomb plant occurred this round.
    private int bombPlantTotalElapsedTicks = 0;

    // Used to generate a training result every n number of ticks
    private int bombPlantYieldResultElapsedTicks = 0;

    // Bombsite char (passed by DemoInfo event args) of current round's plant
    private char? bombsite = null;

    // List of partial results being saved until the winner of the round is known
    private List<TrainingResult> partialResults = new List<TrainingResult>();

    // List of training results ready to be yielded at the next possible
    // opportunity of our enumerator.
    private List<TrainingResult> yieldResults = new List<TrainingResult>();

    public MatchScanner(Stream inputDemoStream)
    {
        _parserStream = inputDemoStream;
        _demoParser = new DemoParser(_parserStream);

        _demoParser.ParseHeader();

        secondsPerTick = 1.0 / _demoParser.TickRate;

        RegisterCallbacks();
    }

    public void Dispose()
    {
        _demoParser.Dispose();
        _parserStream.Dispose();
    }

    /*
    Because we don't know the outcome of the round when parsing
    results, we have to store values locally until the end of each
    round when the 
    */
    public IEnumerable<TrainingResult> EnumerateTrainingResults()
    {
        while (_demoParser.ParseNextTick())
        {
            if (snapshotResultThisTick)
            {
                var result = SnapshotCurrentResult();

                if (result != null)
                {
                    partialResults.Add(result);
                    // Console.WriteLine("Snapshot. currentTick=" + currentTick + ", numPartialResults=" + partialResults.Count);
                }
            }

            foreach (var v in yieldResults)
            {
                yield return v;
            }

            yieldResults.Clear();

            snapshotResultThisTick = false;
        }
    }

    private TrainingResult SnapshotCurrentResult()
    {
        Player[] cts = GetCTs().ToArray();
        Player[] ts = GetTs().ToArray();

        if (cts.Length != 5 || ts.Length != 5)
        {
            Console.WriteLine("Not 5 players on a team!");
            return null;
        }

        TrainingResult result = new TrainingResult()
        {
            bombplant_site = bombsite ?? 'U',
            elapsed_since_bombplant = (float)(secondsPerTick * bombPlantTotalElapsedTicks),
            round_number = roundNumber,
            map_id = _demoParser.Map,
            rounds_per_half = roundNumber <= 30 ? 15 : 3,
            round_of_half = roundNumber <= 30 ?
                (roundNumber - 1) % 15 + 1
                : (roundNumber - 1) % 3 + 1
        };

        for (int i = 0; i < 5; i++)
        {
            var team = Team.CounterTerrorist;
            var x = cts[i];

            var j = i + 1;

            result.SetPlayerIsAlive(team, j, x.IsAlive);
            result.SetPlayerEquipmentValue(team, j, x.CurrentEquipmentValue);
            result.SetPlayerEquippedWeapon(team, j, x.ActiveWeapon?.Weapon ?? EquipmentElement.Unknown);
            result.SetPlayerHasHelmet(team, j, x.HasHelmet);
            result.SetPlayerHasKevlar(team, j, x.Armor > 0);
            result.SetPlayerHP(team, j, x.HP);
            result.SetPlayerHasDefuseKit(j, x.HasDefuseKit);
        }

        for (int i = 0; i < 5; i++)
        {
            var team = Team.Terrorist;
            var x = ts[i];

            var j = i + 1;

            result.SetPlayerIsAlive(team, j, x.IsAlive);
            result.SetPlayerEquipmentValue(team,j, x.CurrentEquipmentValue);
            result.SetPlayerEquippedWeapon(team, j, x.ActiveWeapon?.Weapon ?? EquipmentElement.Unknown);
            result.SetPlayerHasHelmet(team, j, x.HasHelmet);
            result.SetPlayerHasKevlar(team, j, x.Armor > 0);
            result.SetPlayerHP(team, j, x.HP);
        }

        return result;
    }

    private IEnumerable<Player> GetCTs()
        => GetPlayersForTeam(Team.CounterTerrorist);

    private IEnumerable<Player> GetTs()
        => GetPlayersForTeam(Team.Terrorist);

    private IEnumerable<Player> GetPlayersForTeam(Team team)
    {
        int yielded = 0;

        foreach (var kvp in _demoParser.Players)
        {
            if (yielded >= 5)
            {
                continue;
            }

            if (kvp.Value.Team == team)
            {
                yielded++;
                yield return kvp.Value;
            }
        }
    }

    /**********************
    *  Events & Callbacks *
    **********************/
    private void RegisterCallbacks()
    {
        _demoParser.TickDone += OnTickDone;
        _demoParser.FreezetimeEnded += OnFreezetimeEnded;
        _demoParser.RoundEnd += OnRoundEnded;
        _demoParser.BombPlanted += OnBombPlanted;
        _demoParser.BombExploded += OnBombExploded;
        _demoParser.BombDefused += OnBombDefused;
    }

    private void OnTickDone(object sender, TickDoneEventArgs args)
    {
        if (isBombPlanted && !isRoundOver)
        {
            bombPlantTotalElapsedTicks++;
            bombPlantYieldResultElapsedTicks++;

            var elapsedSeconds = secondsPerTick * bombPlantYieldResultElapsedTicks;

            if (elapsedSeconds > 1f)
            {
                snapshotResultThisTick = true;
                bombPlantYieldResultElapsedTicks = 0;
            }
        }
    }

    private void OnFreezetimeEnded(object sender, FreezetimeEndedEventArgs args)
    {
        // Console.WriteLine($"Round {roundNumber} started, tick " + currentTick);

        roundNumber++;
        isBombPlanted = false;
        bombPlantTotalElapsedTicks = 0;
        isRoundOver = false;
    }

    private void OnRoundEnded(object sender, RoundEndedEventArgs args)
    {
        // Console.WriteLine($"Round {roundNumber} ended, tick " + currentTick + ", reason " + args.Reason.ToString());

        foreach (var trainingResult in partialResults)
        {
            trainingResult.team_ct_wins_round = args.Winner == Team.CounterTerrorist;
            yieldResults.Add(trainingResult);
        }

        partialResults.Clear();

        isBombPlanted = false;
        bombPlantedTick = 0;
        bombPlantTotalElapsedTicks = 0;
        isRoundOver = true;
        bombsite = null;
    }

    private void OnBombPlanted(object sender, BombEventArgs args)
    {
        // Console.WriteLine($"Bomb planted, round={roundNumber}, currentTick={currentTick}, site={args.Site}, player={args.Player.Name}");

        isBombPlanted = true;
        bombPlantedTick = _demoParser.CurrentTick;
        bombPlantTotalElapsedTicks = 0;
        bombsite = args.Site;
    }

    private void OnBombExploded(object sender, BombEventArgs args)
    {
        // Console.WriteLine($"Bomb exploded, round={roundNumber}, currentTick={currentTick}");

        isBombPlanted = false;
        bombPlantedTick = 0;
        bombPlantTotalElapsedTicks = 0;
    }

    private void OnBombDefused(object sender, BombEventArgs args)
    {
        // Console.WriteLine($"Bomb defused, round={roundNumber}, currentTick={currentTick}");

        isBombPlanted = false;
        bombPlantedTick = 0;
        bombPlantTotalElapsedTicks = 0;
    }
}