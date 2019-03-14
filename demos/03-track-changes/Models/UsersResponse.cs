// Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. See LICENSE in the project root for license information.
using Newtonsoft.Json;
using System;

namespace msgraphapp.Models
{
  // A change notification.
  public class UsersResponse
  {
    // The type of change.
    [JsonProperty(PropertyName = "value")]
    public User[] Users { get; set; }

    [JsonProperty(PropertyName = "@odata.context")]
    public string Context { get; set; }

    [JsonProperty(PropertyName = "@odata.deltaLink")]
    public string DeltaLink { get; set; }

    [JsonProperty(PropertyName = "@odata.nextLink")]
    public string NextLink { get; set; }

  }

  public class User
  {
    [JsonProperty(PropertyName = "displayName")]
    public string DisplayName { get; set; }

    [JsonProperty(PropertyName = "givenName")]
    public string GivenName { get; set; }

    [JsonProperty(PropertyName = "surname")]
    public string Surname { get; set; }

    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; }

    [JsonProperty(PropertyName = "@removed")]
    public Change Removed { get; set; }
  }

  public class Change{
    [JsonProperty(PropertyName = "reason")]
    public string Reason { get; set; }
  }

}