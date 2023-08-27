using BlogApp.Data;
using BlogApp.Models;
using BlogApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BlogApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private UserService _userService;

        public AccountController(MyAppDataContext dataContext)
        {
            _userService= new UserService(dataContext);
        }

        [HttpGet]
        public IActionResult Get()
        {
            string currentUserEmail = HttpContext.User.Identity.Name;
            User currentUser = _userService.GetUserByLogin(currentUserEmail);
            if (currentUser == null)
            {
                return BadRequest();
            }
            return Ok(currentUser);
        }

        [HttpPost]
        public object Create(UserModel user)
        {
            UserModel newUser = _userService.Create(user);
            return Ok(newUser);
        }

        [HttpPatch]
        public object Update(UserModel user)
        {
            // Check current user from request with User model. 
            string currentUserEmail = HttpContext.User.Identity.Name;
            User currentUser = _userService.GetUserByLogin(currentUserEmail);
            if (currentUser != null && currentUser?.Id!= user.Id)
            {
                return BadRequest();
            }
            // Update user in db.
            _userService.Update(currentUser, user);
            return Ok(new UserModel
            {
                Id = currentUser.Id,
                Name = currentUser.Name,
                Email = currentUser.Email,
                Description = currentUser.Description,
                Photo = currentUser.Photo
            });
        }

        [HttpDelete]
        public IActionResult Delete() 
        {
            string currentUserEmail = HttpContext.User.Identity.Name;
            User currentUser = _userService.GetUserByLogin(currentUserEmail);
            _userService.DeleteUser(currentUser);   
            return Ok();
        }

        [HttpPost]
        public IActionResult GetToken()
        {
            // Get user data from db.
            var userData = _userService.GetUserLoginPassFromBasicAuth(Request);

            // Get identity.
            (ClaimsIdentity claims, int id)? identity = _userService.GetIdentity(userData.login, userData.password);
            if (identity == null) return NotFound("Login or password are not correct!");

            // Create jwt-token.
            var now = DateTime.UtcNow;
            var jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                notBefore: now,
                claims: identity?.claims.Claims,
                expires: now.AddMinutes(AuthOptions.LIFETIME),
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            var tokenModel = new AuthToken(
                minutes: AuthOptions.LIFETIME,
                accessToken: encodedJwt,
                username: userData.login,
                userId: identity.Value.id);
            return Ok(tokenModel);
        }
    }
}
