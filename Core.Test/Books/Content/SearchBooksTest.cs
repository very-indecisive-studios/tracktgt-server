using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Core.Books.Content;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Service.Book;

namespace Core.Test.Books.Content;

[TestClass]
public class SearchBooksTest
{
    private static Mock<IBookService>? MockBookService { get; set; }

    private static IMapper? Mapper { get; set; }
    
    private static SearchBooksHandler? SearchBooksHandler { get; set; }

    [ClassInitialize]
    public static void TestClassInit(TestContext context)
    {
        MockBookService = new Mock<IBookService>();

        var mappingConfig = new MapperConfiguration(mc => { mc.AddProfile<MappingProfiles>(); });
        Mapper = mappingConfig.CreateMapper();

        SearchBooksHandler = new SearchBooksHandler(MockBookService.Object, Mapper);
    }

    [TestCleanup]
    public void TestCaseCleanup()
    {
        MockBookService.Reset();
    }

    [TestMethod]
    [DataRow("ch")]
    [DataRow("chaos")]
    [DataRow("chaos chef")]
    public async Task SearchBooks_APIHit(string gameTitle)
    {
        var fakeAPIBooks = new List<APIBookBasic>
        {
            new("42069", "http://image.example.com", "Chaos Chef: Manual", new List<string> { "Sterling Kwan" }),
            new("12345", "http://image2.example.com", "Chaos Chef Ultimate: Ultimate Manual", new List<string> { "Bryan Seah" })
        };
        
        MockBookService!
            .Setup(service => service.SearchBookByTitle(
                It.Is<string>(s => "chaos chef".Contains(s.ToLower()))))
            .ReturnsAsync(fakeAPIBooks);
        
        var result = await SearchBooksHandler!.Handle(new SearchBooksQuery(gameTitle), CancellationToken.None);
        
        MockBookService.Verify(service => service.SearchBookByTitle(gameTitle), Times.Once);
        Assert.AreEqual(2,result.Items.Count);
        Assert.IsNotNull(result.Items.Find(g => g.RemoteId == fakeAPIBooks[0].Id));
        Assert.IsNotNull(result.Items.Find(g => g.RemoteId == fakeAPIBooks[1].Id));
    }
    
    [TestMethod]
    [DataRow("sma")]
    [DataRow("smash balls")]
    [DataRow("risa_smash")]
    public async Task SearchBooks_APINoHit(string gameTitle)
    {
        MockBookService!
            .Setup(service => service.SearchBookByTitle(It.IsAny<string>()))
            .ReturnsAsync(new List<APIBookBasic>());
        
        var result = await SearchBooksHandler!.Handle(new SearchBooksQuery(gameTitle), CancellationToken.None);
        
        MockBookService.Verify(service => service.SearchBookByTitle(gameTitle), Times.Once);
        Assert.AreEqual(0,result.Items.Count);
    }
}