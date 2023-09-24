using AutoMapper;
using CMC.Kernel.Core.Constants;
using CMC.Kernel.Core.Enums;
using CMC.Kernel.Core.Helpers;
using CMC.Kernel.Core.Infrastructure;
using CMC.Kernel.Core.Persistence;
using CMC.Kernel.Core.Services;
using CMC.Kernel.Core.Wrappers;
using CMC.Kernel.Domain.Entities;
using CMC.Kernel.Infrastructure.Caching.Model;
using CMC.Kernel.Infrastructure.Persistence.Repositories.Lookups;
using CMC.Presentation.Application.DTOs.Questions;
using CMC.Presentation.Application.Helpers;
using CMC.Presentation.Application.Services.Settings;
using CMC.Presentation.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Diagnostics.Tracing.Parsers.AspNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CMC.Presentation.Application.Services.Questions
{
    public class QuestionsService : BaseServiceHandler, IQuestionsService
    {
        #region Fields
        readonly IMapper _mapper;
        readonly IApplicationLogger _logger;
        readonly IWebHostEnvironment _env;
        readonly ILookupRepository _lookupRepository;
        readonly ILookupCategoryRepository _lookupCategoryRepository;
        readonly IRepository<Question> _questionRepository;
        readonly IRepository<Answer> _answerRepository;
        readonly IStringLocalizer<QuestionsService> _localizer;
        readonly ISettingsService _settingsService;
        public static IHttpContextAccessor _httpContextAccessor { get { return new HttpContextAccessor(); } }
        #endregion

        #region Ctor
        public QuestionsService(IMapper mapper,
            IApplicationLogger applicationLogger,
            ILookupRepository lookupRepository,
            IRepository<Question> questionRepository,
            IRepository<Answer> answerRepository,
            ILookupCategoryRepository lookupCategoryRepository,
            IUnitOfWork unitOfWork,
            IValidatorFactory validatorFactory,
            IWebHostEnvironment env,
            ISettingsService settingsService,
            IStringLocalizer<QuestionsService> localizer) : base(validatorFactory, unitOfWork)
        {
            _mapper = mapper;
            _env = env;
            _logger = applicationLogger;
            _lookupRepository = lookupRepository;
            _lookupCategoryRepository = lookupCategoryRepository;
            _questionRepository = questionRepository;
            _answerRepository = answerRepository;
            _unitOfWork = unitOfWork;
            _localizer = localizer;
            _settingsService = settingsService;
        }
        #endregion


        public async Task<Response<object>> Validate(object obj)
        {
            var valid = await ValidateAsync(obj);
            return valid.ConvertToResponseOf<object>(obj);
        }

        /// <summary>
        /// Get all categories of questions
        /// </summary>
        /// <returns></returns>
        public async Task<List<LookupModel>> GetCategories()
        {
            try
            {
                List<LookupModel> categories = new List<LookupModel>();
                categories = await _lookupRepository.GetLookupItems(LookupTypes.QuestionsCategory);
                if (categories.Count > 0)
                {
                    categories.ForEach(category =>
                    {
                        category.Sort = _questionRepository.GetAll(a => a.CategoryID == category.Id && a.IsDeleted != true).Count();
                    });
                }
                return categories;
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "GetQuestionsCategories", null, null, false);
                throw ex;
            }
        }

        /// <summary>
        /// Add Or update question category
        /// </summary>
        /// <param name="categoriesVM"></param>
        /// <returns></returns>
        public async Task<Response> AddOrUpdateCategory(CategoryDTO categoryDTO)
        {
            string imageToDeleteIfExist = "";
            try
            {
                // validate login model for required fields.
                var validModel = await Validate(categoryDTO);
                if (!validModel.Succeeded)
                    return new Response<CategoryDTO>()
                    {
                        BrokenRules = validModel.BrokenRules,
                        StatusCode = (int)HttpStatusCode.BusinessRuleViolation
                    };

                //Resize image in case image too large
                string filePath = null;
                if (categoryDTO.Img != null && categoryDTO.Img.Length > 0)
                {
                    try
                    {
                        using (var stream = categoryDTO.Img.OpenReadStream())
                        {
                            var imageName = Guid.NewGuid().ToString() + Path.GetExtension(categoryDTO.Img.FileName);
                            var imagePath = Path.Combine(_env.WebRootPath, "assets", "images", "categories");
                            filePath = Path.Combine(imagePath, imageName);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                stream.CopyTo(fileStream);
                            }
                            imageToDeleteIfExist = filePath = $"\\{filePath.Substring(filePath.IndexOf("assets"))}";
                        }
                    }
                    catch (Exception ex)
                    {
                        categoryDTO.Img = null;
                        await _logger.LogError(ex, "AddOrUpdateCategory", categoryDTO, null, false);
                        filePath = await _settingsService.GetValue<string>(SystemSettings.DefaultCategoryImgPath);
                    }
                }
                else
                    filePath = await _settingsService.GetValue<string>(SystemSettings.DefaultCategoryImgPath);

                Lookup category = new Lookup();
                if (categoryDTO.Id.HasValue)
                {
                    //Update Catgegory
                    var lookup = await _lookupRepository.GetLookupById(categoryDTO.Id.Value);
                    if (lookup != null)
                    {
                        category.Id = lookup.Id;
                        category.NameEn = categoryDTO.NameEn;
                        category.NameAr = categoryDTO.NameAr;
                        if (!string.IsNullOrEmpty(filePath))
                        {
                            //Means user update new image
                            if (!string.IsNullOrEmpty(lookup.Img) && !lookup.Img.Contains("default"))
                            {
                                //Delete the existing image
                                var imagePathToDelete = Path.Combine(_env.WebRootPath,"assets","images","categories", Path.GetFileName(lookup.Img));
                                if (System.IO.File.Exists(imagePathToDelete))
                                    System.IO.File.Delete(imagePathToDelete);
                            }

                            filePath = $"\\{filePath.Substring(filePath.IndexOf("assets"))}";
                            imageToDeleteIfExist = filePath;
                        }
                        category.Img = imageToDeleteIfExist = filePath;
                        category.CategoryID = lookup.CategoryId;
                        category.ModifiedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId"));
                        category.ModifiedOn = DateTime.Now;
                        _lookupRepository.Update(category);
                        await _lookupRepository.UnitOfWork.SaveChangesAsync();
                    }
                }
                else
                {
                    // Create new Category
                    var questionsCategoryLookup = await _lookupCategoryRepository.GetCategoryId(LookupTypes.QuestionsCategory);

                    imageToDeleteIfExist = filePath = $"\\{filePath.Substring(filePath.IndexOf("assets"))}";

                    category = new Lookup()
                    {
                        CategoryID = questionsCategoryLookup,
                        Img = filePath,
                        NameEn = categoryDTO.NameEn,
                        NameAr = categoryDTO.NameAr,
                        CreatedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId")),
                        CreatedOn = DateTime.Now,
                        IsDeleted = false,
                    };
                    await _lookupRepository.InsertAsync(category);
                    await _lookupRepository.UnitOfWork.SaveChangesAsync();
                }

               
                return new Response()
                {
                    Succeeded = true,
                    Message = category.Id.ToString(),
                    StatusCode = (int)HttpStatusCode.Ok
                };
            }
            catch (Exception ex)
            {
                var imagePathToDelete = Path.Combine(_env.WebRootPath, "assets", "images", "categories", Path.GetFileName(imageToDeleteIfExist));
                if (System.IO.File.Exists(imagePathToDelete) && !imagePathToDelete.Contains("default"))
                    System.IO.File.Delete(imagePathToDelete);

                await _logger.LogError(ex, "AddOrUpdateQuestionCategory", categoryDTO, null, false);
                return new Response()
                {
                    Message = ex.Message,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }

        /// <summary>
        /// Delete Category
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Response> DeleteCategory(int id, bool withQuestions)
        {
            try
            {
                var category = await _lookupRepository.GetAll(a => a.Id == id).SingleOrDefaultAsync();
                if (category == null)
                    return new Response()
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                    };

                var questionsPerCategory = await _questionRepository.GetAll(a => a.CategoryID == id).ToListAsync();
                if (questionsPerCategory.Count > 0)
                {
                    if (withQuestions)
                    {
                        questionsPerCategory.ForEach(question =>
                        {
                            question.IsDeleted = true;
                            question.DeletedOn = DateTime.Now;
                            question.DeletedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId"));
                        });
                        _questionRepository.Update(questionsPerCategory);
                    }
                    else
                    {
                        questionsPerCategory.ForEach(question =>
                        {
                            question.CategoryID = null;
                        });
                        _questionRepository.Update(questionsPerCategory);
                    }
                }

                category.IsDeleted = true;
                category.DeletedOn = DateTime.Now;
                category.DeletedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId"));
                _lookupRepository.Update(category);

                await _questionRepository.UnitOfWork.SaveChangesAsync();
                await _lookupRepository.UnitOfWork.SaveChangesAsync();
                
                return new Response() { Succeeded = true };
            }
            catch (Exception ex)
            {
                return new Response()
                {
                    Message = ex.Message,
                };
            }
        }

        /// <summary>
        /// Delete Existing image for category
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Response> DeleteExistingImg(int id)
        {
            try
            {
                var category = await _lookupRepository.GetAll(a => a.Id == id).SingleOrDefaultAsync();
                if (category == null)
                    return new Response()
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                    };

                if (!string.IsNullOrEmpty(category.Img) && !category.Img.Contains("default"))
                {
                    //Delete the existing image
                    var imagePathToDelete = Path.Combine(_env.WebRootPath, "assets", "images", "categories", Path.GetFileName(category.Img));
                    if (System.IO.File.Exists(imagePathToDelete))
                        System.IO.File.Delete(imagePathToDelete);
                }

                category.Img = await _settingsService.GetValue<string>(SystemSettings.DefaultCategoryImgPath);
                category.ModifiedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId"));
                category.ModifiedOn = DateTime.Now;
                _lookupRepository.Update(category);
                await _lookupRepository.UnitOfWork.SaveChangesAsync();
                return new Response() { Succeeded = true };
            }
            catch (Exception ex)
            {
                return new Response()
                {
                    Message = ex.Message,
                };
            }
        }

        /// <summary>
        /// Get Category by Id with its questions
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Response<CategoryDTO>> GetCategory(int id)
        {
            try
            {
                CategoryDTO categoryDTO = new CategoryDTO();
                var category = await _lookupRepository.GetLookupById(id);
                if (category != null)
                {
                    categoryDTO.Id = category.Id;
                    categoryDTO.NameEn = category.NameEn;
                    categoryDTO.NameAr = category.NameAr;
                    categoryDTO.ImgPath = category.Img;
                    categoryDTO.Questions = _questionRepository.GetAll(a => a.CategoryID == category.Id && a.IsDeleted != true).Include(a => a.Answers).Select(a => new QuestionVM()
                    {
                        Id = a.Id,
                        CategoryId = a.CategoryID,
                        Points = a.Points,
                        Time = a.Timer,
                        TextEn = a.TextEn,
                        TextAr = a.TextAr,
                        Answers = a.Answers.Select(ansewr => new AnswerOptions()
                        {
                            Id = ansewr.Id,
                            TextAr = ansewr.TextAr,
                            TextEn = ansewr.TextEn,
                            IsAnswer = ansewr.IsAnswer
                        }).ToList()
                    }).ToList();
                    return new Response<CategoryDTO>()
                    {
                        Succeeded = true,
                        Data = categoryDTO,
                        StatusCode = (int)HttpStatusCode.Ok
                    };
                }
                else
                {
                    return new Response<CategoryDTO>()
                    {
                        Succeeded = false,
                        StatusCode = (int)HttpStatusCode.NotFound
                    };
                }
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get All questions with option search by category
        /// </summary>
        /// <param name="searchQuestionDTO"></param>
        /// <returns></returns>
        public async Task<PagedResult<QuestionListVM>> GetAllQuestions(SearchQuestionDTO searchQuestionDTO)
        {
            try
            {
                bool IsAr = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName == "ar";

                PagedResult<QuestionListVM> response = new PagedResult<QuestionListVM>();
                var questions = _questionRepository.GetAll(a=>a.CategoryID == searchQuestionDTO.CategoryId && a.IsDeleted != true).AsQueryable();

                var result = questions
                        .WhereIf(!string.IsNullOrEmpty(searchQuestionDTO.QuestionText) && IsAr, a=>a.TextAr.Contains(searchQuestionDTO.QuestionText))
                        .WhereIf(!string.IsNullOrEmpty(searchQuestionDTO.QuestionText) && !IsAr, a => a.TextEn.Contains(searchQuestionDTO.QuestionText))
                        .OrderByDescending(a => a.CreatedOn)
                        .ToQueryResultAsync(searchQuestionDTO.PageNumber, searchQuestionDTO.PageSize);

                response.PageSize = result.Result.PageSize;
                response.CurrentPage = result.Result.CurrentPage;
                response.TotalCount = result.Result.TotalCount;
                response.BrokenRules = result.Result.BrokenRules;
                response.Data = result.Result.Data.Select(x => new QuestionListVM
                {
                    Id = x.Id,
                    Text = IsAr ? x.TextAr : x.TextEn,
                    Points = x.Points,
                    Time = x.Timer
                });

                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // <summary>
        /// Add questions for category
        /// </summary>
        /// <param name="questions"></param>
        /// <returns></returns>
        public async Task<Response> AddQuestions(QuestionVM questionVM)
        {
            string imageToDeleteIfExist = "";
            try
            {
                bool IsAr = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName == "ar";

                // validate login model for required fields.
                var validModel = await Validate(questionVM);
                if (!validModel.Succeeded)
                    return new Response<QuestionVM>()
                    {
                        BrokenRules = validModel.BrokenRules,
                        StatusCode = (int)HttpStatusCode.BusinessRuleViolation
                    };


                Question question = new Question();
                if (questionVM.Id.HasValue)
                {
                    question = await _questionRepository.GetAll(a => a.Id == questionVM.Id.Value && a.IsDeleted != true).Include(a=>a.Answers).FirstOrDefaultAsync();
                    if (question != null)
                    {
                        //Update
                        question.ModifiedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId"));
                        question.ModifiedOn = DateTime.Now;
                    }
                }

                question.CategoryID = questionVM.CategoryId;
                question.TextEn = questionVM.TextEn;
                question.TextAr = questionVM.TextAr;
                question.Timer = questionVM.Time;
                question.Points = questionVM.Points;
                if(questionVM.Id.HasValue)
                {
                    //Check Answers
                    List<Answer> answersDb = new List<Answer>();
                    foreach (var answer in questionVM.Answers)
                    {
                        if (answer.Id.HasValue)
                        {
                            var answerDb = question.Answers.Where(a=>a.Id == answer.Id).FirstOrDefault();
                            if(string.IsNullOrWhiteSpace(answer.TextEn) && string.IsNullOrWhiteSpace(answer.TextAr))
                            {
                                // User delete the option
                                answerDb.IsDeleted = true;
                                answerDb.DeletedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId"));
                                answerDb.DeletedOn = DateTime.Now;
                            }
                            else
                            {
                                //Update
                                answerDb.IsAnswer = answer.IsAnswer;
                                answerDb.TextEn = answer.TextEn;
                                answerDb.TextAr = answer.TextAr;
                                answerDb.ModifiedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId"));
                                answerDb.ModifiedOn = DateTime.Now;
                            }
                            _answerRepository.Update(answerDb);
                        }
                        else
                        {
                            // New option
                            answersDb.Add(new Answer()
                            {
                                QuestionId = question.Id,
                                IsAnswer = answer.IsAnswer,
                                CreatedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId")),
                                CreatedOn = DateTime.Now,
                                TextAr = answer.TextAr,
                                TextEn = answer.TextEn
                            });
                        }
                    }

                    if (answersDb.Count > 0)
                        await _answerRepository.InsertAsync(answersDb);
                    
                    _questionRepository.Update(question);
                    await _answerRepository.UnitOfWork.SaveChangesAsync();
                    await _questionRepository.UnitOfWork.SaveChangesAsync();
                }
                else
                {
                    //Insert
                    question.CreatedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId"));
                    question.CreatedOn = DateTime.Now;
                    List<Answer> answers = new List<Answer>();
                    List<AnswerOptions> answersVM = new List<AnswerOptions>();
                    if (questionVM.AnswertType == (int)AnswersTypes.Text)
                        answersVM = IsAr ? questionVM.Answers.Where(a => !string.IsNullOrEmpty(a.TextAr)).ToList() : questionVM.Answers.Where(a => !string.IsNullOrEmpty(a.TextEn)).ToList();
                    else
                        answersVM = questionVM.Answers.Where(a => a.Img != null).ToList();

                    foreach (var ans in answersVM)
                    {
                        Answer answer = new Answer();
                        answer.IsAnswer = ans.IsAnswer;
                        if(questionVM.AnswertType == (int)AnswersTypes.Text)
                        {
                            answer.TextEn = ans.TextEn;
                            answer.TextAr = ans.TextAr;
                        }
                        else
                        {
                            // Upload the image to database attachments
                            if (ans.Img != null)
                            {
                                try
                                {
                                    using (var stream = ans.Img.OpenReadStream())
                                    {
                                        var imageName = Guid.NewGuid().ToString() + Path.GetExtension(ans.Img.FileName);
                                        var imagePath = Path.Combine(_env.WebRootPath, "assets", "images", "answeres");
                                        string filePath = Path.Combine(imagePath, imageName);

                                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                                        {
                                            stream.CopyTo(fileStream);
                                        }
                                        answer.IsImg = true;
                                        answer.ImgPath = imageToDeleteIfExist = $"\\{filePath.Substring(filePath.IndexOf("assets"))}";
                                    }
                                }
                                catch (Exception ex)
                                {
                                    return new Response()
                                    {
                                        Succeeded = false
                                    };
                                }
                            }
                        }

                        answer.CreatedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId"));
                        answer.CreatedOn = DateTime.Now;
                        answers.Add(answer);
                    }
                    question.Answers = answers;
                    await _questionRepository.InsertAsync(question);
                    await _questionRepository.UnitOfWork.SaveChangesAsync();
                }



                return new Response()
                {
                    Succeeded = true,
                    StatusCode = (int)HttpStatusCode.Ok,
                };
            }
            catch (Exception ex)
            {
                var imagePathToDelete = Path.Combine(_env.WebRootPath, "assets", "images", "answeres", Path.GetFileName(imageToDeleteIfExist));
                System.IO.File.Delete(imagePathToDelete);

                throw ex;
            }
        }

        /// <summary>
        /// Get question by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Response<QuestionVM>> GetQuestion(int id)
        {
            try
            {
                var questionDb = await _questionRepository.GetAll(a => a.Id == id).Include(a => a.Answers.Where(a => a.IsDeleted != true)).FirstOrDefaultAsync();
                if (questionDb != null)
                {
                    QuestionVM questionVM = new QuestionVM();
                    questionVM.Id = id;
                    questionVM.TextEn = questionDb.TextEn;
                    questionVM.TextAr = questionDb.TextAr;
                    questionVM.Points = questionDb.Points;
                    questionVM.Time = questionDb.Timer;
                    questionVM.CategoryId = questionDb.CategoryID;
                    if(questionDb.Answers!=null && questionDb.Answers.Count > 0)
                    {
                        questionDb.Answers.Where(a => a.IsDeleted != true).ToList().ForEach(answer =>
                        {
                            questionVM.Answers.Add(new AnswerOptions()
                            {
                                Id = answer.Id,
                                TextAr = answer.TextAr,
                                TextEn = answer.TextEn,
                                IsAnswer = answer.IsAnswer,
                            });
                            questionVM.AnswertType = answer.IsImg.HasValue && answer.IsImg.Value ? ((int)AnswersTypes.Image) : ((int)AnswersTypes.Text);
                        });
                    }
                    return questionVM;
                }
                else
                    return new Response<QuestionVM>() { StatusCode = (int)HttpStatusCode.NotFound };
            }
            catch (Exception ex)
            {
                return new Response<QuestionVM>()
                {
                    Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message
                };
            }
        }

        /// <summary>
        /// Delete question
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Response> DeleteQuestion(int id)
        {
            try
            {
                var questionDb = await _questionRepository.FindAsync(id);
                if(questionDb != null) 
                {
                    questionDb.IsDeleted = true;
                    questionDb.DeletedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId"));
                    questionDb.DeletedOn = DateTime.Now;
                    _questionRepository.Update(questionDb);
                    await _questionRepository.UnitOfWork.SaveChangesAsync();
                }
                return new Response<QuestionVM>() { Succeeded = true };
            }
            catch (Exception ex)
            {
                return new Response()
                {
                    Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message
                };
            }
        }

        /// <summary>
        /// Get Random question for category in competition
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="questions"></param>
        /// <returns></returns>
        public async Task<Response<QuestionVM>> GetRandomQuestionPerCategory(int categoryId, List<int> questions)
        {
            try
            {
                var questionsDb = await _questionRepository.GetAll(a => a.CategoryID == categoryId && a.IsDeleted != true && !questions.Contains(a.Id))
                    .Include(a=>a.Answers)
                    .ToListAsync();

                if (questionsDb.Count > 0)
                {
                    Random random = new Random();
                    int randomIndex = random.Next(0, questionsDb.Count);
                    var randomQuestion = questionsDb[randomIndex];

                    if (randomQuestion != null)
                    {
                        List<AnswerOptions> answerOptions = new List<AnswerOptions>();
                        foreach (var answer in randomQuestion.Answers.Where(a => a.IsDeleted != true).ToList())
                        {
                            AnswerOptions option = new AnswerOptions();
                            option.Id = answer.Id;
                            option.TextEn = answer.TextEn;
                            option.TextAr = answer.TextAr;
                            option.IsImg = answer.IsImg ?? false;
                            option.ImgPath = answer.ImgPath;
                            option.IsAnswer = answer.IsAnswer;
                            answerOptions.Add(option);
                        }

                        return new Response<QuestionVM>()
                        {
                            Succeeded = true,
                            Data = new QuestionVM()
                            {
                                Id = randomQuestion.Id,
                                CategoryId = categoryId,
                                TextAr = randomQuestion.TextAr,
                                TextEn = randomQuestion.TextEn,
                                Points = randomQuestion.Points,
                                Time = randomQuestion.Timer,
                                Answers = answerOptions
                            }
                        };
                    }
                    else
                        return new Response<QuestionVM>() { Succeeded = false, StatusCode = (int)HttpStatusCode.NotFound };
                }
                else
                    return new Response<QuestionVM>() { Succeeded = false , StatusCode = (int)HttpStatusCode.NotFound };
            }
            catch (Exception ex)
            {
                return new Response<QuestionVM>() { StatusCode = (int)HttpStatusCode.BadRequest };
            }
        }
    }
}
