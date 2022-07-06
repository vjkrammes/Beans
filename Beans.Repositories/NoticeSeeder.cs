
using Beans.Common;
using Beans.Common.Enumerations;
using Beans.Repositories.Entities;
using Beans.Repositories.Interfaces;
using Beans.Repositories.Models;

using Microsoft.Extensions.Configuration;

namespace Beans.Repositories;
public class NoticeSeeder : SeederBase<NoticeEntity, INoticeRepository>, INoticeSeeder
{
    private readonly IUserRepository _userRepository;

    public NoticeSeeder(INoticeRepository repository, IUserRepository userRepository) : base(repository) => _userRepository = userRepository;

    public override async Task SeedAsync(IConfiguration configuration, string sectionName)
    {
        if (configuration is null || string.IsNullOrWhiteSpace(sectionName))
        {
            return;
        }
        var section = configuration.GetSection(sectionName);
        if (section is null)
        {
            return;
        }
        var items = section.Get<NoticeSeedModel[]>();
        if (items is null || !items.Any())
        {
            return;
        }
        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.RecipientEmail) || string.IsNullOrWhiteSpace(item.SenderEmail) || string.IsNullOrWhiteSpace(item.Title)
              || string.IsNullOrWhiteSpace(item.Text))
            {
                Console.WriteLine("Notice Seed Failure: invalid seed model:");
                Console.WriteLine(Tools.DumpObject(item));
                continue;
            }
            if (!DateTime.TryParse(item.NoticeDate, out var noticeDate))
            {
                noticeDate = DateTime.UtcNow;
            }
            var recipient = await _userRepository.ReadAsync(item.RecipientEmail);
            if (recipient is null)
            {
                Console.WriteLine($"Notice Seed Failure: No recipient with the email '{item.RecipientEmail}' was found");
                continue;
            }
            int senderid;
            if (string.Equals(item.SenderEmail, "exchange", StringComparison.OrdinalIgnoreCase))
            {
                senderid = -1;
            }
            else if (string.Equals(item.SenderEmail, "system", StringComparison.OrdinalIgnoreCase))
            {
                senderid = 0;
            }
            else
            {
                var user = await _userRepository.ReadAsync(item.SenderEmail);
                if (user is null)
                {
                    Console.WriteLine($"Notice Seed Failure: No sender with the email '{item.SenderEmail}' was found");
                    continue;
                }
                senderid = user.Id;
            }
            var notice = new NoticeEntity
            {
                Id = 0,
                UserId = recipient.Id,
                SenderId = senderid,
                NoticeDate = noticeDate,
                Title = item.Title,
                Text = item.Text,
                Read = false
            };
            var result = await _repository.InsertAsync(notice);
            if (!result.Successful)
            {
                if (result.ErrorCode != DalErrorCode.Duplicate)
                {
                    Console.WriteLine($"Notice Seed Faulure: {result.ErrorMessage ?? result.ErrorCode.GetDescriptionFromEnumValue()}");
                    Console.WriteLine(Tools.DumpObject(notice));
                }
            }
        }
    }
}
