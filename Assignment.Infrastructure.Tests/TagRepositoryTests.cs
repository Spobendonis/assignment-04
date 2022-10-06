namespace Assignment.Infrastructure.Tests;

public class TagRepositoryTests
{
    private readonly SqliteConnection _connection;
    private readonly KanbanContext _context;
    private readonly TagRepository _repository;

    public TagRepositoryTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();
        var builder = new DbContextOptionsBuilder<KanbanContext>().UseSqlite(_connection);
        _context = new KanbanContext(builder.Options);
        _context.Database.EnsureCreated();

        var custom = new Tag("Custom") {Id = 1};

        var fancy = new Tag("Fancy") {Id = 2};

        _context.Tags.AddRange(custom, fancy);
        _context.SaveChanges();

        _repository = new TagRepository(_context);
    }

    [Fact]
    public void Create()
    {
        var (response, createdId) = _repository.Create(new TagCreateDTO("CoolestTag"));

        response.Should().Be(Created);

        createdId.Should().Be(3);
    }

    [Fact]
    public void DeleteItem() {
        var response = _repository.Delete(2);
        
        response.Should().Be(Deleted);
    }

    [Fact]
    public void CreateTagAlreadyExists() {
        var (response, createdId) = _repository.Create(new TagCreateDTO("Fancy"));

        response.Should().Be(Conflict);

        createdId.Should().Be(2);
    }

    [Fact]
    public void FindTag() {
        var tag = _repository.Find(1);

        tag.Id.Should().Be(1);
        tag.Name.Should().Be("Custom");
    }

    [Fact]
    public void ReadTags() {
        var tags = _repository.Read();

        tags.Count.Should().Be(2);
        tags.ElementAt(0).Should().Be(new TagDTO(1, "Custom"));
        tags.ElementAt(1).Should().Be(new TagDTO(2, "Fancy"));
    }

    [Fact]
    public void UpdateTagUpdated() {
        var response = _repository.Update(new TagUpdateDTO(1, "Crane"));

        response.Should().Be(Updated);

        var tag = _repository.Find(1);

        tag.Id.Should().Be(1);
        tag.Name.Should().Be("Crane");
    }

    [Fact]
    public void UpdateTagNotFound() {
        var response = _repository.Update(new TagUpdateDTO(3, "Crane"));

        response.Should().Be(NotFound);
    }

    [Fact]
    public void UpdateTagConflict() {
        var response = _repository.Update(new TagUpdateDTO(2, "Custom"));

        response.Should().Be(Conflict);
    }

    [Fact]
    public void RemoveWithoutForceGivesConflict() {
        UserRepository URepo = new UserRepository(_context);
        WorkItemRepository WIRepo = new WorkItemRepository(_context);
        var (Uresponse, Uentity) = URepo.Create(new UserCreateDTO("jim", "jim@gmail.com"));
        var (WIresponse, WIentity) = WIRepo.Create(new WorkItemCreateDTO("title", 1, "a test item", new List<string>{"Custom"}));
        var response = _repository.Delete(1);

        response.Should().Be(Conflict);
    }

    [Fact]
    public void RemoveWithForceGivesDeleted() {
        UserRepository URepo = new UserRepository(_context);
        WorkItemRepository WIRepo = new WorkItemRepository(_context);
        var (Uresponse, Uentity) = URepo.Create(new UserCreateDTO("jim", "jim@gmail.com"));
        var (WIresponse, WIentity) = WIRepo.Create(new WorkItemCreateDTO("title", 1, "a test item", new List<string>{"Custom"}));
        var response = _repository.Delete(1, true);

        response.Should().Be(Deleted);
    }
}
