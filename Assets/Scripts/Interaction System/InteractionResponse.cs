
using Unity.VisualScripting;

public class InteractionResponse
{
    public string message { get; private set; }
    public bool result { get; private set; }
    public bool updateVisual { get; private set; }
    public bool display { get; private set; }

    public bool hasMessage => !message.IsUnityNull() && message.Length > 0;

    public InteractionResponse(string message = "", bool result = true, bool display = false, bool updateVisual = false)
    {
        this.message = message;
        this.result = result;
        this.display = display;
        this.updateVisual = updateVisual;
    }
}