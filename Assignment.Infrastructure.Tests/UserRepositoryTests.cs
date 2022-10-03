namespace Assignment.Infrastructure.Tests;

public class UserRepositoryTests
{
    private readonly SqliteConnection _connection;
    private readonly KanbanContext _context;
    private readonly UserRepository _repository;
    
    public UserRepositoryTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();
        var builder = new DbContextOptionsBuilder<KanbanContext>().UseSqlite(_connection);
        _context = new KanbanContext(builder.Options);
        _context.Database.EnsureCreated();

        var bob = new User("Bob", "bob@mail.com") {Id = 1};

        var tim = new User("Tim", "tim@mail.com") {Id = 2};

        _context.Users.AddRange(bob, tim);
        _context.SaveChanges();

        _repository = new UserRepository(_context);
    }

    [Fact]
    public void Create()
    {
        var (response, createdId) = _repository.Create(new UserCreateDTO("Lisa", "lisa@mail.com"));

        response.Should().Be(Created);

        createdId.Should().Be(3);
    }

    [Fact]
    public void Delete()
    {
        var response = _repository.Delete(2);

        response.Should().Be(Deleted);

        var entity = _context.Users.Find(2);

        entity.Should().BeNull();
    }

    [Fact]
    public void Find() => _repository.Find(2).Should().Be(new UserDTO(2, "Tim", "tim@mail.com"));

    [Fact]
    public void Find_Non_Existing() => _repository.Find(42).Should().BeNull();

    [Fact]
    public void Read() => _repository.Read().Should().BeEquivalentTo(new[] { new UserDTO(1, "Bob", "bob@mail.com"), new UserDTO(2, "Tim", "tim@mail.com") });

    [Fact]
    public void Update_Non_Existing() => _repository.Update(new UserUpdateDTO(42, "Tim", "tim@mail.com")).Should().Be(NotFound);

    [Fact]
    public void Update_Conflict()
    {
        var response = _repository.Update(new UserUpdateDTO(2, "Bob", "bob@mail.com"));

        response.Should().Be(Conflict);

        var entity = _context.Users.Find(2)!;

        entity.Name.Should().Be("Tim");
    }

    [Fact]
    public void Update()
    {
        var response = _repository.Update(new UserUpdateDTO(2, "Timmy", "timmy@gmail.com"));

        response.Should().Be(Updated);

        var entity = _context.Users.Find(2)!;

        entity.Name.Should().Be("Timmy");
    }
}
