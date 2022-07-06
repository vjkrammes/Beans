namespace Beans.Repositories.Models;
public class NoticeSeedModel
{
    public string RecipientEmail { get; set; }
    public string SenderEmail { get; set; }
    public string NoticeDate { get; set; }
    public string Title { get; set; }
    public string Text { get; set; }

    public NoticeSeedModel()
    {
        RecipientEmail = string.Empty;
        SenderEmail = string.Empty;
        NoticeDate = string.Empty;
        Title = string.Empty;
        Text = string.Empty;
    }
}
