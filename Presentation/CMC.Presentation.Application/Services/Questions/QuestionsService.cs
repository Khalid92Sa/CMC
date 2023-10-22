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
using CMC.Presentation.Application.Services.Settings;
using CMC.Presentation.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        readonly IRepository<Attachment> _attachmentRepository;
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
            IRepository<Attachment> attachmentRepository,
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
            _attachmentRepository = attachmentRepository;
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
                        var attachmentImg = _attachmentRepository.GetAll(a => a.EntityId == category.Id && a.EntityType == (int)AttachmentTypes.Categories && a.IsDeleted != true).SingleOrDefault();
                        category.Img = attachmentImg != null ? Convert.ToBase64String(attachmentImg.FileData) : null;
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
                    category = new Lookup()
                    {
                        CategoryID = questionsCategoryLookup,
                        NameEn = categoryDTO.NameEn,
                        NameAr = categoryDTO.NameAr,
                        CreatedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId")),
                        CreatedOn = DateTime.Now,
                        IsDeleted = false,
                    };
                    await _lookupRepository.InsertAsync(category);
                    await _lookupRepository.UnitOfWork.SaveChangesAsync();
                }

                if (categoryDTO.Img != null)
                {
                    var currentAttachment = await _attachmentRepository.GetAll(a => a.EntityId == category.Id && a.EntityType == (int)AttachmentTypes.Categories && a.IsDeleted != true).SingleOrDefaultAsync();
                    bool IsUpdateAttachment = currentAttachment != null;
                    if (currentAttachment == null)
                        currentAttachment = new Attachment() { CreatedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId")), CreatedOn = DateTime.Now };
                    else
                    {
                        currentAttachment.ModifiedOn = category.ModifiedOn;
                        currentAttachment.ModifiedBy = category.ModifiedBy;
                    }

                    using (var memoryStream = new MemoryStream())
                    {
                        await categoryDTO.Img.CopyToAsync(memoryStream);
                        currentAttachment.FileName = categoryDTO.Img.FileName;
                        currentAttachment.FileData = memoryStream.ToArray();
                        currentAttachment.EntityId = category.Id;
                        currentAttachment.EntityType = (int)AttachmentTypes.Categories;

                        if (IsUpdateAttachment)
                            _attachmentRepository.Update(currentAttachment);
                        else
                            await _attachmentRepository.InsertAsync(currentAttachment);
                        await _attachmentRepository.UnitOfWork.SaveChangesAsync();
                    }
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
                var category = await _lookupRepository.GetAll(a => a.Id == id && a.IsDeleted != true).SingleOrDefaultAsync();
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
                    category.IsDeleted = true;
                    category.DeletedOn = DateTime.Now;
                    category.DeletedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId"));
                    _lookupRepository.Update(category);
                }
                else
                    _lookupRepository.Delete(category);

                

                await _questionRepository.UnitOfWork.SaveChangesAsync();
                await _lookupRepository.UnitOfWork.SaveChangesAsync();

                return new Response() { Succeeded = true };
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "DeleteCategory", $"Id:{id} - WithQuestions:{withQuestions}", null, false);
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
        public async Task<Response> DeleteExistingImg(int id, AttachmentTypes type)
        {
            try
            {
                var attachment = await _attachmentRepository.GetAll(a => a.EntityType == (int)type && a.EntityId == id && a.IsDeleted != true).SingleOrDefaultAsync();
                if (attachment != null)
                {
                    if(type == AttachmentTypes.Categories)
                        _attachmentRepository.Delete(attachment);
                    else
                    {
                        attachment.IsDeleted = true;
                        attachment.DeletedOn = DateTime.Now;
                        attachment.DeletedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId"));
                        _attachmentRepository.Update(attachment);
                    }
                    await _attachmentRepository.UnitOfWork.SaveChangesAsync();

                    if(type == AttachmentTypes.Questions)
                    {
                        var question = await _questionRepository.GetAll(a => a.Id == id && a.IsDeleted != true).SingleOrDefaultAsync();
                        if (question!=null)
                        {
                            question.HasImg = false;
                            _questionRepository.Update(question);
                            await _questionRepository.UnitOfWork.SaveChangesAsync();
                        }
                    }
                    else if(type == AttachmentTypes.Answers)
                    {
                        // delete also the answer
                        var answer = await _answerRepository.GetAll(a => a.Id == id && a.IsDeleted != true).SingleOrDefaultAsync();
                        if (answer != null)
                        {
                            answer.IsDeleted = true;
                            answer.DeletedOn = DateTime.Now;
                            answer.DeletedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId"));
                            _answerRepository.Update(answer);
                            await _answerRepository.UnitOfWork.SaveChangesAsync();
                        }
                    }
                }
                else
                    return new Response()
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                    };

                return new Response() { Succeeded = true };
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "DeleteExistingImg", $"Id:{id} - type:{type}", null, false);
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
                    var attachmentImg = await _attachmentRepository.GetAll(a => a.EntityId == category.Id && a.EntityType == (int)AttachmentTypes.Categories && a.IsDeleted != true).SingleOrDefaultAsync();
                    if (attachmentImg != null)
                        categoryDTO.ImgPath = Convert.ToBase64String(attachmentImg.FileData);

                    categoryDTO.Questions = _questionRepository.GetAll(a => a.CategoryID == category.Id && a.IsDeleted != true).Include(a => a.Answers).Select(a => new QuestionVM()
                    {
                        Id = a.Id,
                        CategoryId = a.CategoryID,
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
                await _logger.LogError(ex, "GetCategory", id, null, false);
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
                var questions = _questionRepository.GetAll(a => a.CategoryID == searchQuestionDTO.CategoryId && a.IsDeleted != true).AsQueryable();

                var result = questions
                        .WhereIf(!string.IsNullOrEmpty(searchQuestionDTO.QuestionText) && IsAr, a => a.TextAr.Contains(searchQuestionDTO.QuestionText))
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
                });

                return response;
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "GetAllQuestions", searchQuestionDTO, null, false);
                throw ex;
            }
        }


        public async Task<PagedResult<QuestionListVM>> GetLastQuestions()
        {
            try
            {
                bool IsAr = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName == "ar";

                PagedResult<QuestionListVM> response = new PagedResult<QuestionListVM>();
                var questions = _questionRepository.GetAll(a => a.IsDeleted != true)
                    .Include(a => a.Category)
                    .AsQueryable();

                var result = questions
                        .OrderByDescending(a => a.CreatedOn)
                        .ToQueryResultAsync(1, 15);

                response.PageSize = result.Result.PageSize;
                response.CurrentPage = result.Result.CurrentPage;
                response.TotalCount = result.Result.TotalCount;
                response.BrokenRules = result.Result.BrokenRules;
                response.Data = result.Result.Data.Select(x => new QuestionListVM
                {
                    Id = x.Id,
                    Text = IsAr ? x.TextAr : x.TextEn,
                    CategoryName = IsAr ? x.Category.NameAr : x.Category.NameEn
                });

                return response;
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "GetLastQuestions", null, null, false);
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
                bool IsUpdate = questionVM.Id.HasValue;

                if (IsUpdate)
                {
                    question = await _questionRepository.GetAll(a => a.Id == questionVM.Id.Value && a.IsDeleted != true).Include(a => a.Answers).FirstOrDefaultAsync();
                    if (question == null)
                        return new Response() { Succeeded = false, StatusCode = (int)HttpStatusCode.NotFound };
                }

                question.CategoryID = questionVM.CategoryId;
                question.TextEn = questionVM.TextEn;
                question.TextAr = questionVM.TextAr;

                if (IsUpdate)
                {
                    List<Answer> answersDb = new List<Answer>();

                    //Check if user changed the answer type from Img to Text.
                    if (question.AnswersType != questionVM.AnswertType)
                    {
                        question.Answers.ToList().ForEach(answer =>
                        {
                            answer.IsDeleted = true;
                            answer.DeletedOn = DateTime.Now;
                            answer.DeletedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId"));
                            _answerRepository.Update(answer);
                        });
                        await _answerRepository.UnitOfWork.SaveChangesAsync();
                    }
                    question.AnswersType = questionVM.AnswertType.Value;



                    //Check Answers if deleted or create or update
                    foreach (var answer in questionVM.Answers)
                    {
                        if (answer.Id.HasValue)
                        {
                            var answerDb = question.Answers.Where(a => a.Id == answer.Id).FirstOrDefault();
                            answerDb.IsAnswer = answer.IsAnswer;
                            if (questionVM.AnswertType == (int)AnswersTypes.Image)
                            {
                                // Image answer >> AnswersTypes.Image
                                if (answer.Img != null)
                                {
                                    using (var memoryStream = new MemoryStream())
                                    {
                                        await answer.Img.CopyToAsync(memoryStream);
                                        var attachmentAnswer = await _attachmentRepository.GetAll(a => a.EntityType == (int)AttachmentTypes.Answers && a.EntityId == answer.Id && a.IsDeleted != true).SingleOrDefaultAsync();
                                        bool IsUpdateAttachment = attachmentAnswer != null;
                                        if (attachmentAnswer == null)
                                            attachmentAnswer = new Attachment()
                                            {
                                                CreatedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId")),
                                                CreatedOn = DateTime.Now,
                                                EntityId = answer.Id.Value,
                                                EntityType = (int)AttachmentTypes.Answers
                                            };
                                        else
                                        {
                                            attachmentAnswer.ModifiedOn = DateTime.Now;
                                            attachmentAnswer.ModifiedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId"));
                                        }
                                        attachmentAnswer.FileName = answer.Img.FileName;
                                        attachmentAnswer.FileData = memoryStream.ToArray();
                                        if (IsUpdateAttachment)
                                            _attachmentRepository.Update(attachmentAnswer);
                                        else
                                            await _attachmentRepository.InsertAsync(attachmentAnswer);

                                        await _attachmentRepository.UnitOfWork.SaveChangesAsync();
                                    }
                                }
                            }
                            else
                            {
                                // Tesxt answer >> AnswersTypes.Text
                                answerDb.IsImg = false;
                                if (string.IsNullOrWhiteSpace(answer.TextEn) && string.IsNullOrWhiteSpace(answer.TextAr))
                                {
                                    // User delete the option
                                    answerDb.IsDeleted = true;
                                    answerDb.DeletedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId"));
                                    answerDb.DeletedOn = DateTime.Now;
                                }
                                else
                                {
                                    //Update
                                    answerDb.TextEn = answer.TextEn;
                                    answerDb.TextAr = answer.TextAr;
                                    answerDb.ModifiedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId"));
                                    answerDb.ModifiedOn = DateTime.Now;
                                }
                            }
                           
                            _answerRepository.Update(answerDb);
                        }
                        else
                        {
                            // New answer >> Id of answer is null
                            if (answer.Img != null && questionVM.AnswertType == (int)AnswersTypes.Image)
                            {
                                Answer answerDb = new Answer();
                                answerDb.QuestionId = question.Id;
                                answerDb.IsAnswer = answer.IsAnswer;
                                answerDb.CreatedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId"));
                                answerDb.CreatedOn = DateTime.Now;
                                answerDb.IsImg = true;
                                
                                await _answerRepository.InsertAsync(answerDb);
                                await _answerRepository.UnitOfWork.SaveChangesAsync();


                                using (var memoryStream = new MemoryStream())
                                {
                                    await answer.Img.CopyToAsync(memoryStream);
                                    var attachmentAnsewr = new Attachment()
                                    {
                                        CreatedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId")),
                                        CreatedOn = DateTime.Now,
                                        EntityId = answerDb.Id, // The issue is still the answer did not save to database
                                        EntityType = (int)AttachmentTypes.Answers,
                                        FileData = memoryStream.ToArray(),
                                        FileName = answer.Img.FileName
                                    };

                                    await _attachmentRepository.InsertAsync(attachmentAnsewr);
                                    await _attachmentRepository.UnitOfWork.SaveChangesAsync();
                                }
                            }
                            else if(questionVM.AnswertType == (int)AnswersTypes.Text)
                            {
                                // New option Text
                                answersDb.Add(new Answer()
                                {
                                    QuestionId = question.Id,
                                    IsAnswer = answer.IsAnswer,
                                    CreatedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId")),
                                    CreatedOn = DateTime.Now,
                                    TextAr = answer.TextAr,
                                    TextEn = answer.TextEn,
                                    IsImg = false
                                });
                            }
                        }
                    }
                    if (answersDb.Count > 0)
                        await _answerRepository.InsertAsync(answersDb);


                    //Update
                    question.ModifiedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId"));
                    question.ModifiedOn = DateTime.Now;
                    _questionRepository.Update(question);

                    await _answerRepository.UnitOfWork.SaveChangesAsync();
                    await _questionRepository.UnitOfWork.SaveChangesAsync();
                }
                else
                {
                    //Insert
                    question.CreatedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId"));
                    question.CreatedOn = DateTime.Now;
                    question.AnswersType = questionVM.AnswertType.Value;
                    List<Answer> answers = new List<Answer>();
                    List<AnswerOptions> answersVM = new List<AnswerOptions>();
                    if (questionVM.AnswertType == (int)AnswersTypes.Text)
                    {
                        //Save the answers directly.
                       answersVM = IsAr ? questionVM.Answers.Where(a => !string.IsNullOrEmpty(a.TextAr)).ToList() : questionVM.Answers.Where(a => !string.IsNullOrEmpty(a.TextEn)).ToList();
                        foreach (var ans in answersVM)
                        {
                            Answer answer = new Answer();
                            answer.IsAnswer = ans.IsAnswer;
                            answer.TextEn = ans.TextEn;
                            answer.TextAr = ans.TextAr;
                            answer.IsImg = false;
                            answer.CreatedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId"));
                            answer.CreatedOn = DateTime.Now;
                            answers.Add(answer);
                        }
                        question.Answers = answers;
                        await _questionRepository.InsertAsync(question);
                        await _questionRepository.UnitOfWork.SaveChangesAsync();
                    }
                    else
                    {
                        // Images
                        answersVM = questionVM.Answers.Where(a => a.Img != null).ToList();
                        if (answersVM.Count > 0)
                        {
                            // Save firstly the question to database.
                            await _questionRepository.InsertAsync(question);
                            await _questionRepository.UnitOfWork.SaveChangesAsync();

                            foreach (var ans in answersVM)
                            {
                                Answer answer = new Answer();
                                answer.IsAnswer = ans.IsAnswer;
                                answer.CreatedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId"));
                                answer.CreatedOn = DateTime.Now;
                                answer.QuestionId = question.Id;
                                answer.IsImg = true;

                                await _answerRepository.InsertAsync(answer);
                                await _answerRepository.UnitOfWork.SaveChangesAsync();
                                answers.Add(answer);


                                using (var memoryStream = new MemoryStream())
                                {
                                    await ans.Img.CopyToAsync(memoryStream);
                                    var attachmentAnsewr = new Attachment()
                                    {
                                        CreatedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId")),
                                        CreatedOn = DateTime.Now,
                                        EntityId = answer.Id, // The issue is still the answer did not save to database
                                        EntityType = (int)AttachmentTypes.Answers,
                                        FileData = memoryStream.ToArray(),
                                        FileName = ans.Img.FileName
                                    };
                                    await _attachmentRepository.InsertAsync(attachmentAnsewr);
                                    await _attachmentRepository.UnitOfWork.SaveChangesAsync();
                                }
                            }

                            question.Answers = answers;
                            _questionRepository.Update(question);
                            await _questionRepository.UnitOfWork.SaveChangesAsync();
                            await _attachmentRepository.UnitOfWork.SaveChangesAsync();
                        }
                    }
                }


                // Question Img attachment
                if (questionVM.Img != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await questionVM.Img.CopyToAsync(memoryStream);
                        var attachmentQuestion = await _attachmentRepository.GetAll(a => a.EntityType == (int)AttachmentTypes.Questions && a.EntityId == question.Id && a.IsDeleted != true).SingleOrDefaultAsync();
                        bool IsUpdateAttachment = attachmentQuestion != null;
                        if (attachmentQuestion == null)
                            attachmentQuestion = new Attachment()
                            {
                                CreatedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId")),
                                CreatedOn = DateTime.Now,
                                EntityId = question.Id,
                                EntityType = (int)AttachmentTypes.Questions
                            };
                        else
                        {
                            attachmentQuestion.ModifiedOn = DateTime.Now;
                            attachmentQuestion.ModifiedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId"));
                        }

                        attachmentQuestion.FileName = questionVM.Img.FileName;
                        attachmentQuestion.FileData = memoryStream.ToArray();

                        if (IsUpdateAttachment)
                            _attachmentRepository.Update(attachmentQuestion);
                        else
                            await _attachmentRepository.InsertAsync(attachmentQuestion);


                        question.HasImg = true;
                        _questionRepository.Update(question);

                        await _attachmentRepository.UnitOfWork.SaveChangesAsync();
                        await _questionRepository.UnitOfWork.SaveChangesAsync();
                    }
                }

                return new Response()
                {
                    Succeeded = true,
                    StatusCode = (int)HttpStatusCode.Ok,
                };
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "AddQuestions", questionVM, null, false);
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
                    //questionVM.Points = questionDb.Points;
                    //questionVM.Time = questionDb.Timer;
                    questionVM.CategoryId = questionDb.CategoryID;

                    //Check if this question has img.
                    var attachmentQuestion = await _attachmentRepository.GetAll(a => a.EntityType == (int)AttachmentTypes.Questions && a.EntityId == questionDb.Id && a.IsDeleted != true).SingleOrDefaultAsync();
                    if (attachmentQuestion != null)
                        questionVM.ImgPath = Convert.ToBase64String(attachmentQuestion.FileData);

                    if (questionDb.Answers != null && questionDb.Answers.Count > 0)
                    {
                        var questionAnswers = questionDb.Answers.Where(a => a.IsDeleted != true).ToList();
                        if (questionAnswers.Count > 0)
                            questionVM.AnswertType = questionAnswers.Any(a => a.IsImg == true) ? ((int)AnswersTypes.Image) : ((int)AnswersTypes.Text);

                        foreach (var answer in questionAnswers)
                        {
                            AnswerOptions answerOptions = new AnswerOptions();
                            answerOptions.Id = answer.Id;
                            answerOptions.TextAr = answer.TextAr;
                            answerOptions.TextEn = answer.TextEn;
                            answerOptions.IsAnswer = answer.IsAnswer;
                            answerOptions.IsImg = answer.IsImg ?? false;
                            if(answer.IsImg == true)
                            {
                                var attachmentAnswer = await _attachmentRepository.GetAll(a => a.EntityType == (int)AttachmentTypes.Answers && a.EntityId == answer.Id && a.IsDeleted != true).SingleOrDefaultAsync();
                                if (attachmentAnswer != null)
                                {
                                    answerOptions.ImgPath = Convert.ToBase64String(attachmentAnswer.FileData);
                                    answerOptions.ImgName = attachmentAnswer.FileName;
                                }
                            }
                            questionVM.Answers.Add(answerOptions);
                        }
                    }
                    return questionVM;
                }
                else
                    return new Response<QuestionVM>() { StatusCode = (int)HttpStatusCode.NotFound };
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "GetQuestion", id, null, false);
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
                if (questionDb != null)
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
                await _logger.LogError(ex, "DeleteQuestion", id, null, false);
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
                var questionsDb = await _questionRepository.GetAll()
                    .Include(a => a.Answers)
                    .Where(a => a.CategoryID == categoryId && a.IsDeleted != true && !questions.Contains(a.Id) && a.Answers.Any(answer => answer.IsDeleted != true))
                    .ToListAsync();

                if (questionsDb.Count > 0)
                {
                    Random random = new Random();
                    int randomIndex = random.Next(0, questionsDb.Count);
                    var randomQuestion = questionsDb[randomIndex];

                    if (randomQuestion != null)
                    {
                        string questionImg = "";
                        if (randomQuestion.HasImg == true)
                        {
                            var attachmentQuestion = await _attachmentRepository.GetAll(a => a.EntityType == (int)AttachmentTypes.Questions && a.EntityId == randomQuestion.Id && a.IsDeleted != true).SingleOrDefaultAsync();
                            if (attachmentQuestion != null)
                                questionImg = Convert.ToBase64String(attachmentQuestion.FileData);
                        }

                        List<AnswerOptions> answerOptions = new List<AnswerOptions>();
                        foreach (var answer in randomQuestion.Answers.Where(a=>a.IsDeleted!=true).ToList())
                        {
                            AnswerOptions option = new AnswerOptions();
                            option.Id = answer.Id;
                            option.TextEn = answer.TextEn;
                            option.TextAr = answer.TextAr;
                            option.IsImg = answer.IsImg ?? false;
                            if (option.IsImg)
                            {
                                var attachmentAnswer = await _attachmentRepository.GetAll(a => a.EntityType == (int)AttachmentTypes.Answers && a.EntityId == answer.Id && a.IsDeleted != true).SingleOrDefaultAsync();
                                if (attachmentAnswer != null)
                                    option.ImgPath = Convert.ToBase64String(attachmentAnswer.FileData);
                            }
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
                                ImgPath = questionImg,
                                Answers = answerOptions,
                                AnswertType = randomQuestion.AnswersType
                            }
                        };
                    }
                    else
                        return new Response<QuestionVM>() { Succeeded = false, StatusCode = (int)HttpStatusCode.NotFound };
                }
                else
                    return new Response<QuestionVM>() { Succeeded = false, StatusCode = (int)HttpStatusCode.NotFound };
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "GetRandomQuestionPerCategory", $"categoryId:{categoryId} - ListQuestion:{questions}", null, false);
                return new Response<QuestionVM>() { StatusCode = (int)HttpStatusCode.BadRequest };
            }
        }
    }
}
