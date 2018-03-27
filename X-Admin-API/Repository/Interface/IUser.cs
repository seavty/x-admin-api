using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X_Admin_API.Models.DTO.User;

namespace X_Admin_API.Repository.Interface
{
    public interface IUser
    {
        Task<UserProfileViewHasToken> Login(UserCrendential crendential);
        Task<UserProfileViewDTO> GetUserProfile(int userID);
    }
}
