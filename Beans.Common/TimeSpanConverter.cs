
using Beans.Common.Interfaces;

namespace Beans.Common;
public class TimeSpanConverter : ITimeSpanConverter
{
    public TimeSpan Convert(string specification)
    {
        if (string.IsNullOrWhiteSpace(specification))
        {
            return TimeSpan.Zero;
        }
        var value = 0;
        var spec = specification.ToLowerInvariant().Replace(" ", "");
        for (var i = 0; i < spec.Length; i++)
        {
            if (char.IsDigit(spec[i]))
            {
                var digit = int.Parse(spec.Substring(i, 1));
                value = value * 10 + digit;
            }
            else
            {
                var which = spec[i..];
                return which switch
                {
                    "msec" or "millisecond" or "milliseconds" => TimeSpan.FromMilliseconds(value),
                    "s" or "sec" or "second" or "seconds" => TimeSpan.FromSeconds(value),
                    "m" or "min" or "minute" or "minutes" => TimeSpan.FromMinutes(value),
                    "h" or "hour" or "hours" => TimeSpan.FromHours(value),
                    "d" or "day" or "days" => TimeSpan.FromDays(value),
                    "w" or "week" or "weeks" => TimeSpan.FromDays(value * 7),
                    "y" or "year" or "years" => TimeSpan.FromDays(value * 365),
                    _ => TimeSpan.Zero
                };
            }
        }
        return TimeSpan.FromSeconds(value);
    }
}
