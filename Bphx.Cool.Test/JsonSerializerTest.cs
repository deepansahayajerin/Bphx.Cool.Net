using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bphx.Cool.Tests;

[TestClass]
public class JsonSerializerTest
{
  public class Item
  {
    public string Name { get; set; }
    public  int Id { get; set; }
  }

  public class Import
  {
    public string Title { get; set; }

    [JsonIgnore]
    public Array<Item> Items { get; } = new(10);

    [JsonPropertyName("items")]
    public IList<Item> Items_Json
    {
      get => Items;
      set => Items.Assign(value);
    }
  }

  [TestMethod]
  public void Test()
  {
    var import = new Import
    {
      Title = "A",
      Items = 
      {
        new() { Id = 1, Name = "x" },
        new() { Id = 2, Name = "y" }
      }
    };

    var options = new JsonSerializerOptions();
    
    var result = JsonSerializer.Serialize(import, options);

    Console.WriteLine(result);

    var import2 = JsonSerializer.Deserialize<Import>(result, options);

    Assert.IsNotNull(import2);
    Assert.AreEqual(import2.Title, "A");
    Assert.IsNotNull(import2.Items);
    Assert.AreEqual(import2.Items.Count, 2);
    Assert.AreEqual(import2.Items[0].Id, 1);
    Assert.AreEqual(import2.Items[1].Name, "y");
  }
}

