
public interface IGuided
{
    public string guid { get; set; }
    
    public void GenerateGUID()
    {
        guid = System.Guid.NewGuid().ToString();
    }
}