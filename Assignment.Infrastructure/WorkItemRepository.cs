namespace Assignment.Infrastructure;
using Assignment.Core;


public class WorkItemRepository : IWorkItemRepository
{

    private readonly KanbanContext _context;

    public WorkItemRepository(KanbanContext context){
        _context = context;
    }

    public (Response Response, int ItemId) Create(WorkItemCreateDTO item)
    {
       var entity = new WorkItem(item.Title)
        {   
            
            Title = item.Title,
            AssignedToId = item.AssignedToId,
            Description = item.Description,
            Tags = CreateOrUpdateTags(item.Tags).ToHashSet()
            
        };
        _context.Items.Add(entity);
        _context.SaveChanges();

        var created = new WorkItemDetailsDTO(entity.Id, entity.Title, entity.Description, DateTime.UtcNow,
         entity.AssignedTo?.Name, entity.Tags.Select(t =>t.Name).ToList<string>().AsReadOnly(), State.New, DateTime.UtcNow);

        

        return (Created, created.Id);
    }

    public Response Delete(int itemId)
    {
        var WorkItem = _context.Items.Find(itemId);
        Response response = NotFound;

        if (itemId is 0)
        {
            response = NotFound;
        } 
        else if(WorkItem?.State == State.New)
        {
            _context.Items.Remove(WorkItem);
            response = Deleted;
        }
        else if(WorkItem?.State == State.Active)
        {
            WorkItem.State = State.Removed;
        }
        else if(WorkItem?.State == State.Resolved || WorkItem?.State == State.Closed || WorkItem?.State == State.Removed)
        {
            response = Conflict;
        }

        _context.SaveChanges();

        return response;
    }

    public WorkItemDetailsDTO? Find(int itemId)
    {

        var items = from i in _context.Items
                            let tags = i.Tags.Select(t =>t.Name).ToList<string>().AsReadOnly()
                            where i.Id == itemId
                            select new WorkItemDetailsDTO(i.Id, i.Title, i.Description, DateTime.UtcNow, i.AssignedTo == null ? null : i.AssignedTo.Name, tags, i.State,DateTime.UtcNow);

        return items.FirstOrDefault();
    }

    public IReadOnlyCollection<WorkItemDTO> Read()
    {
        var items = from i in _context.Items
                     orderby i.Title
                     select new WorkItemDTO(i.Id, i.Title, i.AssignedTo.Name, i.Tags.Select(t =>t.Name).ToHashSet(),i.State);

        return items.ToArray();
    }

    
    public IReadOnlyCollection<WorkItemDTO> ReadByState(State state)
    {
        var workItems = from i in _context.Items
                where i.State == state
                select new WorkItemDTO(i.Id, i.Title, i.AssignedTo.Name, i.Tags.Select(t =>t.Name).ToHashSet(), i.State);

        return workItems.ToList();
    }

    public IReadOnlyCollection<WorkItemDTO> ReadByTag(string tag)
    {
        var workItems = from i in _context.Items
                where i.Tags.Select(t =>t.Name).ToList<string>().AsReadOnly().Contains(tag)
                select new WorkItemDTO(i.Id, i.Title, i.AssignedTo.Name, i.Tags.Select(t =>t.Name).ToHashSet(), i.State);

        return workItems.ToList();
    }

    public IReadOnlyCollection<WorkItemDTO> ReadByUser(int userId)
    {
          var workItems = from i in _context.Items
                where i.AssignedTo.Id == userId
                select new WorkItemDTO(i.Id, i.Title, i.AssignedTo.Name,i.Tags.Select(t =>t.Name).ToHashSet(), i.State);

        return workItems.ToList();
    }

    public IReadOnlyCollection<WorkItemDTO> ReadRemoved()
    {
        var workItems = from i in _context.Items
                where i.State == State.Removed
                select new WorkItemDTO(i.Id, i.Title, i.AssignedTo.Name, i.Tags.Select(t =>t.Name).ToHashSet(), i.State);

        return workItems.ToList();
    }

    public Response Update(WorkItemUpdateDTO item)
    {

    var entity = _context.Items.Find(item.Id);

        if (entity == null)
        {
            return NotFound;
        }

        if (entity.State != item.State){

            //Change Time?

        }

        if (entity.AssignedToId != item.AssignedToId){
        var users = from u in _context.Users
            where u.Id == item.AssignedToId
            select u;

        if(!users.Any())
        {
            return Response.BadRequest;
        }
        }

        entity.Id = item.Id;
        entity.Title = item.Title;
        entity.AssignedToId = item.AssignedToId;
        entity.Description = item.Description;
        entity.Tags = CreateOrUpdateTags(item.Tags).ToHashSet();
        entity.State = item.State;

        _context.SaveChanges();

        return Response.Updated;

    }

    private User? CreateOrUpdateUser(string? userName, string? userEmail) {

        return userName is null || userEmail is null ? null : _context.Users.FirstOrDefault(u => u.Name == userName && u.Email == userEmail) ?? new User(userName,userEmail);
    } 

    private IEnumerable<Tag> CreateOrUpdateTags(IEnumerable<string> tagNames)
    {
        var existing = _context.Tags.Where(t => tagNames.Contains(t.Name)).ToDictionary(t => t.Name);

        foreach (var tagName in tagNames)
        {
            existing.TryGetValue(tagName, out var tag);

            yield return tag ?? new Tag(tagName);
        }
    }
}
