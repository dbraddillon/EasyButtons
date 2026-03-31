using EasyButtons.Models;

namespace EasyButtons.ViewModels;

public class ButtonGroup(string name, IEnumerable<EasyButton> items) : List<EasyButton>(items)
{
    public string Name { get; } = name;
    public bool HasName => !string.IsNullOrEmpty(Name);
}
