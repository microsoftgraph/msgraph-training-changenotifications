using Newtonsoft.Json;
using System;

namespace msgraphapp.Models
{
  public class GraphToken
  {
    [JsonProperty(PropertyName = "access_token")]
    public string AccessToken { get; set; }
  }
}