using AutoMapper;
using CMC.Kernel.Core.Enums;
using CMC.Kernel.Core.Helpers;
using CMC.Kernel.Core.Infrastructure;
using CMC.Kernel.Core.Persistence;
using CMC.Kernel.Core.Services;
using CMC.Kernel.Core.Wrappers;
using CMC.Kernel.Domain.Entities;
using CMC.Kernel.Infrastructure.Caching.Model;
using CMC.Kernel.Infrastructure.Persistence.Repositories.Lookups;
using CMC.Kernel.Infrastructure.Persistence.UnitOfWork;
using CMC.Presentation.Application.DTOs.Competitions;
using CMC.Presentation.Application.DTOs.Questions;
using CMC.Presentation.Application.Services.Settings;
using CMC.Presentation.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LicenseContext = OfficeOpenXml.LicenseContext;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;


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
        public async Task<List<LookupModel>> GetCategories(bool withImages)
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
                        if (withImages)
                        {
                            var attachmentImg = _attachmentRepository.GetAll(a => a.EntityId == category.Id && a.EntityType == (int)AttachmentTypes.Categories && a.IsDeleted != true).SingleOrDefault();
                            category.Img = attachmentImg != null ? Convert.ToBase64String(attachmentImg.FileData) : null;
                        }
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
                        memoryStream.Position = 0;

                        // Process and resize image
                        byte[] processedImageData;
                        using (var originalStream = new MemoryStream())
                        {
                            await categoryDTO.Img.CopyToAsync(originalStream);
                            originalStream.Position = 0;

                            // Detect format BEFORE loading the image
                            var format = Image.DetectFormat(originalStream);
                            originalStream.Position = 0; // Reset position after detection

                            using (var image = Image.Load(originalStream))
                            {
                                // Check if resizing is needed
                                if (image.Width > 500 || image.Height > 500)
                                {
                                    // Resize maintaining aspect ratio
                                    image.Mutate(x => x.Resize(new ResizeOptions
                                    {
                                        Mode = ResizeMode.Max,
                                        Size = new Size(500, 500)
                                    }));
                                }

                                // Save processed image to byte array IN THE SAME FORMAT
                                using (var outputStream = new MemoryStream())
                                {
                                    if (format != null)
                                    {
                                        // Save in the detected format
                                        image.Save(outputStream, format);
                                    }
                                    else
                                    {
                                        // Fallback: determine format from filename
                                        var extension = Path.GetExtension(categoryDTO.Img.FileName)?.ToLower();
                                        switch (extension)
                                        {
                                            case ".png":
                                                image.SaveAsPng(outputStream);
                                                break;
                                            case ".gif":
                                                image.SaveAsGif(outputStream);
                                                break;
                                            case ".bmp":
                                                image.SaveAsBmp(outputStream);
                                                break;
                                            case ".jpg":
                                            case ".jpeg":
                                                image.SaveAsJpeg(outputStream);
                                                break;
                                            default:
                                                // Default to PNG to preserve transparency
                                                image.SaveAsPng(outputStream);
                                                break;
                                        }
                                    }

                                    processedImageData = outputStream.ToArray();
                                }
                            }
                        }

                        currentAttachment.FileName = categoryDTO.Img.FileName;
                        currentAttachment.FileData = processedImageData;
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
                bool isAr = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName == "ar";
                var response = new PagedResult<QuestionListVM>();

                // Get base query with category and soft delete filters
                var questions = _questionRepository.GetAll(a => a.CategoryID == searchQuestionDTO.CategoryId
                    && a.IsDeleted != true
                    && a.IsArchived != true).AsQueryable();

                // Apply text search filters based on language
                if (!string.IsNullOrEmpty(searchQuestionDTO.QuestionText))
                {
                    questions = isAr
                        ? questions.Where(a => a.TextAr.Contains(searchQuestionDTO.QuestionText))
                        : questions.Where(a => a.TextEn.Contains(searchQuestionDTO.QuestionText));
                }

                // Apply date filtering and sorting
                if (!string.IsNullOrEmpty(searchQuestionDTO.Date))
                {
                    // Parse the date and get start and end of day
                    DateTime dateStart = Convert.ToDateTime(searchQuestionDTO.Date).Date;
                    DateTime dateEnd = dateStart.AddDays(1);

                    // Filter questions from the specified date onwards
                    questions = questions.Where(q => q.CreatedOn >= dateStart);

                    // Apply conditional sorting:
                    // - Questions created on the specific day: order by Id ASC
                    // - Questions created after that day: order by CreatedOn DESC
                    questions = questions
                        .OrderBy(q => q.CreatedOn >= dateEnd ? 1 : 0) // Same day questions first
                        .ThenBy(q => q.CreatedOn < dateEnd ? q.Id : 0) // Order by Id ASC for same day
                        .ThenByDescending(q => q.CreatedOn); // Order by CreatedOn DESC for later dates
                }
                else
                {
                    // No date filter: order all by CreatedOn DESC
                    questions = questions.OrderByDescending(q => q.CreatedOn);
                }

                // Apply pagination
                var result = await questions
                    .ToQueryResultAsync(searchQuestionDTO.PageNumber, searchQuestionDTO.PageSize);

                // Map to response
                response.PageSize = result.PageSize;
                response.CurrentPage = result.CurrentPage;
                response.TotalCount = result.TotalCount;
                response.BrokenRules = result.BrokenRules;
                response.Data = result.Data.Select(x => new QuestionListVM
                {
                    Id = x.Id,
                    Text = isAr ? x.TextAr : x.TextEn,
                });

                return response;
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "GetAllQuestions", searchQuestionDTO, null, false);
                throw;
            }
        }


        public async Task<PagedResult<QuestionListVM>> GetLastQuestions()
        {
            try
            {
                bool IsAr = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName == "ar";

                PagedResult<QuestionListVM> response = new PagedResult<QuestionListVM>();
                var questions = _questionRepository.GetAll(a => a.IsDeleted != true && a.IsArchived != true)
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
                    question = await _questionRepository.GetAll(a => a.Id == questionVM.Id.Value && a.IsDeleted != true && a.IsArchived != true).Include(a => a.Answers).FirstOrDefaultAsync();
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
        /// Archive questions
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<Response> ArchiveQuestions(int type, int categoryId)
        {
            try
            {
                var isArchived = type == 1;

                var rowsAffected = await _questionRepository.ExecuteSqlRawAsync(
                    "UPDATE [CMC].[Questions] SET IsArchived = {0} WHERE CategoryID = {1} AND IsDeleted != 1 OR IsDeleted IS NULL",
                    isArchived,categoryId);

                return new Response() { Succeeded = true, Message = "Success" };
            }
            catch (Exception ex)
            {
                return new Response() { Succeeded = false, Message = "Error ocurred once update archiving" };
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
                    .Where(a => a.CategoryID == categoryId && a.IsDeleted != true && a.IsArchived != true && !questions.Contains(a.Id) && a.Answers.Any(answer => answer.IsDeleted != true))
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

        /// <summary>
        /// Add multiple questions in bulk
        /// </summary>
        /// <param name="bulkQuestionsDTO"></param>
        /// <returns></returns>
        public async Task<Response> AddBulkQuestions(BulkQuestionsDTO bulkQuestionsDTO)
        {
            try
            {
                bool IsAr = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName == "ar";

                var validModel = await Validate(bulkQuestionsDTO);
                if (!validModel.Succeeded)
                    return new Response()
                    {
                        BrokenRules = validModel.BrokenRules,
                        StatusCode = (int)HttpStatusCode.BusinessRuleViolation
                    };

                var successCount = 0;
                var failureCount = 0;
                var errors = new List<string>();
                var questionsToSave = new List<Question>();

                foreach (var questionVM in bulkQuestionsDTO.Questions)
                {
                    try
                    {
                        var questionValidation = await Validate(questionVM);
                        if (!questionValidation.Succeeded)
                        {
                            failureCount++;
                            errors.Add($"Question '{questionVM.TextEn ?? questionVM.TextAr}': {string.Join(", ", questionValidation.BrokenRules.Select(br => br.Message))}");
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(questionVM.TextEn) && string.IsNullOrWhiteSpace(questionVM.TextAr))
                        {
                            failureCount++;
                            errors.Add($"Question: At least one language required");
                            continue;
                        }

                        var validAnswers = questionVM.Answers?.Where(a => !string.IsNullOrWhiteSpace(a.TextEn) || !string.IsNullOrWhiteSpace(a.TextAr)).ToList() ?? new List<AnswerOptions>();

                        if (validAnswers.Count < 2)
                        {
                            failureCount++;
                            errors.Add($"Question '{questionVM.TextEn ?? questionVM.TextAr}': At least 2 answer options required");
                            continue;
                        }

                        if (!validAnswers.Any(a => a.IsAnswer))
                        {
                            failureCount++;
                            errors.Add($"Question '{questionVM.TextEn ?? questionVM.TextAr}': No correct answer specified");
                            continue;
                        }

                        var question = new Question
                        {
                            CategoryID = questionVM.CategoryId,
                            TextEn = questionVM.TextEn?.Trim(),
                            TextAr = questionVM.TextAr?.Trim(),
                            AnswersType = questionVM.AnswertType ?? (int)AnswersTypes.Text,
                            CreatedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId")),
                            CreatedOn = DateTime.Now,
                            IsDeleted = false,
                            Answers = new List<Answer>()
                        };

                        foreach (var answerVM in validAnswers)
                        {
                            question.Answers.Add(new Answer
                            {
                                TextEn = answerVM.TextEn?.Trim(),
                                TextAr = answerVM.TextAr?.Trim(),
                                IsAnswer = answerVM.IsAnswer,
                                IsImg = false,
                                CreatedBy = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("UserId")),
                                CreatedOn = DateTime.Now,
                                IsDeleted = false
                            });
                        }

                        questionsToSave.Add(question);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        failureCount++;
                        errors.Add($"Question error: {ex.Message}");
                        await _logger.LogError(ex, "AddBulkQuestions - Question Error", questionVM, null, false);
                    }
                }

                if (questionsToSave.Count > 0)
                {
                    try
                    {
                        _unitOfWork.BeginTransaction();
                        await _questionRepository.InsertAsync(questionsToSave);
                        await _questionRepository.UnitOfWork.SaveChangesAsync();
                        _unitOfWork.Commit();
                    }
                    catch (Exception ex)
                    {
                        _unitOfWork.Rollback();
                        await _logger.LogError(ex, "AddBulkQuestions - Database Error", questionsToSave.Count, null, false);
                        failureCount += successCount;
                        successCount = 0;
                        errors.Add(IsAr ? "حدث خطأ في قاعدة البيانات أثناء حفظ الأسئلة." : "Database error occurred while saving questions.");
                    }
                }

                var message = successCount > 0
                    ? (IsAr
                        ? $"تم استيراد {successCount} سؤال بنجاح" + (failureCount > 0 ? $". فشل استيراد {failureCount} سؤال." : "")
                        : $"Successfully imported {successCount} questions" + (failureCount > 0 ? $". {failureCount} failed." : ""))
                    : (IsAr
                        ? "لم يتم استيراد أي سؤال بنجاح."
                        : "No questions were imported successfully.");

                return new Response()
                {
                    Succeeded = successCount > 0,
                    Message = message,
                    StatusCode = successCount > 0 ? (int)HttpStatusCode.Ok : (int)HttpStatusCode.BadRequest
                };
            }
            catch (Exception ex)
            {
                try { _unitOfWork.Rollback(); } catch { }
                await _logger.LogError(ex, "AddBulkQuestions", bulkQuestionsDTO, null, false);
                return new Response()
                {
                    Message = ex.Message,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }

        /// <summary>
        /// Validate Excel file structure and data
        /// </summary>
        /// <param name="excelData"></param>
        /// <returns></returns>
        public async Task<Response<List<QuestionVM>>> ValidateExcelQuestions(List<Dictionary<string, object>> excelData)
        {
            try
            {
                var questions = new List<QuestionVM>();
                var errors = new List<string>();

                foreach (var row in excelData.Select((value, index) => new { value, index }))
                {
                    var rowNumber = row.index + 2; // Excel row number (accounting for header)
                    var data = row.value;

                    try
                    {
                        var question = new QuestionVM
                        {
                            TextEn = data.ContainsKey("Question_EN") ? data["Question_EN"]?.ToString()?.Trim() : "",
                            TextAr = data.ContainsKey("Question_AR") ? data["Question_AR"]?.ToString()?.Trim() : "",
                            AnswertType = (int)AnswersTypes.Text,
                            Answers = new List<AnswerOptions>()
                        };

                        // Extract options
                        for (int i = 1; i <= 4; i++)
                        {
                            var optionEn = data.ContainsKey($"Option{i}_EN") ? data[$"Option{i}_EN"]?.ToString()?.Trim() : "";
                            var optionAr = data.ContainsKey($"Option{i}_AR") ? data[$"Option{i}_AR"]?.ToString()?.Trim() : "";

                            if (!string.IsNullOrEmpty(optionEn) || !string.IsNullOrEmpty(optionAr))
                            {
                                question.Answers.Add(new AnswerOptions
                                {
                                    TextEn = optionEn,
                                    TextAr = optionAr,
                                    IsAnswer = false
                                });
                            }
                        }

                        // Set correct answer
                        if (data.ContainsKey("Correct_Answer") && int.TryParse(data["Correct_Answer"].ToString(), out int correctAnswer))
                        {
                            if (correctAnswer >= 1 && correctAnswer <= question.Answers.Count)
                            {
                                question.Answers[correctAnswer - 1].IsAnswer = true;
                            }
                            else
                            {
                                errors.Add($"Row {rowNumber}: Correct answer index {correctAnswer} is out of range");
                            }
                        }
                        else
                        {
                            errors.Add($"Row {rowNumber}: Correct answer not specified or invalid");
                        }

                        // Basic validation
                        if (string.IsNullOrEmpty(question.TextEn) && string.IsNullOrEmpty(question.TextAr))
                        {
                            errors.Add($"Row {rowNumber}: Question text is required");
                            continue;
                        }

                        if (question.Answers.Count < 2)
                        {
                            errors.Add($"Row {rowNumber}: At least 2 answer options are required");
                            continue;
                        }

                        if (!question.Answers.Any(a => a.IsAnswer))
                        {
                            errors.Add($"Row {rowNumber}: No correct answer specified");
                            continue;
                        }

                        questions.Add(question);
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Row {rowNumber}: Error processing row - {ex.Message}");
                    }
                }

                return new Response<List<QuestionVM>>()
                {
                    Succeeded = questions.Count > 0,
                    Data = questions,
                    StatusCode = questions.Count > 0 ? (int)HttpStatusCode.Ok : (int)HttpStatusCode.BadRequest,
                    Message = errors.Count > 0 ? string.Join("; ", errors) : "Questions validated successfully"
                };
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "ValidateExcelQuestions", excelData, null, false);
                return new Response<List<QuestionVM>>()
                {
                    Message = ex.Message,
                    Succeeded = false,
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }

        /// <summary>
        /// Generate Excel template for bulk import
        /// </summary>
        /// <returns></returns>
        public async Task<byte[]> GenerateExcelTemplate()
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage())
                {
                    // Main template sheet with samples
                    var worksheet = package.Workbook.Worksheets.Add("Questions Template");

                    // Create professional styles
                    var headerStyle = package.Workbook.Styles.CreateNamedStyle("HeaderStyle");
                    headerStyle.Style.Font.Bold = true;
                    headerStyle.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    headerStyle.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    headerStyle.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkBlue);
                    headerStyle.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    var dataStyle = package.Workbook.Styles.CreateNamedStyle("DataStyle");
                    dataStyle.Style.WrapText = true;
                    dataStyle.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                    var correctAnswerStyle = package.Workbook.Styles.CreateNamedStyle("CorrectAnswerStyle");
                    correctAnswerStyle.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    correctAnswerStyle.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
                    correctAnswerStyle.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    // Headers with helpful descriptions
                    var headersWithDesc = new (string Header, string Description)[]
                    {
                        ("Question_EN", "Question text in English (required if no Arabic)"),
                        ("Question_AR", "Question text in Arabic (required if no English)"),
                        ("Option1_EN", "First answer option in English (required)"),
                        ("Option1_AR", "First answer option in Arabic (required)"),
                        ("Option2_EN", "Second answer option in English (required)"),
                        ("Option2_AR", "Second answer option in Arabic (required)"),
                        ("Option3_EN", "Third answer option in English (optional)"),
                        ("Option3_AR", "Third answer option in Arabic (optional)"),
                        ("Option4_EN", "Fourth answer option in English (optional)"),
                        ("Option4_AR", "Fourth answer option in Arabic (optional)"),
                        ("Correct_Answer", "Number 1-4 indicating which option is correct")
                    };

                    // Add headers with comments
                    for (int i = 0; i < headersWithDesc.Length; i++)
                    {
                        var cell = worksheet.Cells[1, i + 1];
                        cell.Value = headersWithDesc[i].Header;
                        cell.StyleName = "HeaderStyle";
                        cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);

                        // Add helpful comment tooltips
                        cell.AddComment(headersWithDesc[i].Description, "System");
                        cell.Comment.AutoFit = true;
                    }

                    // Enhanced sample data with various examples
                    var enhancedSampleData = new List<object[]>
                    {
                        new object[]
                        {
                            "What is the capital of France?", "ما هي عاصمة فرنسا؟",
                            "London", "لندن", "Berlin", "برلين",
                            "Paris", "باريس", "Madrid", "مدريد", 3
                        },
                        new object[]
                        {
                            "Which planet is closest to the Sun?", "أي كوكب أقرب إلى الشمس؟",
                            "Venus", "الزهرة", "Mercury", "عطارد",
                            "Earth", "الأرض", "Mars", "المريخ", 2
                        },
                        new object[]
                        {
                            "What is 2 + 2?", "كم يساوي 2 + 2؟",
                            "3", "3", "4", "4",
                            "5", "5", "", "", 2  // Example with only 3 options
                        },
                        new object[]
                        {
                            "Who painted the Mona Lisa?", "من رسم الموناليزا؟",
                            "Pablo Picasso", "بابلو بيكاسو", "Leonardo da Vinci", "ليوناردو دافنشي",
                            "", "", "", "", 2  // Example with only 2 options
                        },
                        new object[]
                        {
                            "What is the largest mammal?", "ما هو أكبر حيوان ثديي؟",
                            "Elephant", "فيل", "Blue Whale", "الحوت الأزرق",
                            "Giraffe", "زرافة", "Hippopotamus", "فرس النهر", 2
                        }
                    };

                    // Add sample data with professional styling
                    for (int row = 0; row < enhancedSampleData.Count; row++)
                    {
                        for (int col = 0; col < enhancedSampleData[row].Length; col++)
                        {
                            var cell = worksheet.Cells[row + 2, col + 1];
                            cell.Value = enhancedSampleData[row][col];

                            if (col == 10) // Correct_Answer column
                                cell.StyleName = "CorrectAnswerStyle";
                            else
                                cell.StyleName = "DataStyle";

                            cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        }
                    }

                    // Set optimal column widths and row heights
                    worksheet.Column(1).Width = 45; // Question_EN
                    worksheet.Column(2).Width = 45; // Question_AR
                    for (int i = 3; i <= 10; i++)
                    {
                        worksheet.Column(i).Width = 25; // Options
                    }
                    worksheet.Column(11).Width = 18; // Correct_Answer

                    // Set row heights for better readability
                    worksheet.Row(1).Height = 25; // Header row
                    for (int i = 2; i <= enhancedSampleData.Count + 1; i++)
                    {
                        worksheet.Row(i).Height = 35; // Data rows
                    }

                    // Add smart data validation for Correct_Answer column
                    var correctAnswerRange = worksheet.Cells[2, 11, 1000, 11];
                    var validation = correctAnswerRange.DataValidation.AddIntegerDataValidation();
                    validation.Formula.Value = 1;
                    validation.Formula2.Value = 4;
                    validation.ErrorTitle = "Invalid Answer";
                    validation.Error = "Correct answer must be between 1 and 4";
                    validation.ShowErrorMessage = true;

                    // Add freeze panes for better navigation
                    worksheet.View.FreezePanes(2, 1);

                    // Create clean empty template sheet
                    var emptySheet = package.Workbook.Worksheets.Add("Empty Template");

                    // Copy headers to empty sheet
                    for (int i = 0; i < headersWithDesc.Length; i++)
                    {
                        var cell = emptySheet.Cells[1, i + 1];
                        cell.Value = headersWithDesc[i].Header;
                        cell.StyleName = "HeaderStyle";
                        cell.AddComment(headersWithDesc[i].Description, "System");
                    }

                    // Apply same formatting to empty sheet
                    emptySheet.Column(1).Width = 45;
                    emptySheet.Column(2).Width = 45;
                    for (int i = 3; i <= 10; i++)
                    {
                        emptySheet.Column(i).Width = 25;
                    }
                    emptySheet.Column(11).Width = 18;
                    emptySheet.Row(1).Height = 25;

                    // Add data validation to empty sheet
                    var emptyCorrectAnswerRange = emptySheet.Cells[2, 11, 1000, 11];
                    var emptyValidation = emptyCorrectAnswerRange.DataValidation.AddIntegerDataValidation();
                    emptyValidation.Formula.Value = 1;
                    emptyValidation.Formula2.Value = 4;
                    emptyValidation.ErrorTitle = "Invalid Answer";
                    emptyValidation.Error = "Correct answer must be between 1 and 4";
                    emptyValidation.ShowErrorMessage = true;

                    emptySheet.View.FreezePanes(2, 1);

                    // Add comprehensive instructions sheet
                    var instructionsSheet = package.Workbook.Worksheets.Add("Instructions");

                    var detailedInstructions = new string[]
                    {
                        "EXCEL IMPORT INSTRUCTIONS FOR QUESTIONS",
                        "",
                        "QUICK START:",
                        "1. Use 'Empty Template' sheet to add your questions",
                        "2. Fill at least Question_EN OR Question_AR for each question",
                        "3. Provide at least 2 answer options (Option1 and Option2)",
                        "4. Set Correct_Answer to 1, 2, 3, or 4",
                        "5. Save file and upload to the system",
                        "",
                        "COLUMN DESCRIPTIONS:",
                        "• Question_EN: Question text in English",
                        "• Question_AR: Question text in Arabic",
                        "• Option1_EN/AR: First answer choice (REQUIRED)",
                        "• Option2_EN/AR: Second answer choice (REQUIRED)",
                        "• Option3_EN/AR: Third answer choice (optional)",
                        "• Option4_EN/AR: Fourth answer choice (optional)",
                        "• Correct_Answer: Number (1-4) indicating correct option",
                        "",
                        "IMPORTANT RULES:",
                        "• At least ONE language (EN or AR) required per question",
                        "• Minimum 2 answer options required per question",
                        "• Correct_Answer must be 1, 2, 3, or 4",
                        "• The correct answer option must not be empty",
                        "• Column names are case-sensitive (don't change them)",
                        "",
                        "PRO TIPS:",
                        "• You can mix questions with 2, 3, or 4 options",
                        "• Leave Option3/Option4 empty for questions with fewer choices",
                        "• Use 'Questions Template' sheet to see examples",
                        "• Test with a few questions first before importing hundreds",
                        "• Questions without both languages will show warnings",
                        "",
                        "EXAMPLES:",
                        "Example 1 - Full question (4 options):",
                        "Question_EN: What is the capital of Italy?",
                        "Option1_EN: Rome, Option2_EN: Milan, Option3_EN: Naples, Option4_EN: Turin",
                        "Correct_Answer: 1",
                        "",
                        "Example 2 - Simple question (2 options):",
                        "Question_EN: Is the Earth round?",
                        "Option1_EN: Yes, Option2_EN: No, Option3_EN: [empty], Option4_EN: [empty]",
                        "Correct_Answer: 1",
                        "",
                        "Example 3 - Bilingual question:",
                        "Question_EN: What color is the sky?, Question_AR: ما لون السماء؟",
                        "Option1_EN: Blue, Option1_AR: أزرق",
                        "Option2_EN: Red, Option2_AR: أحمر",
                        "Correct_Answer: 1",
                        "",
                        "TROUBLESHOOTING:",
                        "• 'Invalid file format' → Save as .xlsx or .xls",
                        "• 'No questions found' → Check if you have data in rows 2+",
                        "• 'Validation errors' → Ensure Correct_Answer is 1-4",
                        "• 'Missing translations' → Add both EN and AR text",
                        "",
                        "NEED HELP?",
                        "Contact your system administrator if you encounter issues."
                    };

                    for (int i = 0; i < detailedInstructions.Length; i++)
                    {
                        instructionsSheet.Cells[i + 1, 1].Value = detailedInstructions[i];

                        // Style different sections
                        if (detailedInstructions[i].Contains("INSTRUCTIONS") || detailedInstructions[i].Contains("QUICK START") ||
                            detailedInstructions[i].Contains("COLUMN DESCRIPTIONS") || detailedInstructions[i].Contains("IMPORTANT RULES") ||
                            detailedInstructions[i].Contains("PRO TIPS") || detailedInstructions[i].Contains("EXAMPLES") ||
                            detailedInstructions[i].Contains("TROUBLESHOOTING") || detailedInstructions[i].Contains("NEED HELP"))
                        {
                            instructionsSheet.Cells[i + 1, 1].Style.Font.Bold = true;
                            instructionsSheet.Cells[i + 1, 1].Style.Font.Size = 12;
                            instructionsSheet.Cells[i + 1, 1].Style.Font.Color.SetColor(System.Drawing.Color.DarkBlue);
                        }
                    }
                    instructionsSheet.Column(1).Width = 100;

                    return package.GetAsByteArray();
                }
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "GenerateExcelTemplate", null, null, false);
                throw;
            }
        }

        /// <summary>
        /// Read Excel file and convert to questions
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<Response<List<QuestionVM>>> ReadExcelFile(IFormFile file)
        {
            bool isEnglish = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName == "en";
            try
            {
                if (file == null || file.Length == 0)
                {
                    return new Response<List<QuestionVM>>
                    {
                        Succeeded = false,
                        Message = "No file provided",
                        StatusCode = (int)HttpStatusCode.BadRequest
                    };
                }

                var questions = new List<QuestionVM>();
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var stream = file.OpenReadStream())
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        return new Response<List<QuestionVM>>
                        {
                            Succeeded = false,
                            Message = "No worksheet found in Excel file",
                            StatusCode = (int)HttpStatusCode.BadRequest
                        };
                    }

                    var rowCount = worksheet.Dimension?.Rows ?? 0;
                    if (rowCount <= 1) // Only header row or empty
                    {
                        return new Response<List<QuestionVM>>
                        {
                            Succeeded = false,
                            Message = "Excel file contains no data rows",
                            StatusCode = (int)HttpStatusCode.BadRequest
                        };
                    }

                    // Read data starting from row 2 (skip header)
                    for (int row = 2; row <= rowCount; row++)
                    {
                        try
                        {
                            var questionEn = worksheet.Cells[row, 1].Text?.Trim();
                            var questionAr = worksheet.Cells[row, 2].Text?.Trim();

                            // Skip empty rows - ADD THIS CHECK
                            if (string.IsNullOrEmpty(questionEn) && string.IsNullOrEmpty(questionAr))
                                continue;

                            var question = new QuestionVM
                            {
                                TextEn = questionEn,
                                TextAr = questionAr,
                                AnswertType = (int)AnswersTypes.Text,
                                Answers = new List<AnswerOptions>()
                            };

                            // Read options
                            for (int i = 1; i <= 4; i++)
                            {
                                var optionEnCol = (i - 1) * 2 + 3; // Column 3, 5, 7, 9
                                var optionArCol = (i - 1) * 2 + 4; // Column 4, 6, 8, 10

                                var optionEn = worksheet.Cells[row, optionEnCol].Text?.Trim();
                                var optionAr = worksheet.Cells[row, optionArCol].Text?.Trim();

                                if (!string.IsNullOrEmpty(optionEn) || !string.IsNullOrEmpty(optionAr))
                                {
                                    question.Answers.Add(new AnswerOptions
                                    {
                                        TextEn = optionEn,
                                        TextAr = optionAr,
                                        IsAnswer = false
                                    });
                                }
                            }

                            // Read correct answer
                            var correctAnswerText = worksheet.Cells[row, 11].Text?.Trim();
                            if (int.TryParse(correctAnswerText, out int correctAnswer) &&
                                correctAnswer >= 1 && correctAnswer <= question.Answers.Count)
                            {
                                question.Answers[correctAnswer - 1].IsAnswer = true;
                            }

                            // Only add questions that have both question text and at least 2 answers
                            if ((!string.IsNullOrEmpty(questionEn) || !string.IsNullOrEmpty(questionAr)) &&
                                question.Answers.Count >= 2)
                            {
                                questions.Add(question);
                            }
                        }
                        catch (Exception ex)
                        {
                            await _logger.LogError(ex, "ReadExcelFile - Row Processing", $"Row: {row}", null, false);
                            // Continue processing other rows
                        }
                    }
                }

                return new Response<List<QuestionVM>>
                {
                    Succeeded = true,
                    Data = questions,
                    Message = isEnglish ? $"Successfully read {questions.Count} questions from Excel file" : $"تم قراءة {questions.Count} سؤال من الملف",
                    StatusCode = (int)HttpStatusCode.Ok
                };
            }
            catch (Exception ex)
            {
                await _logger.LogError(ex, "ReadExcelFile", file?.FileName, null, false);
                return new Response<List<QuestionVM>>
                {
                    Succeeded = false,
                    Message = "Error reading Excel file: " + ex.Message,
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }
    }
}
