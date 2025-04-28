using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Snowflake.EntityFrameworkCore.TestUtilities;

namespace Snowflake.EntityFrameworkCore.FunctionalTests.Query;

public class GearsOfWarQuerySnowflakeFixture : GearsOfWarQueryRelationalFixture
{
    protected override ITestStoreFactory TestStoreFactory => SnowflakeTestStoreFactory.Instance;

    private GearsOfWarData? _expectedData;

    public override ISetSource GetExpectedData()
    {
        if (_expectedData is not null) return _expectedData;
        _expectedData = (GearsOfWarData)base.GetExpectedData();
        foreach (var mission in _expectedData.Missions)
        {
            var missionTimeline = mission.Timeline;
            mission.Timeline = new DateTimeOffset(
                missionTimeline.Year,
                missionTimeline.Month,
                missionTimeline.Day,
                missionTimeline.Hour,
                missionTimeline.Minute,
                missionTimeline.Second,
                0,
                TimeSpan.Zero
            );
            mission.Timeline = mission.Timeline.ToUniversalTime();
        }

        return _expectedData;
    }

    protected override void Seed(GearsOfWarContext context)
    {
        var squads = GearsOfWarData.CreateSquads();
        var missions = GearsOfWarData.CreateMissions();
        var squadMissions = GearsOfWarData.CreateSquadMissions();
        var cities = GearsOfWarData.CreateCities();
        var weapons = GearsOfWarData.CreateWeapons();
        var tags = GearsOfWarData.CreateTags();
        var gears = GearsOfWarData.CreateGears();
        var locustLeaders = GearsOfWarData.CreateLocustLeaders();
        var factions = GearsOfWarData.CreateFactions();
        var locustHighCommands = GearsOfWarData.CreateHighCommands();

        foreach (var mission in missions)
        {
            var missionTimeline = mission.Timeline;
            mission.Timeline = new DateTimeOffset(
                missionTimeline.Year,
                missionTimeline.Month,
                missionTimeline.Day,
                missionTimeline.Hour,
                missionTimeline.Minute,
                missionTimeline.Second,
                0,
                TimeSpan.Zero
            );
            mission.Timeline = mission.Timeline.ToUniversalTime();
        }

        GearsOfWarData.WireUp(
            squads, missions, squadMissions, cities, weapons, tags, gears, locustLeaders, factions, locustHighCommands);

        context.Squads.AddRange(squads);
        context.Missions.AddRange(missions);
        context.SquadMissions.AddRange(squadMissions);
        context.Cities.AddRange(cities);
        context.Weapons.AddRange(weapons);
        context.Tags.AddRange(tags);
        context.Gears.AddRange(gears);
        context.LocustLeaders.AddRange(locustLeaders);
        context.Factions.AddRange(factions);
        context.LocustHighCommands.AddRange(locustHighCommands);
        context.SaveChanges();

        GearsOfWarData.WireUp2(locustLeaders, factions);

        context.SaveChanges();
    }
}