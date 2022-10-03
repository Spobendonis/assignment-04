namespace Assignment.Infrastructure;

public class UserRepository : IUserRepository
{
    private readonly KanbanContext _context;
    public UserRepository(KanbanContext context)
    {
        _context = context;
    }
    public (Response Response, int UserId) Create(UserCreateDTO user)
    {
        var entity = _context.Users.FirstOrDefault(c => c.Name == user.Name);
        Response reponse;
        
        if (entity is null)
        {
            entity = new User(user.Name, user.Email);

            _context.Users.Add(entity);
            _context.SaveChanges();

            reponse = Created;
        } else {
            reponse = Conflict;
        }

        var created = new UserDTO(entity.Id, entity.Name, entity.Email);

        return (reponse, created.Id);
    }

    public Response Delete(int userId, bool force = false)
    {
        var user = _context.Users.Include(c => c.Items).FirstOrDefault(c => c.Id == userId);
        Response response;

        if (user is null){
            response = NotFound;
        } else if (user.Items.Any()){
            response = Conflict;
        } else {
            _context.Users.Remove(user);
            _context.SaveChanges();

            response = Deleted;
        }
        return response;
    }

    public UserDTO? Find(int userId)
    {
        var users = from c in _context.Users
                            where c.Id == userId
                            select new UserDTO(c.Id, c.Name, c.Email);

        return users.FirstOrDefault();
    }

    public IReadOnlyCollection<UserDTO> Read()
    {
        var users = from c in _context.Users
                     orderby c.Name
                     select new UserDTO(c.Id, c.Name, c.Email);

        return users.ToArray();
    }

    public Response Update(UserUpdateDTO user)
    {
        var entity = _context.Users.Find(user.Id);
        Response response;

        if (entity is null)
        {
            response = NotFound;
        }
        else if (_context.Users.FirstOrDefault(c => c.Id != user.Id && c.Name == user.Name) != null)
        {
            response = Conflict;
        }
        else
        {
            entity.Name = user.Name;
            _context.SaveChanges();
            response = Updated;
        }

        return response;
    }   
}
