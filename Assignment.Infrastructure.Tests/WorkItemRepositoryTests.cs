namespace Assignment.Infrastructure.Tests;

public class WorkItemRepositoryTests 
{
    private KanbanContext _context;
    public WorkItemRepository _repo;
    
    private readonly SqliteConnection _connection;

    public WorkItemRepositoryTests(){
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();
        var builder = new DbContextOptionsBuilder<KanbanContext>();
        builder.UseSqlite(_connection);
        var context = new KanbanContext(builder.Options);
        context.Database.EnsureCreated();

            var item1 = new WorkItem("Get Shit done"){Id = 1};
            var item2 = new WorkItem("Make food"){Id = 2};

        context.Items.AddRange(item1, item2);
        context.SaveChanges();

        _context = context;
        _repo = new WorkItemRepository(_context);
}

    [Fact]
    public void CreateWorkItemTest()
    {
        // Given
        var actual = _repo.Create(new WorkItemCreateDTO("Make Food",null,"Go make a non-pasta dish for three people",new List<string>()));
        // When

        // Then
        actual.Response.Should().Be(Response.Created);
        actual.ItemId.Should().Be(3);
    }

    [Fact]
    public void WorkItem_with_State_New_Should_be_deleteable()
    {
         // Given
        var newItem = _repo.Create(new WorkItemCreateDTO("Make Food",null,"Go make a non-pasta dish for three people",new List<string>(){"#Food"}));
        // When
        _repo.Delete(newItem.ItemId);
        
        // Then
        _repo.Find(newItem.ItemId).Should().Be(null);
        
    }

    [Fact]
    public void WorkItem_with_State_Active_should_be_state_removed_when_deleted()
    {
        // Given
        var item = _repo.Create(new WorkItemCreateDTO("Make Food",null,"Go make a non-pasta dish for three people",new List<string>(){"#Food"}));
        var itemDetails = _repo.Find(item.ItemId);
        _repo.Update(new WorkItemUpdateDTO(itemDetails.Id, itemDetails.Title, null, itemDetails.Description, itemDetails.Tags.ToList<string>(), State.Active));
        // When
        _repo.Delete(item.ItemId);
        itemDetails = _repo.Find(item.ItemId);
        // Then
        itemDetails.State.Should().Be(State.Removed);
        
    }
    [Fact]
    public void WorkItem_with_State_Resolved_should_return_Conflict()
    {
        // Given
        var item = _repo.Create(new WorkItemCreateDTO("Make Food",null,"Go make a non-pasta dish for three people",new List<string>(){"#Food"}));
        var itemDetails = _repo.Find(item.ItemId);
        _repo.Update(new WorkItemUpdateDTO(itemDetails.Id, itemDetails.Title, null, itemDetails.Description, itemDetails.Tags.ToList<string>(), State.Resolved));
        // When
        Response response = _repo.Delete(item.ItemId);
        // Then
        response.Should().Be(Response.Conflict);
        
    }

    [Fact]
    public void WorkItem_with_State_Closed_should_return_Conflict()
    {
        // Given
        var item = _repo.Create(new WorkItemCreateDTO("Make Food",null,"Go make a non-pasta dish for three people",new List<string>(){"#Food"}));
        var itemDetails = _repo.Find(item.ItemId);
        _repo.Update(new WorkItemUpdateDTO(itemDetails.Id, itemDetails.Title, null, itemDetails.Description, itemDetails.Tags.ToList<string>(), State.Closed));
        // When
        Response response = _repo.Delete(item.ItemId);
        // Then
        response.Should().Be(Response.Conflict);
    }

    [Fact]
    public void WorkItem_with_State_Removed_should_return_Conflict()
    {
        // Given
        var item = _repo.Create(new WorkItemCreateDTO("Make Food",null,"Go make a non-pasta dish for three people",new List<string>(){"#Food"}));
        var itemDetails = _repo.Find(item.ItemId);
        _repo.Update(new WorkItemUpdateDTO(itemDetails.Id, itemDetails.Title, null, itemDetails.Description, itemDetails.Tags.ToList<string>(), State.Removed));
        // When
        Response response = _repo.Delete(item.ItemId);
        // Then
        response.Should().Be(Response.Conflict);
        
    }

