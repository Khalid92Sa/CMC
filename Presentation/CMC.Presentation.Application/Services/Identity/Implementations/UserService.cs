using AutoMapper;
using CMC.Kernel.Core.Constants;
using CMC.Kernel.Core.Enums;
using CMC.Kernel.Core.Helpers;
using CMC.Kernel.Core.Infrastructure;
using CMC.Kernel.Core.Persistence;
using CMC.Kernel.Core.Services;
using CMC.Kernel.Core.Wrappers;
using CMC.Kernel.Domain.Entities.Identity;
using CMC.Presentation.Application.DTOs.Identity;
using CMC.Presentation.Application.Services.Identity.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Generators;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CMC.Presentation.Application.Services.Identity.Implementations
{
    public class UserService : BaseServiceHandler, IUserService
    {
        #region Fields
        readonly IMapper _mapper;
        readonly IApplicationLogger _logger;
        readonly IRepository<User> _userRepository;
        readonly IRepository<UserRole> _userRoleRepository;
        readonly IStringLocalizer<UserService> _localizer; 
        public static IHttpContextAccessor _httpContextAccessor { get { return new HttpContextAccessor(); } }
        #endregion

        #region Ctor
        public UserService(IRepository<User> userRepository,
            IRepository<UserRole> userRoleRepository,
            IApplicationLogger logger,
            IUnitOfWork unitOfWork,
            IValidatorFactory validatorFactory,
            IStringLocalizer<UserService> localizer, IMapper mapper) : base(validatorFactory, unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
            _logger = logger;
            _localizer = localizer;
            _mapper = mapper;
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
        public async Task<Response<UserDTO>> CreateUser(UserDTO userDTO)
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


                //Check if the username or email is already exist
                var encUserName = Security.Encrypt(userDTO.UserName);
                var encEmail = Security.Encrypt(userDTO.EmailAddress);
                var existUser = await _userRepository.GetAll(u => u.UserName == encUserName || u.EmailAddress == encEmail).FirstOrDefaultAsync();
                if (existUser != null)
                {
                    var duplicatedField = existUser.UserName == encUserName ? "Username" : "EmailAddress";
                    List<ValidationRule> validationRule = new List<ValidationRule>() { new ValidationRule() { Message = $"{_localizer[duplicatedField].Value} {_localizer["AlreadyExist"].Value}" } };
                    return new Response<UserDTO>()
                    {
                        BrokenRules = validationRule,
                        StatusCode = (int)HttpStatusCode.BusinessRuleViolation,
                    };
                }


                //Create User, then his roles.
                var newUser = _mapper.Map<User>(userDTO);
                newUser.IsActive = true;
                newUser.Password = Security.Hash(SystemSettings.DefaultPassword);
                await _userRepository.InsertAsync(newUser);
                await _userRepository.UnitOfWork.SaveChangesAsync();

                List<UserRole> userRoles = new List<UserRole>();
                //Add the roles for the user.
                userDTO.Roles.ForEach(role =>
                {
                    userRoles.Add(new UserRole() { UserId = newUser.Id, RoleId = role.Id });
                });
                await _userRoleRepository.InsertAsync(userRoles);
                await _userRoleRepository.UnitOfWork.SaveChangesAsync();


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
                var existUser = await _userRepository.GetAll(u => u.UserName == username).SingleOrDefaultAsync();
                if (existUser != null)
                {
                    //Validate password
                    bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDTO.Password, existUser.Password);
                    if (isPasswordValid)
                    {
                        var userDTO = new UserDTO()
                        {
                            Id = existUser.Id,
                            Name = existUser.Name,
                            EmailAddress = !string.IsNullOrEmpty(existUser.EmailAddress) ? Security.Decrypt(existUser.EmailAddress) : null,
                            PhoneNumber = !string.IsNullOrEmpty(existUser.PhoneNumber) ? Security.Decrypt(existUser.PhoneNumber) : null
                        };
                        _httpContextAccessor.HttpContext.Session.SetString("UserId", existUser.Id.ToString());
                        _httpContextAccessor.HttpContext.Session.SetString("UserFullName", existUser.Name);

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
                    StatusCode = (int)HttpStatusCode.NotFound,
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
        /// Logout
        /// </summary>
        /// <returns></returns> 
        public Task<Response> Logout()
        {
            throw new NotImplementedException();
        } 
        #endregion
    }
}
