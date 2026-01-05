using SecuritySystem.Credential;

namespace SecuritySystem.UserSource;

public class ErrorMissedUserService<TUser>(IMissedUserErrorSource missedUserErrorSource) : IMissedUserService<TUser>
{
    public virtual TUser GetUser(UserCredential userCredential)
    {
        throw missedUserErrorSource.GetNotFoundException(typeof(TUser), userCredential);
    }

    public virtual IMissedUserService<User> ToSimple()
    {
        return new SimpleErrorMissedUserService(missedUserErrorSource);
    }

    private class SimpleErrorMissedUserService(IMissedUserErrorSource missedUserErrorSource) : IMissedUserService<User>
    {
        public User GetUser(UserCredential userCredential)
        {
            throw missedUserErrorSource.GetNotFoundException(typeof(TUser), userCredential);
        }

        public IMissedUserService<User> ToSimple()
        {
            return this;
        }
    }
}