using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Net;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;

namespace Bphx.Cool.Client
{
  public class ClientFactory
  {
    internal ClientFactory()
    { 
    }

    protected string BaseUrl;

    private static ClientFactory factory;

    public static ClientFactory GetInstance()
    {
      if (factory == null)
      {
        ClientFactory instance; 
        var type = ConfigurationManager.AppSettings["factory"];
        
        if (string.IsNullOrEmpty(type))
        {
          instance = new ClientFactory();
        }
        else
        {
          instance = Activator.CreateInstance(Type.GetType(type)) as ClientFactory;
        }

        instance.BaseUrl = ConfigurationManager.AppSettings["baseUrl"] ?? "";

        factory = instance;
      }

      return factory;
    }

    /// <summary>
    /// Creates a <b>Client</b> instance.
    /// </summary>
    /// <typeparam name="C">a descendant of <b>Client</b> type.</typeparam>
    /// <typeparam name="Request">a request type.</typeparam>
    /// <typeparam name="Response">a response type.</typeparam>
    /// <param name="userName">optional parameter, defines an user name.</param>
    /// <param name="password">optional parameter, defines the user's password.</param>
    /// <returns>an <typeparamref name="C"/> instance.</returns>
    /// <seealso cref="Client"/>
    public virtual C GetClient<C>
    (
      string userName = null,
      string password = null
    ) where C : Client, new()
    {
      var client = new C();

      if (!string.IsNullOrEmpty(userName))
      {
        var handler = new HttpClientHandler()
        {
          Credentials = new NetworkCredential(userName, password),
          PreAuthenticate = true
        };

        client.Transport = new HttpClient(handler);
      }
      else
      {
        client.Transport = new HttpClient();
      }

      var clientName = typeof(C).Name;

      client.Url = ConfigurationManager.AppSettings[clientName + ".url"] ??
        BaseUrl.Replace("{name}", clientName);

      client.Transport.DefaultRequestHeaders.Accept.Clear();
      client.Transport.DefaultRequestHeaders.Accept.Add(
          new MediaTypeWithQualityHeaderValue("application/json"));

      return client;
    }
  }

  /// <summary>
  /// Declares a base type for proxy's client.
  /// </summary>
  public class Client: IDisposable
  {
    private bool disposedValue;
    
    public HttpClient Transport { get; set; }

    public string Url { get; set; }

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          Transport.Dispose();

          Transport = null;
        }

        disposedValue = true;
      }
    }

    public void Dispose()
    {
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// A main entry point for proxy's client.
    /// </summary>
    /// <typeparam name="Request">
    /// a request type, which includes Global and Import data.
    /// </typeparam>
    /// <typeparam name="Response">
    /// a response type, which includes Gloabal and Export data.
    /// </typeparam>
    /// <param name="request">a request instance.</param>
    /// <returns>a response instance.</returns>
    public virtual async Task<Response> Execute<Request, Response>(Request request)
    {
      if (Transport == null)
      {
        throw new ArgumentNullException("Transport");
      }

      if (string.IsNullOrEmpty(Url))
      {
        throw new ArgumentNullException("Url");
      }

      var options = new JsonSerializerOptions
      {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        AllowTrailingCommas = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        NumberHandling = JsonNumberHandling.AllowReadingFromString |
          JsonNumberHandling.AllowReadingFromString,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
      };

//      options.Converters.Add(new TimeSpanToStringConverter());
      options.Converters.Add(new DateTimeToStringConverter());
      options.Converters.Add(new NullableDateTimeToStringConverter());
      options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

      var json = JsonSerializer.SerializeToUtf8Bytes<Request>(request, options);
      
      var content = new ByteArrayContent(json);

      content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
      content.Headers.ContentType.CharSet = "utf-8";

      var message = await Transport.PostAsync(Url, content).ConfigureAwait(false);

      json = await message.Content.ReadAsByteArrayAsync();

      return JsonSerializer.Deserialize<Response>(json, options);
    }
  }
}
