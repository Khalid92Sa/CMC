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
    [CheckSession]
    public class QuestionsController : BaseController
    {
        readonly IQuestionsService _questionsService;
        readonly IApplicationLogger _logger;
        readonly IStringLocalizer<QuestionsController> _localizer;
        readonly ISettingsService _settingService;
        readonly IMapper _mapper;
        public QuestionsController(IQuestionsService questionsService,IApplicationLogger logger, IStringLocalizer<QuestionsController> localizer,ISettingsService settingsService,IMapper mapper)
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
        public async Task<IActionResult> Index()
        {
            var categories = await _questionsService.GetCategories();
            return View(categories);
        }

        /// <summary>
        /// Add Category
        /// </summary>
        /// <returns></returns>
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
                throw ex;
            }
        }

        /// <summary>
        /// Add new category
        /// </summary>
        /// <param name="categoryDTO"></param>
        /// <returns></returns>
        [HttpPost]
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
        public async Task<IActionResult> DeleteCategory(int id, bool withQuestions)
        {
            try
            {
                var result = await _questionsService.DeleteCategory(id, withQuestions);
                return Json(new { isSuccess = result.Succeeded, msg = _localizer["Alert_CategoryDeletedSuccessfully"].Value });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false });
            }
        }

        /// <summary>
        /// Delete Existing Image for category
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> DeleteExistingImg(int id)
        {
            try
            {
                var result = await _questionsService.DeleteExistingImg(id);
                return Json(new { isSuccess = result.Succeeded, msg = result.Succeeded ? _localizer["DeleteExistingImage_SuccessMsg"].Value : _localizer["ErrorOccurred"].Value });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false });
            }
        }

        /// <summary>
        /// Get All Question - Search
        /// </summary>
        /// <param name="searchQuestionDTO"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAllQuestions([FromQuery] SearchQuestionDTO searchQuestionDTO)
        {
            var result = await _questionsService.GetAllQuestions(searchQuestionDTO);
            return Json(result);
        }
        #endregion


        #region Questions
        /// <summary>
        /// Add new Question
        /// </summary>
        /// <returns></returns>
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
                throw ex;
            }
        }

        /// <summary>
        /// Add Question - Post
        /// </summary>
        /// <param name="question"></param>
        /// <returns></returns>
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
                throw ex;
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            try
            {
                var result = await _questionsService.DeleteQuestion(id);
                return Json(new { isSuccess = result.Succeeded, msg = _localizer["QuestionDeletedSuccessfully"].Value });
            }
            catch (Exception ex)
            {
                throw;
            }
        } 
        #endregion
    }
}
