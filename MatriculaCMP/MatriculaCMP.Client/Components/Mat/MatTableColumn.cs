namespace MatriculaCMP.Client.Components.Mat;

public class MatTableColumn
{
    public string Title { get; set; } = "";
    public string? Width { get; set; }
    public Func<object, object?>? GetValue { get; set; }
}
