using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using CMC.Kernel.Core.Configurations;
using CMC.Kernel.Core.Constants;
using CMC.Kernel.Core.Enums;
using CMC.Kernel.Core.Helpers;
using CMC.Kernel.Core.Infrastructure;
using CMC.Kernel.Core.Persistence;
using CMC.Kernel.Core.Services;
using CMC.Kernel.Core.Wrappers;
using CMC.Kernel.Domain.Entities.Identity;
using CMC.Presentation.Application.DTOs.Identity;
using CMC.Presentation.Application.Helpers;
using CMC.Presentation.Application.Services.Settings;
using CMC.Presentation.Domain.Entities.Identity;
using CMC.Presentation.Infrastructure.Persistence.Repositories.OTP;
using System;
using System.Globalization;
using System.Reflection.PortableExecutable;
using System.Threading;
using System.Threading.Tasks;

namespace CMC.Presentation.Application.Services.OTP
{
    public class OTPService : BaseServiceHandler, IOTPService
    {
        private readonly ISettingsService _settingsService;
        private readonly IOTPRepository _oTPRepository;
        //private readonly ISISLService _sISLService;
        private readonly IStringLocalizer<OTPService> _localizer;
        private readonly IApplicationLogger _logger;
        private Configuration _config { set; get; }

        public static IHttpContextAccessor _httpContextAccessor { get { return new HttpContextAccessor(); } }

        public OTPService(IOTPRepository oTPRepository,
            //ISISLService sISLService,
            ISettingsService settingsService,
            IStringLocalizer<OTPService> localizer,
            Configuration config, IApplicationLogger logger,
            IUnitOfWork unitOfWork, IValidatorFactory validatorFactory) : base(validatorFactory, unitOfWork)
        {
            _oTPRepository = oTPRepository;
            //_sISLService = sISLService;
            _settingsService = settingsService;
            _localizer = localizer;
            _config = config;
            _logger = logger;
        }


        //public async Task<Response<object>> GetOTPModel(string mobileNubmer, string type, OTPBack oTPBack, string code = null)
        //{
        //    try
        //    {
        //        var otpModel = await GetOTPModel(mobileNubmer, type, code);
        //        otpModel.Data.BackPage = (int)oTPBack;


        //        return new Response<object>()
        //        {
        //            Data = new { resultCode = LoginRegistrationResults.GoOTP, OTP = otpModel },
        //            Succeeded = true,
        //            StatusCode = (int)LoginRegistrationResults.GoOTP,
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        await _logger.LogError(ex, "GetOTPModel-1", $"mobile:{mobileNubmer}-type:{type}-OTPBack:{oTPBack}-code:{code}", mobileNubmer, false);
        //        return new Response<object>()
        //        {
        //            Succeeded = false,
        //            StatusCode = (int)HttpStatusCode.BadRequest,
        //            Message = _localizer["ErrorOccurred_OTPMsg"].Value
        //        };
        //    }
        //}

        public async Task<Response<object>> ValidateObj(object obj)
        {
            var valid = await ValidateAsync(obj);
            return valid.ConvertToResponseOf<object>(obj);
        }

