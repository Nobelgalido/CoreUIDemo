using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CoreUIDemo.Models;
using CoreUIDemo.Models.ViewModels;

namespace CoreUIDemo.Services
{
    public interface IUserService
    {
        List<UserListItemViewModel> GetUsers(int pageNumber, int pageSize);
        UserListItemViewModel GetUserById(long userId);
        void CreateUser(UserCreateViewModel model);
        void UpdateUser(UserEditViewModel model);
        void DeleteUser(long userId);

        AuthenticatedUserData ValidateCredentials(string username, string password);

        void ChangePassword(UserChangePasswordViewModel model);
    }
}