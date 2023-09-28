using AutoMapper;
using CMC.Kernel.Core.Configurations;
using CMC.Kernel.Core.Constants;
using CMC.Kernel.Core.Enums;
using CMC.Kernel.Core.Helpers;
using CMC.Kernel.Core.Infrastructure;
using CMC.Kernel.Core.Persistence;
using CMC.Kernel.Core.Services;
using CMC.Kernel.Core.Wrappers;
using CMC.Kernel.Domain.Entities.Identity;
using CMC.Kernel.Infrastructure.Caching.Model;
using CMC.Kernel.Infrastructure.Persistence.Repositories;
using CMC.Kernel.Infrastructure.Persistence.Services;
using CMC.Presentation.Application.DTOs.Identity;
using CMC.Presentation.Application.Services.Identity.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Protocols;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace CMC.Presentation.Application.Services.Identity.Implementations
{
    public class UserService : BaseServiceHandler, IUserService
    {
        #region Fields
        readonly IMapper _mapper;
        readonly IApplicationLogger _logger;
        readonly IRepository<User> _userRepository;
        readonly IRepository<UserGroup> _userGroupRepository;
        readonly IGroupPermissionService _groupPermissionService;
        readonly IStringLocalizer<UserService> _localizer;
        private Configuration _config { set; get; }

        public static IHttpContextAccessor _httpContextAccessor { get { return new HttpContextAccessor(); } }
        #endregion

        #region Ctor
        public UserService(IRepository<User> userRepository,
            IRepository<UserGroup> userGroupRepository,
            IGroupPermissionService groupPermissionService,
            IApplicationLogger logger,
            IUnitOfWork unitOfWork,
            IValidatorFactory validatorFactory,
            Configuration config,
            IStringLocalizer<UserService> localizer, IMapper mapper) : base(validatorFactory, unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _groupPermissionService = groupPermissionService;
            _userGroupRepository = userGroupRepository;
            _logger = logger;
            _localizer = localizer;
            _mapper = mapper;
            _config = config;
        }
        #endregion

        #region Methods
        public async Task<Response<object>> Validate(object obj)
        {
            var valid = await ValidateAsync(obj);
            return valid.ConvertToResponseOf<object>(obj);
        }


        /// <summary>
        /// Create new user
        /// </summary>
        /// <param name="userDTO"></param>
        /// <returns></returns>
        public async Task<Response<UserDTO>> CreateOrUpdateUser(UserDTO userDTO)
        {
            try
            {
                // validate login model for required fields.
                var validModel = await Validate(userDTO);
                if (!validModel.Succeeded)
                    return new Response<UserDTO>()
                    {
                        BrokenRules = validModel.BrokenRules,
                        StatusCode = (int)HttpStatusCode.BusinessRuleViolation
                    };


                bool isUpdate = userDTO.Id.HasValue;
                //Check if the username or email is already exist
                var encEmail = Security.Encrypt(userDTO.EmailAddress);
                User existUser = null;
                string encUserName = null;
                if (isUpdate)
                {
                    existUser = await _userRepository.GetAll(u => (u.EmailAddress == encEmail) && u.Id != userDTO.Id.Value && u.IsDeleted != true).FirstOrDefaultAsync();
                }
                else
                {
                    encUserName = Security.Encrypt(userDTO.UserName);
                    existUser = await _userRepository.GetAll(u => u.UserName == encUserName || u.EmailAddress == encEmail && u.IsDeleted != true).FirstOrDefaultAsync();
                }

                if (existUser != null)
                {
                    var duplicatedField = !isUpdate ? (existUser.UserName == encUserName ? "Username" : "EmailAddress") : "EmailAddress";
                    List<ValidationRule> validationRule = new List<ValidationRule>() { new ValidationRule() { Message = $"{_localizer[duplicatedField].Value} {_localizer["AlreadyExist"].Value}" } };
                    return new Response<UserDTO>()
                    {
                        BrokenRules = validationRule,
                        StatusCode = (int)HttpStatusCode.BusinessRuleViolation,
                    };
                }


                if (isUpdate)
                {
                    var mappedUpdatedUser = _mapper.Map<User>(userDTO);
                    
                    var updatedUser = await _userRepository.GetAll(a=>a.Id ==  userDTO.Id).FirstOrDefaultAsync();
                    updatedUser.Name = mappedUpdatedUser.Name;
                    updatedUser.PhoneNumber = mappedUpdatedUser.PhoneNumber;
                    updatedUser.EmailAddress = mappedUpdatedUser.EmailAddress;

                    updatedUser.ModifiedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId"));
                    updatedUser.ModifiedOn = DateTime.Now;
                    
                    var groupUser = await _userGroupRepository.GetAll(a=>a.UserId == userDTO.Id).FirstOrDefaultAsync();
                    groupUser.GroupID = userDTO.GroupId.Value;
                    groupUser.ModifiedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId"));
                    groupUser.ModifiedOn = DateTime.Now;

                    _userGroupRepository.Update(groupUser);
                    await _userGroupRepository.UnitOfWork.SaveChangesAsync();

                    _userRepository.Update(updatedUser);
                    await _userRepository.UnitOfWork.SaveChangesAsync();


                    //Check if the Id of updated user, same of user logged in.
                    var loggedInUser = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId"));
                    if (userDTO.Id == loggedInUser)
                    {
                        //Update session infromation
                        var userInfo = await GetUser(loggedInUser);
                        var userInfoDTO = JsonConvert.SerializeObject(userInfo);
                        _httpContextAccessor.HttpContext.Session.SetString("UserInfoDTO", userInfoDTO);
                        _httpContextAccessor.HttpContext.Session.SetString("UserId", userInfo.Data.Id.ToString());
                        _httpContextAccessor.HttpContext.Session.SetString("UserFullName", userInfo.Data.Name);
                    }
                }
                else
                {
                    //Create User.
                    var newUser = _mapper.Map<User>(userDTO);
                    newUser.CreatedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId"));
                    newUser.CreatedOn = DateTime.Now;
                    newUser.IsActive = true;
                    newUser.Password = Security.Hash(SystemSettings.DefaultPassword);
                    newUser.UserGroups = new List<UserGroup>() 
                    { 
                        new UserGroup()
                        {
                            GroupID = userDTO.GroupId.Value,
                            CreatedBy = newUser.CreatedBy,
                            CreatedOn = newUser.CreatedOn
                        }
                    };
                    //AddGroup to user
                    await _userRepository.InsertAsync(newUser);
                    await _userRepository.UnitOfWork.SaveChangesAsync();
                }
               


                return new Response<UserDTO>()
                {
                    Succeeded = true,
                    StatusCode = (int)HttpStatusCode.Ok,
                    Data = userDTO
                };
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "CreateUser", userDTO, userDTO.UserName, false);
                throw ex;
            }
        }

        /// <summary>
        /// Login
        /// </summary>
        /// <param name="loginDTO"></param>
        /// <returns></returns>
        public async Task<Response<UserDTO>> Login(LoginDTO loginDTO)
        {
            try
            {
                // validate login model for required fields.
                var validModel = await Validate(loginDTO);
                if (!validModel.Succeeded)
                    return new Response<UserDTO>()
                    {
                        BrokenRules = validModel.BrokenRules,
                        StatusCode = (int)HttpStatusCode.BusinessRuleViolation
                    };


                string username = Security.Encrypt(loginDTO.UserName);
                var existUser = await _userRepository.GetAll(u => u.UserName == username)
                    .Include(u=>u.UserGroups)
                    .ThenInclude(g=>g.Group)
                    .SingleOrDefaultAsync();

                if (existUser != null)
                {
                    //Validate password
                    bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDTO.Password, existUser.Password);
                    if (isPasswordValid)
                    {
                        if (!existUser.IsActive)
                            return new Response<UserDTO>()
                            {
                                Succeeded = false,
                                StatusCode = (int)HttpStatusCode.BusinessRuleViolation,
                                BrokenRules = new List<ValidationRule>() { new ValidationRule() { PropertyName = "Password", Message = _localizer["UserNameIsInActive"].Value } }
                            };


                        var userDTO = new UserDTO()
                        {
                            Id = existUser.Id,
                            Name = !string.IsNullOrEmpty(existUser.Name) ? Security.Decrypt(existUser.Name) : null,
                            EmailAddress = !string.IsNullOrEmpty(existUser.EmailAddress) ? Security.Decrypt(existUser.EmailAddress) : null,
                            PhoneNumber = !string.IsNullOrEmpty(existUser.PhoneNumber) ? Security.Decrypt(existUser.PhoneNumber) : null
                        };

                        var userGroup = existUser.UserGroups.Select(gr => gr.Group).FirstOrDefault();
                        userDTO.GroupId = userGroup.Id;
                        userDTO.GroupCode = (GroupsEnum)Enum.Parse(typeof(GroupsEnum), userGroup.Code);

                        var permission = await _groupPermissionService.GetPermissionByGroupId(userDTO.GroupId.Value);
                        userDTO.PermissionCodes = permission.Select(a => a.Code).ToList();

                        var userInfoDTO = JsonConvert.SerializeObject(userDTO);

                        _httpContextAccessor.HttpContext.Session.SetString("UserInfoDTO", userInfoDTO);
                        _httpContextAccessor.HttpContext.Session.SetString("UserId", existUser.Id.ToString());
                        _httpContextAccessor.HttpContext.Session.SetString("UserFullName", userDTO.Name);

                        return new Response<UserDTO>()
                        {
                            Succeeded = true,
                            StatusCode = (int)HttpStatusCode.Ok,
                            Data = userDTO
                        };
                    }
                }

                //Invalid
                return new Response<UserDTO>()
                {
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.BusinessRuleViolation,
                    BrokenRules = new List<ValidationRule>() { new ValidationRule() { PropertyName = "Password", Message = _localizer["UsernameOrPasswordInvalid"].Value } }
                };
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "Login", loginDTO, loginDTO.UserName, false);
                throw ex;
            }
        }

        /// <summary>
        /// Get All user with Search
        /// </summary>
        /// <param name="searchUserDTO"></param>
        /// <returns></returns>
        public async Task<PagedResult<UserListDTO>> GetUsers(SearchUserDTO searchUserDTO)
        {
            try
            {
                bool IsAr = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName == "ar";
                PagedResult<UserListDTO> response = new PagedResult<UserListDTO>();


                List<User> users = new List<User>();
                using (SqlConnection connection = new SqlConnection(_config.ConnectionStrings.Default))
                {
                    connection.Open();
                    using (var command = new SqlCommand("SearchUsers", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@SearchName", searchUserDTO.Name);
                        command.Parameters.AddWithValue("@SearchPhone", searchUserDTO.PhoneNumber);
                        command.Parameters.AddWithValue("@GroupId", searchUserDTO.GroupId);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                User user = new User();
                                user.Id = (int)reader["Id"];
                                user.Name = (string)reader["Name"];
                                user.PhoneNumber = (string)reader["PhoneNumber"];
                                user.EmailAddress = (string)reader["EmailAddress"];
                                user.IsActive = Convert.ToBoolean(reader["IsActive"]);
                                user.UserGroups = new List<UserGroup>() { new UserGroup()
                                {
                                     Group = new Group()
                                     {
                                         NameEn = (string)reader["GroupNameEn"],
                                         NameAr = (string)reader["GroupNameAr"],
                                     }
                                }};
                                users.Add(user);
                            }
                        }
                        connection.Close();
                    }
                }

                var result = await Task.Run(() => users.Skip((searchUserDTO.PageNumber - 1) * searchUserDTO.PageSize)
                    .Take(searchUserDTO.PageSize)
                    .ToList());

                response.PageSize = searchUserDTO.PageSize;
                response.CurrentPage = searchUserDTO.PageNumber;
                response.TotalCount = result.Count;

                response.Data = result.Select(x => new UserListDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                    EmailAddress = x.EmailAddress,
                    PhoneNumber = x.PhoneNumber,
                    GroupName = x.UserGroups.Select(ug => IsAr ? ug.Group.NameAr : ug.Group.NameEn).FirstOrDefault(),
                    IsActive = x.IsActive
                });

                return response;
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "GetUsers", null, null, false);
                return new PagedResult<UserListDTO>
                {
                    Message = ex.Message,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }

        /// <summary>
        /// Get user by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Response<UserDTO>> GetUser(int id)
        {
            try
            {
                var user = await _userRepository.GetAll(a => a.Id == id && a.IsDeleted != true)
                    .Include(a=>a.UserGroups)
                    .ThenInclude(a=>a.Group)
                    .SingleOrDefaultAsync();

                if (user == null)
                    return new Response<UserDTO>()
                    {
                        StatusCode = (int)HttpStatusCode.NotFound
                    };
                
                var userDto = _mapper.Map<UserDTO>(user);
                var userGroup = user.UserGroups.Select(gr => gr.Group).FirstOrDefault();
                userDto.GroupId = userGroup.Id;
                userDto.GroupCode = (GroupsEnum)Enum.Parse(typeof(GroupsEnum), userGroup.Code);

                var permission = await _groupPermissionService.GetPermissionByGroupId(userDto.GroupId.Value);
                userDto.PermissionCodes = permission.Select(a => a.Code).ToList();

                return new Response<UserDTO>()
                {
                    Succeeded = true,
                    Data = userDto,
                    StatusCode = (int)HttpStatusCode.Ok
                };
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "GetUserById", null, null, false);
                throw ex;
            }
        }

        public UserDTO GetLoggedInUser()
        {
            try
            {
                if (!string.IsNullOrEmpty(_httpContextAccessor.HttpContext.Session.GetString("UserInfoDTO")))
                {
                    UserDTO userDTO = JsonConvert.DeserializeObject<UserDTO>(_httpContextAccessor.HttpContext.Session.GetString("UserInfoDTO"));
                    return userDTO;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Delete user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Response> DeleteUser(int id)
        {
            try
            {
                var user = await _userRepository.FindAsync(id);
                if (user != null)
                {
                    user.IsDeleted = true;
                    user.DeletedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId"));
                    user.DeletedOn = DateTime.Now;
                    _userRepository.Update(user);
                    await _userRepository.UnitOfWork.SaveChangesAsync();

                    return new Response { StatusCode = (int)HttpStatusCode.Ok, Succeeded = true };
                }
                else
                    return new Response()
                    {
                        StatusCode = (int)HttpStatusCode.NotFound
                    };
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "DeleteUser", null, null, false);
                return new Response()
                {
                    Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message
                };
            }
        }

        /// <summary>
        /// Check permission for user 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="permissions"></param>
        /// <returns></returns>
        public async Task<bool> CheckCurrentUserPermissions(int userId, params string[] permissions)
        {
            try
            {
                var user = await GetUser(userId);
                if (user.Succeeded)
                {
                    if (user.Data.PermissionCodes.Any(permissions.Contains))
                        return true;
                    else
                        return false;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "CheckCurrentUserPermissions", null, null, false);
                return false;
            }
        }

        public async Task<List<LookupModel>> GetHosts()
        {
            try
            {
                var users = await _userRepository.GetAll()
                    .Include(a => a.UserGroups)
                    .ThenInclude(a => a.Group)
                    .Where(a => a.IsDeleted != true && a.UserGroups.Any(g => g.Group.Code == ((int)GroupsEnum.Host).ToString()))
                    .Select(a => new LookupModel()
                    {
                        Id = a.Id,
                        Name = !string.IsNullOrEmpty(a.Name) ? Security.Decrypt(a.Name) : null,
                        NameAr = !string.IsNullOrEmpty(a.Name) ? Security.Decrypt(a.Name) : null,
                        NameEn = !string.IsNullOrEmpty(a.Name) ? Security.Decrypt(a.Name) : null,
                    }).ToListAsync();

                return users;
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "GetHosts", null, null, false);
                throw ex;
            }
        }

        public async Task<Response> UpdateProfile(ProfileDTO profileDTO)
        {
            try
            {
                int loggedInUser = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId"));
                if (profileDTO.UserId != loggedInUser)
                    return new Response()
                    {
                        StatusCode = (int)HttpStatusCode.NotAuthorized
                    };


                // validate login model for required fields.
                var validModel = await Validate(profileDTO);
                if (!validModel.Succeeded)
                    return new Response<ProfileDTO>()
                    {
                        BrokenRules = validModel.BrokenRules,
                        StatusCode = (int)HttpStatusCode.BusinessRuleViolation
                    };


                var existUser = await _userRepository.GetAll(u => u.Id == loggedInUser).SingleOrDefaultAsync();
                //Validate password

                existUser.Name = Security.Encrypt(profileDTO.Name);
                existUser.EmailAddress = Security.Encrypt(profileDTO.EmailAddress);
                existUser.PhoneNumber = Security.Encrypt(profileDTO.PhoneNumber);

                if (!string.IsNullOrEmpty(profileDTO.CurrentPassword) && !string.IsNullOrEmpty(profileDTO.NewPassword))
                {
                    bool isPasswordValid = BCrypt.Net.BCrypt.Verify(profileDTO.CurrentPassword, existUser.Password);
                    if (!isPasswordValid)
                        return new Response()
                        {
                            StatusCode = (int)HttpStatusCode.BusinessRuleViolation,
                            BrokenRules = new List<ValidationRule>() { new ValidationRule() { PropertyName = "CurrentPassword", Message = _localizer["CurrentPasswordIncorrect"].Value } }
                        };

                    existUser.Password = Security.Hash(profileDTO.NewPassword);
                }


                _userRepository.Update(existUser);
                await _userRepository.UnitOfWork.SaveChangesAsync();


                return new Response()
                {
                    Succeeded = true
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Response> ActivateUser(int userId, bool IsActive)
        {
            try
            {
                var user = await _userRepository.FindAsync(userId);
                if (user == null)
                    return new Response() { StatusCode = (int)HttpStatusCode.NotFound };

                user.IsActive = IsActive;
                _userRepository.Update(user);
                await _userRepository.UnitOfWork.SaveChangesAsync();

                return new Response() { Succeeded = true, StatusCode = (int)HttpStatusCode.Ok };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}