        public async Task<Response> Test()
        {
            try
            {
                var loginDTO = new LoginDTO()
                {
                    MobileNumber = "XX",
                };

                var validModel = await ValidateObj(loginDTO);
                if (!validModel.Succeeded)
                    return new Response<object>()
                    {
                        
                    };
                return new Response();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<Response<OneTimePasswordDTO>> GetOTPModel(string mobile, string type = "", string code = null)
        {
            try
            {
                var oneTimePasswordVM = await GenerateOneTimePassword(mobile);
                if (oneTimePasswordVM.Succeeded && oneTimePasswordVM.StatusCode == (int)HttpStatusCode.Ok)
                {
                    // Send OTP
                    string lang = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.ToUpper();
                    _httpContextAccessor.HttpContext.Session.SetString("OTP", Security.Encrypt(oneTimePasswordVM.Data.SecurityCode));
                    string oTPCode = !string.IsNullOrEmpty(code) ? code : oneTimePasswordVM.Data.SecurityCode;
                    string msg = String.Format(_localizer["SMSCodeMessage"], oTPCode, Environment.NewLine);
                    if (!string.IsNullOrEmpty(type))
                    {
                        string DateTimeNow = DateTime.Now.ToString("dd-MM-yyyy", new CultureInfo("en-US"));
                        if (type == "login")
                            msg = String.Format(_localizer["MsgLoginOTP"], oneTimePasswordVM.Data.SecurityCode, Environment.NewLine, Environment.NewLine, DateTimeNow);
                        else if (type == "register")
                            msg = String.Format(_localizer["MsgRegisterOTP"], oneTimePasswordVM.Data.SecurityCode, Environment.NewLine, Environment.NewLine, DateTimeNow);
                        else if (type == "UpdateMobile")
                            msg = String.Format(_localizer["MsgUpdateMobileOTP"], oneTimePasswordVM.Data.SecurityCode, Environment.NewLine, Environment.NewLine, DateTimeNow);
                    }

                    var sendOTP = SendOTP(mobile, msg, lang);
                    if (sendOTP.Succeeded && sendOTP.StatusCode == (int)HttpStatusCode.Ok)
                    {
                        // OTP sms sent successfully
                    }
                    else
                    {
                        // bad request from sendOTP, exception happened.
                        oneTimePasswordVM.Succeeded = false;
                        oneTimePasswordVM.StatusCode = sendOTP.StatusCode;
                        oneTimePasswordVM.Message = oneTimePasswordVM.Data.ModalMessage = _localizer["ErrorOccurred_OTPMsg"];
                    }
                }
                else
                {// GenerateOneTimePassword not success. or exception

                    if (oneTimePasswordVM.StatusCode == (int)HttpStatusCode.NotAuthenticated)
                    {
                        // Blocked user
                    }
                    else
                    {
                        // Exception [ BadRequest ]
                    }
                }

                if (!_config.OTPSettings.ShowOTP && oneTimePasswordVM.Data != null)
                    oneTimePasswordVM.Data.SecurityCode = null;
                return oneTimePasswordVM;
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, $"GetOTPMobile2-{type}", mobile, mobile, false);
                throw ex;
            }
        }

        public async Task<Response<OneTimePasswordDTO>> GenerateOneTimePassword(string mobile)
        {
            OneTimePasswordDTO oneTimePasswordDTO = new OneTimePasswordDTO();
            try
            {
                Response<OneTimePasswordDTO> response = new Response<OneTimePasswordDTO>();
                oneTimePasswordDTO.NumberOfDigits = await _settingsService.GetValue<int>(SystemSettings.OTPNumberOfDigits);
                oneTimePasswordDTO.MaxNumberOfTrials = await _settingsService.GetValue<int>(SystemSettings.OTPMaxNumberOfTrials);
                oneTimePasswordDTO.CodeExpiryInMinutes = await _settingsService.GetValue<int>(SystemSettings.OTPCodeExpiryInMinutes);
                oneTimePasswordDTO.NumberOfTrials = 1;
                oneTimePasswordDTO.ElapsedTime = await _settingsService.GetValue<int>(SystemSettings.OTPElapsedTimeInSecond);
                oneTimePasswordDTO.CreatedOn = DateTime.Now;
                oneTimePasswordDTO.MobileNumber = mobile;
                oneTimePasswordDTO.HashedMobileNumber = $"+966 {mobile.Substring(0, 2)}XXXX{mobile.Substring(mobile.Length - 3)}";
                oneTimePasswordDTO.SecurityCode = GenerateCode(oneTimePasswordDTO.NumberOfDigits);

                var resultOtpDto = await AddOrUpdate(oneTimePasswordDTO);
                if (false)//resultOtpDto.isBlocked)
                {
                    TimeSpan span = (resultOtpDto.UnlockedDate.Value - DateTime.Now);
                    int minutes = span.Minutes;
                    int seconds = span.Seconds > 0 ? 1 : 0;
                    minutes += seconds;

                    oneTimePasswordDTO.isBlocked = true;
                    response.Succeeded = false;
                    response.StatusCode = (int)HttpStatusCode.NotAuthenticated;
                    oneTimePasswordDTO.ModalMessage = String.Format(_localizer["YouHaveExceeded"], minutes);
                    response.Message = oneTimePasswordDTO.ModalMessage;
                    response.Data = oneTimePasswordDTO;
                    return response;
                }

                response.Data = oneTimePasswordDTO;
                response.Succeeded = true;
                response.StatusCode = (int)HttpStatusCode.Ok;
                return response;
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "GenerateOTP", oneTimePasswordDTO, mobile, false);
                oneTimePasswordDTO.ModalMessage = _localizer["ErrorOccurred_OTPMsg"];
                return new Response<OneTimePasswordDTO>()
                {
                    Message = _localizer["ErrorOccurred_OTPMsg"],
                    Data = oneTimePasswordDTO,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }

        public async Task<Response> ValidateOTP(string code)
        {
            try
            {
                OneTimePasswordDTO oneTimePasswordDTO = new OneTimePasswordDTO();
                oneTimePasswordDTO.SecurityCode = code;
                oneTimePasswordDTO.MaxNumberOfTrials = await _settingsService.GetValue<int>(SystemSettings.OTPMaxNumberOfTrials);
                oneTimePasswordDTO.CodeExpiryInMinutes = await _settingsService.GetValue<int>(SystemSettings.OTPCodeExpiryInMinutes);
                int blockedInMinutes = await _settingsService.GetValue<int>(SystemSettings.OTPBlockPeriodMinutes);
                oneTimePasswordDTO.NoOfTrials = 1;

                bool isUpdateMobile = !string.IsNullOrEmpty(_httpContextAccessor.HttpContext.Session.GetString("UpdateMobile"));
                if (isUpdateMobile)
                    oneTimePasswordDTO.MobileNumber = Security.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("UpdateMobile"));
                else
                    oneTimePasswordDTO.MobileNumber = Security.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("MobileNumber"));

                bool securityCodeMatch = false;
                bool maxNoOfTrialsExceeded = false;
                bool securityCodeExpired = false;

                var oneTimePasswordDb = await GetByMobileNumber(oneTimePasswordDTO.MobileNumber);
                if (oneTimePasswordDb != null)
                {
                    securityCodeMatch = oneTimePasswordDb.SecurityCode == oneTimePasswordDTO.SecurityCode;
                    maxNoOfTrialsExceeded = oneTimePasswordDb.NoOfTrials == oneTimePasswordDTO.MaxNumberOfTrials;
                    if (oneTimePasswordDb.ModifiedDate.HasValue)
                        securityCodeExpired = oneTimePasswordDb.ModifiedDate.Value.AddMinutes(oneTimePasswordDTO.CodeExpiryInMinutes) <= DateTime.Now;
                    else
                        securityCodeExpired = oneTimePasswordDb.CreatedOn.AddMinutes(oneTimePasswordDTO.CodeExpiryInMinutes) <= DateTime.Now;
                }

                oneTimePasswordDTO.SecurityCodeExpired = false; //securityCodeExpired;
                oneTimePasswordDTO.SecurityCodeMatch = securityCodeMatch;
                oneTimePasswordDTO.MaxNumberOfTrailsExceeded = false;//maxNoOfTrialsExceeded;
                string message = "";
                if (securityCodeExpired)
                    message = _localizer["OTPCodeExpired"];
                else if (maxNoOfTrialsExceeded)
                    message = String.Format(_localizer["BlockedOTP"], blockedInMinutes);
                else if (!securityCodeMatch)
                    message = _localizer["OTPInvalid"];

                return new Response()
                {
                    Succeeded = string.IsNullOrEmpty(message),
                    Message = message
                };
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        private string GenerateCode(int numberOfDigits)
        {
            return new RandomNumberGenerator().Next((int)Math.Pow(10, numberOfDigits - 1), (int)(Math.Pow(10, numberOfDigits) - 1)).ToString();
        }

        public async Task<OneTimePasswordDTO> AddOrUpdate(OneTimePasswordDTO dto)
        {
            try
            {
                var oneTimePasswordDb = await GetByMobileNumber(dto.MobileNumber);
                if (oneTimePasswordDb != null)
                {
                    int maxNumberOfSendSMS = await _settingsService.GetValue<int>(SystemSettings.OTPMaxNumberOfSendSMS);
                    oneTimePasswordDb.ModifiedDate = DateTime.Now;
                    oneTimePasswordDb.SecurityCode = dto.SecurityCode;
                    dto.Id = oneTimePasswordDb.Id;
                    if (oneTimePasswordDb.NoOfGenerations == maxNumberOfSendSMS) // The user blocked
                    {
                        if (oneTimePasswordDb.UnlockedDate < DateTime.Now)
                        {
                            oneTimePasswordDb.NoOfTrials = 1;
                            oneTimePasswordDb.NoOfGenerations = 1;
                            oneTimePasswordDb.UnlockedDate = null;
                        }
                        else
                        {
                            dto.UnlockedDate = oneTimePasswordDb.UnlockedDate;
                            oneTimePasswordDb.isBlocked = true;
                            return oneTimePasswordDb;
                        }
                    }
                    else
                    {
                        oneTimePasswordDb.NoOfTrials = 1;
                        oneTimePasswordDb.NoOfGenerations = oneTimePasswordDb.NoOfGenerations + 1;

                        if (oneTimePasswordDb.NoOfGenerations == maxNumberOfSendSMS)
                        {
                            int blockPeriod = await _settingsService.GetValue<int>(SystemSettings.OTPBlockPeriodMinutes);
                            oneTimePasswordDb.UnlockedDate = DateTime.Now.AddMinutes(blockPeriod);
                        }
                    }
                    dto.NoOfTrials = oneTimePasswordDb.NoOfTrials;
                    dto.NoOfGenerations = oneTimePasswordDb.NoOfGenerations;
                    dto.UnlockedDate = oneTimePasswordDb.UnlockedDate;
                    dto.ModifiedDate = oneTimePasswordDb.ModifiedDate;
                    dto.SecurityCode = oneTimePasswordDb.SecurityCode;
                }

                if (dto.Id != 0)
                {
                    OneTimePassword otp = new OneTimePassword()
                    {
                        Id = dto.Id,
                        UnlockedDate = dto.UnlockedDate,
                        NoOfGenerations = dto.NoOfGenerations,
                        ModifiedDate = DateTime.Now,
                        NoOfTrials = dto.NoOfTrials,
                        SecurityCode = dto.SecurityCode
                    };
                    var updateDb = await _oTPRepository.UpdateOtp(otp);
                    return dto;
                }
                else
                {
                    OneTimePassword otp = new OneTimePassword()
                    {
                        CreatedOn = DateTime.Now,
                        MobileNumber = dto.MobileNumber,
                        NoOfGenerations = dto.NoOfGenerations,
                        NoOfTrials = dto.NoOfTrials,
                        SecurityCode = dto.SecurityCode,
                    };
                    var insertDb = await _oTPRepository.InsertOTP(otp);
                    dto.Id = insertDb.Id;
                    dto.CreatedOn = insertDb.CreatedOn;
                    return dto;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<OneTimePasswordDTO> GetByMobileNumber(string mobileNumber)
        {
            try
            {
                var db = await _oTPRepository.GetByMobileNumber(mobileNumber);
                if (db == null)
                    return null;

                return new OneTimePasswordDTO()
                {
                    CreatedOn = db.CreatedOn,
                    Id = db.Id,
                    MobileNumber = db.MobileNumber,
                    ModifiedDate = db.ModifiedDate,
                    NoOfGenerations = db.NoOfGenerations,
                    NumberOfTrials = db.NoOfTrials,
                    UnlockedDate = db.UnlockedDate,
                    SecurityCode = db.SecurityCode,
                    NoOfTrials = db.NoOfTrials
                };
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public Response SendOTP(string mobile, string message, string lang)
        {
            try
            {
                bool oTPEnabled = _config.OTPSettings.OTPEnabled;
                if (oTPEnabled)
                { // Call SISL
                    try
                    {
                        if (!string.IsNullOrEmpty(_config.OTPSettings.SendSMSMobile))
                            mobile = _config.OTPSettings.SendSMSMobile;

                        var isSend = new Response<OneTimePasswordDTO>(); //_sISLService.SendSMS(message, mobile, lang);
                        if (isSend.Succeeded /*&& isSend.Data.IsSend*/)
                        {
                            // sent successfully.
                            return new Response()
                            {
                                Succeeded = true,
                                StatusCode = (int)HttpStatusCode.Ok,
                                Message = "Success"
                            };
                        }
                        else
                        {
                            // SendSMS not sent successfully. or exception in SISL API
                            return new Response()
                            {
                                StatusCode = isSend.StatusCode,
                                Message = isSend.Message,
                                Succeeded = false
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        //Exception once called SISL
                        oTPEnabled = false;
                        return new Response()
                        {
                            Succeeded = false,
                            Message = ex.Message
                        };
                    }
                }
                else
                {
                    // ! OTPEnabled then return success for demo.
                    return new Response()
                    {
                        Succeeded = true,
                        StatusCode = (int)HttpStatusCode.Ok,
                        Message = "Success"
                    };
                }
            }
            catch (Exception ex)
            {
                return new Response()
                {
                    Succeeded = false,
                    Message = ex.Message
                };
            }
        }


    }
}
