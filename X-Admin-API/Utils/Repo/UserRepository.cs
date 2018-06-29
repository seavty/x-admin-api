using System;
using System.Web;
using X_Admin_API.Models.DB;
using X_Admin_API.Models.DTO.Customer;
using System.Threading.Tasks;
using X_Admin_API.Models.DTO.User;
using System.Net;
using System.Data.Entity;
using X_Admin_API.Models.DTO.Setting;
using X_Admin_API.Models.DTO.Warehouse;

namespace X_Admin_API.Repository.Repo
{
    public class UserRepository
    {
        private THEntities db = null;

        public UserRepository()
        {
            db = new THEntities();
        }

        //-> Login
        public async Task<UserProfileViewHasToken> Login(UserCrendential crendential)
        {
            string password = Helper.Helper.EncryptString(crendential.password);
            var user = await db.sys_user.FirstOrDefaultAsync(r => r.user_Deleted == null && r.userName == crendential.userName && r.password == password);

            if (user == null)
                return null;

            Guid token = Guid.NewGuid();
            user.token = Helper.Helper.EncryptString(token.ToString());
            await db.SaveChangesAsync();

            UserProfileViewHasToken userProfileWithToken = new UserProfileViewHasToken();
            userProfileWithToken.userProfile = await GetUserProfile(user.id);
            userProfileWithToken.token = token.ToString();

            return userProfileWithToken;
        }

        //-> GetUserProfile
        public async Task<UserProfileViewDTO> GetUserProfile(int userID)
        {
            var user = await db.sys_user.FirstOrDefaultAsync(x => x.user_Deleted == null && x.id == userID);
            if (user == null)
                throw new HttpException((int)HttpStatusCode.NotFound, "NotFound");
            UserProfileViewDTO userProfile = Helper.Helper.MapDBClassToDTO<sys_user, UserProfileViewDTO>(user);


            //-setting
            SettingViewDTO settingView = null;
            var setting = await db.sys_setting.FirstOrDefaultAsync(x => x.sett_Deleted == null);
            if (setting != null)
            {
                settingView = Helper.Helper.MapDBClassToDTO<sys_setting, SettingViewDTO>(setting);

                CustomerViewDTO customerView = null;
                var customer = await db.tblCustomers.FirstOrDefaultAsync(x => x.cust_Deleted == null && x.id == setting.customerID);
                if (customer != null)
                    customerView = Helper.Helper.MapDBClassToDTO<tblCustomer, CustomerViewDTO>(customer);

                WarehouseViewDTO warehouseView = null;
                var warehouse = await db.tblWarehouses.FirstOrDefaultAsync(x => x.ware_Deleted == null && x.id == setting.warehouseID);
                if (warehouse != null)
                    warehouseView = Helper.Helper.MapDBClassToDTO<tblWarehouse, WarehouseViewDTO>(warehouse);

                settingView.customer = customerView;
                settingView.warehouse = warehouseView;
            }
            userProfile.setting = settingView;
            // TODO when return token to client should encrypt token
            return userProfile;
        }

    }
}