    [Fact]
    public void WorkItem_Just_Created_should_have_State_New()
    {
        // Given
        var item = _repo.Create(new WorkItemCreateDTO("Make Food",null,"Go make a non-pasta dish for three people",new List<string>(){"#Food"}));
        var itemDetails = _repo.Find(item.ItemId);
        // When
       
        // Then
        itemDetails.State.Should().Be(State.New);
        
    }

    [Fact]
    public void New_WorkItems_Should_Have_CreatedTime_equal_currentTime()
    {
         // Given
        var item = _repo.Create(new WorkItemCreateDTO("Make Food",null,"Go make a non-pasta dish for three people",new List<string>(){"#Food"}));
        // When
        var testTime =_repo.Find(item.ItemId).Created;
        // Then
       testTime.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(5));
        
    }

    [Fact]
    public void New_WorkItems_Should_Have_StateUpdatedTime_equal_currentTime()
    {
         // Given
        var item = _repo.Create(new WorkItemCreateDTO("Make Food",null,"Go make a non-pasta dish for three people",new List<string>(){"#Food"}));
        // When
        var testTime =_repo.Find(item.ItemId).StateUpdated;
        // Then
       testTime.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(5));
        
    }

    [Fact]
    public void WorkTimesTags_Should_be_changable_in_Update()
    {
         // Given
        var item = _repo.Create(new WorkItemCreateDTO("Make Food",null,"Go make a non-pasta dish for three people",new List<string>(){"#Food"}));
        var itemDetails = _repo.Find(item.ItemId);
        // When
        _repo.Update(new WorkItemUpdateDTO(itemDetails.Id, itemDetails.Title, null, itemDetails.Description, new List<string>(){"#Food","#Water"}, State.Removed));
        var tags =_repo.Find(item.ItemId).Tags;
        // Then
       tags.Should().Contain("#Water");
        
    }

    [Fact]
    public void WorkTimesTags_Should_be_setable_in_Create()
    {
         // Given
        var item = _repo.Create(new WorkItemCreateDTO("Make Food",null,"Go make a non-pasta dish for three people",new List<string>(){"#Food"}));
        var tags =_repo.Find(item.ItemId).Tags;
        // When
        
        // Then
       tags.Should().Contain("#Food");
        
    }


    [Fact]
    public void Assigning_nonexistingUser_should_return_BadRequest()
    {
         // Given
        var item = _repo.Create(new WorkItemCreateDTO("Make Food",null,"Go make a non-pasta dish for three people",new List<string>(){"#Food"}));
        var itemDetails = _repo.Find(item.ItemId);
        // When
        Response response =_repo.Update(new WorkItemUpdateDTO(itemDetails.Id, itemDetails.Title, 109, itemDetails.Description, new List<string>(){"#Food","#Water"}, State.Removed));
        
        // Then
       response.Should().Be(Response.BadRequest);
        
    }


    [Fact]
    public void Update_Non_Existing()
    {
        var workitem = new WorkItemUpdateDTO(42, "Hey There", null, "This is a non existing description", new List<string>(), State.Removed);

        var status = _repo.Update(workitem);

        status.Should().Be(NotFound);

    // public record WorkItemUpdateDTO(int Id, [StringLength(100)]string Title, int? AssignedToId, string? Description, ICollection<string> Tags, State State);

    }


    [Fact]
    public void Delete()
    {
        var status = _repo.Delete(1);

        status.Should().Be(Deleted);
        _context.Items.Find(1).Should().BeNull();
    }

    [Fact]
    public void Delete_Non_Existing() => _repo.Delete(42).Should().Be(NotFound);

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
