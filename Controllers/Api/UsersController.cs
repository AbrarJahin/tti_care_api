using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StartupProject_Asp.NetCore_PostGRE.Data;
using StartupProject_Asp.NetCore_PostGRE.Data.Models.AppData;
using StartupProject_Asp.NetCore_PostGRE.Data.Models.Identity;
using StartupProject_Asp.NetCore_PostGRE.ViewModels.Api;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace StartupProject_Asp.NetCore_PostGRE.Controllers.Api
{
	[Route("api/[controller]/{action}/{id?}")]
	[ApiController]
	public class UsersController : ControllerBase
	{
		private readonly UserManager<User> _userManager;
		private readonly RoleManager<Role> _roleManager;
		private readonly ApplicationDbContext _context;
		private readonly IConfiguration _configuration;
		private readonly TokenValidationParameters _tokenValidationParameters;

		public UsersController(
			UserManager<User> userManager,
			RoleManager<Role> roleManager,
			ApplicationDbContext context,
			IConfiguration configuration,
			TokenValidationParameters tokenValidationParameters)
		{
			_userManager = userManager;
			_roleManager = roleManager;
			_context = context;
			_configuration = configuration;
			_tokenValidationParameters = tokenValidationParameters;
		}

		//[HttpPost("register")]
		[HttpPost]
		[AllowAnonymous]
		public async Task<IActionResult> Register([FromBody] RegisterViewModel registerVM)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(new { message = "Validation Error", success = false });
			}
			User userExists = await _userManager.FindByEmailAsync(registerVM.Email);
			if (userExists != null)
			{
				return BadRequest(new { message = $"User {registerVM.Email} already exists!", success = false });
			}

			User newUser = new User()
			{
				SecurityStamp = Guid.NewGuid().ToString(),
				FirstName = registerVM.FirstName,
				LastName = registerVM.LastName,
				Email = registerVM.Email,
				PhoneNumber = registerVM.PhoneNumber,
				UserName = registerVM.Email,
				EmailConfirmed = true
			};
			IdentityResult result = await _userManager.CreateAsync(newUser, registerVM.Password);
			if (result.Succeeded)
			{
				//As role is already created, we can only add user
				string roleName = await GetRoleAsync(registerVM.Role);
				IdentityResult addRoleResult = await _userManager.AddToRoleAsync(newUser, roleName);
				if (!addRoleResult.Succeeded)
				{
					return BadRequest(new { message = "User created but role does not assigned", success = false });
				}
				return Ok(new { message = $"User created as {registerVM.Role}", success = true });
			}
			else
			{
				return BadRequest(new { message = result.Errors, success = false });
			}
		}

		private async Task<string> GetRoleAsync(string roleName)
		{
			var roleExist = await _roleManager.RoleExistsAsync(roleName);
			if (!roleExist)
			{
				//create the roles and seed them to the database: Question 1
				IdentityResult roleResult = await _roleManager.CreateAsync(new Role() {
											Name = roleName
				});
				if(roleResult.Succeeded==false)
				{
					throw new CannotUnloadAppDomainException("Role Can't be created");
				}
			}
			return roleName;
		}

		[HttpPost]
		[AllowAnonymous]
		public async Task<IActionResult> LoginAsync([FromBody] LoginViewModel loginVM)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(new { message = "Please provide all required fields", success = false });
			}

			User foundUser = await _userManager.FindByEmailAsync(loginVM.Email);
			if(foundUser!=null && await _userManager.CheckPasswordAsync(foundUser, loginVM.Password))
			{
				//Password is good, so need to add access token in here
				var tokenValue = await GenerateJWTTokenAsync(foundUser);

				return Ok(new { message = "User logged in", success = true, token = tokenValue });
			}
			else
			{
				return Unauthorized(new { message = "User failed loggin in", success = false });
			}
		}

		[HttpPost]
		[AllowAnonymous]
		public async Task<IActionResult> RefreshTokenAsync([FromBody] TokenRequestViewModel tokenRequestVM)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(new { message = "Please provide all required fields", success = false });
			}

			JwtViewModel result = await VerifyAndGenereateTokenAsync(tokenRequestVM);
			return Ok(result);
		}

		private async Task<JwtViewModel> VerifyAndGenereateTokenAsync(TokenRequestViewModel tokenRequestVM)
		{
			JwtSecurityTokenHandler jwtTokenHandler = new JwtSecurityTokenHandler();
			RefreshToken storedToken = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == tokenRequestVM.RefreshToken);
			User dbUser = await _userManager.FindByIdAsync(storedToken.UserId.ToString());
			try
			{
				//Already valid, so don't update it
				ClaimsPrincipal tokenCheckResult = jwtTokenHandler.ValidateToken(
						tokenRequestVM.Token,
						_tokenValidationParameters,
						out SecurityToken securityToken
					);
				return await GenerateJWTTokenAsync(dbUser, storedToken);
			}
			catch (SecurityTokenNoExpirationException)
			{
				if(storedToken.DateExpire>=DateTime.UtcNow)	//Refresh Token still valid
				{
					return await GenerateJWTTokenAsync(dbUser, storedToken);
				}
				else
				{
					return await GenerateJWTTokenAsync(dbUser);
				}
			}
			catch (Exception ex)
			{
				return new JwtViewModel()
				{
					Error = ex.Message
				};
			}
		}

		[HttpGet]
		[Authorize(Roles = "Doctor")]
		//[Authorize]
		public async Task<IActionResult> GetAllUsersAsync(string userType, int startIndex=0, int usersPerPage = 20)
		{
			if (string.IsNullOrEmpty(userType))
			{
				var allUsers = await _context.Users
									.OrderBy(u => u.UserName)
									.Select(u => new
									{
										name = u.FirstName + " " + u.LastName,
										email = u.Email,
										phone = u.PhoneNumber
									})
									.Skip(startIndex)
									.Take(usersPerPage)
									.ToListAsync();
				return Ok(new { allUsers = allUsers });
			}
			else if (userType == "Doctor")
			{
				//IList<User> userList = await _userManager.GetUsersInRoleAsync("Doctor");
				IList<User> userList = await _userManager.GetUsersInRoleAsync("Doctor");
				var doctors = userList.OrderBy(u => u.UserName)
									.Select(u => new
									{
										name = u.FirstName + " " + u.LastName,
										email = u.Email,
										phone = u.PhoneNumber
									})
									.Skip(startIndex)
									.Take(usersPerPage)
									.ToList();
				return Ok(new { doctors = doctors });
			}
			else if (userType == "Patient")
			{
				IList<User> userList = await _userManager.GetUsersInRoleAsync("Patient");
				var patients = userList.OrderBy(u => u.UserName)
									.Select(u => new
									{
										name = u.FirstName + " " + u.LastName,
										email = u.Email,
										phone = u.PhoneNumber
									})
									.Skip(startIndex)
									.Take(usersPerPage)
									.ToList();
				return Ok(new { patients = patients });
			}
			else
			{
				return BadRequest($"Can't return {userType} users");
			}
			throw new ArgumentException($"'{nameof(userType)}' cannot be null or empty.", nameof(userType));
		}

		private async Task<JwtViewModel> GenerateJWTTokenAsync(User user, RefreshToken rToken = null)
		{
			var authClaims = new List<Claim>() {
				new Claim(ClaimTypes.Name, user.UserName),
				new Claim(ClaimTypes.NameIdentifier, user.Email),
				new Claim(JwtRegisteredClaimNames.Email, user.Email),
				new Claim(JwtRegisteredClaimNames.Sub, user.Email),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
			};

			//Add user role claims as well
			IList<string> userRoles = await _userManager.GetRolesAsync(user);
			foreach (string userRole in userRoles)
			{
				authClaims.Add(new Claim(ClaimTypes.Role, userRole));
			}

			var authSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["JWT:Secreat"]));
			var token = new JwtSecurityToken(
					issuer: _configuration["JWT:Issuer"],
					audience: _configuration["JWT:Audience"],
					expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["JWT:AccessTokenTimeoutInMinutes"])),
					claims: authClaims,
					signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
				);
			//int minuites = Convert.ToInt32(_configuration["JWT:AccessTokenTimeoutInMinutes"]);//////
			string jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

			if(rToken!=null)	//Token already there, so don't need to update the token
			{
				JwtViewModel rTokenResponse = new JwtViewModel()
				{
					Token = jwtToken,
					RefreshToken = rToken.Token,
					ExpiresAt = token.ValidTo
				};
				return rTokenResponse;
			}

			//Store a refresh token if possible as well
			RefreshToken refreshToken = await GetOrCreateRefreshToken(token, user);
			JwtViewModel response = new JwtViewModel() {
				Token = jwtToken,
				RefreshToken = refreshToken.Token,
				ExpiresAt = token.ValidTo
			};
			return response;
		}

		private async Task<RefreshToken> GetOrCreateRefreshToken(JwtSecurityToken token, User user)
		{
			//Find Alive refresh token if available, if yes then return that
			RefreshToken searchedRefreshToken = _context.RefreshTokens
				.Where(rt => rt.User == user && rt.isRevoked == false && rt.DateExpire > DateTime.UtcNow)
				.FirstOrDefault();
			if(searchedRefreshToken!=null)
			{
				return searchedRefreshToken;
			}
			RefreshToken refreshToken = new RefreshToken()
			{
				JwtId = token.Id,
				User = user,
				DateExpire = DateTime.UtcNow.AddDays(Convert.ToInt32(_configuration["JWT:RefreshTokenTimeoutInDays"])),
				Token = Guid.NewGuid().ToString() + $"-{DateTime.UtcNow:yyyyMddhmmssffftt}-" + Guid.NewGuid().ToString()
			};
			await _context.RefreshTokens.AddAsync(refreshToken);
			await _context.SaveChangesAsync();
			return refreshToken;
		}
	}
}
