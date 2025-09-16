using CMC.Kernel.Core.Enums;
using CMC.Kernel.Core.Services;
using CMC.Kernel.Core.Wrappers;
using CMC.Kernel.Infrastructure.Caching.Model;
using CMC.Presentation.Application.DTOs.Competitions;
using CMC.Presentation.Application.DTOs.Questions;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CMC.Presentation.Application.Services.Questions
{
    public interface IQuestionsService : IApplicationService
    {
        /// <summary>
        /// Get all categories of questions
        /// </summary>
        /// <returns></returns>
        Task<List<LookupModel>> GetCategories(bool withImages);

        /// <summary>
        /// Add Or update question category
        /// </summary>
        /// <param name="categoriesVM"></param>
        /// <returns></returns>
        Task<Response> AddOrUpdateCategory(CategoryDTO categoryDTO);
        /// <summary>
        /// Delete Category
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Response> DeleteCategory(int id,bool withQuestions);
        /// <summary>
        /// Delete Existing image for category
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Response> DeleteExistingImg(int id,AttachmentTypes type);

        /// <summary>
        /// Get Category by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Response<CategoryDTO>> GetCategory(int id);

        /// <summary>
        /// Get All questions per category
        /// </summary>
        /// <param name="searchQuestionDTO"></param>
        /// <returns></returns>
        Task<PagedResult<QuestionListVM>> GetAllQuestions(SearchQuestionDTO searchQuestionDTO);

        /// <summary>
        /// Get Last questions
        /// </summary>
        /// <returns></returns>
        Task<PagedResult<QuestionListVM>> GetLastQuestions();
        /// <summary>
        /// Add questions for category
        /// </summary>
        /// <param name="questions"></param>
        /// <returns></returns>
        Task<Response> AddQuestions(QuestionVM question);
        /// <summary>
        /// Get question by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Response<QuestionVM>> GetQuestion(int id);
        /// <summary>
        /// Delete Question
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Response> DeleteQuestion(int id);
        /// <summary>
        /// Archive Question
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Response> ArchiveQuestions(int type,int categoryId);
        /// <summary>
        /// Get Random question for category in competition
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="questions"></param>
        /// <returns></returns>
        Task<Response<QuestionVM>> GetRandomQuestionPerCategory(int categoryId, List<int> questions);
        /// <summary>
        /// Add multiple questions in bulk
        /// </summary>
        /// <param name="bulkQuestionsDTO"></param>
        /// <returns></returns>
        Task<Response> AddBulkQuestions(BulkQuestionsDTO bulkQuestionsDTO);

        /// <summary>
        /// Validate Excel questions
        /// </summary>
        /// <param name="excelData"></param>
        /// <returns></returns>
        Task<Response<List<QuestionVM>>> ValidateExcelQuestions(List<Dictionary<string, object>> excelData);

        /// <summary>
        /// Generate Excel template for questions
        /// </summary>
        /// <returns></returns>
        Task<byte[]> GenerateExcelTemplate();

        /// <summary>
        /// Read Excel file and convert to questions
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        Task<Response<List<QuestionVM>>> ReadExcelFile(IFormFile file);

    }
}
