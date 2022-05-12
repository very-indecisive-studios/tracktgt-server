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
public class DeleteTrackedGameTest
{
    private Mock<DatabaseContext>? MockDatabase { get; set; }

    private DeleteTrackedGameHandler? DeleteTrackedGameHandler { get; set; }

    [TestInitialize]
    public void TestCaseInit()
    {
        MockDatabase = new Mock<DatabaseContext>();

        DeleteTrackedGameHandler = new DeleteTrackedGameHandler(MockDatabase.Object);
    }

    [TestMethod]
    public async Task DeleteTrackedGame_ExistsAndUserMatched()
    {
        // Setup
        var fakeTrackedGameId = new Guid();
        var fakeUserRemoteId = "d33Z_NuT5";
        var fakeTrackedGame = new TrackedGame
        {
            Id = fakeTrackedGameId,
            UserRemoteId = fakeUserRemoteId,
        };

        MockDatabase!.Setup(databaseContext => databaseContext.TrackedGames)
            .ReturnsDbSet(new List<TrackedGame> { fakeTrackedGame });

        var command = new DeleteTrackedGameCommand(fakeUserRemoteId, fakeTrackedGameId);
        
        // Execute
        await DeleteTrackedGameHandler!.Handle(command, CancellationToken.None);

        // Verify
        MockDatabase.Verify(databaseContext => databaseContext.TrackedGames.Remove(fakeTrackedGame));
    }
    
    [TestMethod]
    public async Task DeleteTrackedGame_ExistsAndUserNotMatched()
    {
        // Setup
        var fakeTrackedGameId = new Guid();
        var fakeUserRemoteId = "d33Z_NuT5";
        var fakeDiffUserRemoteId = "d33Z_NuT5+L+M41d3Nl35S";
        var fakeTrackedGame = new TrackedGame
        {
            Id = fakeTrackedGameId,
            UserRemoteId = fakeUserRemoteId,
        };

        MockDatabase!.Setup(databaseContext => databaseContext.TrackedGames)
            .ReturnsDbSet(new List<TrackedGame> { fakeTrackedGame });

        var command = new DeleteTrackedGameCommand(fakeDiffUserRemoteId, fakeTrackedGameId);
        
        // Execute
        // Verify
        await Assert.ThrowsExceptionAsync<ForbiddenException>(() => 
            DeleteTrackedGameHandler!.Handle(command, CancellationToken.None));
    }
    
    [TestMethod]
    public async Task DeleteTrackedGame_NotExists()
    {
        // Setup
        var fakeTrackedGameId = new Guid();
        var fakeUserRemoteId = "d33Z_NuT5";

        MockDatabase!.Setup(databaseContext => databaseContext.TrackedGames)
            .ReturnsDbSet(new List<TrackedGame>());

        var command = new DeleteTrackedGameCommand(fakeUserRemoteId, fakeTrackedGameId);
        
        // Execute
        // Verify
        await Assert.ThrowsExceptionAsync<NotFoundException>(() => 
            DeleteTrackedGameHandler!.Handle(command, CancellationToken.None));
    }
}