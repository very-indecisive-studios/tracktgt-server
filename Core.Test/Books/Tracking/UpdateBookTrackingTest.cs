using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Core.Books.Tracking;
using Core.Exceptions;
using Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Persistence;

namespace Core.Test.Books.Tracking;

[TestClass]
public class UpdateBookTrackingTest
{
    private static SqliteConnection? Connection { get; set; }

    private static DbContextOptions<DatabaseContext>? ContextOptions { get; set; }

    private static DatabaseContext? InMemDatabase { get; set; }

    private static UpdateBookTrackingHandler? UpdateBookTrackingHandler { get; set; }

    private static IMapper? Mapper { get; set; }

    [ClassInitialize]
    public static async Task TestClassInit(TestContext context)
    {
        // Setup in memory database
        Connection = new SqliteConnection("Filename=:memory:");
        Connection.Open();

        ContextOptions = new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlite(Connection)
            .Options;

        InMemDatabase = new DatabaseContext(ContextOptions);
        await InMemDatabase.Database.EnsureCreatedAsync();

        var mappingConfig = new MapperConfiguration(mc => { mc.AddProfile<MappingProfiles>(); });
        Mapper = mappingConfig.CreateMapper();

        UpdateBookTrackingHandler = new UpdateBookTrackingHandler(InMemDatabase, Mapper);
    }

    [ClassCleanup]
    public static async Task TestClassCleanup()
    {
        await Connection!.DisposeAsync();
    }

    [TestMethod]
    public async Task UpdateBookTracking_Exists()
    {
        // Setup
        var fakeUserRemoteId = "d33Z_NuT5";
        var fakeBookRemoteId = "69";
        var fakeChaptersRead = 10;
        var fakeFormat = BookTrackingFormat.Digital;
        var fakeStatus = BookTrackingStatus.Planning;
        var fakeOwnership = BookTrackingOwnership.Owned;
        InMemDatabase!.BookTrackings.Add(new BookTracking
        {
            UserRemoteId = fakeUserRemoteId,
            BookRemoteId = fakeBookRemoteId,
            ChaptersRead = fakeChaptersRead,
            Format = fakeFormat,
            Status = fakeStatus,
            Ownership = fakeOwnership
        });
        await InMemDatabase.SaveChangesAsync(CancellationToken.None);

        var newFakeChaptersRead = 25;
        var newFakeFormat = BookTrackingFormat.Physical;
        var newFakeStatus = BookTrackingStatus.Reading;
        var newFakeOwnership = BookTrackingOwnership.Loan;
        var command = new UpdateBookTrackingCommand(fakeUserRemoteId, fakeBookRemoteId, newFakeChaptersRead, 
            newFakeFormat, newFakeStatus, newFakeOwnership);
        
        // Execute
         await UpdateBookTrackingHandler!.Handle(command, CancellationToken.None);
        
        // Verify
        var updatedBookTracking = await InMemDatabase.BookTrackings
            .AsNoTracking()
            .Where(bt => bt.UserRemoteId == fakeUserRemoteId && bt.BookRemoteId == fakeBookRemoteId)
            .FirstOrDefaultAsync(CancellationToken.None);
        Assert.IsNotNull(updatedBookTracking);
        Assert.AreEqual(updatedBookTracking.ChaptersRead, newFakeChaptersRead);
        Assert.AreEqual(updatedBookTracking.Format, newFakeFormat);
        Assert.AreEqual(updatedBookTracking.Status, newFakeStatus);
        Assert.AreEqual(updatedBookTracking.Ownership, newFakeOwnership);
    }

    [TestMethod]
    public async Task UpdateBookTracking_NotExists()
    {
        // Setup
        var fakeUserRemoteId = "d33Z_NuT5";
        var fakeDiffUserRemoteId = "d33Z_NuT5_L+M41d3nL3s5";
        var fakeBookRemoteId = "69";
        var fakeDiffBookRemoteId = "420";
        
        var newFakeHoursPlayed = 25;
        var newFakeFormat = BookTrackingFormat.Physical;
        var newFakeStatus = BookTrackingStatus.Reading;
        var newFakeOwnership = BookTrackingOwnership.Owned;
        
        var commandDiffUser = new UpdateBookTrackingCommand(fakeDiffUserRemoteId, fakeBookRemoteId,
            newFakeHoursPlayed, newFakeFormat, newFakeStatus, newFakeOwnership);
        var commandDiffBook = new UpdateBookTrackingCommand(fakeUserRemoteId, fakeDiffBookRemoteId,
            newFakeHoursPlayed, newFakeFormat, newFakeStatus, newFakeOwnership);
        var commandDiffUserAndBook = new UpdateBookTrackingCommand(fakeDiffUserRemoteId, fakeDiffBookRemoteId,
            newFakeHoursPlayed, newFakeFormat, newFakeStatus, newFakeOwnership);

        // Execute
        // Verify
        await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
            UpdateBookTrackingHandler!.Handle(commandDiffUser, CancellationToken.None));
        await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
            UpdateBookTrackingHandler!.Handle(commandDiffBook, CancellationToken.None));
        await Assert.ThrowsExceptionAsync<NotFoundException>(() =>
            UpdateBookTrackingHandler!.Handle(commandDiffUserAndBook, CancellationToken.None));
    }
}