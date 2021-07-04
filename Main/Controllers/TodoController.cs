using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Main.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Main.Persistence;
using System;

namespace Main.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TodosController : ControllerBase
    {
        private readonly ILogger<TodosController> _logger;
        private readonly ITodoRepository _todoRepository;

        public TodosController(ILogger<TodosController> logger,
            ITodoRepository todoRepository)
        {
            _logger = logger;
            _todoRepository = todoRepository ??
                throw new ArgumentNullException(nameof(todoRepository));
        }

        [HttpPost]
        [Route("{userId}")]
        public async Task<IActionResult> AddTodo(int userId, TodoRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));

            var reviewItem = ToTodoItem(userId, request);
            await _todoRepository.AddAsync(reviewItem);

            return Created(
                Url.Link("GetUserTodo", new
                {
                    userId,
                    productName = request.Name
                }), null);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTodos()
        {
            var results = await _todoRepository.GetAllAsync();
            var reviews = ToTodoResponses(results);
            return ToActionResult(reviews);
        }

        [HttpGet]
        [Route("{userId}")]
        public async Task<IActionResult> GetUserTodos(int userId)
        {
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
            var results = await _todoRepository.GetUserReviewsAsync(userId);
            var reviews = ToTodoResponses(results);
            return ToActionResult(reviews);
        }

        [HttpGet]
        [Route("{userId}/{productName}", Name = "GetUserTodo")]
        public async Task<IActionResult> GetUserTodo(int userId, string productName)
        {
            if (productName == null) throw new ArgumentNullException(nameof(productName));
            if (userId <= 0) throw new ArgumentOutOfRangeException(nameof(userId));
            var result = await _todoRepository.GetReviewAsync(userId, productName);
            var review = ToTodoResponse(result);
            return result is null ? (IActionResult) NotFound(null) : Ok(review);
        }

        private IActionResult ToActionResult(IEnumerable<TodoResponse> reviews)
        {
            return reviews.Any() ? Ok(reviews) : (IActionResult) NotFound(null);
        }

                private static IEnumerable<TodoResponse> ToTodoResponses(
            IEnumerable<TodoItem> results)
        {
            return results?.Select(ToTodoResponse);
        }

        private static TodoResponse ToTodoResponse(TodoItem item)
        {
            if (item is null)
                return default;

            return new TodoResponse
            {
                Name = item.Name,
                UserEmail = item.UserEmail,
                Status = item.Status
            };
        }

        private static TodoItem ToTodoItem(int userId, TodoRequest request)
        {
            return new TodoItem
            {
                Name = request.Name,
                UserEmail = request.UserEmail,
                Status = request.Status
            };
        }
    }
}