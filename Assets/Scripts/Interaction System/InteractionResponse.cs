
using Unity.VisualScripting;

public struct InteractionResponse
{
    public string message { get; private set; }
    public bool result { get; private set; }
    public bool updateVisual { get; private set; }
    public bool display { get; private set; }

    public bool hasMessage => !message.IsUnityNull() && message.Length > 0;

    public InteractionResponse(string message, bool result, bool display = false, bool updateVisual = false)
    {
        this.message = message;
        this.result = result;
        this.display = display;
        this.updateVisual = updateVisual;
    }
}