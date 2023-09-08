using CMC.Kernel.Core.Services;
using CMC.Kernel.Core.Wrappers;
using CMC.Kernel.Infrastructure.Caching.Model;
using CMC.Presentation.Application.DTOs.Questions;
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
        Task<List<LookupModel>> GetCategories();

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
        Task<Response> DeleteExistingImg(int id);

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

    }
}
