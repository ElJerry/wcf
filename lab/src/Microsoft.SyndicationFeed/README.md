# SyndicationFeed
SyndicationFeed provides an easy way to read and write Rss 2.0 and Atom Feeds.

### Requirements:
* [Visual Studio 2017](https://www.visualstudio.com/vs/whatsnew/)
* [DotNet Core 2.0 Preview](https://www.microsoft.com/net/core/preview#windowscmd)

### Building:
* The solution will build in Visual Studio 2017 after cloning.

### Running and Writing Tests:
* Open the solution in Visual Studio 2017.
* Test are located in the Tests project.
* To write a test create a new class or open an existing one, create a method with the [Fact] attribute.
* To run the tests open the Test Explorer and click "Run All" or run each test individually.

# Examples
* A folder with different usage examples can be found [here](examples).

### Create an RssReader ###
```
using (var xmlReader = XmlReader.Create(filePath, new XmlReaderSettings() { Async = true }))
{
    var reader = new Rss20FeedReader(xmlReader);
}
```


### Read and Rss Feed ###
```
var reader = new Rss20FeedReader(xmlReader);
while (await reader.Read())
{
    switch (reader.ElementType)
    {
        case SyndicationElementType.Link:
            ISyndicationLink link = await reader.ReadLink();
            break;

        case SyndicationElementType.Item:
            ISyndicationItem item = await reader.ReadItem();
            break;

        case SyndicationElementType.Person:
            ISyndicationPerson person = await reader.ReadPerson();
            break;

        case SyndicationElementType.Image:
            ISyndicationImage image = await reader.ReadImage();
            break;

        default:
            ISyndicationContent content = await reader.ReadContent();
            break;
    }
}
```
