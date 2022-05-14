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
public class RemoveGameTrackingTest
{
    private static Mock<DatabaseContext>? MockDatabase { get; set; }

    private static RemoveGameTrackingHandler? RemoveGameTrackingHandler { get; set; }

    [ClassInitialize]
    public static void TestClassInit(TestContext context)
    {
        MockDatabase = new Mock<DatabaseContext>();

        RemoveGameTrackingHandler = new RemoveGameTrackingHandler(MockDatabase.Object);
    }
    
    [TestCleanup]
    public void TestCaseCleanup()
    {
        MockDatabase.Reset();
    }

    [TestMethod]
    public async Task RemoveGameTracking_Exists()
    {
        // Setup
        var fakeGameRemoteId = 1;
        var fakeUserRemoteId = "d33Z_NuT5";
        var fakePlatform = "PC";
        var fakeGameTracking = new GameTracking
        {
            GameRemoteId = fakeGameRemoteId,
            UserRemoteId = fakeUserRemoteId,
            Platform = fakePlatform
        };

        MockDatabase!.Setup(databaseContext => databaseContext.GameTrackings)
            .ReturnsDbSet(new List<GameTracking> { fakeGameTracking });

        var command = new RemoveGameTrackingCommand(fakeUserRemoteId, fakeGameRemoteId, fakePlatform);
        
        // Execute
        await RemoveGameTrackingHandler!.Handle(command, CancellationToken.None);

        // Verify
        MockDatabase.Verify(databaseContext => databaseContext.GameTrackings.Remove(fakeGameTracking));
    }

    [TestMethod]
    public async Task RemoveGameTracking_NotExists()
    {
        // Setup
        var fakeGameRemoteId = 1;
        var fakeUserRemoteId = "d33Z_NuT5";
        var fakePlatform = "PC";

        MockDatabase!.Setup(databaseContext => databaseContext.GameTrackings)
            .ReturnsDbSet(new List<GameTracking>());

        var command = new RemoveGameTrackingCommand(fakeUserRemoteId, fakeGameRemoteId, fakePlatform);
        
        // Execute
        // Verify
        await Assert.ThrowsExceptionAsync<NotFoundException>(() => 
            RemoveGameTrackingHandler!.Handle(command, CancellationToken.None));
    }
}