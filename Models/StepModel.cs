namespace ssk.Models;

public class StepModel
{
    public int Index { get; set; }
    public string Text { get; set; } = string.Empty;

    public string DisplayIndex => $"Step{Index + 1}";

    public StepModel() { }

    public StepModel(int index, string text)
    {
        Index = index;
        Text = text;
    }
}
