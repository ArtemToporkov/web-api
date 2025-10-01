using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using WebApi.MinimalApi.Domain;
using WebApi.MinimalApi.Models;

namespace WebApi.MinimalApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json", "application/xml")]
[Consumes("application/json", "application/xml")]
public class UsersController : Controller
{
    // Чтобы ASP.NET положил что-то в userRepository требуется конфигурация
    private IUserRepository _userRepository;
    private IMapper _mapper;
        
    public UsersController(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    [HttpGet("{userId:guid}", Name = nameof(GetUserById))]
    public ActionResult<UserDto> GetUserById([FromRoute] Guid userId)
    {
        var user = _userRepository.FindById(userId);
        if (user is null)
            return NotFound();
        var userDto = _mapper.Map<UserDto>(user);
        return Ok(userDto);
    }

    [HttpPost]
    public IActionResult CreateUser([FromBody] CreateUserRequest? userRequest)
    {
        if (userRequest is null)
            return BadRequest();
        if (!ModelState.IsValid)
            return UnprocessableEntity(ModelState);
        if (userRequest.Login.Any(c => !char.IsLetterOrDigit(c)))
        {
            ModelState.AddModelError(nameof(CreateUserRequest.Login), "Login must consist only of letters and digits");
            return UnprocessableEntity(ModelState);
        }
        var user = _mapper.Map<UserEntity>(userRequest);
        var insertedUser = _userRepository.Insert(user);
        return CreatedAtRoute(nameof(GetUserById), new { userId = insertedUser.Id }, insertedUser.Id);
    }
    
    [HttpPut("{userId}")]
    public IActionResult UpsertUser([FromRoute] string userId, [FromBody] UpsertUserRequest? userRequest)
    {
        if (userRequest is null || !Guid.TryParse(userId, out var guidUserId))
            return BadRequest();
        if (!ModelState.IsValid)
            return UnprocessableEntity(ModelState);
        var user = _mapper.Map(userRequest, new UserEntity(guidUserId));
        _userRepository.UpdateOrInsert(user, out var isInserted);
        return isInserted 
            ? CreatedAtRoute(nameof(GetUserById), new {userId = user.Id}, user.Id) 
            : NoContent();
    }

    [HttpPatch("{userId:guid}")]
    public IActionResult PartiallyUpdateUser([FromRoute] Guid userId, [FromBody] JsonPatchDocument<PartiallyUpdateUserRequest>? request)
    {
        if (request is null)
            return BadRequest();
        var user = _userRepository.FindById(userId);
        if (user is null)
            return NotFound();
        var partiallyUpdateRequest = _mapper.Map<PartiallyUpdateUserRequest>(user);
        request.ApplyTo(partiallyUpdateRequest);
        TryValidateModel(partiallyUpdateRequest);
        if (!ModelState.IsValid)
            return UnprocessableEntity(ModelState);
        var updatedUser = _mapper.Map(partiallyUpdateRequest, user);
        _userRepository.Update(updatedUser);
        return NoContent();
    }
}