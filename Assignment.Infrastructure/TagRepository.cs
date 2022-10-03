namespace Assignment.Infrastructure;

public class TagRepository : ITagRepository
{
    private readonly KanbanContext _context;

    public TagRepository(KanbanContext context)
    {
        _context = context;
    }
    public (Response Response, int TagId) Create(TagCreateDTO tag)
    {
        var entity = _context.Tags.FirstOrDefault(t => t.Name == tag.Name);
        Response response;

        if (entity is null) {
            entity = new Tag(tag.Name);

            _context.Add(entity);
            _context.SaveChanges();

            response = Created;
        } else {
            response = Conflict;
        }
        return (response, entity.Id);
    }

    public Response Delete(int tagId, bool force = false)
    {
        var entity = _context.Tags.Include(w => w.WorkItems).FirstOrDefault(t => t.Id == tagId);
        Response response;

        if (entity is null) {
            response = NotFound;
        } else if (entity.WorkItems.Any()) {
            if (force) {
                foreach (var w in entity.WorkItems) {
                    w.Tags.Remove(entity);
                }
                _context.Remove(entity);
                _context.SaveChanges();
                response = Deleted;
            } else {
                response = Conflict;
            }
        } else {
            _context.Remove(entity);
            _context.SaveChanges();

            response = Deleted;
        }
        return response;
    }

    public TagDTO Find(int tagId)
    {
        var tag = from t in _context.Tags
                    where t.Id == tagId
                    select new TagDTO(t.Id, t.Name);
        return (TagDTO) tag;
    }

    public IReadOnlyCollection<TagDTO> Read()
    {
        var tags = from t in _context.Tags
                    select new TagDTO(t.Id, t.Name);
        return tags.ToList();
    }

    public Response Update(TagUpdateDTO tag)
    {
        var entity = _context.Tags.Find(tag.Id);
        Response response;

        if (entity is null) {
            response = NotFound;
        } else if (_context.Tags.FirstOrDefault(t => t.Id != tag.Id && t.Name == tag.Name) != null) {
            response = Conflict;
        } else {
            entity.Name = tag.Name;
            _context.SaveChanges();
            response = Updated;
        }
        return response; 
    }
}
