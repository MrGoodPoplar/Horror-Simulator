
public struct InteractionResponse
{
    public string message { get; private set; }
    public bool result { get; private set; }
    public bool display { get; private set; }

    public InteractionResponse(string message, bool result, bool display = false)
    {
        this.message = message;
        this.result = result;
        this.display = display;
    }
}