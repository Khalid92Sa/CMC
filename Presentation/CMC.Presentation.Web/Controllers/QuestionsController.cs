using AutoMapper;
using CMC.Kernel.Core.Constants;
using CMC.Kernel.Core.Controllers;
using CMC.Kernel.Core.Enums;
using CMC.Kernel.Core.Infrastructure;
using CMC.Kernel.Infrastructure.Caching.Model;
using CMC.Presentation.Application.ActionFilters;
using CMC.Presentation.Application.DTOs.Questions;
using CMC.Presentation.Application.Services.Questions;
using CMC.Presentation.Application.Services.Settings;
using CMC.Presentation.Domain.Entities;
using iTextSharp.text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CMC.Presentation.Web.Controllers
{
    public class QuestionsController : BaseController
    {
        readonly IQuestionsService _questionsService;
        readonly IApplicationLogger _logger;
        readonly IStringLocalizer<QuestionsController> _localizer;
        readonly ISettingsService _settingService;
        readonly IMapper _mapper;
        public QuestionsController(IQuestionsService questionsService,IApplicationLogger logger, IStringLocalizer<QuestionsController> localizer,
            ISettingsService settingsService,IMapper mapper)
        {
            _questionsService = questionsService;
            _logger = logger;
            _mapper = mapper;
            _localizer = localizer;
            _settingService = settingsService;
        }

        #region Categories
        /// <summary>
        /// Show all categories
        /// </summary>
        /// <returns></returns>
        [RolePermission(PermissionCodes.WebQuestionsView)]
        public async Task<IActionResult> Index()
        {
            try
            {
                var categories = await _questionsService.GetCategories();
                return View(categories);
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "Index_Questions", null, null, false);
                return RedirectToAction("Index", "Error");
            }
            
        }

        /// <summary>
        /// Add Category
        /// </summary>
        /// <returns></returns>
        [RolePermission(PermissionCodes.WebQuestionsCreate)]

        public async Task<IActionResult> AddCategory(int? id)
        {
            try
            {
                CategoryDTO categoryDTO = new CategoryDTO();
                if (id.HasValue)
                {
                    var category = await _questionsService.GetCategory(id.Value);
                    if (category.Succeeded)
                        categoryDTO = category.Data;
                    else if (category.StatusCode == (int)HttpStatusCode.NotFound)
                        return RedirectToAction("Index");
                }
                return View(categoryDTO);
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "AddCategory", id, null, false);
                return RedirectToAction("Index", "Error");
            }
        }

        /// <summary>
        /// Add new category
        /// </summary>
        /// <param name="categoryDTO"></param>
        /// <returns></returns>
        [HttpPost]
        [RolePermission(PermissionCodes.WebQuestionsCreate)]
        public async Task<IActionResult> AddCategory(CategoryDTO categoryDTO)
        {
            try
            {
                var result = await _questionsService.AddOrUpdateCategory(categoryDTO);
                string msg = categoryDTO.Id.HasValue ? _localizer["CategoryUpdatedSuccessfully"].Value : _localizer["CategorySavedSuccessfully"].Value;
                return Json(new { resultCode = result.StatusCode, brokenRoles = result.BrokenRules, category = result.Message, msg = msg });
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "AddCategory", categoryDTO, null, false);
                return Json(new { resultCode = (int)HttpStatusCode.BadRequest });
            }
        }

        /// <summary>
        /// Delete Category
        /// </summary>
        /// <param name="id"></param>
        /// <param name="withQuestions"></param>
        /// <returns></returns>
        [HttpPost]
        [RolePermission(PermissionCodes.WebQuestionsDelete)]
        public async Task<IActionResult> DeleteCategory(int id, bool withQuestions)
        {
            try
            {
                var result = await _questionsService.DeleteCategory(id, withQuestions);
                return Json(new { isSuccess = result.Succeeded, msg = _localizer["Alert_CategoryDeletedSuccessfully"].Value });
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "DeleteCategory", $"Id:{id} - WithQuestions:{withQuestions}", null, false);
                return Json(new { isSuccess = false });
            }
        }

        /// <summary>
        /// Delete Existing Image for category
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [RolePermission(PermissionCodes.WebQuestionsCreate)]
        public async Task<IActionResult> DeleteExistingImg(int id,int type)
        {
            try
            {
                AttachmentTypes attachmentTypes = (AttachmentTypes)type;
                var result = await _questionsService.DeleteExistingImg(id, attachmentTypes);
                return Json(new { isSuccess = result.Succeeded, msg = result.Succeeded ? _localizer["DeleteExistingImage_SuccessMsg"].Value : _localizer["ErrorOccurred"].Value });
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "DeleteExistingImg", $"Id:{id} - type:{type}", null, false);
                return Json(new { isSuccess = false });
            }
        }

        /// <summary>
        /// Get All Question - Search
        /// </summary>
        /// <param name="searchQuestionDTO"></param>
        /// <returns></returns>
        [HttpGet]
        [RolePermission(PermissionCodes.WebQuestionsView)]
        public async Task<IActionResult> GetAllQuestions([FromQuery] SearchQuestionDTO searchQuestionDTO)
        {
            try
            {
                var result = await _questionsService.GetAllQuestions(searchQuestionDTO);
                return Json(result);
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "GetAllQuestions", searchQuestionDTO, null, false);
                return Json(new { });
            }
           
        }


        [HttpGet]
        [RolePermission(PermissionCodes.WebQuestionsView)]
        public async Task<IActionResult> GetLastQuestions()
        {
            try
            {
                var result = await _questionsService.GetLastQuestions();
                return Json(result);
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "GetLastQuestions", null, null, false);
                return Json(new { });
            }

        }
        #endregion


        #region Questions
        /// <summary>
        /// Add new Question
        /// </summary>
        /// <returns></returns>
        [RolePermission(PermissionCodes.WebQuestionsCreate)]
        public async Task<IActionResult> AddQuestion(int? id)
        {
            try
            {
                QuestionVM questionVM = new QuestionVM();
                if (id.HasValue)
                {
                    var question = await _questionsService.GetQuestion(id.Value);
                    if (question.Succeeded)
                        questionVM = question.Data;
                    else
                    {
                        questionVM.Points = await _settingService.GetValue<int>(SystemSettings.QuestionDefaultPoint);
                        questionVM.Time = await _settingService.GetValue<int>(SystemSettings.QuestionDefaultTime);
                    }
                }
                else
                {
                    questionVM.Points = await _settingService.GetValue<int>(SystemSettings.QuestionDefaultPoint);
                    questionVM.Time = await _settingService.GetValue<int>(SystemSettings.QuestionDefaultTime);
                }
                questionVM.Categories = await _questionsService.GetCategories();
                
                return View(questionVM);
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "GetAllQuestions", id, null, false);
                return RedirectToAction("Index", "Error");
            }
        }

        /// <summary>
        /// Add Question - Post
        /// </summary>
        /// <param name="question"></param>
        /// <returns></returns>
        [RolePermission(PermissionCodes.WebQuestionsCreate)]
        [HttpPost]
        public async Task<IActionResult> AddQuestion(QuestionVM question)
        {
            try
            {
                var result = await _questionsService.AddQuestions(question);
                string message = question.Id.HasValue ? _localizer["QuestionInformationSavedSuccessfully"].Value : _localizer["QuestionAddedSuccessfully"].Value;
                return Json(new { isSuccess = result.Succeeded, resultCode = result.StatusCode, brokenRoles = result.BrokenRules, msg = message, category = question.CategoryId });
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "AddQuestion", question, null, false);
                return Json(new { isSuccess = false });
            }
        }

        [HttpDelete]
        [RolePermission(PermissionCodes.WebQuestionsDelete)]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            try
            {
                var result = await _questionsService.DeleteQuestion(id);
                return Json(new { isSuccess = result.Succeeded, msg = _localizer["QuestionDeletedSuccessfully"].Value });
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "DeleteQuestion", id, null, false);
                return Json(new { isSuccess = false });
            }
        } 
        #endregion
    }
}
