using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFDSolution.Transport;
using TFDSolution.Transport.Master;

namespace TFDSolution.Business.Interface
{
    public interface IUserBusines
    {
        ResponseModel SetUserData(UserMast user);
        LoginUserInfo UserLogin(UserMast user);        
        bool UserNameExist(string username);
        UserMast GetUserInfo(Guid UserId);
        int GetActiveUsers();
        ResponseModel ReleaseUserLogin(string userId);

        ResponseModel UpdateUserPassword(UserPasswordModel model);
    }
}
