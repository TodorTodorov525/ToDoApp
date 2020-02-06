using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ToDoApp.Contexts;
using Microsoft.EntityFrameworkCore;
using ToDoApp.Entities;
using ToDoApp.RabbitMQCommunicator;
using RabbitMQ.Client;
using System.Text;

namespace ToDoApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticleController : ControllerBase
    {

        private readonly MyWebApiContext _context;

        private static string defaultStatus = "waiting";

        private string[] ArticleStatus = { "waiting", "in progress", "done" };
        public ArticleController(MyWebApiContext context)
        {
            context.RabbitMQChannel = RabbitCommunicator.StartRabbitCommunicator();
            _context = context;
        }

        // GET: api/Article
        [HttpGet]
        public ActionResult<IEnumerable<Article>> GetArticles()
        {
            RabbitCommunicator.queueName = "articles";
            RabbitCommunicator.message = $"A request for all articles was made";
            RabbitCommunicator.PublishMessage(RabbitCommunicator.queueName, RabbitCommunicator.message, _context.RabbitMQChannel);

            return  _context.Articles;
        } 

        // GET: api/Article/5
        [HttpGet("{id}")]
        public ActionResult<Article> GetSingleArticle(int id)
        {
            Article article = _context.Articles.Find(id);

            if (article == null)
            {
                RabbitCommunicator.queueName = "errors";
                RabbitCommunicator.message = $"Article with id = {id.ToString()} not found";
                RabbitCommunicator.PublishMessage(RabbitCommunicator.queueName, RabbitCommunicator.message,  _context.RabbitMQChannel);

                return NotFound();
            }

            RabbitCommunicator.queueName = "articles";
            RabbitCommunicator.message = $"Article with id = {id.ToString()} was found";
            RabbitCommunicator.PublishMessage(RabbitCommunicator.queueName, RabbitCommunicator.message, _context.RabbitMQChannel);

            return article;
        }

        //POST: api/Article
        [HttpPost]
        public ActionResult<Article> PostArticle(Article article)
        {
            //user not found
            if (_context.Users.Find(article.UserID) == null)
            {
                RabbitCommunicator.queueName = "errors";
                RabbitCommunicator.message = $"Non existing user tried to create an article";
                RabbitCommunicator.PublishMessage(RabbitCommunicator.queueName, RabbitCommunicator.message, _context.RabbitMQChannel);

                return BadRequest();
            }

            article.Status = defaultStatus; //when creating an article it should be waiting
            article.Owner = _context.Users.Find(article.UserID).Username;
            _context.Articles.Add(article);
            _context.Users.Find(article.UserID).Articles.Add(article);
            _context.SaveChanges();

            RabbitCommunicator.queueName = "articles";
            RabbitCommunicator.message = $"An article was created by user {article.Owner}, its ID is {article.Id}";
            RabbitCommunicator.PublishMessage(RabbitCommunicator.queueName, RabbitCommunicator.message, _context.RabbitMQChannel);

            return CreatedAtAction("GetSingleArticle", new Article { Id = article.Id }, article);
        }
        // PUT: api/Article/1
        [HttpPut("{id}")]
        public ActionResult UpdateArticle(int id, Article article)
        {
            //an article cannot change its ID, nor to be assigned with unexisting user, also the status must be one of the already existing
            if (id != article.Id || _context.Users.Find(article.UserID) == null || Article.IsStatusValid(ArticleStatus, article.Status))
            {
                RabbitCommunicator.queueName = "errors";
                RabbitCommunicator.message = $"Error in article update";
                RabbitCommunicator.PublishMessage(RabbitCommunicator.queueName, RabbitCommunicator.message, _context.RabbitMQChannel);

                return BadRequest();
            }
            _context.Entry(article).State = EntityState.Modified;
            _context.SaveChanges();

            RabbitCommunicator.queueName = "article";
            RabbitCommunicator.message = $"An article with id {article.Id} was updated";
            RabbitCommunicator.PublishMessage(RabbitCommunicator.queueName, RabbitCommunicator.message, _context.RabbitMQChannel);

            return NoContent();
        }

        // DELETE: api/Article/1
        [HttpDelete("{id}")]
        public ActionResult<Article> DeleteArticle(int id)
        {
            Article article = _context.Articles.Find(id);
            if (article == null)
            {
                RabbitCommunicator.queueName = "errors";
                RabbitCommunicator.message = $"Error in article delete, probably not found";
                RabbitCommunicator.PublishMessage(RabbitCommunicator.queueName, RabbitCommunicator.message, _context.RabbitMQChannel);

                return NotFound();
            }
            _context.Articles.Remove(article);
            _context.SaveChanges();

            RabbitCommunicator.queueName = "article";
            RabbitCommunicator.message = $"Article was deleted succesfully";
            RabbitCommunicator.PublishMessage(RabbitCommunicator.queueName, RabbitCommunicator.message, _context.RabbitMQChannel);

            return article;
        }
    }
}
