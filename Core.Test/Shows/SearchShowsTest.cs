using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Core.Shows;
using Domain;
using Service.Show;

namespace Core.Test.Shows;

[TestClass]
public class SearchShowsTest
{
    private static Mock<IShowService>? MockShowService { get; set; }

    private static IMapper? Mapper { get; set; }
    
    private static SearchShowsHandler? SearchShowsHandler { get; set; }

    [ClassInitialize]
    public static void TestClassInit(TestContext context)
    {
        MockShowService = new Mock<IShowService>();

        var mappingConfig = new MapperConfiguration(mc => { mc.AddProfile<MappingProfiles>(); });
        Mapper = mappingConfig.CreateMapper();

        SearchShowsHandler = new SearchShowsHandler(MockShowService.Object, Mapper);
    }

    [TestCleanup]
    public void TestCaseCleanup()
    {
        MockShowService.Reset();
    }

    [TestMethod]
    [DataRow("everything")]
    [DataRow("everything everywhere")]
    [DataRow("everything everywhere all")]
    [DataRow("everything everywhere all at")]
    [DataRow("everything everywhere all at once")]
    public async Task SearchShows_APIHit(string showTitle)
    {
        var fakeAPIShows = new List<APIShowBasic>
        {
            new(42069, "http://image.example.com", "everything everywhere all at once - movie", ShowType.Movie),
            new(420, "http://image.example.com", "everything everywhere all at once - the making of series", ShowType.Series)
        };
        
        MockShowService!
            .Setup(service => service.SearchShowByTitle(
                It.Is<string>(s => "everything everywhere all at once".Contains(s.ToLower()))))
            .ReturnsAsync(fakeAPIShows);
        
        var result = await SearchShowsHandler!.Handle(new SearchShowsQuery(showTitle), CancellationToken.None);
        
        MockShowService.Verify(service => service.SearchShowByTitle(showTitle), Times.Once);
        Assert.AreEqual(2,result.Items.Count);
        Assert.IsNotNull(result.Items.Find(s => s.RemoteId == fakeAPIShows[0].Id));
        Assert.IsNotNull(result.Items.Find(s => s.RemoteId == fakeAPIShows[1].Id));
    }
    
    [TestMethod]
    [DataRow("sma")]
    [DataRow("smash balls")]
    [DataRow("risa_smash")]
    public async Task SearchShows_APINoHit(string showTitle)
    {
        MockShowService!
            .Setup(service => service.SearchShowByTitle(It.IsAny<string>()))
            .ReturnsAsync(new List<APIShowBasic>());
        
        var result = await SearchShowsHandler!.Handle(new SearchShowsQuery(showTitle), CancellationToken.None);
        
        MockShowService.Verify(service => service.SearchShowByTitle(showTitle), Times.Once);
        Assert.AreEqual(0,result.Items.Count);
    }
}