using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ToDoApp.Contexts;
using ToDoApp.RabbitMQCommunicator;

namespace ToDoApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly MyWebApiContext _context;

        //private static string queueName, message;

        public UserController(MyWebApiContext context) 
        {
            context.RabbitMQChannel = RabbitCommunicator.StartRabbitCommunicator();
            _context = context; 
        }

        // GET: api/User
        [HttpGet]
        public ActionResult<IEnumerable<User>> GetAllUsers()
        {
            RabbitCommunicator.queueName = "users";
            RabbitCommunicator.message = $"A request for all users was made";
            RabbitCommunicator.PublishMessage(RabbitCommunicator.queueName, RabbitCommunicator.message, _context.RabbitMQChannel);

            return  _context.Users;
        }

        // GET: api/User/5
        [HttpGet("{id}", Name = "Get")]
        public ActionResult<User> GetSingleUser(int id)
        {
            User user = _context.Users.Find(id);

            if (user == null)
            {
                RabbitCommunicator.queueName = "users";
                RabbitCommunicator.message = $"A request for all user with an unexisting user was made";
                RabbitCommunicator.PublishMessage(RabbitCommunicator.queueName, RabbitCommunicator.message, _context.RabbitMQChannel);

                return NotFound();
            }
               

            return user;
        }

        // POST: api/User
        [HttpPost]
        public ActionResult<User> PostUser(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();


            RabbitCommunicator.queueName = "users";
            RabbitCommunicator.message = $"A request for user creation was made";
            RabbitCommunicator.PublishMessage(RabbitCommunicator.queueName, RabbitCommunicator.message, _context.RabbitMQChannel);

            return CreatedAtAction("GetSingleUser", new User { Id = user.Id }, user);
        }

        // DELETE: api/User/5
        [HttpDelete("{id}")]
        public ActionResult<User> Delete(int id)
        {
            User user = _context.Users.Find(id);
            if (user == null)
            {
                RabbitCommunicator.queueName = "users";
                RabbitCommunicator.message = $"User was not found for deletion";
                RabbitCommunicator.PublishMessage(RabbitCommunicator.queueName, RabbitCommunicator.message, _context.RabbitMQChannel);

                return NotFound();
            }
                

            _context.Users.Remove(user);
            _context.SaveChanges();

            RabbitCommunicator.queueName = "users";
            RabbitCommunicator.message = $"User with ID = {user.Id} was removed";
            RabbitCommunicator.PublishMessage(RabbitCommunicator.queueName, RabbitCommunicator.message, _context.RabbitMQChannel);

            return user;
        }
    }
}
