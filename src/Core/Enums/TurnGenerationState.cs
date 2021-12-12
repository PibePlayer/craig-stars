namespace CraigStars
{
    /// <summary>
    /// The state turn generation is in, with order being the value of the enum
    /// </summary>
    public enum TurnGenerationState
    {
        WaitingForPlayers = 0,
        FleetAgeStep = 10,
        FleetScrapStep = 20,
        FleetUnload0Step = 30,
        FleetColonize0Step = 40,
        FleetLoad0Step = 50,
        FleetLoadDunnage0Step = 60,
        FleetMerge0Step = 70,
        FleetRoute0Step = 80,
        PacketMove0Step = 90,
        MysteryTraderMoveStep = 100,
        FleetMoveStep = 110,
        FleetReproduceStep = 120,
        DecaySalvageStep = 130,
        DecayPacketsStep = 140,
        WormholeJiggleStep = 150,
        DetonateMinesStep = 160,
        PlanetMineStep = 170,
        RemoteMineARStep = 180,
        PlanetProductionStep = 190,
        PlayerResearchStep = 200,
        ResearchStealerStep = 210,
        PermaformStep = 220,
        PlanetGrowStep = 230,
        PacketMove1Step = 240,
        FleetRefuelStep = 250,
        RandomCometStrikeStep = 260,
        RandomMineralDiscoveryStep = 270,
        RandomPlanetaryChangeStep = 280,
        FleetBattleStep = 290,
        FleetBombStep = 300,
        MysteryTraderMeetStep = 310,
        RemoteMine0Step = 320,
        FleetUnload1Step = 330,
        FleetColonize1Step = 340,
        FleetLoad1Step = 350,
        FleetLoadDunnage1Step = 360,
        DecayMinesStep = 370,
        FleetLayMinesStep = 380,
        FleetTransferStep = 390,
        FleetMerge1Step = 400,
        FleetRoute1Step = 410,
        InstaformStep = 420,
        FleetSweepMinesStep = 430,
        FleetRepairStep = 440,
        RemoteTerraformStep = 450,
        PlayerScanStep = 460,
        FleetPatrolStep = 470,
        FleetNotifyIdleStep = 480,
        CalculateScoreStep = 490,
        UpdatingPlayers = 500,
        CheckVictoryStep = 510,
        Finished = 520,
        Saving = 530,
    }
}


