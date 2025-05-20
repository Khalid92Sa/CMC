using Microsoft.EntityFrameworkCore;
using CMC.Kernel.Core.Services;
using CMC.Kernel.Infrastructure.Persistence.Repositories.Settings;
using System;
using System.Threading.Tasks;
using CMC.Presentation.Application.DTOs;
using CMC.Kernel.Core.Wrappers;
using CMC.Kernel.Core.Persistence;
using CMC.Kernel.Core.Infrastructure;
using CMC.Kernel.Domain.Entities;
using CMC.Kernel.Core.Enums;
using CMC.Presentation.Application.DTOs.Questions;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace CMC.Presentation.Application.Services.Settings
{
    public class SettingsService : BaseServiceHandler, ISettingsService
    {
        private readonly ISettingRepository _settingRepository;
        private readonly IRepository<Attachment> _attachmentRepository;
        private readonly IApplicationLogger _logger;
        public static IHttpContextAccessor _httpContextAccessor { get { return new HttpContextAccessor(); } }

        public SettingsService(ISettingRepository settingRepository,IRepository<Attachment> attachmentRepository,IApplicationLogger logger)
        {
            _settingRepository = settingRepository;
            _attachmentRepository = attachmentRepository;
            _logger = logger;
        }

        /// <summary>
        /// This method is to get Setting value by its key then return it converted to the specified type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<T> GetValue<T>(string key)
        {
            try
            {
                var setting = await _settingRepository.GetAll(s => s.Key.ToLower() == key.ToLower()).AsNoTracking().FirstOrDefaultAsync();
                if (setting != null)
                    return (T)Convert.ChangeType(setting.Value,typeof(T));
                else
                    return default(T);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Response> DeleteBackgroundImg()
        {
            try
            {
                var currentAttachment = await _attachmentRepository.GetAll(a => a.EntityId == 1 && a.EntityType == (int)AttachmentTypes.BackgroundImg && a.IsDeleted != true).SingleOrDefaultAsync();
                if (currentAttachment != null)
                {

                }
                return new Response() { Succeeded = true, StatusCode = (int)HttpStatusCode.Ok };
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "Setting-DeleteBackgroundImg", null, null, false);
                return new Response()
                {
                    Message = ex.Message,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }

        public async Task<Response> UpdateSystemSettings(SettingDTO settingDTO)
        {
            try
            {
                if (settingDTO.BackgroundImg != null)
                {
                    var currentAttachment = await _attachmentRepository.GetAll(a => a.EntityId == 1 && a.EntityType == (int)AttachmentTypes.BackgroundImg && a.IsDeleted != true).SingleOrDefaultAsync();
                    bool IsUpdateAttachment = currentAttachment != null;
                    if (currentAttachment == null)
                        currentAttachment = new Attachment() { CreatedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId")), CreatedOn = DateTime.Now };
                    else
                    {
                        currentAttachment.ModifiedOn = DateTime.Now;
                        currentAttachment.ModifiedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId"));
                    }

                    using (var memoryStream = new MemoryStream())
                    {
                        await settingDTO.BackgroundImg.CopyToAsync(memoryStream);
                        currentAttachment.FileName = settingDTO.BackgroundImg.FileName;
                        currentAttachment.FileData = memoryStream.ToArray();
                        currentAttachment.EntityId = 1;
                        currentAttachment.EntityType = (int)AttachmentTypes.BackgroundImg;

                        if (IsUpdateAttachment)
                            _attachmentRepository.Update(currentAttachment);
                        else
                            await _attachmentRepository.InsertAsync(currentAttachment);
                        await _attachmentRepository.UnitOfWork.SaveChangesAsync();
                    }
                }
                return new Response() { Succeeded = true, StatusCode = (int)HttpStatusCode.Ok };
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "Setting-UpdateSystemSettings", settingDTO, null, false);
                return new Response()
                {
                    Message = ex.Message,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }
    }
}
