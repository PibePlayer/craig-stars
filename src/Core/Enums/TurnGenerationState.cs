namespace CraigStars
{
    /// <summary>
    /// The field for A tech
    /// </summary>
    public enum TurnGenerationState
    {
        WaitingForPlayers = 0,
        FleetAgeStep = 10,
        FleetScrapStep = 20,
        FleetWaypoint0Step = 30,
        PacketMove0Step = 40,
        MysteryTraderMoveStep = 50,
        FleetMoveStep = 60,
        FleetReproduceStep = 70,
        DecaySalvageStep = 80,
        DecayPacketsStep = 90,
        WormholeJiggleStep = 100,
        DetonateMinesStep = 110,
        PlanetMineStep = 120,
        PlanetProductionStep = 130,
        PlayerResearchStep = 140,
        ResearchStealerStep = 150,
        PermaformStep = 160,
        PlanetGrowStep = 170,
        PacketMove1Step = 180,
        FleetRefuelStep = 190,
        RandomCometStrikeStep = 200,
        RandomMineralDiscoveryStep = 210,
        RandomPlanetaryChangeStep = 220,
        FleetBattleStep = 230,
        FleetBombStep = 240,
        MysteryTraderMeetStep = 250,
        RemoteMineStep = 260,
        FleetWaypoint1Step = 270,
        DecayMinesStep = 280,
        FleetLayMinesStep = 290,
        FleetTransferStep = 300,
        InstaformStep = 310,
        FleetSweepMinesStep = 320,
        FleetRepairStep = 330,
        RemoteTerraformStep = 340,
        PlayerScanStep = 350,
        CalculateScoreStep = 360,
        UpdatingPlayers = 370,
        CheckVictoryStep = 380,
        Finished = 390,
        Saving = 400,
    }
}


