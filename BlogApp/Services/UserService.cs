using BlogApp.Data;
using BlogApp.Models;
using System.Security.Claims;
using System.Text;

namespace BlogApp.Services;

public class UserService
{
    private MyAppDataContext _dataContext;
    public UserService(MyAppDataContext dataContext)
    {
        _dataContext=dataContext;
    }
    
    public UserModel Create(UserModel userModel) 
    {
        User newUser = new User
        {
            Name = userModel.Name,
            Email = userModel.Email,
            Password = userModel.Password,
            Description = userModel.Description,
            Photo = userModel.Photo
        };
        _dataContext.Users.Add(newUser);
        _dataContext.SaveChanges();
        userModel.Id = newUser.Id;
        return userModel;
    }

    public UserModel Update(UserModel userModel)
    {
        User userToUpdate = _dataContext.Users.FirstOrDefault(x => x.Id == userModel.Id);

        if (userToUpdate == null)
            return null;

        User newUser = new User()
        {
            userToUpdate.Name = userModel.Name,
            userToUpdate.Email = userModel.Email,
            userToUpdate.Password = userModel.Password,
            userToUpdate.Description = userModel.Description,
            userToUpdate.Photo = userModel.Photo
        };
        _dataContext.Users.Add(userToUpdate);
        _dataContext.SaveChanges();
        return userModel;
    }

    public void DeleteUser(User user)
    {
        _dataContext.Users.Remove(user);
        _dataContext.SaveChanges();
    }

    public UserModel Update(User userToUpdate, UserModel userModel)
    {
        if (userToUpdate == null)
            return null;

        User newUser = new User()
        {
            userToUpdate.Name = userModel.Name,
            userToUpdate.Email = userModel.Email,
            userToUpdate.Password = userModel.Password,
            userToUpdate.Description = userModel.Description,
            userToUpdate.Photo = userModel.Photo
        };
        _dataContext.Users.Add(userToUpdate);
        _dataContext.SaveChanges();
        return userModel;
    }

    public (string login, string password) GetUserLoginPassFromBasicAuth(HttpRequest request)
    {
        string userName = "";
        string userPass = "";
        string authHeader = request.Headers["Authorization"].ToString();

        if (authHeader != null && authHeader.StartsWith("Basic"))
        {
            string encodedUserNamePass = authHeader.Replace("Basic ", "");
            var encoding = Encoding.GetEncoding("iso-8859-1");
            string[] namePassArray = encoding.GetString(Convert.FromBase64String(encodedUserNamePass)).Split(':');
            userName = namePassArray[0];
            userPass = namePassArray[1];    
        }
        return (userName, userPass);
    }

    public (ClaimsIdentity identity, int id)? GetIdentity(string email, string password)
    {
        User? currentUser = GetUserByLogin(email);

        if (currentUser == null || !VerifyHashPassword(currentUser.Password, password)) return null;

        var claims = new List<Claim>
        {
            new Claim(ClaimsIdentity.DefaultNameClaimType, currentUser.Email),
        };

        ClaimsIdentity claimsIdentity = new ClaimsIdentity(
            claims,
            "Token",
            ClaimsIdentity.DefaultRoleClaimType,
            ClaimsIdentity.DefaultNameClaimType);

        return (claimsIdentity, currentUser.Id);
    }

    public User? GetUserByLogin(object email)
    {
        return _dataContext.Users.FirstOrDefault(x => x.Email == email);
    }

    private bool VerifyHashPassword(string password1, string password2)
    {
        return password1 == password2;
    }
}
