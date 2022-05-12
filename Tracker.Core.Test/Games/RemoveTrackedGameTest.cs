using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.EntityFrameworkCore;
using Tracker.Core.Exceptions;
using Tracker.Core.Games;
using Tracker.Domain;
using Tracker.Persistence;

namespace Tracker.Core.Test.Games;

[TestClass]
public class RemoveTrackedGameTest
{
    private static Mock<DatabaseContext>? MockDatabase { get; set; }

    private static RemoveTrackedGameHandler? DeleteTrackedGameHandler { get; set; }

    [ClassInitialize]
    public static void TestClassInit(TestContext context)
    {
        MockDatabase = new Mock<DatabaseContext>();

        DeleteTrackedGameHandler = new RemoveTrackedGameHandler(MockDatabase.Object);
    }
    
    [TestCleanup]
    public void TestCaseCleanup()
    {
        MockDatabase.Reset();
    }

    [TestMethod]
    public async Task RemoveTrackedGame_Exists()
    {
        // Setup
        var fakeGameRemoteId = 1;
        var fakeUserRemoteId = "d33Z_NuT5";
        var fakeTrackedGame = new TrackedGame
        {
            GameRemoteId = fakeGameRemoteId,
            UserRemoteId = fakeUserRemoteId,
        };

        MockDatabase!.Setup(databaseContext => databaseContext.TrackedGames)
            .ReturnsDbSet(new List<TrackedGame> { fakeTrackedGame });

        var command = new RemoveTrackedGameCommand(fakeUserRemoteId, fakeGameRemoteId);
        
        // Execute
        await DeleteTrackedGameHandler!.Handle(command, CancellationToken.None);

        // Verify
        MockDatabase.Verify(databaseContext => databaseContext.TrackedGames.Remove(fakeTrackedGame));
    }

    [TestMethod]
    public async Task RemoveTrackedGame_NotExists()
    {
        // Setup
        var fakeGameRemoteId = 1;
        var fakeUserRemoteId = "d33Z_NuT5";

        MockDatabase!.Setup(databaseContext => databaseContext.TrackedGames)
            .ReturnsDbSet(new List<TrackedGame>());

        var command = new RemoveTrackedGameCommand(fakeUserRemoteId, fakeGameRemoteId);
        
        // Execute
        // Verify
        await Assert.ThrowsExceptionAsync<NotFoundException>(() => 
            DeleteTrackedGameHandler!.Handle(command, CancellationToken.None));
    }
}