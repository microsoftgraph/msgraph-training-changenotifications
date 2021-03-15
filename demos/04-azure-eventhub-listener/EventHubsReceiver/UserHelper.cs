using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EventHubsReceiver
{
    /// <summary>
    /// Code forom the session (https://www.youtube.com/watch?v=EBbnpFdB92A)
    /// https://gist.github.com/kalyankrishna1/997f7ca1af1f73f8107c1c8cebfbaf3f
    /// </summary>
    public class UserHelper
    {
        GraphServiceClient _graphClient;
        string _tenantDomain = "kkaad.onmicrosoft.com";

        public UserHelper(GraphServiceClient graphClient, string tenantDomain)
        {
            _graphClient = graphClient;
            _tenantDomain = tenantDomain;
        }

        public async Task GraphDeltaQueryExample()
        {
            Microsoft.Graph.User newUser = null;

            #region first sync of users
            try
            {

                Console.WriteLine("=== Getting users");

                /// <summary>
                /// Get the list of users in the tenant - first full sync using delta query
                /// It selects only the displayName and userPrincipalName
                /// This first call ONLY gets the first page of results
                /// Subsequent calls page through the results, until we reach the end, and get the delta link/token
                /// The delta token can then be used to get changes since the last time we called
                /// </summary>
                var userPage = await _graphClient.Users
                    .Delta()
                    .Request()
                    .Select("displayName,userPrincipalName,givenName,surname")
                    .Top(10)
                    .GetAsync();

                /// <summary>
                /// Display users (by paging through the results) and get the delta link
                /// We'll use this same method later to get changes
                /// </summary>
                var deltaLink = await DisplayChangedUsersAndGetDeltaLink(userPage);

                #endregion first sync of users

                #region adding a new user to test delta changes

                Console.WriteLine("=== Adding user");

                /// <summary>
                /// Create a new user
                /// </summary>
                var u = new User()
                {
                    DisplayName = "UsersDeltaQuery Demo User",
                    GivenName = "UsersDeltaQueryDemo",
                    Surname = "User",
                    MailNickname = "UsersDeltaQueryDemoUser",
                    UserPrincipalName = Guid.NewGuid().ToString() + "@" + $"{_tenantDomain}",
                    PasswordProfile = new PasswordProfile() { ForceChangePasswordNextSignIn = true, Password = "D3m0p@55w0rd!" },
                    AccountEnabled = true
                };
                newUser = await _graphClient.Users.Request().AddAsync(u);

                #endregion adding a new user to test delta changes

                #region now get changes since last delta sync

                Console.WriteLine("Press any key to execute delta query.");
                Console.ReadKey();
                Console.WriteLine("=== Getting delta users");

                /// <summary>
                /// Get the first page using the delta link (to see the new user)
                /// </summary>
                userPage.InitializeNextPageRequest(_graphClient, deltaLink);
                userPage = await userPage.NextPageRequest.GetAsync();

                /// <summary>
                /// Display users again and get NEW delta link... notice that only the added user is returned
                /// Keep trying (in case there are replication delays) to get changes
                /// </summary>
                var newDeltaLink = await DisplayChangedUsersAndGetDeltaLink(userPage);
                while (deltaLink.Equals(newDeltaLink))
                {
                    // If the two are equal, then we didn't receive changes yet
                    // Query to get first page using the delta link
                    userPage.InitializeNextPageRequest(_graphClient, deltaLink);
                    userPage = await userPage.NextPageRequest.GetAsync();
                    newDeltaLink = await DisplayChangedUsersAndGetDeltaLink(userPage);
                }

                #endregion now get changes since last delta sync

            }
            catch (Exception ex)
            {
                Console.WriteLine( $"{ex}");
            }
            finally
            {
                //Finally, delete the user
                Console.WriteLine("=== Deleting user");
                //Finally, delete the user
                await _graphClient.Users[newUser.Id].Request().DeleteAsync();

            }
            #region clean-up


            #endregion clean-up
        }

        private static async Task<string> DisplayChangedUsersAndGetDeltaLink(IUserDeltaCollectionPage userPage)
        {
            /// <summary>
            /// Using the first page as a starting point (as the input)
            /// iterate through the first and subsequent pages, writing out the users in each page
            /// until you reach the last page (NextPageRequest is null)
            /// finally set the delta link by looking in the additional data, and return it
            /// </summary>

            /// Iterate through the users
            foreach (var user in userPage)
            {
                if (user.UserPrincipalName != null)
                    Console.WriteLine(user.UserPrincipalName.ToLower() + "\t\t" + user.DisplayName);
            }

            if (userPage.NextPageRequest != null)
            {
                userPage = await userPage.NextPageRequest.GetAsync();
                return await DisplayChangedUsersAndGetDeltaLink(userPage);
            }
            else
            {
                return (string)userPage.AdditionalData["@odata.deltaLink"];
            }
        }
    }
}
