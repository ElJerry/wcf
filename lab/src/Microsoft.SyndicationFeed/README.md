# SyndicationFeed
SyndicationFeed provides an easy way to read and write Rss 2.0 and Atom Syndication Feeds.


### Requirements:
* [Visual Studio 2017](https://www.visualstudio.com/vs/whatsnew/)
* [DotNet Core 2.0 Preview](https://www.microsoft.com/net/core/preview#windowscmd)

### Building:
* The solution will build in Visual Studio 2017 after cloning.

### Running and Writing Tests:
* Open the solution in Visual Studio 2017.
* Test are located in the Tests project.
* Create a new class or open an existing one, create a method with the [Fact] attribute.
* To run the tests open the Test Explorer and click "Run All" or run each test individually.


# Examples
* A folder with different usage examples can be found [here](examples).

### Create RssReader and Fead a Feed ###
```
using (var xmlReader = XmlReader.Create(filePath))
{
    var feedReader = new Rss20FeedReader(xmlReader);

    while(await feedReader.Read())
    {
        switch (feedReader.ElementType)
        {
            // Read category
            case SyndicationElementType.Category:
                ISyndicationCategory category = await feedReader.ReadCategory();
                break;

            // Read Image
            case SyndicationElementType.Image:
                ISyndicationImage image = await feedReader.ReadImage();
                break;

            // Read Item
            case SyndicationElementType.Item:
                ISyndicationItem item = await feedReader.ReadItem();
                break;

            // Read link
            case SyndicationElementType.Link:
                ISyndicationLink link = await feedReader.ReadLink();
                break;

            // Read Person
            case SyndicationElementType.Person:
                ISyndicationPerson person = await feedReader.ReadPerson();
                break;

            // Read content
            default:
                ISyndicationContent content = await feedReader.ReadContent();
                break;
        }
    }
}
```

### Create RssWriter and Write Rss Item ###
```
var sw = new StringWriter();
using (XmlWriter xmlWriter = XmlWriter.Create(sw))
{
    var formatter = new Rss20Formatter();
    var writer = new Rss20FeedWriter(xmlWriter);
      
    // Create item
    var item = new SyndicationItem()
    {
        Title = "Rss Writer Avaliable",
        Description = "The new Rss Writer is now open source!",
        Id = "https://github.com/dotnet/wcf/tree/lab/lab/src/Microsoft.SyndicationFeed/src",
        Published = DateTimeOffset.UtcNow
    };

    item.AddCategory(new SyndicationCategory("Technology"));
    item.AddContributor(new SyndicationPerson() { Email = "test@mail.com" });

    await writer.Write(item);
    xmlWriter.Flush();
}
```
