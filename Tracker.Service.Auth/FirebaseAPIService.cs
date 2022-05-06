using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;

namespace Tracker.Service.User;

public class FirebaseAPIService : IUserService
{
    public FirebaseAPIService(string credentialsPath)
    {
        FirebaseApp.Create(new AppOptions()
        {
            Credential = GoogleCredential.FromFile(credentialsPath)
        });
    }

    public async Task<APIUser?> GetUser(string uid)
    {
        try
        {
            UserRecord userRecord = await FirebaseAuth.DefaultInstance.GetUserAsync(uid);

            return new APIUser
            {
                RemoteId = userRecord.Uid,
                Email = userRecord.Email
            };
        }
        catch (FirebaseAuthException exception)
        {
            return null;
        }
    }
}